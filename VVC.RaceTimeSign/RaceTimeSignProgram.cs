using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
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
        private const string CHECKPOINT_SENSOR_NAME = "Checkpoint Sensor";
        private const string SIGN_COMM_CMD = "timer-sign";

        private IMySensorBlock _checkpointSensor;
        private List<MyDetectedEntityInfo> _detectedEntities = new List<MyDetectedEntityInfo>();

        private DateTime _raceStartTime;
        private bool _raceIsRunning = false;
        private bool _shipDetected = false;

        private IMyBroadcastListener _listener = null;

        private static readonly string[] DIGIT_PREFIXES_13PANEL = { "Min10", "Min1", "Sec10", "Sec1" };
        private Dictionary<string, IMyLightingBlock> _13PanelLights = new Dictionary<string, IMyLightingBlock>();


        private static readonly string[] SEGMENT_NAMES_13PANEL = new string[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M"
        };
        private static readonly bool[][] SEGMENT_PATTERNS_13PANEL = new bool[][]
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

        public Program() {
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            SetupBlocks();
            InitializePanelSegments();

            _listener = IGC.RegisterBroadcastListener(IGCTags.RACE_TIME_SIGN);
            _listener.SetMessageCallback(SIGN_COMM_CMD); // Runs this script with the argument as the message received.
        }

        public void Main(string argument, UpdateType updateSource) {
            if (argument == SIGN_COMM_CMD) {
                argument = _listener.AcceptMessage().Data as string;
            }

            if (argument == "StartRace") {
                _raceStartTime = DateTime.Now;
                _raceIsRunning = true;
                _shipDetected = false;
                Echo("Race started!");
            }

            if (argument == "EndRace") {
                _raceIsRunning = false;
                Echo("Race ended!");
            }

            if (argument == "ToggleDetected") {
                _shipDetected = !_shipDetected;
                Echo($"Ship Detected: {_shipDetected}");
            }

            if (_raceIsRunning) {
                if (_shipDetected == false) {
                    TimeSpan currentDuration = DateTime.Now - _raceStartTime;
                    Update13PanelLightDisplay(currentDuration);

                    if (_checkpointSensor != null) {
                        _checkpointSensor.DetectedEntities(_detectedEntities);
                        if (_detectedEntities.Count > 0) {
                            _shipDetected = true;
                        }
                    }
                }
            } else {
                _shipDetected = false;
            }
        }

        private void SetupBlocks() {
            _checkpointSensor = GridTerminalSystem.GetBlockWithName(CHECKPOINT_SENSOR_NAME) as IMySensorBlock;
            if (_checkpointSensor == null) { }
        }

        private void InitializePanelSegments() {
            var allBlocks = new List<IMyTerminalBlock>();
            GridTerminalSystem.GetBlocks(allBlocks);

            _13PanelLights.Clear();

            foreach (string digitPrefix in DIGIT_PREFIXES_13PANEL) {
                foreach (string segmentName in SEGMENT_NAMES_13PANEL) {
                    string blockName = $"[{digitPrefix}] LightPanel-{segmentName}";
                    IMyLightingBlock light = allBlocks.OfType<IMyLightingBlock>().FirstOrDefault(l => l.CustomName == blockName);
                    if (light != null) _13PanelLights[blockName] = light;
                }
            }
        }

        private void Update13PanelLightDisplay(TimeSpan time) {
            if (_13PanelLights.Count == 0) {
                InitializePanelSegments();
                if (_13PanelLights.Count == 0) return;
            }

            int minutes = time.Minutes;
            int seconds = time.Seconds;

            int min10Digit = minutes / 10;
            int min1Digit = minutes % 10;
            int sec10Digit = seconds / 10;
            int sec1Digit = seconds % 10;

            int[] digits = { min10Digit, min1Digit, sec10Digit, sec1Digit };

            for (int i = 0; i < DIGIT_PREFIXES_13PANEL.Length; i++) {
                string digitPrefix = DIGIT_PREFIXES_13PANEL[i];
                int digit = digits[i];

                if (digit < 0 || digit > 9) digit = 0;

                bool[] pattern = SEGMENT_PATTERNS_13PANEL[digit];

                for (int j = 0; j < SEGMENT_NAMES_13PANEL.Length; j++) {
                    string segmentName = SEGMENT_NAMES_13PANEL[j];
                    string blockName = $"[{digitPrefix}] LightPanel-{segmentName}";
                    IMyLightingBlock light;

                    if (_13PanelLights.TryGetValue(blockName, out light)) {
                        bool segmentShouldBeOn = pattern[j];

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

        private string FormatTimeSpan(TimeSpan ts) {
            int hundredths = (int)(Math.Abs(ts.Milliseconds) / 10);
            string sign = ts.TotalMilliseconds >= 0 ? "" : "-";
            TimeSpan displayTs = ts.TotalMilliseconds >= 0 ? ts : ts.Negate();
            return $"{sign}{displayTs.Minutes:D2}:{displayTs.Seconds:D2}.{hundredths:D2}";
        }

    }
}
