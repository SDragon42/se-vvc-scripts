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
        class RacerDetails {
            public string RacerShipName { get; private set; }
            public long StartTimeTicks { get; private set; }
            public long EndTimeTicks { get; private set; }
            public bool IsRaceActive { get; private set; }
            public Queue<CheckpointLogEntry> CheckpointLog { get; } = new Queue<CheckpointLogEntry>(100);


            private long CurrentTimeTicks => IsRaceActive ? DateTime.Now.Ticks : EndTimeTicks;
            public TimeSpan RaceDuration => TimeSpan.FromTicks(CurrentTimeTicks - StartTimeTicks);

            private long _lastCheckpointTime;


            public void Initialize(string racerShipName = null) {
                RacerShipName = racerShipName;
                StartTimeTicks = 0;
                EndTimeTicks = 0;
                _lastCheckpointTime = 0;
                IsRaceActive = false;
                CheckpointLog.Clear();
            }

            public void Start() {
                if (IsRaceActive)
                    return;
                StartTimeTicks = DateTime.Now.Ticks;
                EndTimeTicks = StartTimeTicks;
                _lastCheckpointTime = StartTimeTicks;
                IsRaceActive = true;
                CheckpointLog.Clear();
            }

            public void Stop() {
                if (!IsRaceActive)
                    return;
                EndTimeTicks = DateTime.Now.Ticks;
                IsRaceActive = false;
            }

            public void AddCheckpoint(string checkpointName, long checkpointTimeTicks) {
                var ticksFromStart = checkpointTimeTicks - StartTimeTicks;
                var ticksFromLastCheckpoint = checkpointTimeTicks - _lastCheckpointTime;
                var entry = new CheckpointLogEntry(checkpointName, ticksFromStart, ticksFromLastCheckpoint);
                CheckpointLog.Enqueue(entry);
                _lastCheckpointTime = checkpointTimeTicks;
            }

        }
    }
}