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
using SEScripts.Helpers;

public class Example : Skeleton
{
    // This scripts shows in one or more lcd panels the contents of one or more cargo containers.

    //======================================== INSTRUCTIONS =========================== 
    //  You need one trigger and one programable block to run this script continuously. Setup instructions:
    //      a) This script should be inside a programable block.
    //      b) Place a Timer block component and set it up to run every second with the following 
    //          actions in this order:
    //              - Run Programmable block (default) 
    //              - Timer block (Start) 
    //      c) Execute Trigger Now action on the Timer block to start the script
    //=================================================================================

    // Name of the container to show the contents
    public string ContainerName = "Small Cargo Container 1 ";
    // Name of the lcd to display the information's where you want to display the contents of destiny container (set text to public)
    public string LcdName = "S.HERM LCD Airlock";
    // The header of text to be shown in the lcd screens (should identify what you are displaying)
    public string Header = "Contents in Cargo Container";
    // Insert here how much of each component you would like to have in destiny container (to show a percentage)
    // Tips: This can be null if you don't want a percentage. 
    public Dictionary<string, int> ComponentDesiredQuantities = new Dictionary<string, int>
        {
            { "SteelPlate",       1000 },
            { "InteriorPlate",     500 },
            { "Motor",             500 },
            { "Construction",      500 },
            { "SmallTube",         300 },
            { "LargeTube",         250 },
            { "Computer",          200 },
            { "MetalGrid",         200 },
            { "BulletproofGlass",   30 },
            { "Display",            20 },
            { "Girder",             10 },
            { "Detector",            0 },
            { "Explosives",          0 },
            { "GravityGenerator",    0 },
            { "Medical",             0 },
            { "PowerCell",           0 },
            { "RadioCommunication",  0 },
            { "Reactor",             0 },
            { "SolarCell",           0 },
            { "SuperConductor",      0 },
            { "Thrust",              0 }
        };
    // Hint: names don't have to be unique. You can count components inside two containers xpto1 and xpto2 if in the name you type xpto

    public void Main(string argument)
    {
        ShowContainerContents.Get(GridTerminalSystem).PrintContents(LcdName, ContainerName, ComponentDesiredQuantities);
    }
}

public class ShowContainerContents
{
    private IMyGridTerminalSystem GTS { get; set; }
    public ShowContainerContents() { }
    private ShowContainerContents(IMyGridTerminalSystem gts)
    {
        GTS = gts;
    }

    public static ShowContainerContents Get(IMyGridTerminalSystem gts)
    {
        return new ShowContainerContents(gts);
    }

    public void PrintContents(string lcdName, string containerName, Dictionary<string, int> componentDesiredQuantities)
    {
        PrintResultsOnLcd(lcdName, StringifyContainerContent(containerName, componentDesiredQuantities));
    }

    public string StringifyContainerContent(string containerName, Dictionary<string, int> componentDesiredQuantities)
    {
        // Get containers 
        var containers = GridBlocksHelper.Prefixed(GTS, containerName).GetCargoContainers();
        if (containers.Count == 0)
            return "Container not found.";

        // Get items in inventories
        var itemsInDestinyInventory = CargoHelper.GetItemsInInventories(containers);

        // Build a string with the items
        var itemsString = string.Empty;
        if (componentDesiredQuantities == null || componentDesiredQuantities.Count == 0)
        {
            foreach (var item in itemsInDestinyInventory.Values)
            {
                itemsString += item.ItemName + " - " + item.Quantity + "\n";
            }
            return itemsString;
        }

        foreach (var desired in componentDesiredQuantities)
        {
            var quantity = itemsInDestinyInventory.ContainsKey(desired.Key) ? itemsInDestinyInventory[desired.Key].Quantity : 0;
            if (quantity == 0 && desired.Value == 0)
                continue;
            var percentage = getPercentage(quantity, desired.Value);

            itemsString += desired.Key + " - " + quantity + "(" + percentage + "%) " + (percentage < 100 ? "<=========" : string.Empty) + "\n";
        }

        return itemsString;
    }

    private int getPercentage(int quantity, int desired)
    {
        if (desired == 0)
            return 100;
        return (int)Math.Round((decimal)quantity / desired * 100);
    }

    public void PrintResultsOnLcd(string lcdName, string results)
    {
        //Get the lcd(s)
        var lcds = GridBlocksHelper.Prefixed(GTS, lcdName).GetLcdsPrefixed();
        if (lcds.Count == 0)
            throw new Exception(string.Format("No lcd found with name starting with {0}", lcdName));

        // Print the message on the lcd(s)
        LcdOutputHelper.ShowResultWithProgress(lcds, results);
    }
}
