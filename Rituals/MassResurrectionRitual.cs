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

namespace CotLMiniMods.Rituals
{
    public class MassResurrectionRitual : CustomRitual
    {
        public override string InternalName { get; } = "MassResurrection_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_leveling.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 700) };

        public override string GetLocalizedName => "Mass Resurrection Ritual";
        public override string GetLocalizedDescription => "Resurrects all dead followers. Beware, your cult may be flooded with followers!";
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
            if (DataManager.Instance.Followers_Dead.Count > 0)
            {
                tempList.Add(DataManager.Instance.Followers_Dead[0]);
            }
            followerSelectInstance.Show(tempList, followerSelectionType: UpgradeSystem.Type.Ritual_Ressurect);
            
            followerSelectInstance.OnFollowerSelected += followerInfo =>
            {
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
                if (followerSelectInstance.FollowerInfoBoxes.Count > 1)
                {
                    followerSelectInstance.FollowerInfoBoxes.RemoveRange(1, followerSelectInstance.FollowerInfoBoxes.Count - 1);
                }
                foreach (FollowerInformationBox followerInfoBox in followerSelectInstance.FollowerInfoBoxes)
                {
                    var size = DataManager.Instance.Followers_Dead.Count;
                    followerInfoBox.FollowerRole.text = "Warning: May lag if many followers!";
                    followerInfoBox.FollowerName.text = "Confirm to resurrect " + size + " Followers?";
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

                foreach (FollowerInfo fi in DataManager.Instance.Followers_Dead)
                {
                    var resurrecting = FollowerBrain.GetOrCreateBrain(fi);

                    //reset the graves
                    foreach (Structures_Grave structuresGrave in StructureManager.GetAllStructuresOfType<Structures_Grave>(FollowerLocation.Base))
                    {
                        if (structuresGrave.Data.FollowerID == resurrecting.Info.ID)
                            structuresGrave.Data.FollowerID = -1;
                    }
                    foreach (Structures_Crypt structuresCrypt in StructureManager.GetAllStructuresOfType<Structures_Crypt>(FollowerLocation.Base))
                    {
                        if (structuresCrypt.Data.MultipleFollowerIDs.Contains(resurrecting.Info.ID))
                            structuresCrypt.WithdrawBody(resurrecting.Info.ID);
                    }
                    foreach (Structures_Morgue structuresMorgue in StructureManager.GetAllStructuresOfType<Structures_Morgue>(FollowerLocation.Base))
                    {
                        if (structuresMorgue.Data.MultipleFollowerIDs.Contains(resurrecting.Info.ID))
                            structuresMorgue.WithdrawBody(resurrecting.Info.ID);
                    }
                    //reset stats
                    resurrecting.ResetStats();
                    if (resurrecting.Info.Age > resurrecting.Info.LifeExpectancy)
                        resurrecting.Info.LifeExpectancy = resurrecting.Info.Age + UnityEngine.Random.Range(20, 30);
                    else
                        resurrecting.Info.LifeExpectancy += UnityEngine.Random.Range(20, 30);

                    //show animation
                    FollowerTask_ManualControl nextTask = new FollowerTask_ManualControl();
                    resurrecting.HardSwapToTask(nextTask);
                    resurrecting.Location = FollowerLocation.Church;
                    resurrecting.DesiredLocation = FollowerLocation.Church;
                    resurrecting.CurrentTask.Arrive();

                    Follower revivedFollower = FollowerManager.CreateNewFollower(resurrecting._directInfoAccess, ChurchFollowerManager.Instance.RitualCenterPosition.position + new Vector3(UnityEngine.Random.Range(-1.5f, 1.5f), UnityEngine.Random.Range(-1.5f, 1.5f)));
                    revivedFollower.SetOutfit(FollowerOutfitType.Worker, false);
                    revivedFollower.Brain.CheckChangeState();
                    Ritual.FollowerToAttendSermon.Add(revivedFollower.Brain);
                    revivedFollower.State.CURRENT_STATE = StateMachine.State.CustomAnimation;
                    GameManager.GetInstance().OnConversationNext(revivedFollower.gameObject, 5f);
                    revivedFollower.Spine.gameObject.SetActive(false);

                    yield return new WaitForSeconds(0.1f);
                    revivedFollower.Spine.gameObject.SetActive(true);

                    DeadWorshipper deadWorshipper1 = null;
                    foreach (DeadWorshipper deadWorshipper2 in DeadWorshipper.DeadWorshippers)
                    {
                        if (deadWorshipper2.StructureInfo.FollowerID == resurrecting.Info.ID)
                        {
                            deadWorshipper1 = deadWorshipper2;
                            break;
                        }
                    }
                    if (deadWorshipper1 != null)
                        StructureManager.RemoveStructure(deadWorshipper1.Structure.Brain);

                    revivedFollower.SetBodyAnimation("Sermons/resurrect", false);
                    revivedFollower.AddBodyAnimation("Reactions/react-enlightened1", false, 0.0f);
                    revivedFollower.AddBodyAnimation("idle", true, 0.0f);

                    groupResurrect++;
                    yield return new WaitForSeconds(0.1f);
                    if (groupResurrect >= 7)
                    {
                        yield return new WaitForSeconds(0.33f);
                        groupResurrect = 0;
                    }

                }
                /*yield return this.WaitFollowersFormCircle();*/
                //remove all dead follower data
                DataManager.Instance.Followers_Dead.Clear();
                DataManager.Instance.Followers_Dead_IDs.Clear();

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
                yield return new WaitForSeconds(7f);
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
