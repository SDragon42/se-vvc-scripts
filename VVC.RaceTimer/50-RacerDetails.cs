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
        
        class RacerDetails {
            const int DefaultMaxCheckpointNameLength = 4;

            public string RacerShipName { get; private set; } = null;
            public long StartTimeTicks { get; private set; } = 0;
            public long EndTimeTicks { get; private set; } = 0;
            public bool IsRaceActive { get; private set; } = false;

            private long _lastCheckpointTicks = 0;
            public List<CheckpointLogEntry> CheckpointLog { get; private set; } = new List<CheckpointLogEntry>(100);
            public int MaxCheckpointNameLength { get; private set; } = DefaultMaxCheckpointNameLength; // default for no values


            private long CurrentTimeTicks => IsRaceActive ? DateTime.Now.Ticks : EndTimeTicks;
            public TimeSpan RaceDuration => TimeSpan.FromTicks(CurrentTimeTicks - StartTimeTicks);


            public void Initialize(string racerShipName = null) {
                RacerShipName = racerShipName;
                StartTimeTicks = 0;
                EndTimeTicks = 0;
                _lastCheckpointTicks = 0;
                IsRaceActive = false;
                CheckpointLog.Clear();
            }

            public void Start() {
                if (IsRaceActive)
                    return;
                StartTimeTicks = DateTime.Now.Ticks;
                EndTimeTicks = StartTimeTicks;
                _lastCheckpointTicks = StartTimeTicks;
                IsRaceActive = true;
                CheckpointLog.Clear();
            }

            public void Stop() {
                if (!IsRaceActive)
                    return;
                EndTimeTicks = DateTime.Now.Ticks;
                AddCheckpoint("FNSH", EndTimeTicks);
                IsRaceActive = false;
            }

            public void AddCheckpoint(string checkpointName, long checkpointTimeTicks) {
                if (!IsRaceActive)
                    return;
                var ticksFromStart = checkpointTimeTicks - StartTimeTicks;
                var ticksFromLastCheckpoint = checkpointTimeTicks - _lastCheckpointTicks;
                var entry = new CheckpointLogEntry(checkpointName, ticksFromStart, ticksFromLastCheckpoint);
                CheckpointLog.Add(entry);
                MaxCheckpointNameLength = Math.Max(MaxCheckpointNameLength, entry.Name.Length);
                _lastCheckpointTicks = checkpointTimeTicks;
            }

        }

        class CheckpointLogEntry {
            public string Name { get; private set; }
            public TimeSpan TimeFromStart { get; private set; }
            public TimeSpan TimeFromLastCheckpoint { get; private set; }

            public CheckpointLogEntry(string name, long ticksFromStart, long ticksFromLastCheckpoint) {
                Name = name;
                TimeFromStart = TimeSpan.FromTicks(ticksFromStart);
                TimeFromLastCheckpoint = TimeSpan.FromTicks(ticksFromLastCheckpoint);
            }
        }

    }
}