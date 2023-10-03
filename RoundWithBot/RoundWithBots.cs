using BepInEx;
using HarmonyLib;
using System.Collections;
using System.Collections.Generic;
using ModdingUtils;
using ModdingUtils.Extensions;
using TMPro;
using RWF.UI;
using Photon.Realtime;
using UnityEngine;
using BepInEx.Configuration;

namespace RoundWithBot
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.pickncards", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("io.olavim.rounds.rwf", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class RoundWithBots : BaseUnityPlugin
    {
        private const string ModId = "com.aalund13.rounds.Round_With_Bot";
        private const string ModName = "Round With Bot";
        public const string Version = "2.3.0"; // What version are we on (major.minor.patch)?
        public const string ModInitials = "RWB";
        public static CardCategory NoBot;

        public static RoundWithBots instance { get; private set; }
        public bool isPicking = false;
        private List<int> botPlayer = new List<int>();



        void Awake()
        {
            // Use this to call any harmony patch files your mod may have
            var harmony = new Harmony(ModId);
            harmony.PatchAll();
        }
        void Start()
        {
            instance = this;

            ConfigHandler.RegesterMenu(ModName, Config);

            NoBot = CardChoiceSpawnUniqueCardPatch.CustomCategories.CustomCardCategories.instance.CardCategory("not-for-bots");
            RWB.RoundWithBot.AddExcludeCard("Remote");
            
            UnboundLib.GameModes.GameModeManager.AddHook(UnboundLib.GameModes.GameModeHooks.HookPlayerPickStart,(_)=> BotPicks());
            UnboundLib.GameModes.GameModeManager.AddHook(UnboundLib.GameModes.GameModeHooks.HookGameStart,(_)=> RegesterBots());
            
        }
        
        IEnumerator RegesterBots() {
            botPlayer.Clear();
            for(int i = 0; i < PlayerManager.instance.players.Count; i++) {
                Player player = PlayerManager.instance.players[i];
                if(player.GetComponent<PlayerAPI>().enabled) {
                    botPlayer.Add(player.playerID);
                    player.data.stats.GetAdditionalData().blacklistedCategories.Add(NoBot);
                    player.GetComponentInChildren<PlayerName>().GetComponent<TextMeshProUGUI>().text = "<#07e0f0>[BOT]";
                }
            }
            RWB.RoundWithBot.SetBotsId();
            yield break;
        }
        IEnumerator BotPicks() {
            StartCoroutine(RWB.RoundWithBot.AiPickCard());
            
            yield break;
        }
    }
}