using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using CotLMiniMods.CCommands.Tasks;

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CotLMiniMods.CustomFollowerCommands
{

    internal class WaiterCommand : CustomFollowerCommand
    {
        public override string InternalName => "Waiter_Command";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/waiter.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.GIVE_WORKER_COMMAND };

        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
                interaction.follower.Brain.HardSwapToTask(new WaiterTask());
            }));
            interaction.Close(true, reshowMenu: false);
        }
    }
}
