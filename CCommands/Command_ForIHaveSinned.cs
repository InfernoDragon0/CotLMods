﻿using System;
using System.Collections.Generic;
using System.IO;
using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using UnityEngine;

namespace CotLMiniMods.CustomFollowerCommands
{

    internal class Command_ForIHaveSinned : CustomFollowerCommand
    {
        public override string InternalName => "Command_ForIHaveSinned";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/flipcoin.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.DEFAULT_COMMAND };

        public override bool IsAvailable(Follower follower)
        {
            return Plugin.SinnedToday;
        }

        public override string GetLockedDescription(Follower follower)
        {
            return "You have already sinned today. Only a little sinning is allowed everyday.";
        }

        public override string GetTitle(Follower follower)
        {
            return "For I Have Sinned";
        }

        public override string GetDescription(Follower follower)
        {
            return "Infuse 10 Strange Materials into your follower to do a little tomfoolery. Once a day.";
        }

        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                new WaitForSeconds(0.5f);

                if (Inventory.GetItemQuantity(Plugin.StrangeMaterialItem) >= 10)
                {
                    Inventory.ChangeItemQuantity(Plugin.StrangeMaterialItem, -10);
                    InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 1, interaction.follower.transform.position); //TODO: change this
                    interaction.follower.Brain.HardSwapToTask((FollowerTask)new FollowerTask_ManualControl());

                    PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
                    PlayerFarming.Instance.Spine.UseDeltaTime = false;
                    PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
                    PlayerFarming.Instance.Spine.UseDeltaTime = false;
                    PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
                    PlayerFarming.Instance.simpleSpineAnimator.Animate("knucklebones/lose-dice", 0, false);

                    interaction.follower.TimedAnimation("Reactions/react-laugh", 2.4f, () =>
                    {
                        interaction.follower.Brain.CompleteCurrentTask();
                        PlayerFarming.Instance.Spine.UseDeltaTime = true;
                    });
                }
                
            }));
            interaction.Close(true, reshowMenu: false);
        }
    }
}
