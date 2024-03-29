﻿using COTL_API.CustomStructures;
using COTL_API.Helpers;
using CotLTemplateMod;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Structures
{
    internal class FridgeStructure : CustomStructure
    {
        public override string InternalName => "Fridge_Structure";
        public override Sprite Sprite => TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/cotlpc.png"));

    }
}
