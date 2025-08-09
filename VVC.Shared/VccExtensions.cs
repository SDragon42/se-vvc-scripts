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
        public static string ToRaceTimeString(this TimeSpan time) => $"{(int)time.TotalMinutes:D2}:{time.Seconds:D2}.{time.Milliseconds:D3}";

        //string FormatTimeSpan(TimeSpan ts) {
        //    var hundredths = (int)(Math.Abs(ts.Milliseconds) / 10);
        //    var sign = ts.TotalMilliseconds >= 0 ? "" : "-";
        //    var displayTs = ts.TotalMilliseconds >= 0 ? ts : ts.Negate();
        //    return $"{sign}{displayTs.Minutes:D2}:{displayTs.Seconds:D2}.{hundredths:D2}";
        //}

        public static void AppendLines(this StringBuilder sb, IEnumerable<string> lines) {
            if (lines == null)
                return;
            foreach (var line in lines) {
                if (line != null) sb.AppendLine(line);
            }
        }

        public static string[] Split(this string argument, char separator, int count) {
            if (string.IsNullOrEmpty(argument)) return new string[] { string.Empty };
            var separatorArray = new[] { separator };
            return (count <= 0)
                ? argument.Split(separatorArray)
                : argument.Split(separatorArray, count);
        }
    }
}
