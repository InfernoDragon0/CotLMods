using CotLTemplateMod;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using src.Extensions;
using Lamb.UI.FollowerInteractionWheel;
using DG.Tweening;
using CotLMiniMods.Structures.Mines;
using CotLMiniMods.Structures.Proxies;
using CotLMiniMods.Items;
using COTL_API.Helpers;
using System.IO;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_SousDesk : Interaction
    {
        public Structure Structure;
        private bool Activating = false;
        private GameObject Player;
        private float Delay = 0.04f;
        public float DistanceToTriggerDeposits = 5f;

        public StructuresData StructureInfo => this.Structure.Structure_Info;
        public ChefDeskStructure ChefDesk => this.Structure.Brain as ChefDeskStructure;
        public override void GetLabel()
        {
            int count = this.StructureInfo.Inventory.Count;
            //this.Interactable = count > 0;
            this.SecondaryLabel = "Select Signature Dish (" + this.ChefDesk.SelectedCookItem + ")";
            this.label = "Upgrades";
        }

        private void Start()
        {
            this.ContinuouslyHold = true;
            this.HasSecondaryInteraction = true;
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled AIO Quarry Interaction");
            Structure = GetComponentInParent<Transform>().GetComponent<Structure>();
        }

        public override void OnInteract(StateMachine state)
        {
            UIUpgradeTreeMenuController upgradeTreeInstance = UIManager.Instance.UpgradeTreeMenuTemplate.Instantiate<UIUpgradeTreeMenuController>();
            upgradeTreeInstance._divineInspirationBackground.sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/chefdesk.png"));
            Plugin.Log.LogInfo("root node was");
            Plugin.Log.LogInfo(upgradeTreeInstance._rootNode._title.text);

            

            upgradeTreeInstance.OnShow += new Action(() =>
            {
                //test creating a node
                UpgradeTreeNode customNode = upgradeTreeInstance._rootNode;
                customNode._title.text = "hello there";
                customNode._state = UpgradeTreeNode.NodeState.Locked;
                
                foreach (UpgradeTreeNode upgradeTreeNode in upgradeTreeInstance._treeNodes)
                {
                    Plugin.Log.LogInfo("tree nodes");
                    Plugin.Log.LogInfo(upgradeTreeNode._title.text);
                    upgradeTreeNode._title.text = "bye bye";
                }
                upgradeTreeInstance._rootNode.Configure(upgradeTreeInstance.TreeTier(), true);
            });

            
            
            upgradeTreeInstance.Show();
            /*if (this.Activating) return;
            base.OnInteract(state);
            this.Activating = true;*/
        }

        public override void OnSecondaryInteract(StateMachine state)
        {
            base.OnSecondaryInteract(state);
            List<InventoryItem.ITEM_TYPE> QuarryItems = this.ChefDesk.CookItems;
            state.CURRENT_STATE = StateMachine.State.InActive;
            state.facingAngle = Utils.GetAngle(state.transform.position, this.transform.position);
            CameraFollowTarget cameraFollowTarget = CameraFollowTarget.Instance;
            cameraFollowTarget.SetOffset(new Vector3(0.0f, 4.5f, 2f));
            cameraFollowTarget.AddTarget(this.gameObject, 1f);
            HUD_Manager.Instance.Hide(false, 0);
            
            UIItemSelectorOverlayController itemSelector = MonoSingleton<UIManager>.Instance.ShowItemSelector(QuarryItems, new ItemSelector.Params()
            {
                Key = "ChefDesk",
                Context = ItemSelector.Context.SetLabel,
                Offset = new Vector2(0.0f, 150f),
                ShowEmpty = true,
                RequiresDiscovery = false,
                HideQuantity = true,
                HideOnSelection = true,
            });
            itemSelector.OnItemChosen += chosenItem =>
            {
                Plugin.Log.LogInfo("item selected " + chosenItem);
                this.ChefDesk.SelectedCookItem = chosenItem;
                HUD_Manager.Instance.Show(0);
            };
            itemSelector.OnCancel += () => HUD_Manager.Instance.Show(0);
            itemSelector.OnHidden += () =>
            {
                cameraFollowTarget.SetOffset(Vector3.zero);
                cameraFollowTarget.RemoveTarget(this.gameObject);
                state.CURRENT_STATE = StateMachine.State.Idle;
                itemSelector = null;
                this.Interactable = true;
                this.HasChanged = true;
            };

        }

        public override void Update()
        {
            if ((this.Player = GameObject.FindWithTag("Player")) == null)
                return;
            
            this.GetLabel();

            if (this.Activating && (this.StructureInfo.Inventory.Count <= 0 || InputManager.Gameplay.GetInteractButtonUp()))
            {
                this.Activating = false;
            }

            if ((double)(this.Delay -= Time.deltaTime) >= 0.0 || !this.Activating)
                return;

            InventoryItem.ITEM_TYPE itemType = Plugin.StrangeMaterialItem;
            AudioManager.Instance.PlayOneShot("event:/followers/pop_in", this.gameObject);
            /*ResourceCustomTarget.Create(this.state.gameObject, this.transform.position, itemType, () => this.AddItem(itemType));*/
            if (Inventory.GetItemQuantity(itemType) > 0)
            {
                Inventory.ChangeItemQuantity(itemType, -1);
                ResourceCustomTarget.Create(this.gameObject, PlayerFarming.Instance.transform.position, itemType, null);
                this.StructureInfo.Inventory.Add(new InventoryItem(itemType));
                this.Delay = 0.04f;
            }
        }

    }
}
