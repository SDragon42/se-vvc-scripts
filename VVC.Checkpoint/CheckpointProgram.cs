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

        // Do not change these values, they are used by the script.
        //////////////////////////////////////////////////////////////////////
        
        Action<string> Debug;
        Action ShowDebugLog;

        public Program() {
            Debug = Echo;
            ShowDebugLog = () => { };

            // Comment these lines to remove debugging displays
            // var log = new DebugLogging(this);
            // log.EchoMessages = true;
            // Debug = (t) => log.AppendLine(t);
            // ShowDebugLog = () => log.UpdateDisplay();
        }

        public void Main(string argument, UpdateType updateSource) {
            var message = $"{argument}|{DateTime.Now.Ticks}";
            IGC.SendBroadcastMessage(IGCTags.CHECKPOINT, message);
            Debug($">>: {message}");
            ShowDebugLog();
        }
    }
}
