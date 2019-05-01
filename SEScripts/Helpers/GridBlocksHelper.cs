using System;
using System.Linq;
using System.Collections.Generic;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;

namespace SEScripts.Helpers
{
    #region SpaceEngineers

    public class GridBlocksHelper
    {
        private IMyGridTerminalSystem GTS { get; set; }

        private GridBlocksHelper(IMyGridTerminalSystem gts)
        {
            GTS = gts;
        }

        public static GridBlocksHelper Get(IMyGridTerminalSystem gts)
        {
            return new GridBlocksHelper(gts);
        }

        public List<IMyTerminalBlock> GetBlocks()
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocks(aux);
            return aux;
        }

        public List<IMyTerminalBlock> GetGroupBlocks(string groupName)
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlockGroupWithName(groupName)?.GetBlocks(aux);
            return aux;
        }

        public List<T> GetBlocksOfTypeByName<T>(string prefix) where T : class
        {
            var aux = new List<IMyTerminalBlock>();
            GTS.GetBlocksOfType<T>(aux, (t) => t.CustomName.StartsWith(prefix));
            return aux.Select(t => t as T).ToList();
        }

        public List<IMyCargoContainer> GetCargoContainers(string prefix)
        {
            return GetBlocksOfTypeByName<IMyCargoContainer>(prefix);
        }

        //public List<IMyTextPanel> GetLcdsPrefixed()
        //{
        //    return GetBlocksOfTypeStartsWithPrefix<IMyTextPanel>();
        //}

        //public IMyMotorStator GetRotor()
        //{
        //    var aux = new List<IMyTerminalBlock>();
        //    GTS.GetBlocksOfType<IMyMotorStator>(aux, NameStartsWithPrefix);

        //    if (aux == null || aux.Count == 0)
        //    {
        //        throw new NullReferenceException(string.Format("Could not find any rotor with name starting by {0}.", Prefix));
        //    }
        //    else if (aux.Count > 1)
        //    {
        //        throw new NullReferenceException(string.Format("Multiple rotors were found with name starting by {0}. Make sure you have only one.", Prefix));
        //    }
        //    return aux[0] as IMyMotorStator;
        //}

        //public List<IMyTerminalBlock> GetSolarPanels()
        //{
        //    var aux = new List<IMyTerminalBlock>();
        //    GTS.GetBlocksOfType<IMySolarPanel>(aux, NameStartsWithPrefix);
        //    if (aux == null || aux.Count == 0)
        //    {
        //        throw new NullReferenceException(string.Format("Could not find any solar panel with name starting by {0}.", Prefix));
        //    }
        //    return aux;
        //}

        //public List<IMyTerminalBlock> GetBatteries()
        //{
        //    var aux = new List<IMyTerminalBlock>();
        //    GTS.GetBlocksOfType<IMyBatteryBlock>(aux, NameStartsWithPrefix);
        //    return aux;
        //}

        //public List<IMyTerminalBlock> GetReactors()
        //{
        //    var aux = new List<IMyTerminalBlock>();
        //    GTS.GetBlocksOfType<IMyReactor>(aux, NameStartsWithPrefix);
        //    return aux;
        //}

        //public List<IMyTerminalBlock> GetCargoContainersWithException()
        //{
        //    var aux = new List<IMyTerminalBlock>();
        //    GTS.GetBlocksOfType<IMyCargoContainer>(aux, NameIsNotException);
        //    return aux;
        //}

        //public List<IMyTerminalBlock> GetAssemblers()
        //{
        //    var aux = new List<IMyTerminalBlock>();
        //    GTS.GetBlocksOfType<IMyAssembler>(aux, NameStartsWithPrefix);
        //    return aux;
        //}

        //public List<IMyTerminalBlock> GetRefineries()
        //{
        //    var aux = new List<IMyTerminalBlock>();
        //    GTS.GetBlocksOfType<IMyRefinery>(aux, NameStartsWithPrefix);
        //    return aux;
        //}

    }

    #endregion SpaceEngineers
}