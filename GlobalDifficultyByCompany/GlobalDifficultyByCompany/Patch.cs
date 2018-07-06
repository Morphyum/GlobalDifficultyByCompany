using BattleTech;
using BattleTech.UI;
using BattleTech.UI.Tooltips;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GlobalDifficultyByCompany {

    [HarmonyPatch(typeof(SimGameState))]
    [HarmonyPatch("GlobalDifficulty", PropertyMethod.Getter)]
    public static class SimGameState_GlobalDifficulty_Getter_Patch {
        static void Postfix(SimGameState __instance, ref float __result) {
            try {
                Settings settings = Helper.LoadSettings();
                int totalMechWorth = 0;
                List<MechDef> mechlist = __instance.ActiveMechs.Values.ToList();

                mechlist = mechlist.OrderByDescending(x => Helper.CalculateCBillValue(x)).ToList();
                int countedmechs = settings.NumberOfMechsCounted;
                if (mechlist.Count < settings.NumberOfMechsCounted) {
                    countedmechs = mechlist.Count;
                }
                for(int i = 0; i < countedmechs; i++) {
                    totalMechWorth += Mathf.RoundToInt(Helper.CalculateCBillValue(mechlist[i]));
                }

                float difficulty = totalMechWorth / settings.CostPerHalfSkull;
                __result = Mathf.Round(difficulty);
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
                    foreach(MechDef mech in mechs) {
                        totalMechWorth += Mathf.RoundToInt(Helper.CalculateCBillValue(mech));
                    }
                    lanceRatingWidget.SetDifficulty(totalMechWorth / settings.CostPerHalfSkull);
                }
            }
            catch (Exception e) {
                Logger.LogError(e);
            }
        }
    }
}