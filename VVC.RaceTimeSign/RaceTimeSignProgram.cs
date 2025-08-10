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
        const string SIGN_COMM_CMD = "timer-sign";

        List<MyDetectedEntityInfo> _detectedEntities = new List<MyDetectedEntityInfo>();

        DateTime _raceStartTime;
        bool _raceIsRunning = false;

        IMyBroadcastListener _listener = null;




        readonly char[] _separator = new char[] { '|' };

        readonly RunningSymbol _runningModule = new RunningSymbol();
        Action<string> Debug;
        Action ShowDebugLog;

        public Program() {
            Debug = Echo;
            ShowDebugLog = () => { };

            // Comment these lines to remove debugging displays
            // var log = new DebugLogging(this);
            // log.EchoMessages = true;
            // Debug = (t) => log.AppendLine(t);
            // ShowDebugLog = () => log.UpdateDisplay();

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            InitializePanelSegments();

            _listener = IGC.RegisterBroadcastListener(IGCTags.RACE_TIME_SIGN);
            _listener.SetMessageCallback(SIGN_COMM_CMD); // Runs this script with the argument as the message received.
        }

        public void Main(string argument, UpdateType updateSource) {
            Echo($"VVC Race Time Sign {_runningModule.GetSymbol()}");
            try {
                var argParts = ProcessArgument(ref argument);

                switch (argParts[0]) {
                    case RaceTimeSignCommands.START: CommandStart(); break;
                    case RaceTimeSignCommands.STOP: CommandStop(argParts[1]); break;
                    case RaceTimeSignCommands.INIT:
                    case RaceTimeSignCommands.RESET: CommandReset(); break;
                    case RaceTimeSignCommands.SET_TIME: CommandSetTime(argParts[1]); break;
                }

                if (_raceIsRunning) {
                    var currentDuration = DateTime.Now - _raceStartTime;
                    Update13PanelLightDisplay(currentDuration); 
                }
            } finally {
                ShowDebugLog();
            }

        }

        private string[] ProcessArgument(ref string argument) {
            if (argument == SIGN_COMM_CMD) {
                argument = _listener.AcceptMessage().Data as string;
                Debug($"cmd: {argument}");
            }

            var parts = argument.Split(_separator, 2);
            return new string[] {
                parts.Length >= 1 ? parts[0].ToLower() : string.Empty,
                parts.Length >= 2 ? parts[1] : string.Empty
            };
        }

        void CommandStart() {
            _raceStartTime = DateTime.Now;
            _raceIsRunning = true;
            Debug("Race started!");
        }
        void CommandStop(string timeString) {
            _raceIsRunning = false;
            CommandSetTime(timeString);
            Debug("Race ended!");
        }
        void CommandReset() {
            _raceIsRunning = false;
            Update13PanelLightDisplay(TimeSpan.Zero);
        }
        void CommandSetTime(string timeString) {
            if (string.IsNullOrWhiteSpace(timeString))
                return;
            TimeSpan time;
            if (TimeSpan.TryParse(timeString, out time)) {
                Update13PanelLightDisplay(time);
            }
        }

    }
}
