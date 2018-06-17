using BattleTech;
using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
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

                mechlist = mechlist.OrderByDescending(x => x.Description.Cost).ToList();
                for(int i = 0; i < settings.NumberOfMechsCounted; i++) {
                    totalMechWorth += mechlist[i].Description.Cost;
                }

                float difficulty = totalMechWorth / settings.CostPerHalfSkull;

                __result = Mathf.Round(difficulty);
            }
            catch (Exception e) {
                Logger.LogError(e);

            }
        }
    }
}