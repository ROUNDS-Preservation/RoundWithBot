using BepInEx;
using HarmonyLib;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using static CardInfo;

namespace SimplyCards
{
    // These are the mods required for our mod to work
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.pickncards", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    // Declares our mod to Bepin
    [BepInPlugin(ModId, ModName, Version)]
    // The game our mod is associated with
    [BepInProcess("Rounds.exe")]
    public class RoundWithBots : BaseUnityPlugin
    {
        private const string ModId = "com.aalund13.rounds.Round_With_Bot";
        private const string ModName = "Round With Bot";
        public const string Version = "1.0.0"; // What version are we on (major.minor.patch)?
        public const string ModInitials = "RWB";

        
        public static RoundWithBots instance { get; private set; }
        private bool isPicking = false;
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
        }

        private IEnumerator ChooseCardWithDelay()
        {
            yield return new WaitForSeconds(0.2f);
            List<GameObject> spawnedCards = (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);
            spawnedCards[0].GetComponent<CardInfo>().RPCA_ChangeSelected(true);
            yield return new WaitForSeconds(DrawNCards.DrawNCards.NumDraws / 5);

            CardChoice cardChoice = CardChoice.instance;
            spawnedCards = (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);

            for (int i = 0; i < PlayerManager.instance.players.Count; i++)
            {
                Player player = PlayerManager.instance.players[i];

                if (player.GetComponent<PlayerAPI>().enabled && botPlayer.Contains(cardChoice.pickrID))
                {
                    UnityEngine.Debug.Log("AI Picks Card");
                    // Do Pick
                    if (GM_ArmsRace.instance != null && GM_ArmsRace.instance.p2Rounds != 0 && GM_ArmsRace.instance.p1Rounds != 0)
                    {
                        AccessTools.Field(typeof(CardChoice), "pickerType").SetValue(CardChoice.instance, PickerType.Team);
                    }



                    Rarity rarestRarity = Rarity.Common; // Initialize with the lowest rarity
                    List<GameObject> rarestCards = new List<GameObject>();
                    CardInfo lastCardInfo = null;
                    int index = 0;
                    foreach (var cardObject in spawnedCards)
                    {
                        CardInfo cardInfo = cardObject.GetComponent<CardInfo>();

                        if (cardInfo != null)
                        {
                            Rarity cardRarity = cardInfo.rarity;
                            if (lastCardInfo  != null)
                            {
                                lastCardInfo.RPCA_ChangeSelected(false);
                            }
                            cardInfo.RPCA_ChangeSelected(true);
                            AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(cardChoice, index);
                            if (cardRarity > rarestRarity)
                            {
                                // Found a card with a higher rarity, clear the list and update the rarest rarity
                                rarestCards.Clear();
                                rarestRarity = cardRarity;
                            }

                            if (cardRarity == rarestRarity)
                            {
                                // Found a card with the rarest rarity, add it to the list
                                rarestCards.Add(cardObject);
                            }
                        }
                        lastCardInfo = cardInfo;
                        index++;
                        yield return new WaitForSeconds(0.35f);
                    }
                    

                    int randomIndex = UnityEngine.Random.Range(0, rarestCards.Count);
                    GameObject cardToPick = rarestCards[randomIndex];


                    // Set currentlySelectedCard to the index of the selected card within the spawnedCards list
                    int selectedCardIndex = spawnedCards.IndexOf(cardToPick);
                    int handIndex = int.Parse(AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").GetValue(cardChoice).ToString());

                    while (handIndex != selectedCardIndex)
                    {
                        CardInfo cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                        cardInfo.RPCA_ChangeSelected(false);
                        if (handIndex > selectedCardIndex)
                        {
                            handIndex--;
                        }
                        else if (handIndex < selectedCardIndex)
                        {
                            handIndex++;
                        }
                        cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                        cardInfo.RPCA_ChangeSelected(true);
                        AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(cardChoice, handIndex);

                        // Wait for some time before the next iteration
                        yield return new WaitForSeconds(0.25f); // Adjust the time as needed
                    }

                    

                    yield return new WaitForSeconds(3f); // Adjust timing as needed

                    try
                    {
                        cardChoice.Pick(cardToPick, true);
                    }
                    catch (Exception err)
                    {
                        UnityEngine.Debug.Log(err);
                    }

                    UIHandler.instance.StopShowPicker();
                    CardChoiceVisuals.instance.Hide();

                    cardChoice.IsPicking = false;

                    yield return new WaitForSeconds(0.25f); // Adjust timing as needed

                    cardChoice.picks = 0;

                    cardChoice.pickrID++;

                    yield break; // Exit the loop after both players have picked
                }
            }
        }

        void Update()
        {
            botPlayer.Clear();
            for (int i = 0; i < PlayerManager.instance.players.Count; i++)
            {
                Player player = PlayerManager.instance.players[i];
                if (player.GetComponent<PlayerAPI>().enabled) 
                {
                    botPlayer.Add(player.playerID);
                }
            }
            if (GM_ArmsRace.instance != null) GM_ArmsRace.instance.roundsToWinGame = 100;
            if (CardChoice.instance != null && CardChoice.instance.IsPicking == true && isPicking == false)
            {
                isPicking = true;

                StartCoroutine(ChooseCardWithDelay());
            }
            else if (CardChoice.instance != null && CardChoice.instance.IsPicking == false && isPicking == true)
            {
                isPicking = false;
            }
        }
    }
}