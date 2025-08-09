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

        static readonly string[] DIGIT_PREFIXES_13PANEL = { "Min10", "Min1", "Sec10", "Sec1" };
        Dictionary<string, IMyLightingBlock> _13PanelLights = new Dictionary<string, IMyLightingBlock>();


        static readonly string[] SEGMENT_NAMES_13PANEL = new string[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M"
        };
        static readonly bool[][] SEGMENT_PATTERNS_13PANEL = new bool[][]
        {
            new bool[] {true, true, true, true, true, true, false, true, true, true, true, true, true}, // 0
            new bool[] {false, false, true, false, true, false, false, true, false, true, false, false, true}, // 1
            new bool[] {true, true, true, false, true, true, true, true, true, false, true, true, true}, // 2
            new bool[] {true, true, true, false, true, false, true, true, false, true, true, true, true}, // 3
            new bool[] {true, false, true, true, true, true, true, true, false, true, false, false, true}, // 4
            new bool[] {true, true, true, true, false, true, true, true, false, true, true, true, true}, // 5
            new bool[] {true, true, true, true, false, true, true, true, true, true, true, true, true}, // 6
            new bool[] {true, true, true, false, true, false, false, true, false, true, false, false, true}, // 7
            new bool[] {true, true, true, true, true, true, true, true, true, true, true, true, true}, // 8
            new bool[] {true, true, true, true, true, true, true, true, false, true, true, true, true}  // 9
        };

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
                    case "start": CommandStart(); break;
                    case "stop": CommandStop(argParts[1]); break;
                    case "init":
                    case "reset": CommandReset(); break;
                    case "settime": CommandSetTime(argParts[1]); break;
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

            var parts = argument.Split('|', 2);
            //var command = parts.Length >= 1 ? parts[0].ToLower() : string.Empty;
            //var data = parts.Length >= 2 ? parts[1].ToLower() : string.Empty;
            return new string[] {
                parts.Length >= 1 ? parts[0].ToLower() : string.Empty,
                parts.Length >= 2 ? parts[1] : string.Empty
            };
        }

        void CommandStart() {
            _raceStartTime = DateTime.Now;
            _raceIsRunning = true;
            Echo("Race started!");
        }
        void CommandStop(string timeString) {
            _raceIsRunning = false;
            CommandSetTime(timeString);
            Echo("Race ended!");
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

        void InitializePanelSegments() {
            var allBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(allBlocks);

            _13PanelLights.Clear();

            foreach (var digitPrefix in DIGIT_PREFIXES_13PANEL) {
                foreach (var segmentName in SEGMENT_NAMES_13PANEL) {
                    var blockName = $"[{digitPrefix}] LightPanel-{segmentName}";
                    var light = allBlocks.OfType<IMyLightingBlock>()
                                         .FirstOrDefault(l => l.CustomName == blockName);
                    if (light != null) _13PanelLights[blockName] = light;
                }
            }
        }

        void Update13PanelLightDisplay(TimeSpan time) {
            if (_13PanelLights.Count == 0) {
                InitializePanelSegments();
                if (_13PanelLights.Count == 0) return;
            }

            var minutes = time.Minutes;
            var seconds = time.Seconds;

            var min10Digit = minutes / 10;
            var min1Digit = minutes % 10;
            var sec10Digit = seconds / 10;
            var sec1Digit = seconds % 10;

            int[] digits = { min10Digit, min1Digit, sec10Digit, sec1Digit };

            for (var i = 0; i < DIGIT_PREFIXES_13PANEL.Length; i++) {
                var digitPrefix = DIGIT_PREFIXES_13PANEL[i];
                var digit = digits[i];

                if (digit < 0 || digit > 9) digit = 0;

                var pattern = SEGMENT_PATTERNS_13PANEL[digit];

                for (var j = 0; j < SEGMENT_NAMES_13PANEL.Length; j++) {
                    var segmentName = SEGMENT_NAMES_13PANEL[j];
                    var blockName = $"[{digitPrefix}] LightPanel-{segmentName}";
                    IMyLightingBlock light;

                    if (_13PanelLights.TryGetValue(blockName, out light)) {
                        var segmentShouldBeOn = pattern[j];

                        if (segmentShouldBeOn) {
                            light.Color = Color.White;
                            light.Intensity = 5f;
                            light.Enabled = true;
                        } else {
                            light.Color = Color.Black;
                            light.Intensity = 0f;
                            light.Enabled = false;
                        }
                    }
                }
            }
        }

    }
}
