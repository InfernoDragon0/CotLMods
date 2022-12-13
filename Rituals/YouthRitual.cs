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
    public class YouthRitual : CustomRitual
    {
        public override string InternalName { get; } = "Youth_Ritual";
        public override Sprite Sprite { get; } = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/ritual_youth.png"));
        public override List<StructuresData.ItemCost> ItemCosts { get; } = new() { 
            new StructuresData.ItemCost(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1000), 
            new StructuresData.ItemCost(Plugin.StrangeMaterialItem, 10) 
        };

        public override string GetLocalizedName => "Youth Ritual";
        public override string GetLocalizedDescription => "Infusing the power of the Strange Material to make all old followers young again. Doubles life expectancy of the affected followers.";
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
            yield return this.StartCoroutine(this.CentreAndAnimatePlayer());
            Interaction_TempleAltar.Instance.SimpleSetCamera.Play();

            yield return this.StartCoroutine(this.WaitFollowersFormCircle());

            PlayerFarming.Instance.simpleSpineAnimator.Animate("rituals/ritual-start", 0, false);
            PlayerFarming.Instance.simpleSpineAnimator.AddAnimate("rituals/ritual-loop", 0, true, 0.0f);
            yield return (object)new WaitForSeconds(1f);

            Interaction_TempleAltar.Instance.RitualLighting.gameObject.SetActive(true);
            ChurchFollowerManager.Instance.StartRitualOverlay();

            BiomeConstants.Instance.ChromaticAbberationTween(2f, BiomeConstants.Instance.ChromaticAberrationDefaultValue, 1f);
            BiomeConstants.Instance.VignetteTween(2f, BiomeConstants.Instance.VignetteDefaultValue, 0.7f);

            DOTween.To((DOGetter<float>)(() => GameManager.GetInstance().CamFollowTarget.targetDistance), (DOSetter<float>)(x => GameManager.GetInstance().CamFollowTarget.targetDistance = x), 6.5f, 1f).SetEase<TweenerCore<float, float, FloatOptions>>(Ease.OutSine);
            
            foreach (FollowerBrain oldBrain in Ritual.FollowerToAttendSermon)
            {
                if (oldBrain.Info.CursedState == Thought.OldAge || oldBrain.Info.OldAge)
                {
                    oldBrain.Info.LifeExpectancy *= 2;
                    oldBrain.RemoveCurseState(Thought.OldAge);
                    Follower f = FollowerManager.FindFollowerByID(oldBrain.Info.ID);
                    f.TimedAnimation("Reactions/react-determined2", 2f, () =>
                    {
                        oldBrain.ResetStats();
                        f.SetOutfit(FollowerOutfitType.Follower, false);
                    });
                    yield return new WaitForSeconds(0.2f);
                    
                }
                else continue;
                
            }

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

            Interaction_TempleAltar.Instance.SimpleSetCamera.Reset();
            this.CompleteRitual();
        }

    }
}
