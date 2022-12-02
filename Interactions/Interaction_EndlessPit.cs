using CotLTemplateMod;
using Lamb.UI.FollowerSelect;
using Lamb.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using src.Extensions;
using Lamb.UI.FollowerInteractionWheel;
using Unity.Mathematics;
using Random = UnityEngine.Random;

namespace CotLMiniMods.Interactions
{
    internal class Interaction_EndlessPit : Interaction
    {
        private bool Activated;
        public override void GetLabel()
        {
            this.label = "Add Followers for Energy. [0 Followers][0 Energy]";
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Plugin.Log.LogInfo("I am enabled Endless Pit");
        }

        public override void OnInteract(StateMachine state)
        {
            if (this.Activated) return;

            if (true)
            {
                //check if lamb is holding a corpse, then add it in, doesnt matter the follower id, it is endless after all
            }
            
        }


    }
}
