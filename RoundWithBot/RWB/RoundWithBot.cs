using HarmonyLib;
using ModdingUtils.Utils;
using RarityLib.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnboundLib;
using UnityEngine;

namespace RoundWithBot.RWB
{
    public class RoundWithBot
    {
        public static List<int> botsId = new List<int>();
        public static List<CardInfo> excludeCards = new List<CardInfo>();

        private static bool Debug = true;

        private static void Log(string message, bool log = true)
        {
            if (Debug && log)
            {
                UnityEngine.Debug.Log(message);
            }
        }
        public static void AddExcludeCard(CardInfo excludeCard, bool log = true) {
           if(excludeCard == null) {
                Log("Card is null", log);
                return;
            }
            excludeCard.categories = excludeCard.categories.AddItem(RoundWithBots.NoBot).ToArray();
            excludeCards.Add(excludeCard);
            Log("'" + excludeCard.cardName+ "' Have be added to the exclude cards", log);
        }
        public static void AddExcludeCard(String excludeCardName, bool log = true)
        {
            CardInfo card = UnboundLib.Utils.CardManager.GetCardInfoWithName(excludeCardName);
            AddExcludeCard(card,log);
        }

        public static bool isAExcludeCard(CardInfo card)
        {
            if (excludeCards.Contains(card)) return true;
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

            float rarestRarityModifier = spawnCards.Select(card => RarityUtils.GetRarityData(card.GetComponent<CardInfo>().rarity).relativeRarity).Min();
            List<GameObject> rarestCards = spawnCards.Where(card => RarityUtils.GetRarityData(card.GetComponent<CardInfo>().rarity).relativeRarity == rarestRarityModifier).ToList();
            return rarestCards;
        }

        public static List<GameObject> GetSpawnCards(bool log = true)
        {
            Log("Getting spawn cards", log);
            return (List<GameObject>)AccessTools.Field(typeof(CardChoice), "spawnedCards").GetValue(CardChoice.instance);
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
            int randomIndex = UnityEngine.Random.Range(0, rarestCards.Count-1);
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
            CardChoice.instance.Pick(spawnCards[(int)CardChoice.instance.GetFieldValue("currentlySelectedCard")], true);
            yield break;
        }

        public static IEnumerator AiPickCard()
        {
            yield return new WaitUntil(() => { return CardChoice.instance.IsPicking &&
               ((List<GameObject>)CardChoice.instance.GetFieldValue("spawnedCards")).Count == ((Transform[])CardChoice.instance.GetFieldValue("children")).Count() && 
               !((List<GameObject>)CardChoice.instance.GetFieldValue("spawnedCards")).Any(card => { return card == null; }); }); //wait untill all the cards are generated
            for (int i = 0; i < PlayerManager.instance.players.Count; i++)
            {
                Player player = PlayerManager.instance.players[i];

                if (player.GetComponent<PlayerAPI>().enabled && botsId.Contains(CardChoice.instance.pickrID))
                {
                    UnityEngine.Debug.Log("AI picking card");
                    List<GameObject> spawnCards = GetSpawnCards();
                    spawnCards[0].GetComponent<CardInfo>().RPCA_ChangeSelected(true);
                    yield return new WaitForSeconds(0.25f);

                    yield return CycleThroughCards(0.30f, spawnCards);

                    yield return new WaitForSeconds(1f);

                    List<GameObject> rarestCards = GetRarestCards(spawnCards);
                    yield return GoToCards(rarestCards, spawnCards, 0.20f);
                    yield return new WaitForSeconds(1f);
                    yield return PickCard(spawnCards);
                    break;
                }
            }
            yield break;
        }
    }
}

