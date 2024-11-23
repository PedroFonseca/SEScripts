using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using VRage.Scripting;
using VRageMath;

namespace SEScripts.Helpers
{
    #region SpaceEngineers

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

        public static void ShowResultWithProgress(List<IMyTextPanel> lcds, string message, string title = "=================================", int timer = 0)
        {
            if (lcds == null || lcds.Count == 0)
                return;

            message = title + "\n" + message + "\n  " + getTimmerChar(timer);

            var msg = new LcdMessage(message, Color.White);
            foreach (var lcd in lcds)
            {
                ShowMessageOnLcd(lcd, msg);
            }
        }

        public static void ShowLinesWithProgress(List<IMyTextPanel> lcds, IEnumerable<string> messages, string title = "=================================", int timer = 0)
        {
            if (lcds == null || lcds.Count == 0)
                return;

            var text = title + "\n" + string.Join("\n", messages) + "\n  " + getTimmerChar(timer);

            var msg = new LcdMessage(text, Color.White);
            foreach (var lcd in lcds)
            {
                ShowMessageOnLcd(lcd, msg);
            }
        }

        public static string getTimmerChar(int timmer)
        {
            switch (timmer)
            {
                case 1: return "\\";
                case 2: return "|";
                case 3: return "/";
                default: timmer = 0; return "-";
            }
        }

        public static void ShowMessageOnLcd(IMyTextPanel lcd, LcdMessage message, bool append = false)
        {
            if (lcd == null) return;

            lcd.WriteText(message.Text, append);
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
                lcd.WriteText(message.Text+ '\n', true);
            }
        }
    }

    #endregion SpaceEngineers
}