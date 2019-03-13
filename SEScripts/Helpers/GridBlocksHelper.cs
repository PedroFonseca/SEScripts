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
    public class GridBlocksHelper
    {
        public string Prefix { get; private set; }
        private IMyGridTerminalSystem GTS { get; set; }
        public GridBlocksHelper() { }
        private GridBlocksHelper(IMyGridTerminalSystem gts, string prefix)
        {
            Prefix = prefix;
            GTS = gts;
        }

        public static GridBlocksHelper Get(IMyGridTerminalSystem gts, string prefix)
        {
            return new GridBlocksHelper(gts, prefix);
        }

        private bool NameStartsWithPrefix(IMyTerminalBlock block)
        {
            if (string.IsNullOrEmpty(Prefix))
                return true;
            return block.CustomName.StartsWith(Prefix) && block.IsFunctional;
        }

        private bool NameEqualsPrefix(IMyTerminalBlock block)
        {
            if (string.IsNullOrEmpty(Prefix))
                return true;
            return block.CustomName.Equals(Prefix) && block.IsFunctional;
        }

        private List<T> ConvertToListOf<T>(List<IMyTerminalBlock> list) where T : class
        {
            var result = new List<T>();
            foreach (var elem in list)
            {
                result.Add(elem as T);
            }
            return result;
        }

        private List<T> GetBlocksOfTypeStartsWithPrefix<T>() where T : class
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<T>(aux, NameStartsWithPrefix);
            return ConvertToListOf<T>(aux);
        }

        private List<T> GetBlocksOfTypeByName<T>() where T : class
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<T>(aux, NameStartsWithPrefix);
            return ConvertToListOf<T>(aux);
        }

        public List<IMyTextPanel> GetLcdsPrefixed()
        {
            return GetBlocksOfTypeStartsWithPrefix<IMyTextPanel>();
        }

        public IMyMotorStator GetRotor()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyMotorStator>(aux, NameStartsWithPrefix);

            if (aux == null || aux.Count == 0)
            {
                throw new NullReferenceException(string.Format("Could not find any rotor with name starting by {0}.", Prefix));
            }
            else if (aux.Count > 1)
            {
                throw new NullReferenceException(string.Format("Multiple rotors were found with name starting by {0}. Make sure you have only one.", Prefix));
            }
            return aux[0] as IMyMotorStator;
        }

        public List<IMyTerminalBlock> GetSolarPanels()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMySolarPanel>(aux, NameStartsWithPrefix);
            if (aux == null || aux.Count == 0)
            {
                throw new NullReferenceException(string.Format("Could not find any solar panel with name starting by {0}.", Prefix));
            }
            return aux;
        }

        public List<IMyTerminalBlock> GetBatteries()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyBatteryBlock>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetReactors()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyReactor>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetCargoContainers()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyCargoContainer>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetAssemblers()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyAssembler>(aux, NameStartsWithPrefix);
            return aux;
        }

        public List<IMyTerminalBlock> GetRefineries()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<IMyRefinery>(aux, NameStartsWithPrefix);
            return aux;
        }
    }
}