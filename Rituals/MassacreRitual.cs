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
    public class MassacreRitual : CustomRitual
    {
        public override string InternalName { get; } = "Massacre_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_massres.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new(Plugin.StrangeMaterialItem, 12) };

        public override string GetLocalizedName => "Ritual of Massacre";
        public override string GetLocalizedDescription => "Kills every follower except 1 random follower.";
        public override float FaithChange { get; } = 10;
        public override FollowerTrait.TraitType RitualTrait { get; } = FollowerTrait.TraitType.None;
        
        public override float Cooldown => 60f;

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
            FollowerInfo selectedSurvivor = null;
            if (DataManager.Instance.Followers_Dead.Count > 0)
            {
                tempList.Add(DataManager.Instance.Followers_Dead[0]);
            }
            List<FollowerSelectEntry> FSE = Ritual.GetFollowerSelectEntriesForSermon();
            followerSelectInstance.Show(FSE, followerSelectionType: UpgradeSystem.Type.Ritual_Ascend);
            
            followerSelectInstance.OnFollowerSelected += followerInfo =>
            {
                selectedSurvivor = followerInfo;
                //we dont care who was selected, if one follower was selection, means it is confirmed
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
                /*if (followerSelectInstance.FollowerInfoBoxes.Count > 1)
                {
                    followerSelectInstance.FollowerInfoBoxes.RemoveRange(1, followerSelectInstance.FollowerInfoBoxes.Count - 1);
                }*/
                foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
                {
                    var size = DataManager.Instance.Followers_Dead.Count;
                    followerInfoBox.FollowerRole.text = "Select one follower to be left alive.";
                    /*followerInfoBox.FollowerName.text = "Confirm to kill all Followers?";*/
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

                foreach (FollowerBrain allBrain in FollowerBrain.AllBrains)
                {
                    if (allBrain.CurrentTaskType == FollowerTaskType.AttendTeaching)
                        (allBrain.CurrentTask as FollowerTask_AttendRitual).WorshipTentacle();
                }

                yield return (object)new WaitForSeconds(0.5f);
                AudioManager.Instance.PlayOneShot("event:/rituals/resurrect");
                yield return (object)new WaitForSeconds(1.2f);

                var groupResurrect = 0;

                ChurchFollowerManager.Instance.GodRays.gameObject.SetActive(true);
                ChurchFollowerManager.Instance.GodRays.GetComponent<ParticleSystem>().Play();
                ChurchFollowerManager.Instance.Goop.gameObject.SetActive(true);
                ChurchFollowerManager.Instance.Goop.Play("Show");
                ChurchFollowerManager.Instance.Goop.GetComponentInChildren<MeshRenderer>().material.SetColor("_TintCOlor", Color.red);
                yield return (object)new WaitForSeconds(0.5f);

                bool survivor = false;
                var tempList2 = new List<Follower>();

                foreach(FollowerSelectEntry fs in FSE)
                {
                    if (survivor == false && selectedSurvivor == null)
                    {
                        survivor = true;
                        continue;
                    }
                    if (survivor == false && fs.FollowerInfo == selectedSurvivor)
                    {
                        survivor = true;
                        continue;
                    }

                    var killing = FollowerManager.FindFollowerByID(fs.FollowerInfo.ID);
                    ChurchFollowerManager.Instance.RemoveBrainFromAudience(killing.Brain);
                    killing.TimedAnimation("ascend", 1.3f, () =>
                    {
                         killing.Die(NotificationCentre.NotificationType.Ascended, false, force: true);
                    });

                    Ritual.FollowerToAttendSermon.Remove(killing.Brain);
                    
                   /* tempList2.Add(killing);*/

                    groupResurrect++;
                    yield return new WaitForSeconds(0.1f);
                    if (groupResurrect >= 7)
                    {
                        yield return new WaitForSeconds(0.33f);
                        groupResurrect = 0;
                    }
                }

                this.StopSacrificePortalEffect();
                ChurchFollowerManager.Instance.StopSacrificePortalEffect();
                ChurchFollowerManager.Instance.Goop.gameObject.SetActive(false);
                ChurchFollowerManager.Instance.GodRays.SetActive(false);
                ChurchFollowerManager.Instance.Sparkles.Stop();

               
                PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
                PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);

                foreach (FollowerBrain followerBrain in Ritual.FollowerToAttendSermon)
                {
                    if (followerBrain.CurrentTask is FollowerTask_AttendRitual)
                        (followerBrain.CurrentTask as FollowerTask_AttendRitual).Cheer();
                }
                yield return new WaitForSeconds(7f);

                /*foreach (Follower died in tempList2)
                {
                    Destroy(died.gameObject);
                }*/
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
    }
}
