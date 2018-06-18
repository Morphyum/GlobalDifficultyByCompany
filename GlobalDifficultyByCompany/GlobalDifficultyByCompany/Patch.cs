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
}