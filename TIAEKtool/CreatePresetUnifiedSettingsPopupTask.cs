using PLC.Types;
using Siemens.Engineering;
using Siemens.Engineering.Hmi.Screen;
using Siemens.Engineering.Hmi.Tag;
using Siemens.Engineering.HmiUnified.UI.Base;
using Siemens.Engineering.HmiUnified.UI.Controls;
using Siemens.Engineering.HmiUnified.UI.Dynamization;
using Siemens.Engineering.HmiUnified.UI.Dynamization.Script;
using Siemens.Engineering.HmiUnified.UI.Enum;
using Siemens.Engineering.HmiUnified.UI.Screens;
using Siemens.Engineering.HmiUnified.UI.Widgets;
using Siemens.Engineering.SW.Types;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TIAEKtool
{
    public class CreatePresetUnifiedSettingsPopupTask : SequentialTask
    {

        TiaPortal portal;
        HmiScreenComposition folder;
        string popupName;
        string groupName;
        string culture;
        IList<PresetTag> tags;
        public CreatePresetUnifiedSettingsPopupTask(TiaPortal portal, IList<PresetTag> tags, HmiScreenComposition folder, string popup_name, string group_name, string culture)
        {
            this.portal = portal;
            this.tags = tags;
            this.folder = folder;
            popupName = popup_name;
            groupName = group_name;
            this.culture = culture;
            Description = "Create preset popup screen " + popupName;
           
        }

        private static string MakeString(string input)
        {
            string output = "\"" + input.Replace("\"", "\\\"") + "\"";
            return output;
        }

        private string ParseMarkup(string text, ref HashSet<string> tag_set)
        {
            Regex re_tag = new Regex(@"<hmitag\s+((\w+)\s*=\s*""([^""]*)""\s*)*>([^<])*</hmitag>");
            MatchCollection matches = re_tag.Matches(text);
            string output = "";
            bool preceded = false;
            int pos = 0;
            foreach (Match match in matches)
            {
                int start_pos = match.Groups[0].Captures[0].Index;
                if (start_pos > pos)
                {
                    if (preceded) output += " + ";
                    preceded = true;
                    output += MakeString(text.Substring(pos, start_pos - pos));
                }
                pos = start_pos + match.Groups[0].Captures[0].Length;
                var names = match.Groups[2].Captures;
                var values = match.Groups[3].Captures;
                string name = null;
                for (int i = 0; i < names.Count; i++) {
                    if (names[i].Value == "name")
                    {
                        name = values[i].Value;
                    }
                }
                if (name == null) throw new Exception("<hmitag> has no tag name attribute.");
                tag_set.Add(name);
                if (preceded) output += " + ";
                preceded = true;
                output += "ts(" + MakeString(name) + ").Value";
            }

            if (text.Length > pos)
            {
                if (preceded) output += " + ";
                output += MakeString(text.Substring(pos, text.Length - pos));
            }
            return output.ToString();
            
        }
        protected override void DoWork()
        {
            lock (portal)
            {

                try
                {

                    
                    HmiScreen popup = folder.Find(popupName);
                    //HmiScreenItemBase gauge = popup.ScreenItems.Find("GaugeMeter_1");
                    //HmiScreenItemBase text_box = popup.ScreenItems.Find("Text box_2");
                    if (popup != null)
                    {
                        var load_event = popup.EventHandlers.Find(HmiScreenEventType.Loaded);
                        if (load_event == null)
                        {
                            load_event = popup.EventHandlers.Create(HmiScreenEventType.Loaded);
                        }
                      
                        HashSet<string> tag_set = new HashSet<string>();
                        string info_setup = "let presetInfo = [";
                        int index = 1;
                        foreach (var tag in tags)
                        {
                            info_setup += "{";
                            info_setup += "descr:" + ParseMarkup(tag.labels[culture],ref tag_set) + ",\n";
                            info_setup += "type:" + MakeString(tag.readTagPath.Type.ToString()) + ",\n";
                         
                            if (tag.state_labels != null)
                            {

                                info_setup += "states:{";
                                foreach (var label in tag.state_labels)
                                {
                                    string replace = ParseMarkup(label.Value[culture], ref tag_set);
                                    info_setup += "\"" + label.Key + "\":" + replace + ",\n";
                                }
                                info_setup += "},";
                            }
                            info_setup += "},";
                            index++;
                        }
                        info_setup += "];\n";
                        info_setup += "HMIRuntime.Timers.SetTimeout(function() {\nScreen.FindItem('PresetList').Properties['PresetInfo'] = JSON.stringify(presetInfo);\n}, 500);\n"; 

                        string read_tags = "let ts = Tags.CreateTagSet([\n";
                        foreach (string tag in tag_set)
                        {
                            read_tags += MakeString(tag) + ",\n";
                        }
                        read_tags += "]);\n";
                        read_tags += "ts.Read();\n";
                        load_event.Script.ScriptCode = "\n" + read_tags + info_setup + "\n"; ;
                    }
                    else
                    {
                        LogMessage(MessageLog.Severity.Info, "Couldn't find popup " + popupName+", skipping.");
                    }
                }
                catch (Exception ex)
                {
                    LogMessage(MessageLog.Severity.Error, "Failed to update popup screen:\n" + ex.Message);
                    return;
                }
            
            }
        }
    }
}