using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CotLMiniMods.CustomFollowerCommands
{

    internal class BoneMineCommand : CustomFollowerCommand
    {
        public override string InternalName => "BoneMine_Command";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/bonemines.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.GIVE_WORKER_COMMAND };

        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
                interaction.follower.Brain.HardSwapToTask(new FollowerTask_BoneMiner());
            }));
            interaction.Close(true, reshowMenu: false);
        }
    }
}
