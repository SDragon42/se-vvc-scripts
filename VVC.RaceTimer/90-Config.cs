// <mdk sortorder="50" />
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

        int _configHashCode = 0;
        readonly MyIni Ini = new MyIni();

        string _tag_CurrentRaceInfo = "[VVC-CurrentInfo]";
        string _tag_PreviousRaceInfo1 = "[VVC-PreviousInfo1]";
        string _tag_PreviousRaceInfo2 = "[VVC-PreviousInfo2]";
        string _tag_RaceStandings = "[VVC-RaceStandings]";
        string _tag_StartConnector = "[VVC-RaceStart]";
        string _tag_ActionRelayTransmitter = "[VVC-Transmitter]";
        int _channelId_ResetCheckpoints = 100;
        

        const string SECTION_REQUIRED_BLOCK_TAGS = "Required Block Tags";
        readonly MyIniKey Key_ActionRelayTransmitter = new MyIniKey(SECTION_REQUIRED_BLOCK_TAGS, "Action Relay Transmitter");
        readonly MyIniKey Key_StartConnector = new MyIniKey(SECTION_REQUIRED_BLOCK_TAGS, "Start Connector");
        

        const string SECTION_BLOCK_TAGS = "Block Tags";
        readonly MyIniKey Key_CurrentRaceInfo = new MyIniKey(SECTION_BLOCK_TAGS, "Current Race Info");
        readonly MyIniKey Key_PreviousRaceInfo1 = new MyIniKey(SECTION_BLOCK_TAGS, "Previous Race Info 1");
        readonly MyIniKey Key_PreviousRaceInfo2 = new MyIniKey(SECTION_BLOCK_TAGS, "Previous Race Info 2");
        readonly MyIniKey Key_RaceStandings = new MyIniKey(SECTION_BLOCK_TAGS, "Race Standings");
        

        const string SECTION_CHANNEL_IDS = "Channel IDs";
        readonly MyIniKey Key_ChannelId_ResetCheckpoints = new MyIniKey(SECTION_CHANNEL_IDS, "Reset Checkpoints");




        void LoadConfig() {
            var tmpHashCode = Me.CustomData.GetHashCode();
            if (_configHashCode == tmpHashCode) return;
            _configHashCode = tmpHashCode;
            LoadINI(Ini, Me.CustomData);

            _tag_ActionRelayTransmitter = Ini.Add(Key_ActionRelayTransmitter, _tag_ActionRelayTransmitter).ToString();
            _tag_StartConnector = Ini.Add(Key_StartConnector, _tag_StartConnector).ToString();

            _tag_CurrentRaceInfo = Ini.Add(Key_CurrentRaceInfo, _tag_CurrentRaceInfo).ToString();
            _tag_PreviousRaceInfo1 = Ini.Add(Key_PreviousRaceInfo1, _tag_PreviousRaceInfo1).ToString();
            _tag_PreviousRaceInfo2 = Ini.Add(Key_PreviousRaceInfo2, _tag_PreviousRaceInfo2).ToString();
            _tag_RaceStandings = Ini.Add(Key_RaceStandings, _tag_RaceStandings).ToString();

            _channelId_ResetCheckpoints = Ini.Add(Key_ChannelId_ResetCheckpoints, _channelId_ResetCheckpoints).ToInt32();

            SaveConfig();
        }
        void SaveConfig() {
            Ini.Set(Key_ActionRelayTransmitter, _tag_ActionRelayTransmitter);
            Ini.Set(Key_StartConnector, _tag_StartConnector);

            Ini.Set(Key_CurrentRaceInfo, _tag_CurrentRaceInfo);
            Ini.Set(Key_PreviousRaceInfo1, _tag_PreviousRaceInfo1);
            Ini.Set(Key_PreviousRaceInfo2, _tag_PreviousRaceInfo2);
            Ini.Set(Key_RaceStandings, _tag_RaceStandings);

            Ini.Set(Key_ChannelId_ResetCheckpoints, _channelId_ResetCheckpoints);

            var text = Ini.ToString();
            _configHashCode = text.GetHashCode();
            Me.CustomData = text;
        }

        readonly List<IMyThrust> LiftThrusters = new List<IMyThrust>();

        static void LoadINI(MyIni ini, string text) {
            ini.Clear();
            if (!ini.TryParse(text)) {
                var tmp = text.Replace('<', '[').Replace('>', ']').Replace(':', '=');
                if (!ini.TryParse(tmp))
                    ini.EndContent = text;
            }
        }

    }
}
