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
using System.Diagnostics;
using System.ComponentModel;


namespace TTS_Interface
{
    /// <summary>MenuEntry</summary>
    public class MenuEntry
    {
        /// <summary></summary>
        public delegate void TargetCallBack();

        private String name;
        private String confirm_of_the_action;

        /// <summary>
        /// Options
        /// </summary>
        private Menu Target;
        private TargetCallBack Target_Code;
        private String Explorer_Action;



        /// <summary></summary>
        public MenuEntry(String name)
        {
            this.name = name;
            this.Target = null;
            this.Target_Code = null;
            this.Explorer_Action = "";
            this.confirm_of_the_action = "";
        }

        /// <summary>
        /// MenuEntry that link to a Menu
        /// </summary>
        public MenuEntry(String name, Menu Target)
        {
            this.name = name;
            this.Target = Target;
            this.Target_Code = null;
            this.Explorer_Action = "";
            this.confirm_of_the_action = "";
        }

        /// <summary>
        /// MenuEntry that execute an explorer action
        /// </summary>
        public MenuEntry(String name, String Explorer_Action, String confirm_of_the_action)
        {
            this.name = name;
            this.Target = null;
            this.Target_Code = null;
            this.Explorer_Action = Explorer_Action;
            this.confirm_of_the_action = confirm_of_the_action;
        }


        /// <summary>
        /// MenuEntry that execute a function
        /// </summary>
        public MenuEntry(String name, TargetCallBack Target_Code)
        {
            this.name = name;
            this.Target = null;
            this.Target_Code = Target_Code;
            this.Explorer_Action = "";
            this.confirm_of_the_action = "";
        }

        /// <summary>
        /// Get MenuEntry name
        /// </summary>
        public String GetName()
        {
            return name;
        }

        /// <summary>
        /// Execute the action connected to the menu
        /// </summary>
        public void Run(SpeechIO IO)
        {
            if (confirm_of_the_action.Length != 0) IO.Out("", confirm_of_the_action);

            if (Target != null) Target.Run(IO);
            if (Target_Code!=null) Target_Code();
            if (Explorer_Action != "")
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = "C:\\Programmi\\Internet Explorer\\iexplore.exe";
                p.StartInfo.Arguments = Explorer_Action;
                p.Start();
            }
        }

    }
}
