using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using VRage;
using VRage.Collections;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRageMath;

namespace IngameScript {
    public partial class Program : MyGridProgram {

        const double BLOCK_RELOAD_TIME = 10;

        const string PREVIOUS_RACE_LCD_NAME = "LCD Panel - Previous Race Info";
        const string PREVIOUS_RACE_2_LCD_NAME = "LCD Panel - Previous Race Info 2";
        const string CONNECTOR_TAG = "[VVC-RaceStart]";
        const string ACTION_RELAY_TRANSMITTER_NAME = "Action Relay - Transmitter";
        const int CHANNEL_RESET_CHECKPOINTS = 100;

        const string BLANK_SHIP_NAME = "[Name your damn ship!]";
        const int MAX_DISPLAY_SHIP_NAME_LENGTH = 19;

        readonly char[] _splitChar = new char[] { '|' };

    }
}
