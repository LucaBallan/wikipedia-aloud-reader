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


namespace TTS_Interface
{
    /// <summary>Menu</summary>
    public class Menu
    {
        
        private bool exit_option;
        private bool exit_after_selection;
        private String Title;
        private String Entry_Title;
        private List<MenuEntry> MenuList;

        /// <summary></summary>
        public enum MenuType {
            /// <summary></summary>
            ROOT_MENU,
            /// <summary></summary>
            SELECTION_MENU,
            /// <summary></summary>
            NORMAL_MENU         
        };


        /// <summary>
        /// Default constructor
        /// 
        /// Title        = Title of the menu
        /// Entry_Title  = Title used to describe this menu
        /// MenuType     = ROOT_MENU      (you cannot exit from it)
        ///                NORMAL_MENU    (it has an option to exit from it)
        ///                SELECTION_MENU (it exit just after the selection)
        ///</summary>
        public Menu(String Title, String Entry_Title, MenuType type)
        {
            this.Title = Title;
            this.Entry_Title = Entry_Title;
            this.exit_option = (type == MenuType.NORMAL_MENU);
            this.exit_after_selection = (type == MenuType.SELECTION_MENU);
            MenuList = new List<MenuEntry>();
        }

        /// <summary>
        /// Add an entry to the Menu
        /// </summary>
        public void AddEntry(MenuEntry x)
        {
            MenuList.Add(x);
        }


        /// <summary>
        /// Run the menu
        /// 
        /// SELECTION_MENU -> returns the name of the selected item
        /// NORMAL_MENU    -> returns ""
        /// </summary>
        public String Run(SpeechIO IO)
        {
            int choice;
            char input;
            MenuEntry[] menu_array = MenuList.ToArray();
            
            while (true)
            {
                IO.ClearOutputBuffer();
                IO.ClearInputBuffer();

                // Leggi menu
                IO.Out("------ {0} ------" + Environment.NewLine + Environment.NewLine, "{0}.", new object[] { IO.translate(Title) });
                for(int i=0;i<menu_array.Length;i++)
                    IO.Out(" {0}. {1}" + Environment.NewLine, " {0} {1}", new object[] { (i + 1), IO.translate(MenuList[i].GetName()) });
                if (exit_option) IO.Out(" {0}. {1}" + Environment.NewLine, " {0} {1}", new object[] { (menu_array.Length + 1), IO.translate("Go back to the previous menu") });
                
                // Wait input
                IO.Out(Environment.NewLine + "{0}: ", "", new object[] { IO.translate("Choice") });
                while (true) {
                    KeyInput key_in = IO.GetChar();
                    if (!key_in.isValid) return "";
                    input = key_in.ASCII;
                    
                    if (Char.IsDigit(input)) {
                        choice = (input - '0')-1;
                        if (choice < menu_array.Length) break;
                        if ((exit_option) && (choice == menu_array.Length)) {
                            choice = -1;
                            break;
                        }
                    }
                    if (input == ' ') {
                        choice = -2;
                        break;
                    }
                }

                IO.Out(Environment.NewLine + Environment.NewLine, "");
                if (choice == -2) continue;
                if (choice == -1) return "";

                menu_array[choice].Run(IO);
                if (exit_after_selection) return menu_array[choice].GetName();
            }
        }

        /// <summary></summary>
        public MenuEntry GetCallerMenuEntry()
        {
            return new MenuEntry(Entry_Title, this);
        }
    }
}
