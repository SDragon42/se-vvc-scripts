// <mdk sortorder="1000" />
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

        class DebugLogging : Logging {
            public const string DefaultDebugPanelName = "DEBUG";

            readonly MyGridProgram _gridProg;
            readonly string _displayName;
            int _displayIndex;


            public DebugLogging(MyGridProgram thisObj, string displayName = null, int displayIndex = 0) {
                _gridProg = thisObj;
                _displayName = string.IsNullOrWhiteSpace(displayName) ? DefaultDebugPanelName : displayName;
                _displayIndex = displayIndex >= 0 ? displayIndex : 0;
                MaxTextLinesToKeep = -1;
            }


            public bool EchoMessages { get; set; } = false;


            public override void Clear() {
                base.Clear();
                UpdateDisplay();
            }
            public void UpdateDisplay() {
                if (!Enabled) return;
                var text = GetLogText();
                if (EchoMessages && text.Length > 0) _gridProg.Echo(text);
                WriteToDisplay(text);
            }
            void WriteToDisplay(string text) {
                if (!Enabled) return;
                var b = _gridProg.GridTerminalSystem.GetBlockWithName(_displayName);
                if (b == null) return;
                if (!b.IsSameConstructAs(_gridProg.Me)) return;
                var d = b as IMyTextSurfaceProvider;
                if (d == null) return;
                if (_displayIndex < 0 || _displayIndex >= d.SurfaceCount) return;
                var surface = d.GetSurface(_displayIndex);
                surface.ContentType = ContentType.TEXT_AND_IMAGE;
                surface.WriteText(text);
            }
        }
        
    }
}
