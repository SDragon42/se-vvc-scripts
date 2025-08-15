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

        void CommandReset() {
            _racerDetails.Initialize();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, RaceTimeSignCommands.RESET);
            _actionRelayTransmitter?.SendSignal(CHANNEL_RESET_CHECKPOINTS);
        }

        void CommandInit() {
            if (_racerDetails.IsRaceActive) {
                Debug("Race is currently running");
                return;
            }
            var shipName = RetrieveConnectedShipName();
            _racerDetails.Initialize(shipName,
                                     _standings.GetTimeForShip(shipName));
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, RaceTimeSignCommands.INIT);
            _actionRelayTransmitter?.SendSignal(CHANNEL_RESET_CHECKPOINTS);
        }

        void CommandStart() {
            if (_racerDetails.IsRaceActive) {
                Debug("Race is currently running");
                return;
            }
            _racerDetails.Start();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, $"{RaceTimeSignCommands.START}|{_racerDetails.StartTimeTicks}");
        }

        void CommandCheckpoint(string commsData) {
            if (!_racerDetails.IsRaceActive) {
                Debug("Race is not currently running");
                return;
            }

            if (string.IsNullOrEmpty(commsData)) {
                Debug("Invalid checkpoint data received.");
                return;
            }

            var parts = commsData.Split(_splitChar, 2);
            if (parts.Length != 2) {
                Debug($"Invalid checkpoint data received: {commsData}");
                return;
            }

            var name = parts[0];

            long ticks;
            if (!long.TryParse(parts[1], out ticks)) {
                Debug($"Invalid ticks value: {parts[1]}");
                return;
            }

            _racerDetails.AddCheckpoint(name, ticks);
        }

        void CommandStop() {
            if (!_racerDetails.IsRaceActive) {
                Debug("Race is not currently running");
                return;
            }
            _racerDetails.Stop();
            IGC.SendBroadcastMessage(IGCTags.RACE_TIME_SIGN, $"{RaceTimeSignCommands.STOP}|{_racerDetails.RaceDuration}");
            _updatePreviousRaceInfo = true;

            _standings.AddToStanding(_racerDetails.RacerShipName, _racerDetails.RaceDuration);
            _updateRaceStandings = true;
        }

        void CommandClearStandings() {
            _standings.Clear();
            _updateRaceStandings = true;
        }


        string RetrieveConnectedShipName() {
            if (_raceStartConnectors.Count == 0)
                return null;

            var connector = _raceStartConnectors.FirstOrDefault(b => b.Status == MyShipConnectorStatus.Connected);
            if (connector == null)
                return null;

            var shipName = connector.OtherConnector?.CubeGrid.CustomName;
            return !string.IsNullOrEmpty(shipName)
                ? shipName
                : BLANK_SHIP_NAME;
        }

    }
}
