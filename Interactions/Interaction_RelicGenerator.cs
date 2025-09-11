using CotLTemplateMod;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using src.Extensions;
using Lamb.UI.FollowerInteractionWheel;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using CotLMiniMods.Structures.Proxies;
using CotLMiniMods.Structures;
using System.Collections;
using UnityEngine.XR;
using src.UINavigator;
using UnityEngine.UI;
using Lamb.UI.Menus.PlayerMenu;
using COTL_API.CustomTarotCard;
using TMPro;
using System.Linq;
using Lamb.UI.PauseDetails;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_RelicGenerator : Interaction
    {
        public Structure Structure;
        private bool Activated;
        public StructuresData StructureInfo => this.Structure.Structure_Info;
        public Structures_RelicGenerator cCauldron => this.Structure.Brain as Structures_RelicGenerator;

        public int CurrentSuccesses = 0;
        public int maxSuccesses = 20;
        private Selectable cachedSelectable;


        public UICookingMinigameOverlayController _uiCookingMinigameOverlayController;

        public override void GetLabel()
        {
            this.label = "Trials of the Gods";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Relic Generator");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();

        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;
            this.Activated = true;

            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
            this.StartCoroutine(this.DoRoutine());
        }

        private IEnumerator DoRoutine()
        {
            HUD_Manager.Instance.Hide(false, 0);
            GameManager.GetInstance().CameraSetTargetZoom(4f);
            LetterBox.Show(false);
            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
            PlayerFarming.Instance.state.facingAngle = -90f;

            AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_pull", this.gameObject);
            PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-start", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("cards/cards-loop", 0, true, 0.0f);
            yield return new WaitForSeconds(0.75f);

            var ui = MonoSingleton<UIManager>.Instance.PauseDetailsMenuTemplate.Instantiate();
            MonoSingleton<UINavigatorNew>.Instance.OnDefaultSetComplete += OnSelection;
            ui.Show(false);
            ui.OnHidden += this.OnHidden;

            CreateWishUIPaused(ui);

        }

        private void CreateWishUIPaused(UIPauseDetailsMenuController ui)
        {
            Plugin.Log.LogInfo("1");
            var PauseDetailsMenuContainer = ui.transform.Find("PauseDetailsMenuContainer");
            Plugin.Log.LogInfo("2");
            var leftTransformContent = PauseDetailsMenuContainer.Find("Left").Find("Transform").Find("Content");
            Plugin.Log.LogInfo("3");
            var playerScreen = leftTransformContent.Find("Player Screen");
            Plugin.Log.LogInfo("4 + " + playerScreen.name);
            CharacterMenu.OpeningPlayerFarming = playerFarming;

            var characterMenu = playerScreen.GetComponent<CharacterMenu>();
            Plugin.Log.LogInfo("5");
            characterMenu.Show(true);

            var leftTransformNavigation = PauseDetailsMenuContainer.Find("Left").Find("Transform").Find("Navigation").Find("Transform");
            Plugin.Log.LogInfo("66");
            leftTransformNavigation.gameObject.SetActive(false); //hide navigation bar

            Plugin.Log.LogInfo("7");
            var content = playerScreen.Find("Scroll View").Find("Viewport").Find("Content");
            var playerName = content.Find("Player Name").GetComponent<TMP_Text>();
            playerName.text = "Trials of the Gods";
            Plugin.Log.LogInfo("8");

            content.Find("Health").gameObject.SetActive(false);
            content.Find("Details").gameObject.SetActive(false);
            var relicsContent = content.Find("Active Relic Effects Content");
            Destroy(content.Find("Active Relic Effects Content").Find("No Relics").gameObject);
            relicsContent.gameObject.SetActive(false);
            Plugin.Log.LogInfo("9");

            var tarotCardsContent = content.Find("Tarot Cards Content");
            
            var relicTitle = content.Find("Active Relic Effects").GetComponent<TMP_Text>();
            relicTitle.text = "Augments";
            var tarotTitle = content.Find("Tarot Cards Header").GetComponent<TMP_Text>();
            tarotTitle.text = "Trials";
            var instructions = tarotCardsContent.Find("No Items").GetComponent<TMP_Text>();
            var clonedInstructions = Instantiate(instructions.gameObject, content);
            clonedInstructions.transform.SetSiblingIndex(1);
            instructions.gameObject.SetActive(false);
            clonedInstructions.GetComponent<TMP_Text>().text = "";
            var clonedTarotCardsContent = Instantiate(tarotCardsContent.gameObject, content);
            Plugin.Log.LogInfo("10");

            foreach (Transform child in content)
            {
                if (child.name == "Spacer")
                {
                    child.gameObject.SetActive(false);
                }
            }

            var trials = Plugin.proxyTrials.Keys.ToList();
            var augments = Plugin.proxyAugments.Keys.ToList();


            foreach (var card in trials)
            {
                var slot = characterMenu._tarotCardItemRunTemplate.Instantiate(tarotCardsContent);
                slot.Configure(card);
                if (Plugin.proxyTrialsEnabled.Any(x => x == card))
                {
                    slot.TarotCard.Spine.material = slot.TarotCard._superRareMaterial;
                }
                else
                {
                    slot.TarotCard.Spine.material = slot.TarotCard._normalMaterial;
                }
            }

            foreach (var card in augments)
            {
                var slot = characterMenu._tarotCardItemRunTemplate.Instantiate(clonedTarotCardsContent.transform);
                slot.Configure(card);
                if (Plugin.proxyAugmentsEnabled.Any(x => x == card))
                {
                    slot.TarotCard.Spine.material = slot.TarotCard._superRareMaterial;
                }
                else
                {
                    slot.TarotCard.Spine.material = slot.TarotCard._normalMaterial;
                }
            }
        }

        private void OnSelection(Selectable obj)
        {

            Plugin.Log.LogInfo("OnDefaultSetComplete fired for " + obj.name);
            cachedSelectable = obj;
            // this._card1.Show(true);
            // this._card2.Hide(true);
        }

        private void OnHidden()
        {
            this.Activated = false;
            MonoSingleton<UINavigatorNew>.Instance.OnDefaultSetComplete -= OnSelection;
            cachedSelectable = null;
            this.StartCoroutine(this.BackToIdleRoutine());
            //Time.timeScale = 1f;
            //HUD_Manager.Instance.Show();
        }

        private IEnumerator BackToIdleRoutine()
        {
            PlayerFarming.Instance.Spine.UseDeltaTime = false;
            PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);

            PlayerFarming.Instance.simpleSpineAnimator.Animate("cards/cards-stop-seperate", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);
            yield return new WaitForSeconds(1.5f);

            PlayerFarming.Instance.Spine.UseDeltaTime = true;
            LetterBox.Hide();
            HUD_Manager.Instance.Show(0);
            AudioManager.Instance.PlayOneShot("event:/tarot/tarot_card_close", this.gameObject);
            GameManager.GetInstance().CameraResetTargetZoom();
            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
            yield return null;
        }
        
         public override void Update()
        {
            base.Update();
            if (!this.Activated) return;

            if (Input.GetMouseButtonDown(0))
            {
                //just say that this card is activated, whichever was set as selectable
                if (cachedSelectable == null)
                {
                    Plugin.Log.LogInfo("Mouse click, but cachedSelectable is null");
                    return;
                }
                if (cachedSelectable.TryGetComponent(out TarotCardItem_Run tarotRun))
                {
                    var cardData = tarotRun._card;
                    Plugin.Log.LogInfo("Mouse click, will toggle " + cardData.CardType);
                    var proxyTrial = Plugin.proxyTrials.ContainsKey(cardData.CardType);
                    var proxyAugment = Plugin.proxyAugments.ContainsKey(cardData.CardType);
                    if (!proxyTrial && !proxyAugment) //should never happen
                    {
                        Plugin.Log.LogInfo("Mouse click, but card is not a proxy trial or augment");
                        return;
                    }

                    if (Plugin.proxyTrialsEnabled.Any(x => x == cardData.CardType) || Plugin.proxyAugmentsEnabled.Any(x => x == cardData.CardType))
                    {
                        //remove
                        Plugin.Log.LogInfo("Removing from wished cards");
                        if (proxyTrial) Plugin.proxyTrialsEnabled.Remove(cardData.CardType);
                        if (proxyAugment) Plugin.proxyAugmentsEnabled.Remove(cardData.CardType);
                        tarotRun.TarotCard.Spine.material = tarotRun.TarotCard._normalMaterial;
                    }
                    else
                    {
                        //add
                        Plugin.Log.LogInfo("Adding to wished cards");
                        if (proxyTrial) Plugin.proxyTrialsEnabled.Add(cardData.CardType);
                        if (proxyAugment) Plugin.proxyAugmentsEnabled.Add(cardData.CardType);
                        tarotRun.TarotCard.Spine.material = tarotRun.TarotCard._superRareMaterial;
                    }
                }
                else
                {
                    Plugin.Log.LogInfo("Mouse click, but cachedSelectable has no tarot component");
                }
                
            }
        }

    }
}
