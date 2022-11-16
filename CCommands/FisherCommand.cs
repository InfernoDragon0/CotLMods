using COTL_API.CustomFollowerCommand;
using COTL_API.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CotLTemplateMod.CustomFollowerCommands
{

    internal class FisherCommand : CustomFollowerCommand
    {
        public override string InternalName => "Fisher_Command";

        public override Sprite CommandIcon => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/fish.png"));
        public override List<FollowerCommandCategory> Categories { get; } = new List<FollowerCommandCategory> { FollowerCommandCategory.GIVE_WORKER_COMMAND };


        public override string GetTitle(Follower follower)
        {
            return "Fisher";
        }

        public override string GetDescription(Follower follower)
        {
            return "Go fishing in the fishing hut";
        }

        public override void Execute(interaction_FollowerInteraction interaction,
            FollowerCommands finalCommand)
        {

            interaction.StartCoroutine(interaction.FrameDelayCallback(delegate
            {
                interaction.eventListener.PlayFollowerVO(interaction.generalAcknowledgeVO);
                foreach (StructureBrain structureBrain in StructureManager.StructuresAtLocation(FollowerLocation.Base))
                {
                    if (structureBrain is ITaskProvider && (structureBrain is Structures_FishingHut))
                    {
                        if (!structureBrain.ReservedForTask)
                            interaction.follower.Brain.HardSwapToTask(new FisherTask(structureBrain.Data.ID));
                    }
                        
                }
                //interaction.follower.Brain.HardSwapToTask(new FollowerTask_Fisherman());
            }));
            interaction.Close();
        }
    }
}
