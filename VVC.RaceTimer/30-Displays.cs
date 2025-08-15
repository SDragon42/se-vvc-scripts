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

        private void DisplayRaceInfo() {
            var text = BuildRaceInfoText();
            WriteToAllDisplays(_currentRaceDisplays, "\nCurrent Race\n\n" + text);
            if (_updatePreviousRaceInfo) {
                _updatePreviousRaceInfo = false;
                WriteToAllDisplays(_previousRaceDisplays, "\nPrevious Race\n\n" + text);
                if (!string.IsNullOrEmpty(_previousRaceInfo)) {
                    WriteToAllDisplays(_previousRace2Displays, "\nPrevious Race 2\n\n" + _previousRaceInfo);
                }
                _previousRaceInfo = text;
            }
        }

        private string BuildRaceInfoText() {
            var duration = _racerDetails.RaceDuration;

            var text = new StringBuilder();
            text.AppendLine($"Ship: {_racerDetails.RacerShipName}\n");
            if (_racerDetails.IsRaceActive || duration.Ticks > 0) {
                var header = "".PadRight(_racerDetails.MaxCheckpointNameLength) + "     Split       Total";
                text.AppendLine(header);
            }

            foreach (var entry in _racerDetails.CheckpointLog) {
                text.AppendLine(ChkLine(entry.Name, _racerDetails.MaxCheckpointNameLength, entry.TimeFromLastCheckpoint, entry.TimeFromStart));
            }

            if (_racerDetails.IsRaceActive) {
                text.AppendLine(ChkLine("", _racerDetails.MaxCheckpointNameLength, null, duration));
            } else if (duration.Ticks > 0) {
                text.AppendLine($"\nFinal Time : {duration.ToRaceTimeString()}");
            }

            return text.ToString();
        }

        static string ChkLine(string name, int maxNameLength, TimeSpan? split, TimeSpan total) {
            return $"{name.PadRight(maxNameLength)}{split?.ToRaceTimeString() ?? "",12}{total.ToRaceTimeString(),12}";
        }

        private void WriteToAllDisplays(IEnumerable<IMyTextPanel> displays, string text) {
            foreach (var surface in displays) {
                surface.WriteText(text);
            }
        }

        long _standingsTextHash = 0;
        private void DisplayRaceStandings() {
            if (_raceStandingsDisplays.Count == 0) return;

            var update = _updateRaceStandings || _raceStandingsDisplays.Any(d => d.GetText().GetHashCode() != _standingsTextHash);
            if (!update) return;

            var sb = new StringBuilder();
            sb.AppendLine("         Race Standings");
            sb.AppendLine("--------------------------------");

            var i = 0;
            foreach (var entry in _standings.GetStandings()) {
                i++;

                var name = entry.Key.Substring(0, Math.Min(entry.Key.Length, MAX_DISPLAY_SHIP_NAME_LENGTH));
                name = name.PadRight(MAX_DISPLAY_SHIP_NAME_LENGTH, '.');

                var time = entry.Value.ToRaceTimeString();

                sb.AppendLine($"{i,2} {name} {time}");
            }

            var text = sb.ToString();
            _standingsTextHash = text.GetHashCode();
            _updateRaceStandings = false;
            WriteToAllDisplays(_raceStandingsDisplays, text);
        }

    }
}