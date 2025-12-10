using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using CotLMiniMods.CCommands.Tasks;

namespace CotLMiniMods.CustomFollowerCommands
{

    internal class CrystalMineCommand : CustomFollowerCommand
    {
        public override string InternalName => "CrystalMine_Command";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/crystalmines.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.GIVE_WORKER_COMMAND };


        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
                interaction.follower.Brain.HardSwapToTask(new FollowerTask_CrystalMiner());
            }));
            interaction.Close(true, reshowMenu: false);
        }
    }
}
