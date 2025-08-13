// <mdk sortorder="10" />
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
        double _timeLastBlockLoad = BLOCK_RELOAD_TIME * 2; // Force reload on first run

        const string PREVIOUS_RACE_LCD_NAME = "LCD Panel - Previous Race Info";
        const string PREVIOUS_RACE_2_LCD_NAME = "LCD Panel - Previous Race Info 2";
        const string CONNECTOR_TAG = "[VVC-RaceStart]";
        const string ACTION_RELAY_TRANSMITTER_NAME = "Action Relay - Transmitter";
        const int CHANNEL_RESET_CHECKPOINTS = 100;

        const string BLANK_SHIP_NAME = "[Name your damn ship!]";


        IMyBroadcastListener _listener = null;
        readonly List<IMyTextPanel> _currentRaceDisplays = new List<IMyTextPanel>();
        readonly List<IMyTextPanel> _previousRaceDisplays = new List<IMyTextPanel>();
        readonly List<IMyTextPanel> _previousRace2Displays = new List<IMyTextPanel>();
        readonly List<IMyTextPanel> _raceStandingsDisplays = new List<IMyTextPanel>();
        readonly List<IMyShipConnector> _raceStartConnectors = new List<IMyShipConnector>();
        IMyTransponder _actionRelayTransmitter;

        Action<string> Debug;
        Action ShowDebugLog;

        // Race tracking variables
        RacerDetails _racerDetails = new RacerDetails();
        bool _updatePreviousRaceInfo = false;
        string _previousRaceInfo = string.Empty;

        RaceStandings _standings = new RaceStandings();
        bool _updateRaceStandings = true;

        readonly char[] _splitChar = new char[] { '|' };


        public Program() {
            Debug = (t) => { };
            ShowDebugLog = () => { };
            // Comment this block to remove debugging displays
            // {
            //     var log = new DebugLogging(this);
            //     log.EchoMessages = true;
            //     Debug = (t) => log.AppendLine(t);
            //     ShowDebugLog = () => log.UpdateDisplay();
            // }

            var pbDisplay = Me.GetSurface(0);
            pbDisplay.ContentType = ContentType.TEXT_AND_IMAGE;
            pbDisplay.Alignment = TextAlignment.CENTER;
            pbDisplay.FontSize = 2f;
            pbDisplay.FontColor = new Color(0, 150, 200);
            pbDisplay.WriteText("VCC Script:\nRace Timer");

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            _listener = IGC.RegisterBroadcastListener(IGCTags.CHECKPOINT);
            _listener.SetMessageCallback(RaceCenterCommands.CHECKPOINT);

            _standings.Load(Storage);

            CommandReset();
        }

        public void Save() {
            Storage = _standings.Save();
        }

        private void LoadBlocks() {
            var reloadBlocks = _timeLastBlockLoad >= BLOCK_RELOAD_TIME;
            if (!reloadBlocks)
                return;

            _timeLastBlockLoad = 0;

            GridTerminalSystem.GetBlocksOfType(_currentRaceDisplays, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_CurrentRaceInfo));
            GridTerminalSystem.GetBlocksOfType(_previousRaceDisplays, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_PreviousRaceInfo1));
            GridTerminalSystem.GetBlocksOfType(_previousRace2Displays, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_PreviousRaceInfo2));
            GridTerminalSystem.GetBlocksOfType(_raceStandingsDisplays, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_RaceStandings));
            GridTerminalSystem.GetBlocksOfType(_raceStartConnectors, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_StartConnector));

            _actionRelayTransmitter = GridTerminalSystem.GetBlockOfTypeWithFirst<IMyTransponder>(b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_ActionRelayTransmitter));
        }

    }
}
