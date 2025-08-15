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
            // { // Un-comment this block to add debugging displays
            //     var log = new DebugLogging(this);
            //     log.EchoMessages = false;
            //     Debug = (t) => log.AppendLine(t);
            //     ShowDebugLog = () => log.UpdateDisplay();
            // }

            Runtime.UpdateFrequency = UpdateFrequency.Update10;

            InitializePanelSegments();

            _listener = IGC.RegisterBroadcastListener(IGCTags.RACE_TIME_SIGN);
            _listener.SetMessageCallback(RaceTimeSignCommands.SET_TIME);

            var pbDisplay = Me.GetSurface(0);
            pbDisplay.ContentType = ContentType.TEXT_AND_IMAGE;
            pbDisplay.Alignment = TextAlignment.CENTER;
            pbDisplay.FontSize = 2f;
            pbDisplay.FontColor = new Color(0, 150, 200);
            pbDisplay.WriteText("VCC Script:\nRace Time\nSign");
        }

    }
}
