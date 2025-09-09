using CotLTemplateMod;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using src.Extensions;
using Lamb.UI.FollowerInteractionWheel;
using System.Collections;
using COTL_API.CustomTarotCard;
using System.Linq;
using Lamb.UI.KitchenMenu;
using TMPro;
using UnityEngine.Events;
using src.UI.InfoCards;
using src.UINavigator;
using UnityEngine.UI;
using Lamb.UI.Menus.PlayerMenu;
using UnityEngine.EventSystems;
using Lamb.UI.PauseDetails;
using src.UI.Items;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_Wish : Interaction
    {
        public Structure Structure;
        private bool Activated;
        private Transform clonedCurrentlyQueued;
        private Selectable cachedSelectable;

        private Dictionary<GameObject, TarotCards.TarotCard> cardPoolSlots = [];
        private Dictionary<GameObject, TarotCards.TarotCard> wishedSlots = [];
        public override void GetLabel()
        {
            this.label = "Select Cards for Next Run";
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
                    Plugin.Log.LogInfo("Mouse click, will toggle " + cardData.CardType + " level " + cardData.UpgradeIndex);
                    if (Plugin.wishedCards.Any(x => x.CardType == cardData.CardType))
                    {
                        //remove
                        Plugin.Log.LogInfo("Removing from wished cards");
                        Plugin.wishedCards.RemoveAll(x => x.CardType == cardData.CardType);
                        tarotRun.TarotCard.Spine.material = tarotRun.TarotCard._normalMaterial;
                    }
                    else
                    {
                        //add
                        Plugin.Log.LogInfo("Adding to wished cards");
                        Plugin.wishedCards.Add(cardData);
                        tarotRun.TarotCard.Spine.material = tarotRun.TarotCard._superRareMaterial;
                    }
                }
                else if (cachedSelectable.TryGetComponent(out ActiveRelicItem activeRelicItem))
                {
                    var relicData = activeRelicItem.RelicData;
                    Plugin.Log.LogInfo("Mouse click, will toggle relic " + relicData.RelicType);
                    Plugin.relicData = relicData.RelicType;
                    //highlight it RGBA 0.32 0.471 0.546 1 then the rest would be gray 0.1 0.1 0.1 1
                    foreach (Transform relicItem in activeRelicItem.transform.parent)
                    {
                        var background = relicItem.Find("Transform").Find("Background").GetComponent<Image>();
                        var relicData2 = relicItem.GetComponent<ActiveRelicItem>().RelicData;
                        if (Plugin.relicData != relicData2.RelicType)
                        {
                            background.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                        }
                        else
                        {
                            background.color = new Color(0.32f, 0.471f, 0.546f, 1f);
                        }
                    }


                }
                else
                {
                    Plugin.Log.LogInfo("Mouse click, but cachedSelectable has no tarot or relic component");
                }
                
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Wish");
            //load as of 1.1.4
            MonoSingleton<UIManager>.Instance.LoadDungeonAssets();
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
            yield return (object)new WaitForSeconds(0.75f);

            //create UI
            // var ui = MonoSingleton<UIManager>.Instance.FollowerKitchenMenuControllerTemplate.Instantiate();
            // ui.Show(this.Structure.Structure_Info, null);
            var ui = MonoSingleton<UIManager>.Instance.PauseDetailsMenuTemplate.Instantiate();
            MonoSingleton<UINavigatorNew>.Instance.OnDefaultSetComplete += OnSelection;
            ui.Show(false);
            ui.OnHidden += this.OnHidden;

            CreateWishUIPaused(ui);

            // if (card1 != null && card2 != null)
            // {
            //     UITarotChoiceOverlayController tarotChoiceOverlayInstance = MonoSingleton<UIManager>.Instance.ShowTarotChoice(card1, card2);
            //     tarotChoiceOverlayInstance.OnTarotCardSelected += (System.Action<TarotCards.TarotCard>)(card =>
            //     {
            //         TarotCards.TarotCard card3 = card1;
            //         if (card == card1)
            //             card3 = card2;
            //         if (CoopManager.CoopActive)
            //             GameManager.GetInstance().StartCoroutine(this.DelayEffectsRoutine(card3, 0.0f, this.playerFarming.isLamb ? PlayerFarming.players[1] : PlayerFarming.players[0]));

            //         this.StartCoroutine(this.BackToIdleRoutine(card, 0.0f));
            //         /*DataManager.Instance.PlayerRunTrinkets.Remove(GetOther(card));*/
            //         this.Activated = false;
            //     });

            //     UITarotChoiceOverlayController overlayController = tarotChoiceOverlayInstance;
            //     overlayController.OnHidden = overlayController.OnHidden + (System.Action)(() => tarotChoiceOverlayInstance = null);
            // }

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
            // var playerButton = leftTransformNavigation.Find("Player Button").GetComponent<Button>();
            // //press the button
            // playerButton.onClick.Invoke();
            // Plugin.Log.LogInfo("67");
            leftTransformNavigation.gameObject.SetActive(false); //hide navigation bar

            // foreach (Transform child in leftTransformContent)
            // {
            //     child.gameObject.SetActive(false);
            //     if (child.name == "Player Screen")
            //     {
            //         child.gameObject.SetActive(true);
            //     }
            // }
            Plugin.Log.LogInfo("7");
            var content = playerScreen.Find("Scroll View").Find("Viewport").Find("Content");
            var playerName = content.Find("Player Name").GetComponent<TMP_Text>();
            playerName.text = "Your Wishes";
            Plugin.Log.LogInfo("8");

            content.Find("Health").gameObject.SetActive(false);
            content.Find("Details").gameObject.SetActive(false);
            Destroy(content.Find("Active Relic Effects Content").Find("No Relics").gameObject);
            Plugin.Log.LogInfo("9");

            var tarotCardsContent = content.Find("Tarot Cards Content");
            var relicTitle = content.Find("Active Relic Effects").GetComponent<TMP_Text>();
            relicTitle.text = "Relics";
            var instructions = tarotCardsContent.Find("No Items").GetComponent<TMP_Text>();
            var clonedInstructions = Instantiate(instructions.gameObject, content);
            clonedInstructions.transform.SetSiblingIndex(1);
            instructions.gameObject.SetActive(false);
            clonedInstructions.GetComponent<TMP_Text>().text = "Click on the Tarot Cards and 1 Relic that you wish to have in your next run. Highlighted cards and relic will be applied when you start the run.";
            Plugin.Log.LogInfo("10");

            var unusedFoundTrinkets = TarotCards.GetUnusedFoundTrinkets(playerFarming, true);
            unusedFoundTrinkets.AddRange([.. CustomTarotCardManager.CustomTarotCardList.Keys]);

            foreach (var card in unusedFoundTrinkets)
            {
                var slot = characterMenu._tarotCardItemRunTemplate.Instantiate(tarotCardsContent);
                slot.Configure(card);
                if (Plugin.wishedCards.Any(x => x.CardType == card))
                {
                    slot.TarotCard.Spine.material = slot.TarotCard._superRareMaterial;
                }
                else
                {
                    slot.TarotCard.Spine.material = slot.TarotCard._normalMaterial;
                }
            }

            foreach (RelicType RelicTypeEnum in Enum.GetValues(typeof(RelicType)))
            {
                var relic = EquipmentManager.GetRelicData(RelicTypeEnum);
                if (relic == null) continue; //skip none and any missing relics
                Plugin.Log.LogInfo("Adding relic " + relic.RelicType);
                ActiveRelicItem relicItem = characterMenu.AddActiveRelic(relic);

                var background = relicItem.gameObject.transform.Find("Transform").Find("Background").GetComponent<Image>();
                if (Plugin.relicData == RelicType.None) continue;
                if (Plugin.relicData != RelicTypeEnum)
                {
                    background.color = new Color(0.1f, 0.1f, 0.1f, 1f);
                }
                else
                {
                    background.color = new Color(0.32f, 0.471f, 0.546f, 1f);
                }
            }
            
        }

        private void CreateWishUI(UIFollowerKitchenMenuController ui)
        {
            //change title
            Plugin.Log.LogInfo("1");
            var refineryMenuController = ui.transform.Find("RefineryMenuContainer");
            Plugin.Log.LogInfo("2");

            var leftTransform = refineryMenuController.Find("Left").Find("Transform");
            Plugin.Log.LogInfo("3");

            var title = leftTransform.Find("Title").GetComponentInChildren<TMP_Text>();
            Plugin.Log.LogInfo("4");

            title.text = "Your Wishes";
            Plugin.Log.LogInfo("5");

            //CONTENT AREA
            var content = leftTransform.Find("Content").Find("Scroll View").Find("Viewport").Find("Content");
            Plugin.Log.LogInfo("6");

            //change sub title 1
            var subTitle = content.Find("Items Header").GetComponent<TMP_Text>();
            var clonedSubTitle = Instantiate(subTitle, content);
            subTitle.gameObject.SetActive(false); //hide original
            Plugin.Log.LogInfo("7");
            clonedSubTitle.text = "Card Pool";

            //grab a copy of gameobject to clone
            // var slotClone = content.Find("Refined Content").GetComponentInChildren<RecipeItem>();
            // Plugin.Log.LogInfo("8" + slotClone.name);

            var rightContainer = refineryMenuController.Find("Right").Find("InfoCardContainer");
            var cardController = rightContainer.GetComponent<RecipeInfoCardController>();
            MonoSingleton<UINavigatorNew>.Instance.OnDefaultSetComplete -= new Action<Selectable>(cardController.OnSelection);
            MonoSingleton<UINavigatorNew>.Instance.OnDefaultSetComplete += OnSelection;

            //======== create card slots VANILLA========
            var refinedContent = content.Find("Refined Content");
            Plugin.Log.LogInfo("refined content clone and hide");
            var clonedRefinedContent = Instantiate(refinedContent, content);
            refinedContent.gameObject.SetActive(false); //hide


            var unusedFoundTrinkets = TarotCards.GetUnusedFoundTrinkets(playerFarming, true);

            var pauseMenuTemplate = MonoSingleton<UIManager>.Instance.PauseDetailsMenuTemplate.gameObject;
            var playerScreen = pauseMenuTemplate.transform.Find("PauseDetailsMenuContainer").Find("Left").Find("Transform").Find("Content").Find("Player Screen");
            var tarotTemplate = playerScreen.GetComponent<CharacterMenu>()._tarotCardItemRunTemplate;

            foreach (var card in unusedFoundTrinkets)
            {
                Plugin.Log.LogInfo("Creating slot for " + card);
                if (Plugin.wishedCards.Any(x => x.CardType == card)) continue; //skip if already wished for

                Plugin.Log.LogInfo("Post Creating slot for " + card);
                var newSlot = Instantiate(tarotTemplate, clonedRefinedContent);
                newSlot.Configure(card);

                // RecipeItem newSlot = Instantiate(ui.recipeIconPrefab, clonedRefinedContent);
                // newSlot.Configure(InventoryItem.ITEM_TYPE.MEAL_POOP, false, false);

                // newSlot.GetComponent<MMButton>().onClick.RemoveAllListeners();
                // newSlot._amountText.gameObject.SetActive(false);
                // newSlot._hungerContainer.gameObject.SetActive(false);
                // newSlot._starContainer.gameObject.SetActive(false);
                // newSlot._alert.gameObject.SetActive(false);
                // ui.OverrideDefault(newSlot.Button);
                // ui.OverrideDefaultOnce(newSlot.Button);
                // ui.ActivateNavigation();

                // var transform = newSlot.transform.Find("Transform");
                // var amount = transform.Find("Amount");
                // Plugin.Log.LogInfo("get loop amount");
                // amount.gameObject.SetActive(false);

                // var hungerIcon = transform.Find("Hunger Icon");
                // hungerIcon.gameObject.SetActive(false);

                var tarotCard = new TarotCards.TarotCard(card, TarotCards.GetMaxTarotCardLevel(card));
                // newSlot.gameObject.SetActive(true);
                // newSlot.GetComponent<MMButton>().onClick.AddListener(() => SelectCard(newSlot, tarotCard));
                // newSlot.Selectable
                cardPoolSlots.Add(newSlot.gameObject, tarotCard);
            }

            //change sub title 2
            var subTitle2 = content.Find("Queue Text Container").Find("Queue Header").GetComponent<TMP_Text>();
            var clonedSubTitle2 = Instantiate(subTitle2, content);
            subTitle2.gameObject.SetActive(false);
            Plugin.Log.LogInfo("9");
            clonedSubTitle2.text = "Wished Cards";

            var subTitleCount = content.Find("Queue Text Container").Find("Queue Number"); //what to do with this?
            subTitleCount.gameObject.SetActive(false);
            Plugin.Log.LogInfo("10");

            var currentlyQueued = content.Find("Currently Queued");
            clonedCurrentlyQueued = Instantiate(currentlyQueued, content);

            foreach (Transform child in clonedCurrentlyQueued)
            {
                child.gameObject.SetActive(false);
            }

            currentlyQueued.gameObject.SetActive(false); //hide original
            Plugin.Log.LogInfo("13");

        }

        private void SelectCard(RecipeItem slot, TarotCards.TarotCard card)
        {
            Plugin.Log.LogInfo("Selecting card " + card.CardType);
            if (clonedCurrentlyQueued == null)
            {
                Plugin.Log.LogInfo("clonedCurrentlyQueued is null");
                return;
            }

            Plugin.wishedCards.Add(card);
            slot.gameObject.SetActive(false); //hide original in pool

            var clonedSlot = Instantiate(slot, clonedCurrentlyQueued);
            clonedSlot.GetComponent<MMButton>().onClick.RemoveAllListeners();
            clonedSlot.GetComponent<MMButton>().onClick.AddListener(() => RemoveCard(slot, clonedSlot, card));
            clonedSlot.gameObject.SetActive(true);
            wishedSlots.Add(clonedSlot.gameObject, card);
            
        }

        private void RemoveCard(RecipeItem oldSlot, RecipeItem slot, TarotCards.TarotCard card)
        {
            Plugin.Log.LogInfo("Removing card " + card.CardType);
            Plugin.wishedCards.RemoveAll(x => x.CardType == card.CardType);
            oldSlot.gameObject.SetActive(true);
            wishedSlots.Remove(slot.gameObject);
            Destroy(slot.gameObject);
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


        private TarotCards.TarotCard GetCard(bool customFirst = true)
        {
            
            TarotCards.TarotCard card = null;
            bool alreadyTaken = false;
            if (customFirst) {
                TarotCards.Card cardData = CustomTarotCardManager.CustomTarotCardList.Keys.ElementAt(UnityEngine.Random.Range(0, CustomTarotCardManager.CustomTarotCardList.Count));

                card = new TarotCards.TarotCard(cardData, 0);
                /*Plugin.Log.LogInfo("Before contain card " + DataManager.Instance.PlayerRunTrinkets.Contains(card));*/
            }

            
            foreach(var cardz in PlayerFarming.Instance.RunTrinkets)
            {
                if (card == null) break;
                
                if (cardz.CardType == card.CardType)
                {
                    Plugin.Log.LogInfo("custom already taken");
                    alreadyTaken = true;
                }
            }

            if (alreadyTaken || card == null) //if already have the custom card, then draw a vanilla card
            {
                if (!DataManager.Instance.FirstTarot && TarotCards.DrawRandomCard(this.playerFarming, true) != null)
                {
                    DataManager.Instance.FirstTarot = true;
                    card = new TarotCards.TarotCard(TarotCards.Card.Lovers1, 0);
                }
                else
                    card = TarotCards.DrawRandomCard(this.playerFarming, true);
            }
            
            if (card != null)
                TrinketManager.AddEncounteredTrinket(card, this.playerFarming);

            return card;
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
    }
}
