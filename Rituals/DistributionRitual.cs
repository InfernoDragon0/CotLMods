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

namespace CotLMiniMods.Rituals
{
    public class DistributionRitual : CustomRitual
    {
        public override string InternalName { get; } = "Distribution_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_leveling.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 300) };

        public override string GetLocalizedName => "Distribution Ritual";
        public override string GetLocalizedDescription => "Distribute your wealth among the followers! They will instantly level up.";
        public override float FaithChange { get; } = 10;
        public override FollowerTrait.TraitType RitualTrait { get; } = FollowerTrait.TraitType.None;
        
        public override float Cooldown => 60f;

        private EventInstance loopedSound;
        public override void Play()
        {
            base.Play();
            Plugin.Log.LogInfo("Starting ritual for " + upgradeType + " and " + RitualType);
            GameManager.GetInstance().StartCoroutine(this.RitualRoutine());
        }
        //todo: the actual ritual
        private IEnumerator RitualRoutine()
        {
            AudioManager.Instance.PlayOneShot("event:/rituals/generic_start_ritual");
            Interaction_TempleAltar.Instance.SimpleSetCamera.Play();
            yield return this.StartCoroutine(this.WaitFollowersFormCircle());
            
            /*PlayerFarming.Instance.GoToAndStop(ChurchFollowerManager.Instance.RitualCenterPosition.position, GoToCallback: ((System.Action)(() =>
            {*/
                Interaction_TempleAltar.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
                PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
                PlayerFarming.Instance.state.transform.DOMove(ChurchFollowerManager.Instance.RitualCenterPosition.position, 0.1f).SetEase<TweenerCore<Vector3, Vector3, VectorOptions>>(Ease.InOutSine).SetUpdate<TweenerCore<Vector3, Vector3, VectorOptions>>(true);
            /*})));*/

            PlayerFarming.Instance.Spine.skeleton.FindBone("ritualring").Rotation += 60f;
            PlayerFarming.Instance.Spine.skeleton.UpdateWorldTransform();
            PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0.0f);

            Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
            ChurchFollowerManager.Instance.StartRitualOverlay();

            BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
            BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);

            DOTween.To((DOGetter<float>)(() => GameManager.GetInstance().CamFollowTarget.targetDistance), (DOSetter<float>)(x => GameManager.GetInstance().CamFollowTarget.targetDistance = x), 6.5f, 1f).SetEase<TweenerCore<float, float, FloatOptions>>(Ease.OutSine);
            
            yield return (object)new WaitForSeconds(1.2f);
            this.loopedSound = AudioManager.Instance.CreateLoop("event:/rituals/coin_loop", PlayerFarming.Instance.gameObject, true);
            float delay = 0.0f;
            foreach (FollowerBrain followerBrain in Ritual.GetFollowersAvailableToAttendSermon())
            {
                this.StartCoroutine(this.GiveCoins(FollowerManager.FindFollowerByID(followerBrain.Info.ID), 5f, delay));
                delay += 0.1f;
            }
            yield return (object)new WaitForSeconds(5f);
            AudioManager.Instance.StopLoop(this.loopedSound);
            yield return (object)new WaitForSeconds(1.2f);
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
            float EndingDelay = 0.0f;

            foreach (FollowerBrain followerBrain in Ritual.FollowerToAttendSermon)
            {
                FollowerBrain brain = followerBrain;
                brain.AddThought(Thought.AlmsToThePoorRitual);
                brain.Stats.Adoration = brain.Stats.MAX_ADORATION;
                Follower f = FollowerManager.FindFollowerByID(brain.Info.ID);
                
                f.AdorationUI.BarController.SetBarSize(brain.Stats.Adoration / brain.Stats.MAX_ADORATION, false, true);
                f.StartCoroutine(brain.AddAdorationIE(f, FollowerBrain.AdorationActions.Sermon, null));
                
                /*brain.AddAdoration(FollowerBrain.AdorationActions.Ritual_AlmsToPoor, (System.Action)(() =>
                {
                    float Delay = UnityEngine.Random.Range(0.1f, 0.5f);
                    EndingDelay += Delay;
                    GameManager.GetInstance().StartCoroutine(this.DelayFollowerReaction(brain, Delay));
                }));*/
            }
            yield return new WaitForSeconds(3f + EndingDelay);

            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            this.CompleteRitual();

            yield return new WaitForSeconds(1f);
            CultFaithManager.AddThought(Thought.Cult_AlmsToPoor, -1, 1f);
        }

        private IEnumerator GiveCoins(Follower follower, float totalTime, float delay)
        {
            yield return new WaitForSeconds(delay);
            int randomCoins = 50;
            float increment = (totalTime - delay) / (float)randomCoins;
            for (int i = 0; i < randomCoins; ++i)
            {
                AudioManager.Instance.PlayOneShot("event:/followers/pop_in", PlayerFarming.Instance.transform.position);
                ResourceCustomTarget.Create(follower.gameObject, PlayerFarming.Instance.transform.position, InventoryItem.ITEM_TYPE.BLACK_GOLD, new System.Action(this.PlayCoinSound));
                yield return new WaitForSeconds(increment);
            }
        }

        private void PlayCoinSound() => AudioManager.Instance.PlayOneShot("event:/rituals/coins", PlayerFarming.Instance.transform.position);
    }
}
