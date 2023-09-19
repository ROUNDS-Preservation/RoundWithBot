using HarmonyLib;
using InControl;
using RWF.UI;
using System.Linq;
using TMPro;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RoundWithBot.Pacthes.RWF
{
    [HarmonyPatch(typeof(CharacterSelectionInstance))]
    internal class CharacterSelectionInstancePatch
    {

        [HarmonyPatch("Start")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        private static void Postfix(CharacterSelectionInstance __instance)
        {
            __instance.currentlySelectedFace = UnityEngine.Random.Range(0, 7);
        }

        [HarmonyPatch("Update")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        private static bool Prefix(CharacterSelectionInstance __instance)
        {
            if (__instance.currentPlayer == null)
            {
                return false;
            }
            

            if (__instance.currentPlayer.GetComponent<PlayerAPI>().enabled)
            {
                __instance.currentPlayer.data.playerVel.SetFieldValue("simulated", false);
                if(__instance.currentPlayer.data.playerActions == null) {
                    __instance.currentPlayer.data.playerActions = new PlayerActions();
                    __instance.currentPlayer.data.playerActions.Device = InputDevice.Null;
                }

                if(Input.GetKeyDown(KeyCode.R))
                {
                    AccessTools.Method(typeof(CharacterSelectionInstance), "ReadyUp").Invoke(__instance, null);
                    return false;
                }
            }
            return true;
        }
    }
}
