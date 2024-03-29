﻿using COTL_API.Helpers;
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
using static UnityEngine.ParticleSystem.PlaybackState;

namespace CotLMiniMods.Rituals
{
    public class MistletoeRitual : CustomRitual
    {
        public override string InternalName { get; } = "Mistletoe_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_fusion.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new(Plugin.StrangeMaterialItem, 6) };

        public override string GetLocalizedName => "Ritual of Love";
        public override string GetLocalizedDescription => "Select two followers. Using the power of Strange Materials, the relationship status of both selected followers will be maxed (Lovers).";
        public override float FaithChange { get; } = 20;
        public override float Cooldown => 60f;
        public override FollowerTrait.TraitType RitualTrait { get; } = FollowerTrait.TraitType.None;

        private Follower leaderFollower;
        private Follower secondaryFollower;

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
                this.leaderFollower = FollowerManager.FindFollowerByID(followerInfo.ID);
            };
            followerSelectInstance.OnShow += () => //can change this to traits
            {
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
                AudioManager.Instance.PlayOneShot("event:/ritual_sacrifice/select_follower", PlayerFarming.Instance.gameObject);
                this.leaderFollower.Brain.CompleteCurrentTask();
                FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
                this.leaderFollower.Brain.HardSwapToTask((FollowerTask)nextTask);
                this.leaderFollower.Brain.InRitual = true;
                yield return null;
                
                this.leaderFollower.SetOutfit(this.leaderFollower.Brain.Info.Outfit, true);
                this.leaderFollower.HoodOff(onComplete: ((System.Action)(() =>
                {
                    foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
                    {
                        if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                        {
                            allBrain.CurrentTask.RecalculateDestination();
                            allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
                        }
                    }
                    this.leaderFollower.Spine.AnimationState.SetAnimation(1, "walk", true);
                    nextTask.GoToAndStop(this.leaderFollower, Interaction_TempleAltar.Instance.PortalEffect.transform.position + Vector3.left * 0.75f, () =>
                    {
                        this.leaderFollower.Spine.AnimationState.SetAnimation(1, "idle", true);
                    });
                    
                    Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
                    GameManager.GetInstance().StartCoroutine(this.GetAbsorptionFollower());
                })));
                GameManager.GetInstance().OnConversationNext(this.leaderFollower.gameObject);
            }

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

            followerSelectInstance.OnShow += () =>
            {
                foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
                {
                    if (followerInfoBox.FollowerInfo.ID == this.leaderFollower.Brain.Info.ID)
                    {
                        followerInfoBox.gameObject.SetActive(false);
                        continue;
                    }

                    var brain = followerInfoBox.followBrain;
                    IDAndRelationship relationship = brain.Info.GetOrCreateRelationship(this.leaderFollower.Brain.Info.ID);
                    followerInfoBox.FollowerRole.text = $"Relation: {relationship.CurrentRelationshipState}";
                }
            };
            
            followerSelectInstance.OnFollowerSelected += followerInfo =>
            {
                this.secondaryFollower = FollowerManager.FindFollowerByID(followerInfo.ID);
            };
            followerSelectInstance.OnHidden += (() => followerSelectInstance = null);
            
            while (followerSelectInstance != null)
                yield return null;

            this.secondaryFollower.Brain.CompleteCurrentTask();
            FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
            this.secondaryFollower.Brain.HardSwapToTask((FollowerTask)nextTask);
            
            foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
            {
                if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                {
                    allBrain.CurrentTask.RecalculateDestination();
                    allBrain.CurrentTask.Setup(FollowerManager.FindFollowerByID(allBrain.Info.ID));
                }
            }
            this.secondaryFollower.Spine.AnimationState.SetAnimation(1, "walk", true);
            nextTask.GoToAndStop(this.secondaryFollower, Interaction_TempleAltar.Instance.PortalEffect.transform.position + Vector3.right * 0.75f, () =>
            {
                Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
                GameManager.GetInstance().StartCoroutine(this.DoMistletoeRoutine());
            });
            

            yield return null;
        }

        private IEnumerator DoMistletoeRoutine()
        {
            Plugin.Log.LogInfo("Start mistletoe");
            this.leaderFollower.State.facingAngle = Utils.GetAngle(this.leaderFollower.transform.position, this.secondaryFollower.transform.position);
            this.secondaryFollower.State.facingAngle = Utils.GetAngle(this.secondaryFollower.transform.position, this.leaderFollower.transform.position);
            this.leaderFollower.TimedAnimation("Conversations/greet-nice", 1.9f, () =>
            {
            });

            yield return new WaitForSeconds(0.2f);

            this.secondaryFollower.TimedAnimation("Conversations/greet-nice", 1.9f, () =>
            {
            });
            yield return new WaitForSeconds(2.1f);
            

            this.leaderFollower.TimedAnimation("kiss", 4f, () =>
            {
                this.leaderFollower.TimedAnimation("cheer", 2f, () =>
                {
                    this.leaderFollower.AddThought(Thought.NewLover);
                });
            });
            yield return new WaitForSeconds(0.2f);
            
            this.secondaryFollower.TimedAnimation("kiss", 4f, () =>
            {
                this.secondaryFollower.TimedAnimation("cheer", 2f, () =>
                {
                    this.secondaryFollower.AddThought(Thought.NewLover);
                });
            });

            foreach (FollowerBrain followerBrain in Ritual.FollowerToAttendSermon)
            {
                if (followerBrain.CurrentTask is FollowerTask_AttendRitual)
                    (followerBrain.CurrentTask as FollowerTask_AttendRitual).Cheer();
            }

            yield return new WaitForSeconds(6.2f);
            
            IDAndRelationship relationship = this.secondaryFollower.Brain.Info.GetOrCreateRelationship(this.leaderFollower.Brain.Info.ID);
            relationship.CurrentRelationshipState = IDAndRelationship.RelationshipState.Lovers;

            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);
            yield return new WaitForSeconds(0.5f);
            this.CompleteRitual();

            yield return null;
        }

    }
}
