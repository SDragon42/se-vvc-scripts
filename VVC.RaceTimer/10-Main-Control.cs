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

        public void Main(string argument, UpdateType updateSource) {
            _timeLastBlockLoad += Runtime.TimeSinceLastRun.TotalSeconds;

            try {
                LoadConfig();
                LoadBlocks();

                if (argument == RaceCenterCommands.CHECKPOINT && (updateSource & UpdateType.IGC) == UpdateType.IGC) {
                    var commData = _listener.AcceptMessage().Data as string;
                    CommandCheckpoint(commData);
                    return;
                }

                argument = argument.ToLower();
                if (!string.IsNullOrEmpty(argument)) {
                    Debug($"cmd: {argument}");
                    switch (argument) {
                        case RaceCenterCommands.START: CommandStart(); break;
                        case RaceCenterCommands.STOP: CommandStop(); break;
                        case RaceCenterCommands.INIT: CommandInit(); break;
                        case RaceCenterCommands.RESET: CommandReset(); break;
                        case RaceCenterCommands.CLEAR_STANDINGS: CommandClearStandings(); break;
                        default: Debug($"Unknown command: {argument}"); break;
                    }
                }

            } finally {
                DisplayRaceInfo();
                DisplayRaceStandings();
                ShowDebugLog();
            }

        }

    }
}
