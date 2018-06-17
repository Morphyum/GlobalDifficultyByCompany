using Harmony;
using System.Reflection;

namespace GlobalDifficultyByCompany
{
    public class GlobalDifficultyByCompany
    {
        internal static string ModDirectory;
        public static void Init(string directory, string settingsJSON) {
            var harmony = HarmonyInstance.Create("de.morphyum.AdjustedMechSalvage");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            ModDirectory = directory;
        }
    }
}
