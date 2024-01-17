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
using src.Extensions;
using Unity.Mathematics;
using CotLMiniMods.Helpers;

namespace CotLMiniMods.Rituals
{
    public class ExiledRitual : CustomRitual
    {
        public override string InternalName { get; } = "Exiled_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_exile.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new(Plugin.StrangeMaterialItem, 3) };

        public override string GetLocalizedName => "Ritual of Exile";
        public override string GetLocalizedDescription => "Removes a follower from existence, permanently leaving the cult. Warning: this action is irreversible!";
        public override float FaithChange { get; } = 10;
        public override FollowerTrait.TraitType RitualTrait { get; } = FollowerTrait.TraitType.None;
        
        public override float Cooldown => 60f;
        public Follower followerToLeave;

        public override void Play()
        {
            base.Play();
            Plugin.Log.LogInfo("Starting ritual for " + upgradeType + " and " + RitualType);
            GameManager.GetInstance().StartCoroutine(this.RitualRoutine());
        }
        //todo: the actual ritual
        private IEnumerator RitualRoutine()
        {
            bool Cancelled = false;
            AudioManager.Instance.PlayOneShot("event:/rituals/generic_start_ritual");
            yield return this.StartCoroutine(this.CentreAndAnimatePlayer());
            Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
            yield return this.StartCoroutine(this.WaitFollowersFormCircle());
            
            PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
            PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
            PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0.0f);

            Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
            ChurchFollowerManager.Instance.StartRitualOverlay();
            
            UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate<UIFollowerSelectMenuController>();
            List<FollowerInfo> tempList = new();
            
            List<FollowerSelectEntry> FSE = Helper.MakeSimpleFSEFromList(DataManager.Instance.Followers);
            followerSelectInstance.Show(Ritual.GetFollowerSelectEntriesForSermon(), followerSelectionType: UpgradeSystem.Type.Ritual_Sacrifice);
            
            followerSelectInstance.OnFollowerSelected += followerInfo =>
            {
                followerToLeave = FollowerManager.FindFollowerByID(followerInfo.ID);
                UIManager.PlayAudio("event:/ritual_sacrifice/ritual_begin");
            };

            followerSelectInstance.OnCancel = followerSelectInstance.OnCancel + (System.Action)(() =>
            {
                GameManager.GetInstance().StartCoroutine(this.EndRitual());
                Cancelled = true;
                this.CompleteRitual(true);
                this.CancelFollowers();
            });
            
            followerSelectInstance.OnHidden += (() => followerSelectInstance = null);
            
            followerSelectInstance.OnShow += () => //can change this to traits
            {
                foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
                {
                    followerInfoBox.FollowerRole.text = "Warning: permanently erases follower!!";
                }
            };

            while (followerSelectInstance != null && !Cancelled)
                yield return null;

            if (!Cancelled)
            {
                yield return new WaitForSeconds(1.2f);
                ChurchFollowerManager.Instance.PlayOverlay(ChurchFollowerManager.OverlayType.Ritual, "resurrect");
                yield return new WaitForSeconds(0.5f);
                this.PlaySacrificePortalEffect();
                
                this.followerToLeave.Brain.CompleteCurrentTask();
                FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
                this.followerToLeave.Brain.HardSwapToTask((FollowerTask)nextTask);
                this.followerToLeave.Brain.InRitual = true;

                this.followerToLeave.SetOutfit(this.followerToLeave.Brain.Info.Outfit, true);

                foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
                {
                    if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                        (allBrain.CurrentTask as FollowerTask_AttendRitual).WorshipTentacle();
                }

                yield return (object)new WaitForSeconds(0.5f);
                AudioManager.Instance.PlayOneShot("event:/rituals/resurrect");
                yield return (object)new WaitForSeconds(1.2f);

                this.followerToLeave.HoodOff(onComplete: () =>
                {
                    this.followerToLeave.Spine.AnimationState.SetAnimation(1, "walk", true);
                    this.followerToLeave.gameObject.transform.DOMove(Interaction_TempleAltar.Instance.PortalEffect.transform.position, 2.5f).OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>((TweenCallback)(() =>
                    {
                        Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
                        GameManager.GetInstance().StartCoroutine(this.ExileRoutine());
                    }));
                });
                yield return new WaitForSeconds(10f);
                    

                this.StopSacrificePortalEffect();
                ChurchFollowerManager.Instance.StopSacrificePortalEffect();
                Interaction_TempleAltar.Instance.PulseDisplacementObject(PlayerFarming.Instance.CameraBone.transform.position);
                PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
                PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);

                foreach (FollowerBrain followerBrain in Ritual.FollowerToAttendSermon)
                {
                    if (followerBrain.CurrentTask is FollowerTask_AttendRitual)
                        (followerBrain.CurrentTask as FollowerTask_AttendRitual).Cheer();
                }
                yield return new WaitForSeconds(3f);
            }

            ChurchFollowerManager.Instance.EndRitualOverlay();
            GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
            Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            this.CompleteRitual();
        }


        private IEnumerator EndRitual()
        {
            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);
            yield return new WaitForSeconds(0.6666667f);
        }

        private IEnumerator ExileRoutine()
        {
            ChurchFollowerManager.Instance.RemoveBrainFromAudience(this.followerToLeave.Brain);
            this.followerToLeave.Spine.AnimationState.SetAnimation(1, "Reactions/react-scared", false);

            yield return new WaitForSeconds(1.9f);
            this.followerToLeave.Spine.AnimationState.SetAnimation(1, "Spawn-Out-Portal", false);
            yield return new WaitForSeconds(2.8f);
            this.followerToLeave.Brain.LeftCult = true;
            this.followerToLeave.LeaveCult();
        }
    }
}
