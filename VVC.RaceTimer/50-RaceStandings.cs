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

        class RaceStandings {
            readonly Dictionary<string, TimeSpan> _raceStandings = new Dictionary<string, TimeSpan>();

            public void Load(string data) {
                if (string.IsNullOrEmpty(data)) return;
                var lines = data.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines) {
                    var parts = line.Split('|');
                    if (parts.Length != 2) continue;

                    var racerName = parts[0];
                    if (string.IsNullOrEmpty(racerName)) continue;

                    long timeTicks;
                    if (!long.TryParse(parts[1], out timeTicks)) continue;

                    AddToStanding(racerName, new TimeSpan(timeTicks));
                }
            }
            public string Save() {
                var sb = new StringBuilder();
                foreach (var entry in _raceStandings)
                    sb.AppendLine($"{entry.Key}|{entry.Value.Ticks}");
                return sb.ToString();
            }

            public void Clear() {
                _raceStandings.Clear();
            }

            public void AddToStanding(string shipName, TimeSpan time) {
                if (_raceStandings.ContainsKey(shipName)) {
                    if (time < _raceStandings[shipName])
                        _raceStandings[shipName] = time;
                } else {
                    _raceStandings.Add(shipName, time);
                }
            }

            public IEnumerable<KeyValuePair<string, TimeSpan>> GetStandings() {
                return _raceStandings.OrderBy(entry => entry.Value);
            }

        }

    }
}