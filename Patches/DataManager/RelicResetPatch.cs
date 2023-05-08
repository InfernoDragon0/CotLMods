using CotLMiniMods;
using HarmonyLib;
using I2.Loc;
using MMRoomGeneration;
using MMTools;
using Socket.Newtonsoft.Json.Utilities.LinqBridge;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace CotLTemplateMod.Patches
{
    [HarmonyPatch]

    internal class RelicResetPatch
    {
        public static RelicType temp = RelicType.None;

        // This patch edits bell interaction
        [HarmonyPatch(typeof(DataManager), nameof(DataManager.SetNewRun))]
        [HarmonyPrefix]
        public static void DataManager_SetNewRun(out RelicType __state)
        {
            //collect the loot
            if (Plugin.relicNoReset.Value)
            {
                Plugin.Log.LogInfo("GOT cyrrent relic");
                Plugin.Log.LogInfo(DataManager.Instance.CurrentRelic);
                __state = DataManager.Instance.CurrentRelic;
                temp = DataManager.Instance.CurrentRelic;
            }
            else
            {
                __state = RelicType.None;
            }
        }

        [HarmonyPatch(typeof(DataManager), nameof(DataManager.SetNewRun))]
        [HarmonyPostfix]
        public static void DataManager_SetNewRun_Post(RelicType __state)
        {
            //collect the loot
            if (__state != RelicType.None)
            {
                Plugin.Log.LogInfo("GOT old relic");
                Plugin.Log.LogInfo(__state);
                DataManager.Instance.CurrentRelic = __state;
                PlayerFarming.Instance.playerRelic.EquipRelic(EquipmentManager.GetRelicData(DataManager.Instance.CurrentRelic), false);
            }
        }

        [HarmonyPatch(typeof(PlayerFarming), nameof(PlayerFarming.Awake))]
        [HarmonyPostfix]
        public static void DataManager_ResetRunData_Post()
        {
            //collect the loot
            Plugin.Log.LogInfo("Forced Relic");
            Plugin.Log.LogInfo(temp);
            
            if (temp != RelicType.None)
            {
                PlayerFarming.Instance.playerRelic.EquipRelic(EquipmentManager.GetRelicData(temp), false);
            }
        }

    }
}
