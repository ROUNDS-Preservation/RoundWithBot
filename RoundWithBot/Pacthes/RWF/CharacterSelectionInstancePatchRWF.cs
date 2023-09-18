using HarmonyLib;
using InControl;
using System.Linq;
using UnboundLib;
using UnityEngine;

namespace RoundWithBot.Pacthes.RWF
{
    [HarmonyPatch(typeof(CharacterSelectionInstance))]
    internal class CharacterSelectionInstancePatch
    {
        [HarmonyPatch("Update")]
        [HarmonyBefore("io.olavim.rounds.rwf")]
        private static bool Prefix(CharacterSelectionInstance __instance)
        {
            if (__instance.currentPlayer == null)
            {
                return false;
            }
            if (__instance.currentPlayer.GetComponent<PlayerAPI>().enabled && !__instance.isReady)
            {
                __instance.currentPlayer.data.playerVel.SetFieldValue("simulated", false);
                if(__instance.currentPlayer.data.playerActions == null) {
                    __instance.currentPlayer.data.playerActions = new PlayerActions();
                    __instance.currentPlayer.data.playerActions.Device = InputDevice.Null;
                }
                __instance.currentlySelectedFace = 7; //lets players use the last face as their bot face
                UnityEngine.Debug.Log("Finding FaceSelector");
                Transform faceSelector = __instance.transform.GetChild(0);
                UnityEngine.Debug.Log("Found FaceSelector. Finding Grid");
                Transform grid = faceSelector.GetChild(0);
                Debug.Log("Found Grid. Finding faceSelector_Buttons");

                foreach (Transform child in grid)
                {
                    GameObject childObject = child.gameObject;

                    if (childObject.name != "FaceSelector_Button")
                    {
                        childObject.SetActive(false);
                    }
                    else if (childObject.name == "FaceSelector_Button")
                    {
                        Transform Locked = child.Find("LOCKED");
                        Locked.gameObject.SetActive(true);
                    }
                }
                AccessTools.Method(typeof(CharacterSelectionInstance), "ReadyUp").Invoke(__instance, null);
                return false;
            }
            return true;
        }
    }
}
