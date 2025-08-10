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

        // Configuration values
        const string LCD_PANEL_NAME = "LCD Panel - Race Info";

        const int MAX_CHECKPOINT_NAME_LENGTH = 4;


        // Do not change these values, they are used by the script.
        //////////////////////////////////////////////////////////////////////

        // Command values
        // const string CMD_START = "start";
        // const string CMD_STOP = "stop";
        // const string CMD_INIT = "init";
        // const string CMD_RESET = "reset";
        // const string CMD_CHECKPOINT = "checkpoint";



        IMyBroadcastListener _listener = null;
        readonly List<IMyTextPanel> _displaySurfaces = new List<IMyTextPanel>();
        readonly RunningSymbol _runningModule = new RunningSymbol();
        Action<string> Debug;
        Action ShowDebugLog;

        // Race tracking variables
        RacerDetails _racerDetails = new RacerDetails();

        readonly char[] _splitChar = new char[] { '|' };
        readonly string CheckpointTimeHeader = "".PadRight(MAX_CHECKPOINT_NAME_LENGTH) + "    Last CP      Total";


        public Program() {
            Debug = Echo;
            ShowDebugLog = () => { };

            // Comment these lines to remove debugging displays
            // var log = new DebugLogging(this);
            // log.EchoMessages = true;
            // Debug = (t) => log.AppendLine(t);
            // ShowDebugLog = () => log.UpdateDisplay();

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            _listener = IGC.RegisterBroadcastListener(IGCTags.CHECKPOINT);
            _listener.SetMessageCallback(RaceCenterCommands.CHECKPOINT); // Runs this script with the argument as the message received.

            GridTerminalSystem.GetBlocksOfType(_displaySurfaces, surface => surface.CustomName == LCD_PANEL_NAME);

            CommandReset(); // Reset the timer on startup.
        }

        public void Main(string argument, UpdateType updateSource) {
            Echo($"VVC Race Timer {_runningModule.GetSymbol()}");
            try {
                argument = argument.ToLower();
                Debug($"cmd: {argument}");
                switch (argument) {
                    case "": break;
                    case RaceCenterCommands.START: CommandStart(); break;
                    case RaceCenterCommands.STOP: CommandStop(); break;
                    case RaceCenterCommands.INIT:
                    case RaceCenterCommands.RESET: CommandReset(); break;
                    case RaceCenterCommands.CHECKPOINT: CommandCheckpoint(); break;
                }
            } finally {
                DisplayRaceInfo();
                ShowDebugLog();
            }

        }

        void CommandStart() {
            _racerDetails.Start();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, RaceTimeSignCommands.START);
        }

        void CommandStop() {
            _racerDetails.Stop();
            var action = $"{RaceTimeSignCommands.STOP}|{_racerDetails.RaceDuration}";
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, action);
        }

        void CommandReset() {
            _racerDetails.Initialize();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, RaceTimeSignCommands.RESET);
        }

        void CommandCheckpoint() {
            var commsData = SplitValue(_listener.AcceptMessage().Data as string);
            if (commsData.Length < 2) {
                Debug("Invalid checkpoint data received.");
                return;
            }

            long ticks;
            if (long.TryParse(commsData[1], out ticks)) {
                _racerDetails.AddCheckpoint(commsData[0], ticks);
            } else {
                Debug($"Invalid ticks value: {commsData[1]}");
            }
        }

        string[] SplitValue(string argument) {
            if (string.IsNullOrEmpty(argument)) return new string[] { string.Empty };
            return argument.Split(_splitChar, 2);
        }


        private void DisplayRaceInfo() {
            var message = BuildRaceInfoText();
            WriteToAllDisplays(message);
        }
        private string BuildRaceInfoText() {
            var text = new StringBuilder();

            var timeString = _racerDetails.RaceDuration.ToRaceTimeString();
            text.AppendLine($"Time: {timeString}");

            if (_racerDetails.CheckpointLog.Count > 0) {
                text.AppendLine();
                text.AppendLine(CheckpointTimeHeader);
                foreach (var entry in _racerDetails.CheckpointLog) {
                    var timeFromStart = entry.TimeFromStart.ToRaceTimeString();
                    var timeFromLastCheckpoint = entry.TimeFromLastCheckpoint.ToRaceTimeString();
                    text.AppendLine($"{entry.Name} | {timeFromLastCheckpoint} | {timeFromStart} |");
                }
            }

            return text.ToString();
        }

        private void WriteToAllDisplays(string text) {
            foreach (IMyTextSurface surface in _displaySurfaces) {
                surface.WriteText(text);
            }
        }

    }
}
