using BepInEx.Configuration;
using ModdingUtils;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnboundLib;
using UnboundLib.Utils.UI;
using UnityEngine;

namespace RoundWithBot
{
    

    internal class ConfigHandler
    {
        public static ConfigEntry<bool> RandomizationFace;
        public static ConfigEntry<int> SelectedFace;
        public static ConfigEntry<bool> DebugMode;

        public static void RegesterMenu(string modName, ConfigFile config)
        {
            Unbound.RegisterMenu(modName, () => { }, NewGui, null, false);
            RandomizationFace = config.Bind(modName, "RandomizationFace", true, "Enable randomization of bots' faces.");
            SelectedFace = config.Bind(modName, "SelectedFace", 0, "Set the index for bots to select a specific face.");
            DebugMode = config.Bind(modName, "DebugMode", false, "Enabled or disabled Debug Mode");
        }

        public static void addBlank(GameObject menu)
        {
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 30);
        }

        public static void NewGui(GameObject menu)
        {
            MenuHandler.CreateText("Rounds With Bots | Config", menu, out TextMeshProUGUI _, 60);
            addBlank(menu);
            void RandomizationFaceChanged(bool val)
            {
                RandomizationFace.Value = val;
            }
            MenuHandler.CreateToggle(RandomizationFace.Value, "Randomization Face", menu, RandomizationFaceChanged, 30);
            addBlank(menu);
            MenuHandler.CreateText("Selected Face | <#c41010> Only Work When 'Randomization Face' Config Is Off!", menu, out TextMeshProUGUI _, 30);
            addBlank(menu);
            void SelectedFaceChanged(float val)
            {
                SelectedFace.Value = (int)Math.Round(val);
            }
            MenuHandler.CreateSlider("Selected Face", menu, 30, 0, 7, SelectedFace.Value, SelectedFaceChanged, out UnityEngine.UI.Slider _, true);
            addBlank(menu);
            void DebugModeChanged(bool val)
            {
                DebugMode.Value = val;
            }
            MenuHandler.CreateToggle(DebugMode.Value, "<#c41010> Debug Mode", menu, DebugModeChanged, 30);
        }
    }
}
