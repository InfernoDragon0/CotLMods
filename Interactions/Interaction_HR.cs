using CotLTemplateMod;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using src.Extensions;
using Lamb.UI.FollowerInteractionWheel;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_HR : Interaction
    {
        private bool Activated;
        public override void GetLabel()
        {
            this.label = "Open Follower Management";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled HR");
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;
            this.Activated = true;

            GameManager.GetInstance().OnConversationNew();

            //Time.timeScale = 0.0f; //set timescale if want to pause

            HUD_Manager.Instance.Hide(false, 0);
            UIFollowerSelectMenuController followerSelectMenu = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate<UIFollowerSelectMenuController>();

            List<FollowerSelectEntry> followerSelectEntries = new();
            foreach (Follower follower in Follower.Followers)
            {
                followerSelectEntries.Add(new FollowerSelectEntry(follower.Brain._directInfoAccess, FollowerManager.GetFollowerAvailabilityStatus(follower.Brain, true)));
            }

            followerSelectMenu.Show(followerSelectEntries, false, UpgradeSystem.Type.Count, true, true, true);

            

            UIFollowerSelectMenuController selectMenuController1 = followerSelectMenu;
            
            selectMenuController1.OnFollowerSelected = selectMenuController1.OnFollowerSelected + new System.Action<FollowerInfo>(this.OnFollowerChosen);
            selectMenuController1.OnShown += new System.Action(() =>
            {
                foreach (FollowerInformationBox fib in followerSelectMenu._followerInfoBoxes)
                {
                    fib.FollowerRole.text = fib.AgeString + " | Working As: " + fib._followerInfo.FollowerRole.ToString();
                }
            });
            UIFollowerSelectMenuController selectMenuController2 = followerSelectMenu;
            selectMenuController2.OnHidden = selectMenuController2.OnHidden + (System.Action)(() =>
            {
                followerSelectMenu = null;
                this.OnHidden();
                //this.HasChanged = true;
            });

            selectMenuController2.OnCancel = selectMenuController2.OnCancel + (System.Action)(() =>
            {
                followerSelectMenu = null;
                this.OnHidden3();
                //this.HasChanged = true;
            });
        }

        private void OnHidden()
        {
            this.Activated = false;
            //Time.timeScale = 1f;
            //HUD_Manager.Instance.Show();
        }

        private void OnHidden2(FollowerCommands[] f)
        {
            this.Activated = false;
            //Time.timeScale = 1f;
            //HUD_Manager.Instance.Show();

        }

        private void OnHidden3()
        {
            this.Activated = false;
            //Time.timeScale = 1f;
            HUD_Manager.Instance.Show();
            GameManager.GetInstance().OnConversationEnd();
        }

        private void OnFollowerChosen(FollowerInfo followerInfo)
        {
            if (FollowerInfo.GetInfoByID(followerInfo.ID) != null)
            {
                //GameManager.GetInstance().OnConversationEnd();

                Follower followerById = FollowerManager.FindFollowerByID(followerInfo.ID);
                Plugin.Log.LogInfo("follower selected: " + followerById.Brain.Info.Name);

                List<CommandItem> commandItems = FollowerCommandGroups.GiveWorkerCommands(followerById);

                UnityEngine.Object.FindObjectOfType<CameraFollowTarget>().SetOffset(new Vector3(0.0f, 0.0f, -1f));
                UIFollowerInteractionWheelOverlayController overlayController = MonoSingleton<UIManager>.Instance.FollowerInteractionWheelTemplate.Instantiate<UIFollowerInteractionWheelOverlayController>();
                overlayController.Show(followerById, commandItems);
                overlayController.OnItemChosen = overlayController.OnItemChosen + new System.Action<FollowerCommands[]>(followerById.Interaction_FollowerInteraction.OnFollowerCommandFinalized);
                overlayController.OnItemChosen = overlayController.OnItemChosen + new System.Action<FollowerCommands[]>(this.OnHidden2);

                overlayController.OnCancel = overlayController.OnCancel + new System.Action(this.OnHidden3);
                GameManager.GetInstance().OnConversationNew();
                GameManager.GetInstance().OnConversationNext(followerById.gameObject, 4f);
                //HUD_Manager.Instance.Hide(false, 0);
            }
            
        }
    }
}
