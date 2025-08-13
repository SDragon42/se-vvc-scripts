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

        DateTime _raceStartTime;
        bool _raceIsRunning = false;

        IMyBroadcastListener _listener = null;

        readonly char[] _separator = new char[] { '|' };

        Action<string> Debug;
        Action ShowDebugLog;


        public Program() {
            Debug = (t) => { };
            ShowDebugLog = () => { };
            // Comment this block to remove debugging displays
            // {
            //     var log = new DebugLogging(this);
            //     log.EchoMessages = true;
            //     Debug = (t) => log.AppendLine(t);
            //     ShowDebugLog = () => log.UpdateDisplay();
            // }

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            InitializePanelSegments();

            _listener = IGC.RegisterBroadcastListener(IGCTags.RACE_TIME_SIGN);
            _listener.SetMessageCallback(RaceTimeSignCommands.SET_TIME);
        }

    }
}
