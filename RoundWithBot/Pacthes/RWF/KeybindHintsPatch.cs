using HarmonyLib;
using UnityEngine;
using RWF.UI;
using TMPro;
using RWF;

namespace RoundWithBot.Pacthes.RWF
{
    // Create a class to define your patch
    [HarmonyPatch(typeof(KeybindHints))]
    internal class KeybindHintsPatch
    {
        // The original method you want to patch
        [HarmonyPatch("CreateLocalHints")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        private static void Postfix()
        {
            if (PlayerPrefs.GetInt(RWFMod.GetCustomPropertyKey("ShowKeybinds"), 1) != 0)
            {
                KeybindHints.AddHint("to ready up all bots", "[R]");
            }
        }
    }
}
