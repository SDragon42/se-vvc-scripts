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
    static class VccExtensions {
        public static string ToRaceTimeString(this TimeSpan time) {
            return $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";
        }

        public static string ToRaceTimeShortString(this TimeSpan time) {
            return (time.TotalSeconds >= 60)
                    ? $"{(int)time.TotalMinutes}:{time.Seconds:D2}.{time.Milliseconds:D3}"
                    : $"{time.Seconds}.{time.Milliseconds:D3}";
        }
    }
}
