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
using System.Windows.Forms;
using gma.System.Windows;
using System.Threading;
using System.Runtime.InteropServices;

namespace TTS_Interface
{
    /// <summary>
    /// GeneralIOForm
    /// </summary>
    public class GeneralIOForm : System.Windows.Forms.Form
    {
        private UserActivityHook actHook;
        private System.Windows.Forms.TextBox textBox;
        private char[] char_key_buffer;
        private KeyEventArgs []key_buffer;
        private int first_free, next_to_read;
        private const int MAX_QUEUE_DIM = 100;
        private bool closing;

        /// <summary>
        /// Input key monitor.
        /// Note: you can wait to this obj if you are waiting for a key
        /// </summary>
        public Object Queue_Monitor_Object;


        
        /// <summary>
        /// Default constructor.
        /// </summary>
        public GeneralIOForm()
		{
			InitializeComponent();
            Queue_Monitor_Object = new Object();
            key_buffer = new KeyEventArgs[MAX_QUEUE_DIM];
            char_key_buffer = new char[MAX_QUEUE_DIM];
            first_free = 0;
            next_to_read = 0;
            closing = false;
        }


		private void InitializeComponent() {
            this.textBox = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // textBox
            // 
            this.textBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.textBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox.Font = new System.Drawing.Font("Courier New", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.World);
            this.textBox.Location = new System.Drawing.Point(4, 12);
            this.textBox.Multiline = true;
            this.textBox.Name = "textBox";
            this.textBox.ReadOnly = true;
            this.textBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox.Size = new System.Drawing.Size(322, 383);
            this.textBox.TabIndex = 3;
            // 
            // GeneralIOForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(328, 398);
            this.Controls.Add(this.textBox);
            this.Name = "GeneralIOForm";
            this.Text = "TTS Interface";
            this.Load += new System.EventHandler(this.MainFormLoad);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClose);
            this.ResumeLayout(false);
            this.PerformLayout();
		}

        private void MainFormLoad(object sender, System.EventArgs e) {
            actHook = new UserActivityHook();
            actHook.KeyPress += new UserActivityHook.HookKeyEventHandler(MyKeyPress);
            actHook.Start();
		}

        private void OnClose(object sender, FormClosingEventArgs e)
        {
            Monitor.Enter(Queue_Monitor_Object);
            closing = true;
            Monitor.PulseAll(Queue_Monitor_Object);
            Monitor.Exit(Queue_Monitor_Object);
        }

        private void MyKeyPress(KeyPressEventArgs info, KeyEventArgs extended_info)
        {
            Monitor.Enter(Queue_Monitor_Object);
            if (((first_free + 1) % MAX_QUEUE_DIM) != next_to_read) {
                if (info != null) char_key_buffer[first_free] = info.KeyChar;
                else char_key_buffer[first_free] = (char)0;
                key_buffer[first_free] = extended_info;
                first_free = (first_free + 1) % MAX_QUEUE_DIM;
                Monitor.Pulse(Queue_Monitor_Object);
            }
            Monitor.Exit(Queue_Monitor_Object);
        }

        private delegate void CloseDelegate();
        private delegate void AppendTextDelegate(string txt, bool clear);
        private void AppendText(string txt, bool clear)
        {
            if (closing) return;
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new AppendTextDelegate(AppendText), new object[] { txt, clear });
            }
            else
            {
                if (clear) textBox.Clear();
                textBox.AppendText(txt);
                textBox.SelectionStart = textBox.Text.Length;
            }
        }


        /// <summary>
        /// Close the output window.
        /// </summary>
        public void CloseWindow()
        {
            if (closing) return;
            if (this.InvokeRequired)
            {
                this.Invoke(new CloseDelegate(CloseWindow));
            }
            else
            {
                this.Close();
            }
        }





        /// <summary>
        /// Clear input buffer
        /// </summary>
        public void ClearInputBuffer()
        {
            if (closing) return;
            Monitor.Enter(Queue_Monitor_Object);
            next_to_read = first_free;
            Monitor.Exit(Queue_Monitor_Object);
        }

        /// <summary>
        /// Get input key
        /// </summary>
        public KeyObject GetChar()
        {
            if (closing) return (new KeyObject());
            
            Monitor.Enter(Queue_Monitor_Object);
            while (first_free == next_to_read) {
                if (!closing) Monitor.Wait(Queue_Monitor_Object);
                if (closing) {
                    Monitor.PulseAll(Queue_Monitor_Object);
                    Monitor.Exit(Queue_Monitor_Object);
                    return (new KeyObject());
                }
            }

            KeyObject e = new KeyObject(key_buffer[next_to_read], char_key_buffer[next_to_read]);
            next_to_read = (next_to_read + 1) % MAX_QUEUE_DIM;

            if (first_free != next_to_read) Monitor.Pulse(Queue_Monitor_Object);
            Monitor.Exit(Queue_Monitor_Object);

            return e;
        }

        /// <summary>
        /// Get input key
        /// </summary>
        public KeyObject inkey()
        {
            if (closing) return (new KeyObject());
            KeyObject e=null; 

            Monitor.Enter(Queue_Monitor_Object);
            if (first_free != next_to_read) {
                e = new KeyObject(key_buffer[next_to_read], char_key_buffer[next_to_read]);
                next_to_read = (next_to_read + 1) % MAX_QUEUE_DIM;
                if (first_free != next_to_read) Monitor.Pulse(Queue_Monitor_Object);
            }
            Monitor.Exit(Queue_Monitor_Object);

            if (e == null) return (new KeyObject());
            return e;
        }


        /// <summary>
        /// Write output
        /// </summary>
        public void Write(string txt)
		{
            AppendText(txt,false);
        }
        
        /// <summary>
        /// Clear output buffer
        /// </summary>
        public void ClearOutputBuffer()
        {
            AppendText("", true);
        }



        [DllImport("user32.dll", EntryPoint="SystemParametersInfo")]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam, uint pvParam, uint fWinIni);
        [DllImport("user32.dll", EntryPoint="SetForegroundWindow")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        [DllImport("User32.dll", EntryPoint="ShowWindowAsync")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);
        

        /// <summary>
        /// Force the focus on this windows
        /// </summary>
        public void ForceFocus()
        {
            SystemParametersInfo((uint)0x2001, 0, 0, 0x0002 | 0x0001);
            SetFocus();
            this.Deactivate += new EventHandler(GeneralIOForm_LostFocus);
        }

        /// <summary>
        /// Set the focus on this windows
        /// </summary>
        public void SetFocus()
        {
            this.BeginInvoke(new EventHandler(GeneralIOForm_LostFocus), null);
        }

        /// <summary>
        /// Disable ForceFocus
        /// </summary>
        public void DisableForceFocus()
        {
            this.Deactivate -= new EventHandler(GeneralIOForm_LostFocus);
        }

        private void GeneralIOForm_LostFocus(object sender, EventArgs e)
        {
            SystemParametersInfo( (uint) 0x2001, 1, 1, 0x0002 | 0x0001);
            ShowWindowAsync(this.Handle, 1);
            SetForegroundWindow(this.Handle);
            this.textBox.Focus();
        }

    }			
}
