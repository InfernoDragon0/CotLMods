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
    public class SuperchargeRitual : CustomRitual
    {
        public override string InternalName { get; } = "Supercharge_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_reversal.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new(Plugin.StrangeMaterialItem, 2) };

        public override string GetLocalizedName => "Ritual of Supercharge";
        public override string GetLocalizedDescription => "Select a follower. Infusing them with Strange Material to increase their speed by 0.1.";
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
                    var brain = followerInfoBox.followBrain;
                    followerInfoBox.FollowerRole.text = $"Movement: {this.selectedFollower._speed}/{brain.CurrentState.MaxSpeed}";
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
            Plugin.Log.LogInfo("increase speed routine");
            this.selectedFollower.Spine.AnimationState.SetAnimation(1, "devotion/devotion-collect-loopstart-whiteyes", false);
            yield return new WaitForSeconds(1.3f);
            this.selectedFollower.Spine.AnimationState.SetAnimation(1, "devotion/devotion-collect-loop-whiteyes", true);

            this.selectedFollower._speed += 0.1f;

            yield return new WaitForSeconds(2f);
            ChurchFollowerManager.Instance.EndRitualOverlay();
            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            this.EndRitual();
            this.CompleteRitual();
            yield return null;
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
