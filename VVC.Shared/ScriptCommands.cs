// <mdk sortorder="500" />
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

        static class RaceCenterCommands {
            public const string START = "start";
            public const string STOP = "stop";
            public const string INIT = "init";
            public const string RESET = "reset";
            public const string CHECKPOINT = "checkpoint";

        }

        static class RaceTimeSignCommands {
            public const string START = "start";
            public const string STOP = "stop";
            public const string INIT = "init";
            public const string RESET = "reset";
            public const string SET_TIME = "settime";

        }

    }
}