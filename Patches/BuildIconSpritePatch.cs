using COTL_API.Helpers;
using HarmonyLib;
using Lamb.UI.BuildMenu;
using MMTools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Patches
{
    [HarmonyPatch]
    internal class BuildIconSpritePatch
    {
        // This patch adds the icons to the build menu
        //[HarmonyPatch(typeof(BuildMenuItem), nameof(BuildMenuItem.Configure))]
        [HarmonyPatch(typeof(TypeAndPlacementObjects), nameof(TypeAndPlacementObjects.GetByType))]
        [HarmonyPostfix]
        public static void TypeAndPlacementObjects_GetByType(StructureBrain.TYPES Type, ref global::TypeAndPlacementObject __result)
        {
            Plugin.Log.LogInfo("adding build menu icon for " + Type.ToString());
            Sprite sprite = null;
            switch (Type)
            {
                case StructureBrain.TYPES.FISHING_HUT:
                    sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/Icon_FishingHut.png"));
                    break;
                case StructureBrain.TYPES.FOOD_STORAGE:
                    sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/Icon_FoodStorage.png"));
                    break;
                case StructureBrain.TYPES.FOOD_STORAGE_2:
                    sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/Icon_FoodStorage2.png"));
                    break;
                case StructureBrain.TYPES.KITCHEN:
                    sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/Icon_Kitchen.png"));
                    break;
                case StructureBrain.TYPES.KITCHEN_II:
                    sprite = TextureHelper.CreateSpriteFromPath(Path.Combine(Plugin.PluginPath, "Assets/Icon_Kitchen.png"));
                    break;

            }
            if (sprite != null)
            {
                __result.IconImage = sprite;
            }
        }
    }
}
