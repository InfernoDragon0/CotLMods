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

namespace CotLMiniMods.Interactions
{
    internal class Interaction_Wish : Interaction
    {
        private bool Activated;
        public override void GetLabel()
        {
            this.label = "20 Gold for Wishes";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Wish");
            //load as of 1.1.4
            MonoSingleton<UIManager>.Instance.LoadDungeonAssets();
        }

        public override void OnInteract(StateMachine state)
        {
            

            if (Inventory.GetItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD) < 20)
            {
                return;
            }

            if (this.Activated) return;
            this.Activated = true;

            Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, -20);
            ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, this.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, null);

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
            yield return (object)new WaitForSeconds(0.1f);

            GameManager.GetInstance().CameraSetTargetZoom(6f);

            
            TarotCards.TarotCard card1 = this.GetCard();
            TarotCards.TarotCard card2 = this.GetCard(false);
            if (card1 != null && card2 != null)
            {
                UITarotChoiceOverlayController tarotChoiceOverlayInstance = MonoSingleton<UIManager>.Instance.ShowTarotChoice(card1, card2);
                tarotChoiceOverlayInstance.OnTarotCardSelected += (System.Action<TarotCards.TarotCard>)(card =>
                {
                    this.StartCoroutine(this.BackToIdleRoutine(card, 0.0f));
                    DataManager.Instance.PlayerRunTrinkets.Remove(GetOther(card));
                    this.Activated = false;
                });
                
                UITarotChoiceOverlayController overlayController = tarotChoiceOverlayInstance;
                overlayController.OnHidden = overlayController.OnHidden + (System.Action)(() => tarotChoiceOverlayInstance = null);
            }
            else if (card1 != null || card2 != null)
            {
                if (card1 != null)
                    UITrinketCards.Play(card1, (System.Action)(() => this.StartCoroutine(this.BackToIdleRoutine(card1, 0.0f))));
                else if (card2 != null)
                    UITrinketCards.Play(card2, (System.Action)(() => this.StartCoroutine(this.BackToIdleRoutine(card2, 0.0f))));

                this.Activated = false;
            }
            else //no more cards to use
            {
                int i = -1;
                while (++i <= 20)
                {
                    AudioManager.Instance.PlayOneShot("event:/chests/chest_item_spawn", this.gameObject);
                    CameraManager.shakeCamera(UnityEngine.Random.Range(0.4f, 0.6f));
                    PickUp pickUp = InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, this.transform.position + Vector3.back, 0.0f);
                    pickUp.SetInitialSpeedAndDiraction(4f + UnityEngine.Random.Range(-0.5f, 1f), (float)(270 + UnityEngine.Random.Range(-90, 90)));
                    pickUp.MagnetDistance = 3f;
                    pickUp.CanStopFollowingPlayer = false;
                    yield return new WaitForSeconds(0.01f);
                }
                yield return new WaitForSeconds(1f);
                this.StartCoroutine(this.BackToIdleRoutine(null, 0.0f));
                this.Activated = false;
            }

            TarotCards.TarotCard GetOther(TarotCards.TarotCard card) => card == card1 ? card2 : card1;
        }

        private IEnumerator BackToIdleRoutine(TarotCards.TarotCard card, float delay)
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
            GameManager.GetInstance().StartCoroutine(this.DelayEffectsRoutine(card, delay));
            GameManager.GetInstance().CameraResetTargetZoom();
            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
            yield return null;
        }

        private IEnumerator DelayEffectsRoutine(TarotCards.TarotCard card, float delay)
        {
            yield return (object)new WaitForSeconds(0.2f + delay);
            if (card != null)
                TrinketManager.AddTrinket(card);
        }

        private TarotCards.TarotCard GetCard(bool customFirst = true)
        {
            
            TarotCards.TarotCard card = null;
            bool alreadyTaken = false;
            if (customFirst) {
                TarotCards.Card cardData = CustomTarotCardManager.CustomTarotCardList.Keys.ElementAt(UnityEngine.Random.Range(0, CustomTarotCardManager.CustomTarotCardList.Count));

                card = new TarotCards.TarotCard(cardData, 0);
                Plugin.Log.LogInfo("Before contain card " + DataManager.Instance.PlayerRunTrinkets.Contains(card));
            }

            
            foreach(var cardz in DataManager.Instance.PlayerRunTrinkets)
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
                if (!DataManager.Instance.FirstTarot && TarotCards.DrawRandomCard() != null)
                {
                    DataManager.Instance.FirstTarot = true;
                    card = new TarotCards.TarotCard(TarotCards.Card.Lovers1, 0);
                }
                else
                    card = TarotCards.DrawRandomCard();
            }
            
            if (card != null)
                DataManager.Instance.PlayerRunTrinkets.Add(card);
            
            return card;
        }

        private void OnHidden()
        {
            this.Activated = false;
            //Time.timeScale = 1f;
            //HUD_Manager.Instance.Show();
        }
    }
}
