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
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace TTS_Interface
{
    /// <summary>
    /// input key
    /// </summary>
    public class KeyInput
    {
        /// <summary>
        /// ascii code (mapped)
        /// </summary>
        public char ASCII;
        /// <summary>
        /// key code (un-mapped)
        /// </summary>
        public Keys Key;
        /// <summary>
        /// is valid key
        /// </summary>
        public bool isValid;
        /// <summary>
        /// Key name (un-mapped)
        /// </summary>
        public String Name;

        /// <summary>
        /// Default constructur
        /// </summary>
        public KeyInput()
        {
            ASCII = ' ';
            Key = Keys.Space;
            isValid = false;
            Name = null;
        }
    }

    /// <summary>
    /// BasicIO
    /// Basic input/output
    /// </summary>
    public class BasicIO
    {
        private Thread IOForm_Thread;
        private SortedDictionary<string, string> messages;
        private String keyboard_remap_in;
        private String keyboard_remap_out;
        /// <summary>
        /// Real IO device
        /// </summary>
        protected GeneralIOForm IO;

        /// <summary>
        /// Inform if the IO interface is still working.
        /// </summary>
        public Mutex IsRunning;
        /// <summary>
        /// Event called when the IO interface closes
        /// </summary>
        public event EventHandler Closed;


        private void IOForm_Thread_c()
        {
            IsRunning = new Mutex(true);
            IO = new GeneralIOForm();
            Application.Run(IO);
            IsRunning.ReleaseMutex();
            if (Closed != null) Closed(this, null);
        }

        /// <summary>
        /// Perform string translation
        /// </summary>
        public String translate(String message)
        {
            try
            {
                return messages[message];
            }
            catch (KeyNotFoundException)
            {
                return message;
            }
        }

        private void xml_parse(String id, String value)
        {
            if (id.CompareTo("keyboard_remap_code") == 0)
            {
                keyboard_remap_in = value.Substring(0, value.IndexOf('|') );
                keyboard_remap_out = value.Substring(value.IndexOf('|') + 1);
            }
            else
            {
                messages[id] = value;
            }
        }

        private bool load_language(String language)
        {
            messages = new SortedDictionary<string, string>();
            if (File.Exists("language.xml"))
            {
                using (XmlReader reader = XmlReader.Create("language.xml"))
                {
                    if (!reader.ReadToFollowing("messages")) return false;
                    if (!reader.ReadToFollowing(language)) return false;
                    if (!reader.ReadToDescendant("msg")) return false;
                    if (messages.Count > 0) messages.Clear();
                    
                    xml_parse(reader["id"], reader.ReadString());
                    while (reader.ReadToNextSibling("msg")) xml_parse(reader["id"], reader.ReadString());
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BasicIO(String language)
        {
            IO = null;
            Closed = null;
            IsRunning = null;
            keyboard_remap_in = null;
            keyboard_remap_out = null;
            IOForm_Thread = new Thread(IOForm_Thread_c);
            IOForm_Thread.Start();
            while (IO == null) Thread.Sleep(50);
            if (!load_language(language))
            {
                Thread.Sleep(500);
                Out("Language {0} not found.", language);
            }
        }

        /// <summary>
        /// Default destructor.
        /// </summary>
        ~BasicIO()
        {
            Close();
        }

        /// <summary>
        /// Close the output window.
        /// </summary>
        public void Close()
        {
            IO.CloseWindow();
        }
        
        /// <summary>
        /// Output a string.
        /// </summary>
        public void Out(String str)
        {
            if (str.Length <= 1) IO.Write(str);
            else IO.Write(translate(str));
        }

        /// <summary>
        /// Output a string without translate it.
        /// </summary>
        public void OutNT(String str)
        {
            IO.Write(str);
        }

        /// <summary>
        /// Output a string.
        /// str is translated in the selected language.
        /// NB: any string in args must be translated manually using translate(..)
        /// </summary>
        public void Out(String str, params object[] args)
        {
            if (args == null)
            {
                if (str.Length <= 1) IO.Write(str);
                else IO.Write(translate(str));
            }
            else
            {
                if (str.Length <= 1) IO.Write(String.Format(str, args));
                else IO.Write(String.Format(translate(str), args));
            }
        }

        /// <summary>
        /// Clear output buffer
        /// </summary>
        public void ClearOutputBuffer()
        {
            IO.ClearOutputBuffer();
        }

        /// <summary>
        /// Clear input buffer
        /// </summary>
        public void ClearInputBuffer()
        {
            IO.ClearInputBuffer();
        }

        /// <summary>
        /// Get input key
        /// </summary>
        public KeyInput GetChar()
        {
            KeyObject o = IO.GetChar();
            KeyInput x = new KeyInput ();
            x.isValid = o.isvalid();
            if (o.isvalid())
            {
                x.Key = o.code().KeyCode;
                x.ASCII = o.ASCII().ToString().ToLower().ToCharArray()[0];
                x.Name = o.code().KeyCode.ToString();
                if (keyboard_remap_in != null)
                {
                    int index = keyboard_remap_in.IndexOf(x.ASCII);
                    if (index != -1) x.ASCII = keyboard_remap_out.ToCharArray()[index];
                }
            }
            return x;
        }

    }
}
