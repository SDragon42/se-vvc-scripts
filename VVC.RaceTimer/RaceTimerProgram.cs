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
        readonly List<IMyShipConnector> _raceStartConnectors = new List<IMyShipConnector>();
        IMyTransponder _actionRelayTransmitter;
        readonly RunningSymbol _runningModule = new RunningSymbol();
        readonly Logging _log = new Logging();

        Action<string> Debug;
        Action ShowDebugLog;

        // Race tracking variables
        RacerDetails _racerDetails = new RacerDetails();
        bool _updatePreviousRaceInfo = false;
        string _previousRaceInfo = string.Empty;
        bool _doNotRun = false;

        readonly char[] _splitChar = new char[] { '|' };


        public Program() {
            Debug = Echo;
            ShowDebugLog = () => { };

            // Comment these lines to remove debugging displays
            // var log = new DebugLogging(this);
            // log.EchoMessages = true;
            // Debug = (t) => log.AppendLine($"{DateTime.Now:mm:ss.fff}> {t}");
            // ShowDebugLog = () => log.UpdateDisplay();

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            _listener = IGC.RegisterBroadcastListener(IGCTags.CHECKPOINT);
            _listener.SetMessageCallback(RaceCenterCommands.CHECKPOINT);

            CommandReset();
        }

        public void Main(string argument, UpdateType updateSource) {
            _timeLastBlockLoad += Runtime.TimeSinceLastRun.TotalSeconds;
            Echo($"VVC Race Timer {_runningModule.GetSymbol()}");

            try {
                LoadConfig();
                LoadBlocks();

                if (_doNotRun) return;

                MainLoop(argument, updateSource);

            } finally {
                Echo(_log.GetLogText());
                DisplayRaceInfo();
                ShowDebugLog();
            }

        }

        private void LoadBlocks() {
            var reloadBlocks = _timeLastBlockLoad >= BLOCK_RELOAD_TIME;
            if (!_doNotRun && !reloadBlocks)
                return;

            _doNotRun = false;
            _timeLastBlockLoad = 0;

            GridTerminalSystem.GetBlocksOfType(_currentRaceDisplays, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_CurrentRaceInfo));
            GridTerminalSystem.GetBlocksOfType(_previousRaceDisplays, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_PreviousRaceInfo1));
            GridTerminalSystem.GetBlocksOfType(_previousRace2Displays, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_PreviousRaceInfo2));
            GridTerminalSystem.GetBlocksOfType(_raceStartConnectors, b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_StartConnector));

            _actionRelayTransmitter = GridTerminalSystem.GetBlockOfTypeWithFirst<IMyTransponder>(b => b.IsSameConstructAs(Me) && Collect.IsTagged(b, _tag_ActionRelayTransmitter));

            _log.Clear();
            _log.AppendLine("Required Blocks...");
            _log.AppendLine($"  Action Relay: {(_actionRelayTransmitter != null ? "Found" : "NOT FOUND!!")}");
            _log.AppendLine($"  Race Start Connectors: {_raceStartConnectors.Count}");
            if (_actionRelayTransmitter == null || _raceStartConnectors.Count == 0) {
                _log.AppendLine($"\nMissing Required Blocks!");
                _doNotRun = true;
            }
            _log.AppendLine();

            _log.AppendLine("Other Blocks...");
            _log.AppendLine($"  Current Race Displays: {_currentRaceDisplays.Count}");
            _log.AppendLine($"  Previous Race Displays 1: {_previousRaceDisplays.Count}");
            _log.AppendLine($"  Previous Race Displays 2: {_previousRace2Displays.Count}");
            _log.AppendLine();
        }

        void MainLoop(string argument, UpdateType updateSource) {
            if (argument == RaceCenterCommands.CHECKPOINT && (updateSource & UpdateType.IGC) == UpdateType.IGC) {
                var commData = _listener.AcceptMessage().Data as string;
                CommandCheckpoint(commData);
                return;
            }

            argument = argument.ToLower();
            if (!string.IsNullOrEmpty(argument)) {
                Debug($"cmd: {argument}");
                switch (argument) {
                    case RaceCenterCommands.START: CommandStart(); break;
                    case RaceCenterCommands.STOP: CommandStop(); break;
                    case RaceCenterCommands.INIT: CommandInit(); break;
                    case RaceCenterCommands.RESET: CommandReset(); break;
                    default: Debug($"Unknown command: {argument}"); break;
                }
            }
        }

        void CommandReset() {
            _racerDetails.Initialize();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, RaceTimeSignCommands.RESET);
            _actionRelayTransmitter?.SendSignal(CHANNEL_RESET_CHECKPOINTS);
        }
        void CommandInit() {
            if (_racerDetails.IsRaceActive) {
                Debug("Race is currently running");
                return;
            }
            var shipName = GetShipName();
            _racerDetails.Initialize(shipName);
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, RaceTimeSignCommands.INIT);
            _actionRelayTransmitter?.SendSignal(CHANNEL_RESET_CHECKPOINTS);
        }
        void CommandStart() {
            if (_racerDetails.IsRaceActive) {
                Debug("Race is currently running");
                return;
            }
            _racerDetails.Start();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, $"{RaceTimeSignCommands.START}|{_racerDetails.StartTimeTicks}");
        }
        void CommandCheckpoint(string commsData) {
            if (!_racerDetails.IsRaceActive) {
                Debug("Race is not currently running");
                return;
            }

            if (string.IsNullOrEmpty(commsData)) {
                Debug("Invalid checkpoint data received.");
                return;
            }

            var parts = commsData.Split(_splitChar, 2);
            if (parts.Length != 2) {
                Debug($"Invalid checkpoint data received: {commsData}");
                return;
            }

            var name = parts[0];

            long ticks;
            if (!long.TryParse(parts[1], out ticks)) {
                Debug($"Invalid ticks value: {parts[1]}");
            }

            _racerDetails.AddCheckpoint(name, ticks);
        }
        void CommandStop() {
            if (!_racerDetails.IsRaceActive) {
                Debug("Race is not currently running");
                return;
            }
            _racerDetails.Stop();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, $"{RaceTimeSignCommands.STOP}|{_racerDetails.RaceDuration}");
            _updatePreviousRaceInfo = true;
        }

        string GetShipName() {
            if (_raceStartConnectors.Count == 0) return null;
            var connector = _raceStartConnectors.FirstOrDefault(b => b.Status == MyShipConnectorStatus.Connected);
            if (connector == null) return null;
            var shipName = connector.OtherConnector?.CubeGrid.CustomName;
            return !string.IsNullOrEmpty(shipName)
                ? shipName
                : BLANK_SHIP_NAME;
        }

        private void DisplayRaceInfo() {
            var message = BuildRaceInfoText();
            WriteToAllDisplays(_currentRaceDisplays, message);
            if (_updatePreviousRaceInfo) {
                _updatePreviousRaceInfo = false;
                WriteToAllDisplays(_previousRaceDisplays, "Previous Race\n\n" + message);
                if (!string.IsNullOrEmpty(_previousRaceInfo))
                    WriteToAllDisplays(_previousRace2Displays, "Previous Race 2\n\n" + _previousRaceInfo);
                _previousRaceInfo = message;
            }
        }
        private string BuildRaceInfoText() {
            var text = new StringBuilder();

            var timeString = _racerDetails.RaceDuration.ToRaceTimeString();
            if (_racerDetails.RacerShipName != null)
                text.AppendLine($"Ship: {_racerDetails.RacerShipName}");
            text.AppendLine($"Time: {timeString}");

            if (_racerDetails.CheckpointLog.Count > 0) {
                text.AppendLine();
                var maxNameLength = _racerDetails.CheckpointLog.Max(entry => entry.Name.Length);
                var header = "".PadRight(maxNameLength) + "     Split       Total";
                text.AppendLine(header);
                foreach (var entry in _racerDetails.CheckpointLog) {
                    var timeFromStart = entry.TimeFromStart.ToRaceTimeString();
                    var timeFromLastCheckpoint = entry.TimeFromLastCheckpoint.ToRaceTimeString();
                    text.AppendLine($"{entry.Name.PadRight(maxNameLength)} | {timeFromLastCheckpoint} | {timeFromStart} |");
                }
            }

            return text.ToString();
        }

        private void WriteToAllDisplays(IEnumerable<IMyTextPanel> displays, string text) {
            foreach (IMyTextSurface surface in displays) {
                surface.WriteText(text);
            }
        }

    }
}
