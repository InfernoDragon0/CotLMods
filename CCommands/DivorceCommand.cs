using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CotLMiniMods.CustomFollowerCommands
{

    internal class DivorceCommand : CustomFollowerCommand
    {
        public override string InternalName => "Divorce_Command";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/divorce.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.DEFAULT_COMMAND };


        public override string GetTitle(Follower follower)
        {
            return "Divorce";
        }

        public override string GetDescription(Follower follower)
        {
            return "Unmarry your follower";
        }

        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                if (!interaction.follower.Brain.Info.MarriedToLeader)
                {
                    Plugin.Log.LogInfo("not married");
                    interaction.follower.Brain.HardSwapToTask((FollowerTask)new FollowerTask_ManualControl());
                    interaction.follower.TimedAnimation("shrug", 2.5f, () =>
                    {
                        interaction.follower.Brain.CompleteCurrentTask();
                    });
                }
                else
                {
                    Plugin.Log.LogInfo("is married");
                    interaction.follower.Brain.HardSwapToTask((FollowerTask)new FollowerTask_ManualControl());
                    interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
                    interaction.follower.TimedAnimation("Reactions/react-grieve-sad", 3.5f, () =>
                    {
                        interaction.follower.Brain.Info.MarriedToLeader = false;
                        interaction.follower.TimedAnimation("tantrum-big", 6f, () =>
                        {
                            //perhaps dissent at a chance?
                            interaction.follower.Brain.CompleteCurrentTask();
                        });

                    });
                }
                
            }));
            interaction.Close();
        }
    }
}
