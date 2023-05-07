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
using Lamb.UI.Assets;

namespace CotLMiniMods.Rituals
{
    public class RitualFrenzyRitual : CustomRitual
    {
        public override string InternalName { get; } = "Ritual_Frenzy_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_frenzy.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 666) };

        public override string GetLocalizedName => "Ritual Frenzy";
        public override string GetLocalizedDescription => "Immediately resets all your ritual cooldowns.";
        public override float FaithChange { get; } = 10;
        public override float Cooldown => 0f;
        public override FollowerTrait.TraitType RitualTrait { get; } = FollowerTrait.TraitType.None;

        //todo: implement cooldowns
        public override void Play()
        {
            base.Play();
            GameManager.GetInstance().StartCoroutine(this.RitualRoutine());
        }
        //todo: the actual ritual
        private IEnumerator RitualRoutine()
        {
            AudioManager.Instance.PlayOneShot("event:/rituals/generic_start_ritual");
            yield return this.StartCoroutine(this.WaitFollowersFormCircle());
            
            Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
            /*PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position, GoToCallback: ((System.Action)(() =>
            {*/
            Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
            PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
            PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position, 0.1f).SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(Ease.InOutSine).SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(true);

            PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
            PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
            PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0.0f);
            /*})));*/

            

            Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
            ChurchFollowerManager.Instance.StartRitualOverlay();

            BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
            BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);

            DOTween.To((DOGetter<float>)(() => GameManager.GetInstance().CamFollowTarget.targetDistance), (DOSetter<float>)(x => GameManager.GetInstance().CamFollowTarget.targetDistance = x), 6.5f, 1f).SetEase<TweenerCore<float, float, FloatOptions>>(Ease.OutSine);

            foreach (UpgradeSystem.UpgradeCoolDown ucd in DataManager.Instance.UpgradeCoolDowns)
            {
                yield return (object)new WaitForSeconds(1f);
                //as of 1.20 this does not exist (yet?)
                /*ResourceCustomTarget.Create(PlayerFarming.Instance.gameObject, Interaction_TempleAltar.Instance.transform.position, RitualIconMapping.GetImage(ucd.Type), null);*/
                AudioManager.Instance.PlayOneShot("event:/building/building_bell_ring", PlayerFarming.Instance.gameObject);

            }

            //and reset cooldown
            UpgradeSystem.ClearAllCoolDowns();
            yield return (object)new WaitForSeconds(2f);
            Interaction_TempleAltar.Instance.PulseDisplacementObject(PlayerFarming.Instance.CameraBone.transform.position);
            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-stop", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("idle", 0, true, 0.0f);

            foreach (FollowerBrain followerBrain in Ritual.FollowerToAttendSermon)
            {
                if (followerBrain.CurrentTask is FollowerTask_AttendRitual)
                    (followerBrain.CurrentTask as FollowerTask_AttendRitual).Cheer();
            }
            yield return (object)new WaitForSeconds(0.5f);
            BiomeConstants.Instance.ChromaticAbberationTween(1f, 1f, BiomeConstants.Instance.ChromaticAberrationDefaultValue);
            BiomeConstants.Instance.VignetteTween(1f, 0.7f, BiomeConstants.Instance.VignetteDefaultValue);

            ChurchFollowerManager.Instance.EndRitualOverlay();
            GameManager.GetInstance().CamFollowTarget.targetDistance = 11f;
            Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(false);
            
            yield return new WaitForSeconds(3f);

            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            this.CompleteRitual();
            
        }

    }
}
