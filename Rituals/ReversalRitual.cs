using COTL_API.Helpers;
using CotLTemplateMod;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using FMOD.Studio;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using Spine;
using src.Extensions;

namespace CotLMiniMods.Rituals
{
    public class ReversalRitual : CustomRitual
    {
        public override string InternalName { get; } = "Reversal_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_fusion.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 300), new(Plugin.StrangeMaterialItem, 3) };

        public override string GetLocalizedName => "Ritual of Reversal";
        public override string GetLocalizedDescription => "Select a follower. Infusing them with Strange Material to transform the first negative trait into its positive counterpart.";
        public override float FaithChange { get; } = 10;
        public override float Cooldown => 60f;
        public override FollowerTrait.TraitType RitualTrait { get; } = FollowerTrait.TraitType.None;

        private Follower selectedFollower;

        //todo: implement cooldowns
        public override void Play()
        {
            base.Play();
            GameManager.GetInstance().StartCoroutine(this.RitualRoutine());
        }
        //todo: the actual ritual
        private IEnumerator RitualRoutine()
        {
            yield return this.StartCoroutine(this.CentreAndAnimatePlayer());
            Interaction_TempleAltar.Instance.SimpleSetCamera.Play();

            yield return this.StartCoroutine(this.WaitFollowersFormCircle());

            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0.0f);
            yield return (object)new WaitForSeconds(1f);

            bool Cancelled = false;
            UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate<UIFollowerSelectMenuController>();
            followerSelectInstance.Show(Ritual.GetFollowersAvailableToAttendSermon());

            followerSelectInstance.OnFollowerSelected += followerInfo =>
            {
                this.selectedFollower = FollowerManager.FindFollowerByID(followerInfo.ID);
            };
            followerSelectInstance.OnShow += () => //can change this to traits
            {
                
                foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
                {
                    string allTraits = "";
                    foreach (FollowerTrait.TraitType trait in followerInfoBox.followBrain.Info.Traits)
                    {
                        allTraits += trait.ToString() + " | ";
                    }
                    followerInfoBox.FollowerRole.text = "Traits: " + allTraits;
                   
                }
            };
            followerSelectInstance.OnCancel = followerSelectInstance.OnCancel + (System.Action)(() =>
            {
                GameManager.GetInstance().StartCoroutine(this.EndRitual());
                Cancelled = true;
                this.CompleteRitual(true);
                this.CancelFollowers();
            });

            followerSelectInstance.OnHidden += (() => followerSelectInstance = null);
            while (followerSelectInstance != null && !Cancelled)
                yield return null;
            
            if (!Cancelled)
            {
                this.selectedFollower.Brain.CompleteCurrentTask();
                FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
                this.selectedFollower.Brain.HardSwapToTask((FollowerTask)nextTask);
                this.selectedFollower.Brain.InRitual = true;
                yield return null;
                
                this.selectedFollower.SetOutfit(this.selectedFollower.Brain.Info.Outfit, true);
                this.selectedFollower.HoodOff(onComplete: ((System.Action)(() =>
                {
                    ChurchFollowerManager.Instance.RemoveBrainFromAudience(this.selectedFollower.Brain);
                    foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
                    {
                        if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                        {
                            allBrain.CurrentTask.RecalculateDestination();
                            allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
                        }
                    }
                    this.selectedFollower.Spine.AnimationState.SetAnimation(1, "walk", true);
                    this.selectedFollower.gameObject.transform.DOMove(Interaction_TempleAltar.Instance.PortalEffect.transform.position, 2.5f).OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>((TweenCallback)(() =>
                    {
                        Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
                        GameManager.GetInstance().StartCoroutine(this.CollectNegativeTraitRoutine());
                    }));
                })));
                GameManager.GetInstance().OnConversationNext(this.selectedFollower.gameObject);
            }

        }

        private IEnumerator CollectNegativeTraitRoutine()
        {
            Plugin.Log.LogInfo("NegativeTrait routine");
            this.selectedFollower.Spine.AnimationState.SetAnimation(1, "devotion/devotion-collect-loopstart-whiteyes", false);
            yield return new WaitForSeconds(1.3f);
            this.selectedFollower.Spine.AnimationState.SetAnimation(1, "devotion/devotion-collect-loop-whiteyes", true);

            foreach (FollowerTrait.TraitType trait in this.selectedFollower.Brain.Info.Traits)
            {
                if (!FollowerTrait.IsPositiveTrait(trait))
                {
                    yield return new WaitForSeconds(1f);
                    ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, this.selectedFollower.transform.position, FollowerTrait.GetIcon(trait), null);
                    AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.gameObject);
                    GameManager.GetInstance().StartCoroutine(this.DoConversionRoutine(trait));
                    yield return null;
                    break;
                }
                
            }
            
            yield return null;
        }
        
        private IEnumerator DoConversionRoutine(FollowerTrait.TraitType trait)
        {
            GameManager.GetInstance().OnConversationNext(this.selectedFollower.gameObject, 4f);
            
            ChurchFollowerManager.Instance.PlayOverlay(ChurchFollowerManager.OverlayType.Sacrifice, "1");

            Ritual.FollowerToAttendSermon.Remove(this.selectedFollower.Brain);


            int followerID = this.selectedFollower.Brain.Info.ID;
            yield return (object)new WaitForSeconds(0.5f);

            var positiveTrait = FollowerTrait.TraitType.None;

            //convert to positive trait
            foreach (KeyValuePair<FollowerTrait.TraitType, FollowerTrait.TraitType> exclusiveTrait in FollowerTrait.ExclusiveTraits)
            {
                if (exclusiveTrait.Key == trait)
                {
                    positiveTrait = exclusiveTrait.Value;
                    break;
                }
                if (exclusiveTrait.Value == trait)
                {
                    positiveTrait = exclusiveTrait.Key;
                    break;
                }
            }
            if (positiveTrait != FollowerTrait.TraitType.None)
            {
                this.selectedFollower.AddTrait(positiveTrait);
                Plugin.Log.LogInfo("Added a trait " + positiveTrait);
            }

            BiomeConstants.Instance.ChromaticAbberationTween(1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 7f);
            
            foreach (FollowerBrain allBrain in FollowerBrain.AllBrains) //need to stop cheering while selecting 2nd follower
            {
                if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                    (allBrain.CurrentTask as FollowerTask_AttendRitual).Cheer();
            }
            
            ChurchFollowerManager.Instance.StartRitualOverlay();
            AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/ritual_end", PlayerFarming.Instance.gameObject);
            yield return new WaitForSeconds(0.7f);
            
            Interaction_TempleAltar.Instance.PulseDisplacementObject(this.selectedFollower.CameraBone.transform.position);
            yield return new WaitForSeconds(3.16666651f);
            Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
            yield return new WaitForSeconds(0.3f);
            GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 8f);
            yield return new WaitForSeconds(0.5f);
            
            yield return new WaitForSeconds(1f);
            GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 6f);
            
            JudgementMeter.ShowModify(DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SacrificeEnthusiast) ? 1 : -1);
            ChurchFollowerManager.Instance.EndRitualOverlay();
            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            yield return new WaitForSeconds(0.5f);
            this.CompleteRitual();
        }

        private IEnumerator EndRitual()
        {
            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);
            yield return new WaitForSeconds(0.6666667f);
        }

    }
}
