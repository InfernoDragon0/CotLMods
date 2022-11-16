using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CotLTemplateMod.CustomFollowerCommands
{

    internal class SilkMineCommand : CustomFollowerCommand
    {
        public override string InternalName => "SilkMine_Command";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/silkmines.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.GIVE_WORKER_COMMAND };

        public override string GetTitle(Follower follower)
        {
            return "Mine Silk";
        }

        public override string GetDescription(Follower follower)
        {
            return "Mines Silk";
        }

        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
                interaction.follower.Brain.HardSwapToTask(new FollowerTask_SilkMiner());
            }));
            interaction.Close();
        }
    }
}
