using BattleTech;
using BattleTech.UI;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GlobalDifficultyByCompany {

    [HarmonyPatch(typeof(SimGameState), "GetNormalizedDifficulty")]
    public static class SimGameState_GetNormalizedDifficulty_Patch {
        static void Postfix(SimGameState __instance, int __result) {
            Settings settings = Helper.LoadSettings();
            if (settings.ScalePlanets) {
                __result = Mathf.RoundToInt(Mathf.Clamp(__instance.GlobalDifficulty, 0, 10));
            }
        }
    }

    [HarmonyPatch(typeof(Starmap), "PopulateMap", new Type[] { typeof(SimGameState) })]
    public static class Starmap_PopulateMap_Patch {
        static void Prefix(SimGameState simGame) {
            Settings settings = Helper.LoadSettings();
            if (settings.ScalePlanets) {
                foreach (StarSystem system in simGame.StarSystems) {
                    AccessTools.Field(typeof(StarSystemDef), "DefaultDifficulty").SetValue(system.Def, 0);
                    AccessTools.Field(typeof(StarSystemDef), "DifficultyList").SetValue(system.Def, new List<int>());
                    AccessTools.Field(typeof(StarSystemDef), "DifficultyModes").SetValue(system.Def, new List<SimGameState.SimGameType>());
                }
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState))]
    [HarmonyPatch("GlobalDifficulty", MethodType.Getter)]
    public static class SimGameState_GlobalDifficulty_Getter_Patch {
        static void Postfix(SimGameState __instance, ref float __result) {
            try {
                Settings settings = Helper.LoadSettings();
                if (settings.ScalePlanets) {
                    int totalMechWorth = 0;
                    List<MechDef> mechlist = __instance.ActiveMechs.Values.ToList();
                    mechlist = mechlist.OrderByDescending(x => Helper.CalculateCBillValue(x)).ToList();
                    int countedmechs = settings.NumberOfMechsCounted;
                    if (mechlist.Count < settings.NumberOfMechsCounted) {
                        countedmechs = mechlist.Count;
                    }
                    for (int i = 0; i < countedmechs; i++) {
                        totalMechWorth += Mathf.RoundToInt(Helper.CalculateCBillValue(mechlist[i]));
                    }

                    float difficulty = totalMechWorth / settings.CostPerHalfSkull;
                    __result = Mathf.Round(difficulty);
                }
                else {
                    __result = 0;
                }
            }
            catch (Exception e) {
                Logger.LogError(e);

            }
        }
    }

    [HarmonyPatch(typeof(LanceHeaderWidget), "RefreshLanceInfo")]
    public static class LanceHeaderWidget_RefreshLanceInfo {

        static void Postfix(LanceHeaderWidget __instance, List<MechDef> mechs) {
            try {
                LanceConfiguratorPanel LC = (LanceConfiguratorPanel)AccessTools.Field(typeof(LanceHeaderWidget), "LC").GetValue(__instance);
                if (LC.IsSimGame) {
                    Settings settings = Helper.LoadSettings();
                    SGDifficultyIndicatorWidget lanceRatingWidget = (SGDifficultyIndicatorWidget)AccessTools.Field(typeof(LanceHeaderWidget), "lanceRatingWidget").GetValue(__instance);
                    TextMeshProUGUI label = lanceRatingWidget.transform.parent.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault(t => t.transform.name == "label-lanceRating");
                    label.text = "Lance Rating";
                    int totalMechWorth = 0;
                    foreach (MechDef mech in mechs) {
                        totalMechWorth += Mathf.RoundToInt(Helper.CalculateCBillValue(mech));
                    }
                    lanceRatingWidget.SetDifficulty(Mathf.Min(10, totalMechWorth / settings.CostPerHalfSkull));
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState), "ShowDifficultMissionPopup")]
    public static class SimGameState_ShowDifficultMissionPopup {

        static bool Prefix(SimGameState __instance, SimGameInterruptManager ___interruptQueue) {
            try {
                ___interruptQueue.QueuePauseNotification("Difficult Mission", "Careful, Commander. This drop looks like it might require more firepower than that.", __instance.GetCrewPortrait(SimGameCrew.Crew_Darius), string.Empty, new Action(__instance.RoomManager.CmdCenterRoom.lanceConfigBG.LC.ContinueConfirmClicked), "CONFIRM", null, "BACK");
                return false;
            }
            catch (Exception e) {
                Logger.LogError(e);
                return true;
            }
        }
    }
}