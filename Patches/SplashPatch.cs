using HarmonyLib;
using Lamb.UI;
using MMTools;
using System;
using System.Collections.Generic;
using System.Text;

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]
    internal class SplashPatch
    {
        // This patch skips splash
        [HarmonyPatch(typeof(LoadMainMenu), nameof(LoadMainMenu.RunSplashScreens), MethodType.Enumerator)]
        [HarmonyPrefix]
        public static void LoadMainMenu_RemoveSplash()
        {
            if (!Plugin.skipSplash.Value) return;
            MMTransition.Play(MMTransition.TransitionType.ChangeSceneAutoResume, MMTransition.Effect.BlackFade, "Main Menu", 1f, "", (System.Action)null);
            return;
        }
    }
}
