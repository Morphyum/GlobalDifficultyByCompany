using BattleTech;
using BattleTech.UI;
using Harmony;
using HBS;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace GlobalDifficultyByCompany {

    [HarmonyPatch(typeof(SimGameState), "GetNormalizedDifficulty")]
    public static class SimGameState_GetNormalizedDifficulty_Patch {
        static void Postfix(SimGameState __instance, ref int __result) {
            Settings settings = Helper.LoadSettings();
            if (settings.Mode == 0) {
                __result = Mathf.RoundToInt(Mathf.Clamp(__instance.GlobalDifficulty, 0, 10));
            }
        }
    }

    [HarmonyPatch(typeof(Starmap), "PopulateMap", new Type[] { typeof(SimGameState) })]
    public static class Starmap_PopulateMap_Patch {

        static void Prefix(Starmap __instance, SimGameState simGame) {
            Settings settings = Helper.LoadSettings();
            if (settings.Mode == 0) {
                foreach (StarSystem system in simGame.StarSystems) {
                    AccessTools.Field(typeof(StarSystemDef), "DefaultDifficulty").SetValue(system.Def, 0);
                    AccessTools.Field(typeof(StarSystemDef), "DifficultyList").SetValue(system.Def, new List<int>());
                    AccessTools.Field(typeof(StarSystemDef), "DifficultyModes").SetValue(system.Def, new List<SimGameState.SimGameType>());
                }
            }
        }

        static void Postfix(Starmap __instance, SimGameState simGame) {
            Settings settings = Helper.LoadSettings();
            if (settings.Mode == 2) {
                foreach (StarSystem system in simGame.StarSystems) {
                    StarSystem capital = simGame.StarSystems.Find(x => x.Name.Equals(Helper.GetCapital(system.Owner)));
                    if (capital != null) {
                        StarSystemNode systemByID = __instance.GetSystemByID(system.ID);
                        StarSystemNode systemByID2 = __instance.GetSystemByID(capital.ID);
                        AStar.PathFinder starmapPathfinder = new AStar.PathFinder();
                        starmapPathfinder.InitFindPath(systemByID, systemByID2, 1, 1E-06f, new Action<AStar.AStarResult>(OnPathfindingComplete));
                        while (!starmapPathfinder.IsDone) {
                            starmapPathfinder.Step();
                        }
                    }
                    else {
                        AccessTools.Field(typeof(StarSystemDef), "DefaultDifficulty").SetValue(system.Def, 1);
                    }
                    AccessTools.Field(typeof(StarSystemDef), "DifficultyList").SetValue(system.Def, new List<int>());
                    AccessTools.Field(typeof(StarSystemDef), "DifficultyModes").SetValue(system.Def, new List<SimGameState.SimGameType>());
                }
            }
        }
        private static void OnPathfindingComplete(AStar.AStarResult result) {
            try {
                int baseDifficulty = 5;
                GameInstance game = LazySingletonBehavior<UnityGameInstance>.Instance.Game;
                Dictionary<Faction, int> allCareerFactionReputations = game.Simulation.GetAllCareerFactionReputations();
                Settings settings = Helper.LoadSettings();
                int count = result.path.Count;
                StarSystemNode starSystemNode = (StarSystemNode)result.path[0];
                int rangeDifficulty = 0;
                int repModifier = 0;
                Faction repFaction = starSystemNode.System.Owner;
                if (!Helper.IsCapital(starSystemNode.System)) {
                    rangeDifficulty = Mathf.RoundToInt((count - 1));
                } else {
                    repFaction = Helper.GetFactionForCapital(starSystemNode.System);
                }
                if (allCareerFactionReputations.ContainsKey(repFaction)) {
                    int repOfOwner = allCareerFactionReputations[repFaction];
                    repModifier = Mathf.CeilToInt(repOfOwner / 20f);
                }
                else {
                    //Logger.LogLine("RepMissing: " + starSystemNode.System.Owner.ToString());
                    //Logger.LogLine("Def: " + starSystemNode.System.Def.Owner.ToString());
                }
                int endDifficulty = Mathf.Clamp(baseDifficulty + rangeDifficulty - repModifier, 1, 10);
                AccessTools.Field(typeof(StarSystemDef), "DefaultDifficulty").SetValue(starSystemNode.System.Def, endDifficulty);
            }catch(Exception e) {
                Logger.LogError(e);
            }
        }
    }

    [HarmonyPatch(typeof(SimGameState))]
    [HarmonyPatch("GlobalDifficulty", MethodType.Getter)]
    public static class SimGameState_GlobalDifficulty_Getter_Patch {
        static void Postfix(SimGameState __instance, ref float __result) {
            try {
                Settings settings = Helper.LoadSettings();
                if (settings.Mode == 0) {
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
                    __result = Mathf.Min(10, Mathf.Round(difficulty));
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