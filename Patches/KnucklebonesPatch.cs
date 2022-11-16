using COTL_API.CustomStructures;
using CotLMiniMods.Interactions;
using CotLMiniMods.Structures;
using CotLTemplateMod;
using HarmonyLib;
using src.UI.Menus;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CotLMiniMods.Patches
{
    [HarmonyPatch]
    internal class KnucklebonesPatch
    {
        // This patch to costum kb
        [HarmonyPatch(typeof(KBOpponent), nameof(KBOpponent._playerIdleAnimation), MethodType.Getter)]
        [HarmonyPostfix]
        public static void KBOpponent_playerIdleAnimation(KBOpponent __instance, ref string __result)
        {
            if (__instance._opponent.Config.OpponentName.Contains("custom"))
            {
                Plugin.Log.LogInfo("custom source kb");
                __result = "action";
            }
        }

        // This patch to costum kb
        [HarmonyPatch(typeof(KBOpponent), nameof(KBOpponent._playDiceAnimation), MethodType.Getter)]
        [HarmonyPostfix]
        public static void KBOpponent_playDiceAnimation(KBOpponent __instance, ref string __result)
        {
            if (__instance._opponent.Config.OpponentName.Contains("custom"))
            {
                Plugin.Log.LogInfo("custom source kb");
                __result = "wave";
            }
        }

        // This patch to costum kb
        [HarmonyPatch(typeof(KBOpponent), nameof(KBOpponent._playerTakeDiceAnimation), MethodType.Getter)]
        [HarmonyPostfix]
        public static void KBOpponent_playerTakeDiceAnimation(KBOpponent __instance, ref string __result)
        {
            if (__instance._opponent.Config.OpponentName.Contains("custom"))
            {
                Plugin.Log.LogInfo("custom source kb");
                __result = "Conversations/react-nice1";
            }
        }

        // This patch to costum kb
        [HarmonyPatch(typeof(KBOpponent), nameof(KBOpponent._playerLostDiceAnimation), MethodType.Getter)]
        [HarmonyPostfix]
        public static void KBOpponent_playerLostDiceAnimation(KBOpponent __instance, ref string __result)
        {
            if (__instance._opponent.Config.OpponentName.Contains("custom"))
            {
                Plugin.Log.LogInfo("custom source kb");
                __result = "Reactions/react-admire3";
            }
        }

        // This patch to costum kb
        [HarmonyPatch(typeof(KBOpponent), nameof(KBOpponent._playerWonAnimation), MethodType.Getter)]
        [HarmonyPostfix]
        public static void KBOpponent_playerWonAnimation(KBOpponent __instance, ref string __result)
        {
            if (__instance._opponent.Config.OpponentName.Contains("custom"))
            {
                Plugin.Log.LogInfo("custom source kb");
                __result = "Reactions/react-happy1";
            }
        }

        // This patch to costum kb
        [HarmonyPatch(typeof(KBOpponent), nameof(KBOpponent._playerWonLoop), MethodType.Getter)]
        [HarmonyPostfix]
        public static void KBOpponent_playerWonLoop(KBOpponent __instance, ref string __result)
        {
            if (__instance._opponent.Config.OpponentName.Contains("custom"))
            {
                Plugin.Log.LogInfo("custom source kb");
                __result = "cheer";
            }
        }

        // This patch to costum kb
        [HarmonyPatch(typeof(KBOpponent), nameof(KBOpponent._playerLostAnimation), MethodType.Getter)]
        [HarmonyPostfix]
        public static void KBOpponent_playerLostAnimation(KBOpponent __instance, ref string __result)
        {
            if (__instance._opponent.Config.OpponentName.Contains("custom"))
            {
                Plugin.Log.LogInfo("custom source kb");
                __result = "fight-lose";
            }
        }

        // This patch to costum kb
        [HarmonyPatch(typeof(KBOpponent), nameof(KBOpponent._playerLostLoop), MethodType.Getter)]
        [HarmonyPostfix]
        public static void KBOpponent_playerLostLoop(KBOpponent __instance, ref string __result)
        {
            if (__instance._opponent.Config.OpponentName.Contains("custom"))
            {
                Plugin.Log.LogInfo("custom source kb");
                __result = "idle-sad";
            }
        }

    }
}
