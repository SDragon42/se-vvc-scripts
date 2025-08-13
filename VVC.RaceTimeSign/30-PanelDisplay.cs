// <mdk sortorder="20" />
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
    partial class Program {

        static readonly string[] DIGIT_PREFIXES_13PANEL = { "Min10", "Min1", "Sec10", "Sec1" };
        readonly Dictionary<string, IMyLightingBlock[]> _13PanelLights = new Dictionary<string, IMyLightingBlock[]>();
        static readonly string[] SEGMENT_NAMES_13PANEL = new string[]
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M"
        };
        static readonly bool[][] SEGMENT_PATTERNS_13PANEL = new bool[][]
        {
            /*0*/ new bool[] {true, true, true, true, true, true, false, true, true, true, true, true, true},
            /*1*/ new bool[] {false, false, true, false, true, false, false, true, false, true, false, false, true},
            /*2*/ new bool[] {true, true, true, false, true, true, true, true, true, false, true, true, true},
            /*3*/ new bool[] {true, true, true, false, true, false, true, true, false, true, true, true, true},
            /*4*/ new bool[] {true, false, true, true, true, true, true, true, false, true, false, false, true},
            /*5*/ new bool[] {true, true, true, true, false, true, true, true, false, true, true, true, true},
            /*6*/ new bool[] {true, true, true, true, false, true, true, true, true, true, true, true, true},
            /*7*/ new bool[] {true, true, true, false, true, false, false, true, false, true, false, false, true},
            /*8*/ new bool[] {true, true, true, true, true, true, true, true, true, true, true, true, true},
            /*9*/ new bool[] {true, true, true, true, true, true, true, true, false, true, true, true, true}
        };


        void InitializePanelSegments() {
            var allPanelBlocks = new List<IMyLightingBlock>();
            GridTerminalSystem.GetBlocksOfType(allPanelBlocks);

            _13PanelLights.Clear();

            foreach (var digitPrefix in DIGIT_PREFIXES_13PANEL) {
                foreach (var segmentName in SEGMENT_NAMES_13PANEL) {
                    var blockName = $"[{digitPrefix}] LightPanel-{segmentName}";
                    var lights = allPanelBlocks.Where(l => l.CustomName == blockName).ToArray();
                    if (lights.Length > 0) _13PanelLights[blockName] = lights;
                }
            }
        }

        void Update13PanelLightDisplay(TimeSpan time) {
            if (_13PanelLights.Count == 0) {
                InitializePanelSegments();
                if (_13PanelLights.Count == 0) {
                    Debug("No 13-panel lights found. Please ensure the blocks are named correctly.");
                    return;
                }
            }

            var min10Digit = time.Minutes / 10;
            var min1Digit = time.Minutes % 10;
            var sec10Digit = time.Seconds / 10;
            var sec1Digit = time.Seconds % 10;

            int[] digits = { min10Digit, min1Digit, sec10Digit, sec1Digit };

            for (var i = 0; i < DIGIT_PREFIXES_13PANEL.Length; i++) {
                var digitPrefix = DIGIT_PREFIXES_13PANEL[i];
                var digit = digits[i];

                if (digit < 0 || digit > 9) digit = 0;

                var pattern = SEGMENT_PATTERNS_13PANEL[digit];

                for (var j = 0; j < SEGMENT_NAMES_13PANEL.Length; j++) {
                    var segmentName = SEGMENT_NAMES_13PANEL[j];
                    var blockName = $"[{digitPrefix}] LightPanel-{segmentName}";
                    IMyLightingBlock[] lights;

                    if (_13PanelLights.TryGetValue(blockName, out lights)) {
                        var segmentShouldBeOn = pattern[j];

                        foreach (var light in lights) {
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
}