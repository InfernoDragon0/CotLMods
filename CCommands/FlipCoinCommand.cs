using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CotLMiniMods.CustomFollowerCommands
{

    internal class FlipCoinCommand : CustomFollowerCommand
    {
        public override string InternalName => "Flip_Coin_Command";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/flipcoin.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.DEFAULT_COMMAND };

        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                GameManager.GetInstance().OnConversationNew();
                new WaitForSeconds(0.5f);
                if (Random.Range(1, 100) < 50)
                {
                    InventoryItem.Spawn(InventoryItem.ITEM_TYPE.BLACK_GOLD, 5, interaction.follower.transform.position);
                    interaction.follower.Brain.HardSwapToTask((FollowerTask)new FollowerTask_ManualControl());
                    PlayerFarming.Instance.state.CURRENT_STATE = StateMachine.State.CustomAnimation;
                    PlayerFarming.Instance.Spine.UseDeltaTime = false;
                    PlayerFarming.Instance.simpleSpineAnimator.Animate("build", 0, true);
                    PlayerFarming.Instance.Spine.UseDeltaTime = false;
                    PlayerFarming.Instance.Spine.skeleton.Update(Time.deltaTime);
                    PlayerFarming.Instance.simpleSpineAnimator.Animate("reactions/react-happy", 0, false);

                    interaction.follower.TimedAnimation("Reactions/react-sad", 1.3f, () =>
                    {
                        interaction.follower.Brain.CompleteCurrentTask();
                        GameManager.GetInstance().OnConversationEnd();
                        PlayerFarming.Instance.Spine.UseDeltaTime = true;


                    });
                }
                else
                {
                    Inventory.ChangeItemQuantity(InventoryItem.ITEM_TYPE.BLACK_GOLD, -5);
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
                        GameManager.GetInstance().OnConversationEnd();
                        PlayerFarming.Instance.Spine.UseDeltaTime = true;


                    });
                }
                
                
                
            }));
            interaction.Close(true, reshowMenu: false);
        }
    }
}
