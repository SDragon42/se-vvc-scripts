// <mdk sortorder="15" />
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
                    text.AppendLine($"{entry.Name.PadRight(maxNameLength)}   {timeFromLastCheckpoint}   {timeFromStart}");
                }
            }

            return text.ToString();
        }

        private void WriteToAllDisplays(IEnumerable<IMyTextPanel> displays, string text) {
            foreach (var surface in displays) {
                surface.WriteText(text);
            }
        }

        const int MAX_DISPLAY_SHIP_NAME_LENGTH = 19;

        long _standingsTextHash = 0;
        private void DisplayRaceStandings(bool forceUpdate = false) {
            if (_raceStandingsDisplays.Count == 0) return;

            forceUpdate |= _raceStandingsDisplays.Any(d => d.GetText().GetHashCode() != _standingsTextHash);

            if (!forceUpdate) return;

            var sb = new StringBuilder();
            sb.AppendLine("         Race Standings");
            sb.AppendLine(new string('-', 32));

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
            WriteToAllDisplays(_raceStandingsDisplays, text);
        }

    }
}