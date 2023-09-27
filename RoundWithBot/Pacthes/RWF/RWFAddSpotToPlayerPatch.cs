using HarmonyLib;
using RWF.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace RoundWithBot.Pacthes.RWF {

    [HarmonyPatch(typeof(PlayerSpotlight))]
    internal class RWFAddSpotToPlayerPatch {
        [HarmonyPatch("AddSpotToPlayer")]
        
        private static bool Prefix(Player player) 
        {
            
            return !player.GetComponent<PlayerAPI>().enabled;
        }
    }
}
