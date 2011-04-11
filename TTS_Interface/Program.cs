//
// TTS_Interface: software for voice assisted text input and menu selection 
//
//    Copyright (C) 2011 Luca Ballan (ballanlu@gmail.com) http://www.inf.ethz.ch/personal/lballan/
//
//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see <http://www.gnu.org/licenses/>.
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Configuration;
using System.Reflection;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.ComponentModel;


namespace TTS_Interface
{
    class Program
    {
        private static SpeechIO IO;
        private static String language;

        static public void PrintHelp()
        {
            System.Console.WriteLine("Usage: TTS_Interface language voice_name");
            System.Console.WriteLine(Environment.NewLine);
        }

        [STAThread]
        public static void Main(string[] args)
        {
            language = "en";
            String voice = "Microsoft Anna";
            if (args.Length >= 2)
            {
                language = args[0];
                voice = args[1];
            }
            else
            {
                PrintHelp();            
            }

            IO = new SpeechIO(language, voice, 2);

            Menu Main = new Menu("Main Menu", "", Menu.MenuType.ROOT_MENU);
                    Menu Call_Menu = new Menu("Skype call", "Make a skype call", Menu.MenuType.NORMAL_MENU);
                    Call_Menu.AddEntry(SkypeEntry("Contact 1", "skype_account1"));
                    Call_Menu.AddEntry(SkypeEntry("Contact 2", "skype_account2"));
                    Call_Menu.AddEntry(SkypeEntry("Contact 3", "skype_account2"));
                Main.AddEntry(Call_Menu.GetCallerMenuEntry());
                Main.AddEntry(new MenuEntry("Search on Wikipedia", new MenuEntry.TargetCallBack(Program.WikiSearch)));

            Main.Run(IO);
            IO.Close();
        }

        private static void WikiSearch()
        {
            String Text = IO.GetText("Write the text to search.");
            if (Text.Length != 0) 
            {
                try
                {
                    Process p = new Process();
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = false;
                    p.StartInfo.FileName = "WikiReader.exe";
                    p.StartInfo.Arguments = "-s \"" + Text + "\" -l " + language;
                    
                    if (p.Start())
                    {
                        p.WaitForExit();
                    }
                    else
                    {
                        IO.Out("Internal error.");
                    }
                }
                catch (Exception)
                {
                    IO.Out("Internal error.");
                }
            }
        }

        private static MenuEntry SkypeEntry(String Name, String Skype_Name)
        {
            return new MenuEntry(String.Format(IO.translate("Call {0}"),Name), "Skype:" + Skype_Name, String.Format(IO.translate("Calling {0}... Please wait."),Name));
        }

    }
}
