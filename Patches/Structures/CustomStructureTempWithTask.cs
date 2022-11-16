using COTL_API.CustomStructures;
using System.Collections.Generic;

namespace CotLMiniMods.Patches.Structures
{
    public abstract class CustomStructureTempWithTask : CustomStructure, ITaskProvider
    {
        public override string InternalName => "Custom_Task_Structure";
        public abstract FollowerTask StructureTask { get; }

        public bool CheckOverrideComplete() => true;

        public FollowerTask GetOverrideTask(FollowerBrain brain)
        {
            return null;
        }

        public void GetAvailableTasks(ScheduledActivity activity, SortedList<float, FollowerTask> sortedTasks)
        {
            if (activity != ScheduledActivity.Work || this.ReservedForTask)
                return;

            sortedTasks.Add(StructureTask.Priorty, StructureTask);
        }
    }
}
