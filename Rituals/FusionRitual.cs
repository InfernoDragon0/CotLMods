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
    public class FusionRitual : CustomRitual
    {
        public override string InternalName { get; } = "Fusion_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_fusion.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1000) };

        public override string GetLocalizedName => "Fusion Ritual";
        public override string GetLocalizedDescription => "Sacrifice a selected Follower. The next selected Follower gets the sacrificed follower's traits.";
        public override float FaithChange { get; } = 20;
        public override float Cooldown => 60f;
        public override FollowerTrait.TraitType RitualTrait { get; } = FollowerTrait.TraitType.None;

        private Follower sacrificeFollower;
        private Follower absorbtionFollower;

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
            followerSelectInstance.Show(Ritual.GetFollowerSelectEntriesForSermon(), followerSelectionType: UpgradeSystem.Type.Ritual_Sacrifice);

            followerSelectInstance.OnFollowerSelected += followerInfo =>
            {
                this.sacrificeFollower = FollowerManager.FindFollowerByID(followerInfo.ID);
                UIManager.PlayAudio("event:/ritual_sacrifice/ritual_begin");
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
            Plugin.Log.LogInfo("on hidden");

            followerSelectInstance.OnHidden += (() => followerSelectInstance = null);
            while (followerSelectInstance != null && !Cancelled)
                yield return null;
            
            if (!Cancelled)
            {
                AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
                this.sacrificeFollower.Brain.CompleteCurrentTask();
                FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
                this.sacrificeFollower.Brain.HardSwapToTask((FollowerTask)nextTask);
                this.sacrificeFollower.Brain.InRitual = true;
                yield return null;
                
                this.sacrificeFollower.SetOutfit(this.sacrificeFollower.Brain.Info.Outfit, true);
                this.sacrificeFollower.HoodOff(onComplete: ((System.Action)(() =>
                {
                    ChurchFollowerManager.Instance.RemoveBrainFromAudience(this.sacrificeFollower.Brain);
                    foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
                    {
                        if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                        {
                            allBrain.CurrentTask.RecalculateDestination();
                            allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
                        }
                    }
                    this.sacrificeFollower.Spine.AnimationState.SetAnimation(1, "walk", true);
                    this.sacrificeFollower.gameObject.transform.DOMove(Interaction_TempleAltar.Instance.PortalEffect.transform.position, 2.5f).OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>((TweenCallback)(() =>
                    {
                        

                        Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
                        GameManager.GetInstance().StartCoroutine(this.PreSacrificeRoutine());
                    }));
                })));
                GameManager.GetInstance().OnConversationNext(this.sacrificeFollower.gameObject);
            }

        }

        private IEnumerator PreSacrificeRoutine()
        {
            Plugin.Log.LogInfo("Pre sacrifice routine");
            this.sacrificeFollower.Spine.AnimationState.SetAnimation(1, "devotion/devotion-collect-loopstart-whiteyes", false);
            yield return new WaitForSeconds(1.3f);
            this.sacrificeFollower.Spine.AnimationState.SetAnimation(1, "devotion/devotion-collect-loop-whiteyes", true);

            foreach (FollowerTrait.TraitType trait in this.sacrificeFollower.Brain.Info.Traits)
            {
                yield return new WaitForSeconds(1f);
                ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, this.sacrificeFollower.transform.position, FollowerTrait.GetIcon(trait), null);
                AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.gameObject);
            }
            GameManager.GetInstance().StartCoroutine(this.DoSacrificeRoutine());
            yield return null;
        }

        private IEnumerator DoSacrificeRoutine()
        {
            GameManager.GetInstance().OnConversationNext(this.sacrificeFollower.gameObject, 4f);
            
            ChurchFollowerManager.Instance.PlayOverlay(ChurchFollowerManager.OverlayType.Sacrifice, "1");
            this.sacrificeFollower.Spine.AnimationState.Event += new Spine.AnimationState.TrackEntryEventDelegate(this.HandleSacrificeAnimationStateEvent);
            this.sacrificeFollower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;

            Ritual.FollowerToAttendSermon.Remove(this.sacrificeFollower.Brain);
            
            double num1 = (double)this.sacrificeFollower.SetBodyAnimation("sacrifice-tentacles", false);
            
            int followerID = this.sacrificeFollower.Brain.Info.ID;
            yield return (object)new WaitForSeconds(0.5f);
            this.PlaySacrificePortalEffect();
            Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
            BiomeConstants.Instance.ChromaticAbberationTween(1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 7f);
            foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
            {
                if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                    (allBrain.CurrentTask as FollowerTask_AttendRitual).WorshipTentacle();
            }
            yield return (object)new WaitForSeconds(1.5f);
            
            foreach (FollowerBrain allBrain in FollowerBrain.AllBrains) //need to stop cheering while selecting 2nd follower
            {
                if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                    (allBrain.CurrentTask as FollowerTask_AttendRitual).Cheer();
            }
            
            ChurchFollowerManager.Instance.StartRitualOverlay();
            AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/ritual_end", PlayerFarming.Instance.gameObject);
            yield return new WaitForSeconds(0.7f);
            
            Interaction_TempleAltar.Instance.PulseDisplacementObject(this.sacrificeFollower.CameraBone.transform.position);
            yield return new WaitForSeconds(3.16666651f);
            Interaction_TempleAltar.Instance.PulseDisplacementObject(Interaction_TempleAltar.Instance.PortalEffect.transform.position);
            yield return new WaitForSeconds(0.3f);
            GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 8f);
            yield return new WaitForSeconds(0.5f);
            
            yield return new WaitForSeconds(1f);
            GameManager.GetInstance().OnConversationNext(Interaction_TempleAltar.Instance.RitualCameraPosition, 6f);
            this.StopSacrificePortalEffect();
            this.sacrificeFollower.Spine.AnimationState.Event -= new Spine.AnimationState.TrackEntryEventDelegate(this.HandleSacrificeAnimationStateEvent);
            
            foreach (FollowerBrain followerBrain in Ritual.FollowerToAttendSermon)
            {
                if (this.sacrificeFollower.Brain.Info.CursedState == Thought.OldAge)
                    followerBrain.AddThought(Thought.SacrificedOldFollower);
            }
            
            FollowerManager.FollowerDie(this.sacrificeFollower.Brain.Info.ID, NotificationCentre.NotificationType.SacrificeFollower);
            UnityEngine.Object.Destroy(this.sacrificeFollower.gameObject);
            
            JudgementMeter.ShowModify(DataManager.Instance.CultTraits.Contains(FollowerTrait.TraitType.SacrificeEnthusiast) ? 1 : -1);
            ChurchFollowerManager.Instance.EndRitualOverlay();
            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            
            ++DataManager.Instance.STATS_Sacrifices;
            this.StartCoroutine(this.WaitFollowersFormCircle());
            
            yield return this.GetAbsorptionFollower();
        }

        private IEnumerator EndRitual()
        {
            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);
            yield return new WaitForSeconds(0.6666667f);
        }

        private IEnumerator GetAbsorptionFollower()
        {
            UIFollowerSelectMenuController followerSelectInstance = MonoSingleton<UIManager>.Instance.FollowerSelectMenuTemplate.Instantiate<UIFollowerSelectMenuController>();
            followerSelectInstance.Show(Ritual.GetFollowerSelectEntriesForSermon(), cancellable: false);
            
            followerSelectInstance.OnFollowerSelected += followerInfo =>
            {
                this.absorbtionFollower = FollowerManager.FindFollowerByID(followerInfo.ID);
                UIManager.PlayAudio("event:/ritual_sacrifice/ritual_begin");
            };
            followerSelectInstance.OnHidden += (() => followerSelectInstance = null);
            
            while (followerSelectInstance != null)
                yield return null;

            this.absorbtionFollower.Brain.CompleteCurrentTask();
            FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
            this.absorbtionFollower.Brain.HardSwapToTask((FollowerTask)nextTask);
            
            

            ChurchFollowerManager.Instance.RemoveBrainFromAudience(this.absorbtionFollower.Brain);
            foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
            {
                if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                {
                    allBrain.CurrentTask.RecalculateDestination();
                    allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
                }
            }
            this.absorbtionFollower.Spine.AnimationState.SetAnimation(1, "walk", true);
            this.absorbtionFollower.gameObject.transform.DOMove(Interaction_TempleAltar.Instance.PortalEffect.transform.position, 2.5f).OnComplete<TweenerCore<Vector3, Vector3, VectorOptions>>((TweenCallback)(() =>
            {
                Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
                GameManager.GetInstance().StartCoroutine(this.FusionAddTraits());
            }));
            

            yield return null;
        }

        private IEnumerator FusionAddTraits()
        {
            Plugin.Log.LogInfo("Start fusion");
            this.absorbtionFollower.Brain.CompleteCurrentTask();
            FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
            this.absorbtionFollower.Brain.HardSwapToTask((FollowerTask)nextTask);
            
            this.absorbtionFollower.TimedAnimation("devotion/devotion-collect-loopstart", 1.3f, () =>
            {
                this.absorbtionFollower.SimpleAnimator.Animate("devotion/devotion-collect-loop", 1, true);
            });
            yield return new WaitForSeconds(1f);
            
            foreach (FollowerTrait.TraitType trait in this.sacrificeFollower.Brain.Info.Traits)
            {
                Plugin.Log.LogInfo("Looping fusion");
                if (!this.absorbtionFollower.Brain.Info.Traits.Contains(trait))
                {
                    ResourceCustomTarget.Create(this.absorbtionFollower.gameObject, PlayerFarming.Instance.transform.position, FollowerTrait.GetIcon(trait), null);
                    AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.gameObject);
                    this.absorbtionFollower.AddTrait(trait);
                    yield return new WaitForSeconds(0.5f);
                }
            }

            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);
            float num3 = 0.5f;
            yield return new WaitForSeconds(0.5f);
            
            foreach (FollowerBrain brain in Ritual.FollowerToAttendSermon)
            {
                float Delay = UnityEngine.Random.Range(0.1f, 0.5f);
                num3 += Delay;
                this.StartCoroutine(this.DelayFollowerReaction(brain, Delay));
            }
            yield return new WaitForSeconds(0.5f);
            this.CompleteRitual();

            yield return null;
        }

        private void HandleSacrificeAnimationStateEvent(TrackEntry trackEntry, Spine.Event e)
        {
            string name = e.Data.Name;
            if (!(name == "Shake-small"))
            {
                if (!(name == "Shake-big"))
                {
                    if (!(name == "CamOffset-Add"))
                    {
                        if (!(name == "CamOffset-Remove"))
                            return;
                        GameManager.GetInstance().CamFollowTarget.SetOffset(Vector3.zero);
                        BiomeConstants.Instance.DepthOfFieldTween(0.15f, 8.7f, 26f, 1f, 200f);
                        BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
                    }
                    else
                    {
                        GameManager.GetInstance().CamFollowTarget.SetOffset(new Vector3(0.0f, 0.0f, 1f));
                        BiomeConstants.Instance.DepthOfFieldTween(0.5f, 7f, 8f, 1f, 150f);
                    }
                }
                else
                {
                    CameraManager.instance.ShakeCameraForDuration(0.6f, 0.7f, 0.6f);
                    GameManager.GetInstance().OnConversationNext(this.sacrificeFollower.CameraBone, 8f);
                    Interaction_TempleAltar.Instance.PulseDisplacementObject(this.sacrificeFollower.CameraBone.transform.position);
                    BiomeConstants.Instance.ImpactFrameForDuration();
                }
            }
            else
            {
                CameraManager.instance.ShakeCameraForDuration(0.4f, 0.5f, 0.3f);
                GameManager.GetInstance().OnConversationNext(this.sacrificeFollower.CameraBone, 6f);
                Interaction_TempleAltar.Instance.PulseDisplacementObject(this.sacrificeFollower.CameraBone.transform.position);
            }
        }
    }
}
