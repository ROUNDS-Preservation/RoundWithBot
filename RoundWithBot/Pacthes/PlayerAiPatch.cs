using HarmonyLib;
using System;
using System.Reflection;
using UnboundLib;
using UnityEngine;

namespace RoundWithBot.Pacthes
{
    [HarmonyPatch(typeof(PlayerAIPhilip))]
    internal class PlayerAiPatch
    {
        [HarmonyPatch("Update")]
        private static void Postfix(PlayerAIPhilip __instance)
        {
            // Find an instance of OutOfBoundsHandler in the scene
            OutOfBoundsHandler outOfBoundsHandlerInstance = GameObject.FindObjectOfType<OutOfBoundsHandler>();

            if (outOfBoundsHandlerInstance == null)
            {
                // Handle the case where the component is not found
                UnityEngine.Debug.LogError("OutOfBoundsHandler not found in the scene.");
                return;
            }
            
            MethodInfo getPointMethod = AccessTools.Method(typeof(OutOfBoundsHandler), "GetPoint");
            GeneralInput input = (GeneralInput)AccessTools.Field(typeof(PlayerAPI), "input").GetValue(__instance.GetComponentInParent<PlayerAPI>());

            // Invoke the GetPoint method on the outOfBoundsHandlerInstance
            Vector3 bound = (Vector3)getPointMethod.Invoke(outOfBoundsHandlerInstance, new object[] { __instance.gameObject.transform.position });

            // Calculate the absolute differences between the bot's position and the boundaries
            float diffX = Mathf.Abs(__instance.gameObject.transform.position.x - bound.x);
            float diffY = Mathf.Abs(__instance.gameObject.transform.position.y - bound.y);

            // Define the maximum allowed distance from the boundaries
            float maxDistance = 1.0f;

            // Check if the bot is near the boundaries but not inside them
            bool isNearBoundaries = (diffX <= maxDistance || diffY <= maxDistance) && (diffX >= maxDistance || diffY >= maxDistance);

            if (isNearBoundaries)
            {
                RWB.RoundWithBot.Log("Bot Is Near Boundaries");
                input.shieldWasPressed = true;
            }
        }
    }
}
