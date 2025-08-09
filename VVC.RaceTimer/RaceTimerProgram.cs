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


        // Do not change these values, they are used by the script.
        //////////////////////////////////////////////////////////////////////

        // Command values
        const string CMD_START = "start";
        const string CMD_STOP = "stop";
        const string CMD_RESET = "reset";
        const string CMD_CHECKPOINT = "checkpoint";



        IMyBroadcastListener _listener = null;
        readonly Queue<string> _checkpointLog = new Queue<string>(100);
        readonly List<IMyTextPanel> _displaySurfaces = new List<IMyTextPanel>();
        readonly RunningSymbol _runningModule = new RunningSymbol();
        readonly DebugLogging _debug;

        // Race tracking variables
        long _startTime;
        long _currentTime;
        bool _isRaceActive;

        readonly char[] _splitChar = new char[] { '|' };


        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            _debug = new DebugLogging(this);

            _listener = IGC.RegisterBroadcastListener(IGCTags.CHECKPOINT);
            _listener.SetMessageCallback(CMD_CHECKPOINT); // Runs this script with the argument as the message received.

            GridTerminalSystem.GetBlocksOfType(_displaySurfaces, surface => surface.CustomName == LCD_PANEL_NAME);

            CommandReset(); // Reset the timer on startup.
        }

        public void Main(string argument, UpdateType updateSource) {
            Echo($"VVC Race Timer {_runningModule.GetSymbol()}");
            try {
                switch (argument.ToLower()) {
                    case "": break;
                    case CMD_START: CommandStart(); break;
                    case CMD_STOP: CommandStop(); break;
                    case CMD_RESET: CommandReset(); break;
                    case CMD_CHECKPOINT: CommandCheckpoint(); break;
                }
            } finally {
                DisplayRaceInfo();
                _debug.UpdateDisplay();
            }

        }

        void CommandStart() {
            _startTime = DateTime.Now.Ticks;
            _currentTime = _startTime;
            _isRaceActive = true;
            _checkpointLog.Clear();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, "start", TransmissionDistance.AntennaRelay);
        }

        void CommandStop() {
            _currentTime = DateTime.Now.Ticks;
            _isRaceActive = false;
            var action = "stop|" + CalculateElapsedTime(GetRaceTimeTicks()).ToString();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, action, TransmissionDistance.AntennaRelay);
        }

        void CommandReset() {
            _startTime = DateTime.Now.Ticks;
            _currentTime = _startTime;
            _isRaceActive = false;
            _checkpointLog.Clear();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, "reset", TransmissionDistance.AntennaRelay);
        }

        void CommandCheckpoint() {
            var commsData = SplitValue(_listener.AcceptMessage().Data as string);
            if (commsData.Length < 2)
                return;

            long ticks;
            if (long.TryParse(commsData[1], out ticks)) {
                var logMessage = $"{commsData[0]}: {CalculateElapsedTime(ticks).ToRaceTimeString()}";
                _checkpointLog.Enqueue(logMessage);
            }
        }

        string[] SplitValue(string argument) {
            if (string.IsNullOrEmpty(argument)) return new string[] { string.Empty };
            return argument.Split(_splitChar, 2);
        }




        private void DisplayRaceInfo() {
            var message = new StringBuilder();
            var elapsedTime = CalculateElapsedTime(GetRaceTimeTicks()).ToRaceTimeString();
            message.AppendLine($"Time: {elapsedTime}");
            message.AppendLine();
            message.AppendLines(_checkpointLog);
            WriteToAllDisplays(message.ToString());
        }
        private void WriteToAllDisplays(string text) {
            foreach (IMyTextSurface surface in _displaySurfaces) {
                surface.WriteText(text);
            }
        }

        private long GetRaceTimeTicks() => _isRaceActive ? DateTime.Now.Ticks : _currentTime;
        private TimeSpan CalculateElapsedTime(long currentTicks) => new TimeSpan(currentTicks - _startTime);

    }
}
