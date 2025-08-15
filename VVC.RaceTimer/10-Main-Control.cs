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

                string commandData = null;
                var command = ParseArgument(argument, out commandData);

                if (command == RaceCenterCommands.CHECKPOINT && (updateSource & UpdateType.IGC) == UpdateType.IGC) {
                    var commData = _listener.AcceptMessage().Data as string;
                    CommandCheckpoint(commData);
                    return;
                }
                
                if (!string.IsNullOrEmpty(command)) {
                    Debug($"cmd: {command}");
                    switch (command) {
                        case RaceCenterCommands.START: CommandStart(); break;
                        case RaceCenterCommands.STOP: CommandStop(); break;
                        case RaceCenterCommands.INIT: CommandInit(); break;
                        case RaceCenterCommands.RESET: CommandReset(); break;
                        case RaceCenterCommands.CLEAR_STANDINGS: CommandClearStandings(); break;
                        case RaceCenterCommands.REMOVE_STANDING: CommandRemoveStanding(commandData); break;
                        default: Debug($"Unknown command: {command}"); break;
                    }
                }

            } finally {
                DisplayRaceInfo();
                DisplayRaceStandings();
                ShowDebugLog();
            }

        }

        string ParseArgument(string argument, out string commandData) {
            commandData = null;
            if (string.IsNullOrEmpty(argument))
                return null;

            var parts = argument.Split(new[] { ' ' }, 2);
            if (parts.Length >= 2)
                commandData = parts[1].Trim();

            return parts[0].Trim().ToLower();
        }

    }
}
