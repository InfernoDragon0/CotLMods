using COTL_API.Tasks;
using Spine;
using UnityEngine;

namespace CotLTemplateMod.CustomFollowerCommands
{
    internal class EatWaiterTask : FollowerTask_EatMeal
    {
        public EatWaiterTask(int mealID) : base(mealID)
        {
        }

        public override Vector3 UpdateDestination(Follower follower)
        {
            return follower.Brain.LastPosition;
        }

        public override void OnStart()
        {
            //this.SetState(FollowerTaskState.WaitingForLocation);
            this.SetState(FollowerTaskState.Doing); //eat straightaway
        }

    }
}
