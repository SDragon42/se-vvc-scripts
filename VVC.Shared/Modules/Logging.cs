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
        class Logging {

            readonly List<string> _lines = new List<string>();

            public Logging(int maxLines2Keep = 10) {
                Enabled = true;
                MaxTextLinesToKeep = maxLines2Keep;
            }

            string _lineBuffer = string.Empty;

            public bool Enabled { get; set; }

            int _maxLines;
            public int MaxTextLinesToKeep {
                get { return _maxLines; }
                set { _maxLines = Math.Max(0, value); }
            }


            public virtual void Clear() {
                if (!Enabled) return;
                _lines.Clear();
                _lineBuffer = string.Empty;
            }

            public void Append(string text, params object[] args) {
                if (!Enabled) return;
                _lineBuffer += string.Format(text, args);
            }

            public void AppendLine() => AppendLine(string.Empty);
            public void AppendLine(string text, params object[] args) {
                if (!Enabled) return;
                Append(text, args);
                _lines.Add(_lineBuffer);
                _lineBuffer = string.Empty;
                TrimLines();
            }

            void TrimLines() {
                if (_maxLines <= 0) return;
                while (_lines.Count > _maxLines)
                    _lines.RemoveAt(0);
            }

            public string GetLogText() {
                var sb = new StringBuilder();
                foreach (var line in _lines) sb.AppendLine(line);
                if (!string.IsNullOrWhiteSpace(_lineBuffer))
                    sb.AppendLine(_lineBuffer);
                return sb.ToString();
            }
        }
    }
}
