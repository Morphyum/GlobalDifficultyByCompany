using BattleTech;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GlobalDifficultyByCompany {
    public class Helper {

        public static Settings LoadSettings() {
            try {
                using (StreamReader r = new StreamReader($"{GlobalDifficultyByCompany.ModDirectory}/settings.json")) {
                    string json = r.ReadToEnd();
                    return JsonConvert.DeserializeObject<Settings>(json);
                }
            }
            catch (Exception ex) {
                Logger.LogError(ex);
                return null;
            }
        }

        public static float CalculateCBillValue(MechDef mech) {
            float currentCBillValue = 0f;
            float num = 10000f;
            currentCBillValue = (float)mech.Chassis.Description.Cost;
            float num2 = 0f;
            num2 += mech.Head.CurrentArmor;
            num2 += mech.CenterTorso.CurrentArmor;
            num2 += mech.CenterTorso.CurrentRearArmor;
            num2 += mech.LeftTorso.CurrentArmor;
            num2 += mech.LeftTorso.CurrentRearArmor;
            num2 += mech.RightTorso.CurrentArmor;
            num2 += mech.RightTorso.CurrentRearArmor;
            num2 += mech.LeftArm.CurrentArmor;
            num2 += mech.RightArm.CurrentArmor;
            num2 += mech.LeftLeg.CurrentArmor;
            num2 += mech.RightLeg.CurrentArmor;
            num2 *= UnityGameInstance.BattleTechGame.MechStatisticsConstants.CBILLS_PER_ARMOR_POINT;
            currentCBillValue += num2;
            for (int i = 0; i < mech.Inventory.Length; i++) {
                MechComponentRef mechComponentRef = mech.Inventory[i];
                currentCBillValue += (float)mechComponentRef.Def.Description.Cost;
            }
            currentCBillValue = Mathf.Round(currentCBillValue / num) * num;
            return currentCBillValue;
        }

        // Capitals by faction
        private static Dictionary<Faction, string> capitalsByFaction = new Dictionary<Faction, string> {
            { Faction.Kurita, "Luthien" },
            { Faction.Davion, "New Avalon" },
            { Faction.Liao, "Sian" },
            { Faction.Marik, "Atreus (FWL)" },
            { Faction.Rasalhague, "Rasalhague" },
            { Faction.Ives, "St. Ives" },
            { Faction.Oberon, "Oberon" },
            { Faction.TaurianConcordat, "Taurus" },
            { Faction.MagistracyOfCanopus, "Canopus" },
            { Faction.Outworld, "Alpheratz" },
            { Faction.Circinus, "Circinus" },
            { Faction.Marian, "Alphard (MH)" },
            { Faction.Lothian, "Lothario" },
            { Faction.AuriganRestoration, "Coromodir" },
            { Faction.Steiner, "Tharkad" },
            { Faction.ComStar, "Terra" },
            { Faction.Castile, "Asturias" },
            { Faction.Chainelane, "Far Reach" },
            { Faction.ClanBurrock, "Albion (Clan)" },
            { Faction.ClanCloudCobra, "Zara (Homer 2850+)" },
            { Faction.ClanCoyote, "Tamaron" },
            { Faction.ClanDiamondShark, "Strato Domingo" },
            { Faction.ClanFireMandrill, "Shadow" },
            { Faction.ClanGhostBear, "Arcadia (Clan)" },
            { Faction.ClanGoliathScorpion, "Dagda (Clan)" },
            { Faction.ClanHellsHorses, "Kirin" },
            { Faction.ClanIceHellion, "Hector" },
            { Faction.ClanJadeFalcon, "Ironhold" },
            { Faction.ClanNovaCat, "Barcella" },
            { Faction.ClansGeneric, "Strana Mechty" },
            { Faction.ClanSmokeJaguar, "Huntress" },
            { Faction.ClanSnowRaven, "Lum" },
            { Faction.ClanStarAdder, "Sheridan (Clan)" },
            { Faction.ClanSteelViper, "New Kent" },
            { Faction.ClanWolf, "Tiber (Clan)" },
            { Faction.Delphi, "New Delphi" },
            { Faction.Elysia, "Blackbone (Nyserta 3025+)" },
            { Faction.Hanse, "Bremen (HL)" },
            { Faction.JarnFolk, "Trondheim (JF)" },
            { Faction.Tortuga, "Tortuga Prime" },
            { Faction.Valkyrate, "Gotterdammerung" },
            { Faction.Axumite, "Thala" }
            //,{ Faction.WordOfBlake, "Hope (Randis 2988+)" }
        };

        private static ILookup<string, Faction> capitalsBySystemName = capitalsByFaction.ToLookup(pair => pair.Value, pair => pair.Key);
        public static bool IsCapital(StarSystem system) {
            bool isCapital = false;
            try {
                if (capitalsBySystemName.Contains(system.Name)) {
                    isCapital = true;
                }
            }
            catch (Exception ex) {
                Logger.LogError(ex);
            }
            return isCapital;
        }

        public static string GetCapital(Faction faction) {
            try {
                if (capitalsByFaction.Keys.Contains(faction)) {
                    return capitalsByFaction[faction];
                }
            }
            catch (Exception ex) {
                Logger.LogError(ex);
            }
            return null;
        }

        public static Faction GetFactionForCapital(StarSystem system) {
            try {
                if (capitalsByFaction.Values.Contains(system.Name)) {
                    return capitalsByFaction.FirstOrDefault(x => x.Value == system.Name).Key;
                }
            }
            catch (Exception ex) {
                Logger.LogError(ex);
            }
            return Faction.INVALID_UNSET;
        }

    }
}