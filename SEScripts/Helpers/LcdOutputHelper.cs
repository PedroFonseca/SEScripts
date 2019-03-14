using System;
using System.Collections.Generic;
using System.Text;
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\VRage.Math.dll
//Add reference to steam\SteamApps\common\SpaceEngineers\bin64\Sandbox.Common.dll
//Only 5 game namespaces are allowed in Programmable blocks
//http://steamcommunity.com/sharedfiles/filedetails/?id=360966557
//using Sandbox.ModAPI;  // NOT AVAILABLE for Programmable blocks
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
//using Sandbox.Common.ObjectBuilders;
using VRage;
using VRage.Library;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRageMath;
using VRage.Game.ModAPI.Ingame;

namespace SEScripts.Helpers
{
    public static class LcdOutputHelper
    {
        public static void ShowResult(IMyTextPanel lcd, string message)
        {
            ShowMessageOnLcd(lcd, new LcdMessage(message, Color.White));
        }

        public static void ShowResult(List<IMyTextPanel> lcds, string message)
        {
            if (lcds == null || lcds.Count == 0)
                return;
            var msg = new LcdMessage(message, Color.White);
            foreach (var lcd in lcds)
            {
                ShowMessageOnLcd(lcd, msg);
            }
        }

        public static void ShowResultWithProgress(List<IMyTextPanel> lcds, string message, string title = "=================================")
        {
            if (lcds == null || lcds.Count == 0)
                return;

            message = title + "\n" + message + "\n  " + getTimmerChar();

            var msg = new LcdMessage(message, Color.White);
            foreach (var lcd in lcds)
            {
                ShowMessageOnLcd(lcd, msg);
            }
        }

        // Timmer is used to show something different every iteration
        static int timmer = 0;
        private static string getTimmerChar()
        {
            // Move timmer
            timmer++;

            switch (timmer)
            {
                case 1: return "\\";
                case 2: return "|";
                case 3: return "/";
                default: timmer = 0; return "-";
            }
        }

        public static void ShowMessageOnLcd(IMyTextPanel lcd, LcdMessage message)
        {
            if (lcd == null) return;

            lcd.WritePublicText(message.Text);
            lcd.ShowPublicTextOnScreen();
            lcd.SetValue<Color>("FontColor", message.FontColor);
            lcd.SetValue<Color>("BackgroundColor", message.BackgroundColor);
            lcd.SetValueFloat("FontSize", message.FontSize);
        }

        // Doesn't work
        public static void ShowMessagesOnLcd(IMyTextPanel lcd, List<LcdMessage> messages)
        {
            if (lcd == null) return;

            foreach (var message in messages)
            {
                lcd.SetValue<Color>("FontColor", message.FontColor);
                lcd.SetValue<Color>("BackgroundColor", message.BackgroundColor);
                lcd.SetValueFloat("FontSize", message.FontSize);
                lcd.WritePublicText(message.Text, true);
            }
            lcd.ShowPublicTextOnScreen();
        }

        public struct LcdMessage
        {
            public string Text { get; set; }
            public Color FontColor { get; set; }
            public Color BackgroundColor { get; set; }
            public float FontSize { get; set; }

            public LcdMessage(string text)
            {
                Text = text;
                FontColor = Color.White;
                BackgroundColor = Color.Black;
                FontSize = 1.1f;
            }

            public LcdMessage(string text, Color fontColor)
            {
                Text = text;
                FontColor = fontColor;
                BackgroundColor = Color.Black;
                FontSize = 1.1f;
            }
        }
    }
}