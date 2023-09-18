using HarmonyLib;
using ModdingUtils.Utils;
using RarityLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoundWithBot.RWB
{
    public class RoundWithBot
    {
        public static List<int> botsId = new List<int>();
        public static List<String> excludeCards = new List<String>();

        private static bool Debug = true;

        private static void Log(string message, bool log = true)
        {
            if (Debug && log)
            {
                UnityEngine.Debug.Log(message);
            }
        }

        public static void AddExcludeCard(String excludeCardName, bool log = true)
        {
            excludeCards.Add(excludeCardName.ToUpper());
            Log("'" + excludeCardName + "' Have be added to the exclude cards", log);
        }

        public static bool isAExcludeCard(CardInfo card)
        {
            if (excludeCards.Contains(card.cardName.ToUpper())) return true;
            return false;
        }

        public static void SetBotsId(bool log = true)
        {
            Log("Getting bots player.", log);
            botsId.Clear();
            for (int i = 0; i < PlayerManager.instance.players.Count; i++)
            {
                Player player = PlayerManager.instance.players[i];
                if (player.GetComponent<PlayerAPI>().enabled)
                {
                    botsId.Add(player.playerID);
                    Log("Bot '" + player.playerID + "' Have be added to the list of bots id.", log);
                }
            }
            Log("Successfully get list of bots player.", log);
        }

        public static List<GameObject> GetRarestCards(List<GameObject> spawnCards, bool log = true)
        {
            Log("getting rarest cards...", log);
            List<GameObject> spawnedCards = GetSpawnCards();

            float rarestRarityModifier = float.MaxValue; // Initialize with the highest possible value
            List<GameObject> rarestCards = new List<GameObject>();
            CardInfo lastCardInfo = null;
            int index = 0;

            foreach (var cardObject in spawnedCards)
            {
                CardInfo cardInfo = cardObject.GetComponent<CardInfo>();

                if (cardInfo != null)
                {
                    Log("Checking is '" + cardInfo.cardName + "' more rare is last card.", log);
                    float cardRarityModifier = RarityUtils.GetRarityData(cardInfo.rarity).relativeRarity;
                    if (isAExcludeCard(cardInfo))
                    {
                        Log("'" + cardInfo.cardName + "' Is a exclude card. Skiping card", log);
                        continue;
                    }
                    if (cardRarityModifier < rarestRarityModifier)
                    {
                        // Found a card with a higher rarity modifier, clear the list and update the rarest rarity modifier
                        Log("'" + cardInfo.cardName + "' Is more rare then last card. clearing list.", log);
                        rarestCards.Clear();
                        rarestRarityModifier = cardRarityModifier;
                    }
                    else
                    {
                        Log("'" + cardInfo.cardName + "' Is not more rare then last card.", log);
                    }

                    if (cardRarityModifier == rarestRarityModifier)
                    {
                        // Found a card with the highest rarity modifier, add it to the list
                        Log("Adding '" + cardInfo.cardName + "' to the list.", log);
                        rarestCards.Add(cardObject);
                    }
                }
                lastCardInfo = cardInfo;
                index++;
            }
            int randomIndex = UnityEngine.Random.Range(0, spawnCards.Count);
            if (rarestCards.Count == 0) rarestCards.Add(spawnCards[randomIndex].gameObject);
            Log("Successfully get list of rarest cards.", log);
            return rarestCards;
        }

        public static List<GameObject> GetSpawnCards(bool log = true)
        {
            Log("Getting spawn cards", log);
            return (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);
        }

        public static IEnumerator WaitForOneSpawnCards(bool log = true)
        {
            Log("Waiting for one spawn cards to spawn...", log);
            List<GameObject> spawnedCards = GetSpawnCards();
            do
            {
                spawnedCards = GetSpawnCards(false);
                yield return null; // Wait for the next frame before checking again
            }

            while (spawnedCards.Count <= 1);
            Log("One spawn cards spawn", log);
            yield break;
        }

        public static IEnumerator WaitForSpawnCards(bool log = true)
        {
            Log("Waiting for all spawn cards to spawn...", log);
            List<GameObject> spawnedCards = GetSpawnCards();
            do
            {
                spawnedCards = GetSpawnCards(false);
                yield return null; // Wait for the next frame before checking again
            }

            while (spawnedCards.Count != CardChoice.instance.transform.childCount);
            Log("All spawn cards spawn", log);
            yield break;
        }

        public static IEnumerator CycleThroughCards(float delay, List<GameObject> spawnedCards, bool log = true)
        {
            Log("Cycling through cards", log);

            CardInfo lastCardInfo = null;
            int index = 0;

            foreach (var cardObject in spawnedCards)
            {
                CardInfo cardInfo = cardObject.GetComponent<CardInfo>();

                Log("Cycling through '" + cardInfo.cardName + "' card", log);
                if (lastCardInfo != null)
                {
                    lastCardInfo.RPCA_ChangeSelected(false);
                }
                cardInfo.RPCA_ChangeSelected(true);
                AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(CardChoice.instance, index);

                lastCardInfo = cardInfo;
                index++;
                yield return new WaitForSeconds(delay);
            }
            Log("Successfully gone through all cards");
            yield break;
        }

        public static IEnumerator GoToCards(List<GameObject> rarestCards, List<GameObject> spawnedCards, float delay, bool log = true)
        {
            int randomIndex = UnityEngine.Random.Range(0, rarestCards.Count);
            GameObject cardToPick = rarestCards[randomIndex];
            Log("Going to '" + cardToPick + "' card", log);

            // Set currentlySelectedCard to the index of the selected card within the spawnedCards list
            int selectedCardIndex = spawnedCards.IndexOf(cardToPick);
            int handIndex = int.Parse(AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").GetValue(CardChoice.instance).ToString());

            while (handIndex != selectedCardIndex)
            {
                CardInfo cardInfo = spawnedCards[handIndex].GetComponent<CardInfo>();
                cardInfo.RPCA_ChangeSelected(false);
                Log("Currently on '" + cardInfo + "' card", log);
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
                AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").SetValue(CardChoice.instance, handIndex);

                // Wait for some time before the next iteration
                yield return new WaitForSeconds(delay); // Adjust the time as needed
            }
            Log("Successfully got to '" + cardToPick + "' card", log);
            yield break;
        }

        public static IEnumerator PickCard(List<GameObject> spawnCards)
        {
            try
            {
                CardChoice.instance.Pick(spawnCards[int.Parse(AccessTools.Field(typeof(CardChoice), "currentlySelectedCard").GetValue(CardChoice.instance).ToString())], true);
            }
            catch (Exception err)
            {
                UnityEngine.Debug.Log(err);
            }

            UIHandler.instance.StopShowPicker();
            CardChoiceVisuals.instance.Hide();

            CardChoice.instance.IsPicking = false;

            yield return new WaitForSeconds(0.25f); // Adjust timing as needed

            CardChoice.instance.picks = 0;

            CardChoice.instance.pickrID++;

            yield break;
        }

        public static IEnumerator AiPickCard()
        {
            SetBotsId();
            for (int i = 0; i < PlayerManager.instance.players.Count; i++)
            {
                Player player = PlayerManager.instance.players[i];

                if (player.GetComponent<PlayerAPI>().enabled && botsId.Contains(CardChoice.instance.pickrID))
                {
                    UnityEngine.Debug.Log("AI picking card");

                    yield return WaitForOneSpawnCards();
                    List<GameObject> spawnCards = GetSpawnCards();
                    spawnCards[0].GetComponent<CardInfo>().RPCA_ChangeSelected(true);

                    yield return WaitForSpawnCards();
                    yield return new WaitForSeconds(0.25f);

                    spawnCards = GetSpawnCards();

                    yield return CycleThroughCards(0.30f, spawnCards);

                    yield return new WaitForSeconds(1f);

                    List<GameObject> rarestCards = GetRarestCards(spawnCards);
                    yield return GoToCards(rarestCards, spawnCards, 0.20f);
                    yield return new WaitForSeconds(3f);
                    yield return PickCard(spawnCards);
                }
            }
            yield break;
        }
    }
}

