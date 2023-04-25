using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using Lamb.UI;
using src.UI.Menus;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using src.Extensions;
using System;

namespace CotLMiniMods.CustomFollowerCommands
{

    internal class KnucklebonesCommand : CustomFollowerCommand
    {
        public override string InternalName => "Knucklebones_Command";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/knucklebones.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.DEFAULT_COMMAND };

        public UIKnuckleBonesController _knuckleBonesInstance;

        public override string GetTitle(Follower follower)
        {
            return "Knucklebones";
        }

        public override string GetDescription(Follower follower)
        {
            return "Play a casual game of Knucklebones with me!";
        }

        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                interaction.follower.Brain.HardSwapToTask((FollowerTask)new FollowerTask_ManualControl());
                //preload assets as of 1.1.4 as they are not loaded
                MonoSingleton<UIManager>.Instance.LoadKnucklebonesAssets().YieldUntilCompleted();
                
                interaction.follower.TimedAnimation("action", 1f, () =>
                {
                    //GameManager.GetInstance().OnConversationNew();
                    //interactionKnucklebones.goopTransition.FadeIn(1f);
                    PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.InActive;
                    //yield return (object)new WaitForSeconds(1f);
                    
                    
                    _knuckleBonesInstance = MonoSingleton<UIManager>.Instance.KnucklebonesTemplate.Instantiate<UIKnuckleBonesController>();
                    SimulationManager.Pause();

                    UIKnuckleBonesController knuckleBonesInstance1 = _knuckleBonesInstance;

                    knuckleBonesInstance1.OnHidden += new System.Action(SimulationManager.UnPause);

                    _knuckleBonesInstance.Show(createOpponent(interaction.follower), 5);
                    UIKnuckleBonesController knuckleBonesInstance2 = _knuckleBonesInstance;
                    // ISSUE: reference to a compiler-generated method
                    knuckleBonesInstance2.OnHidden = knuckleBonesInstance2.OnHidden + new System.Action(this.ContinueToKnucklebones);
                    _knuckleBonesInstance.OnGameCompleted += new Action<UIKnuckleBonesController.KnucklebonesResult>(this.CompleteGame);
                    _knuckleBonesInstance.OnGameQuit += new System.Action(this.GameQuit);
                    interaction.follower.Brain.CompleteCurrentTask();
                });
                
            }));
            interaction.Close(true, reshowMenu: false);
        }

        private void ContinueToKnucklebones()
        {
            Plugin.Log.LogInfo("cont kb ");
        }

        private void CompleteGame(UIKnuckleBonesController.KnucklebonesResult result)
        {
            Plugin.Log.LogInfo("copmleted kb " + result.ToString());
            SimulationManager.UnPause();
            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
        }

        private void GameQuit()
        {
            Plugin.Log.LogInfo("quitted knb");
            SimulationManager.UnPause();
            PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.Idle;
        }


        public KnucklebonesOpponent createOpponent(Follower follower)
        {
            var kb = new KnucklebonesOpponent();
            kb.Spine = follower.Spine;
            kb.Tag = KnucklebonesOpponent.OppnentTags.Ratau;
            kb.Config = ScriptableObject.CreateInstance<KnucklebonesPlayerConfiguration>();
            kb.Config._opponentName = "custom_" + follower.Brain.Info.Name;
            kb.Config._difficulty = 5;
            kb.Config._maxBet = 5;
            kb.Config._positionOffset = new Vector2(0, 0);
            kb.Config._scale = new Vector2(2, 2);
            kb.Config._initialSkinName = follower.Brain.Info.SkinName;
            kb.Config._spine = follower.Spine.skeletonDataAsset;
            kb.Config._variableToChangeOnWin = DataManager.Variables.Knucklebones_Opponent_0_Won;
            kb.Config._variableToShow = DataManager.Variables.Knucklebones_Opponent_0_Won;

            return kb;
        }
    }
}
