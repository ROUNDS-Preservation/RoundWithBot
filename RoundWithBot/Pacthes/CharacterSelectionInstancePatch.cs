using HarmonyLib;

namespace RoundWithBot.Pacthes
{
    [HarmonyPatch(typeof(CharacterSelectionInstance))]
    internal class CharacterSelectionInstancePatch
    {
        [HarmonyPatch("Update")]
        private static bool Prefix(CharacterSelectionInstance __instance)
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("io.olavim.rounds.rwf")) return true;
            if (__instance.currentPlayer == null)
            {
                return false;
            }
            if (__instance.currentPlayer.GetComponent<PlayerAPI>().enabled && !__instance.isReady)
            {
                AccessTools.Method(typeof(CharacterSelectionInstance), "ReadyUp").Invoke(__instance, null);
                return false;
            }
            return true;
        }
    }
}
