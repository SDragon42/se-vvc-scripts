// <mdk sortorder="10" />
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

        void CommandStart(string startTimeTicks) {
            long startTicks;
            _raceStartTime = long.TryParse(startTimeTicks, out startTicks)
                ? new DateTime(startTicks)
                : DateTime.Now;
            _raceIsRunning = true;
            Debug("Race started!");
        }

        void CommandStop(string timeString) {
            _raceIsRunning = false;
            CommandSetTime(timeString);
            Debug("Race ended!");
        }

        void CommandReset() {
            _raceIsRunning = false;
            Update13PanelLightDisplay(TimeSpan.Zero);
        }
        
        void CommandSetTime(string timeString) {
            if (string.IsNullOrWhiteSpace(timeString))
                return;
            TimeSpan time;
            if (TimeSpan.TryParse(timeString, out time)) {
                Update13PanelLightDisplay(time);
            }
        }

    }
}
