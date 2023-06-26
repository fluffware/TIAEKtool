
using Siemens.Engineering.HmiUnified;
using Siemens.Engineering.HmiUnified.UI.Base;
using Siemens.Engineering.HmiUnified.UI.Controls;
using Siemens.Engineering.HmiUnified.UI.Dynamization;
using Siemens.Engineering.HmiUnified.UI.Dynamization.Script;
using Siemens.Engineering.HmiUnified.UI.Enum;
using Siemens.Engineering.HmiUnified.UI.Parts;
using Siemens.Engineering.HmiUnified.UI.Screens;
using Siemens.Engineering.HmiUnified.UI.Shapes;
using Siemens.Engineering.HmiUnified.UI.Widgets;
using System;
using System.Drawing;

namespace TIAEKtool.hmi_builder
{
    public class HmiBuilder
    {
        public static void Build(HmiSoftware hmi)
        {
            var prefix = "MenuButton_";
            var suffix = "_1";
            var x = 0;
            var y = 0;
            HmiScreen screen = hmi.Screens.Find("scratch");
            var frame = screen.ScreenItems.Create<HmiRectangle>(prefix+"Frame"+suffix);
            frame.Width = 500;
            frame.Height = 60;
            frame.BackColor = Color.FromArgb(60,60,67);
            frame.BorderWidth = 0;

            var label = screen.ScreenItems.Create<HmiTextBox>(prefix + "Label" + suffix);
            label.Width = 258;
            label.Height = 60;
            label.Left = 71+x;
            label.Top = 0+y;
            label.Font.Name = HmiFontName.SiemensSans;
            label.Font.Size = 25;
            label.ForeColor = Color.White;
            // label.GetAttribute("Font");
            



            /*HmiFaceplateContainer faceplate = screen.ScreenItems.Create<HmiFaceplateContainer>("Menu_Button_");
            faceplate.ContainedType = "RightButton";
            faceplate.Width = 500;
            faceplate.Height = 60;
            HmiFaceplateInterface intf = faceplate.Interface.Find("Graphics_RL");
                    Console.WriteLine("If: " + intf.Value+", "+intf.Value.GetType());
                }
            }
            
            foreach (HmiScreen screen in hmi.Screens)
            {
                Console.WriteLine("Screen: " + screen.Name);
                foreach (DynamizationBase dyns in screen.Dynamizations)
                {
                      if (dyns is ScriptDynamization script)
                    {
                        script.GlobalDefinitionAreaScriptCode += "";
                        Console.WriteLine("Script: \n" + script.ScriptCode);
                    }
                      if (dyns is TagDynamization tag)
                    {
                        Console.WriteLine("Tag: " + tag.Tag);
                    }
                }
            }*/
            
        }
    }
}
