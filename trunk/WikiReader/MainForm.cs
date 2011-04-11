//
// WikiReader: software to read aloud wikipedia pages.
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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using SpeechLib;
using System.Windows.Forms;
using System.Threading;
using DotNetWikiBot;

namespace WikiReader
{
    public partial class WikiReader : Form
    {
        UserActivityHook actHook;
        SpVoice voice;
        String ttr_local;
        bool loop;
        bool exit_window;
        bool playing;
        int  offset;

        public WikiReader(String ttr, String voice_name)
        {
            InitializeComponent();
            Hide();

            offset        = 0;
            playing       = false;
            exit_window   = false;
            loop          = true;
            ttr_local     = ttr;

            actHook = new UserActivityHook();
            actHook.KeyDown += new UserActivityHook.KeyInfoEventHandler(UbiquitousKeyDown);

            voice = new SpVoice();
            ISpeechObjectTokens v=voice.GetVoices("Name = " + voice_name, "");
            if (v.Count == 0)
                System.Console.WriteLine(Bot.Msg("Voice unavailable. Using the default voice."));
            else 
                voice.Voice = v.Item(0);
            
            this.Load += new System.EventHandler(this.MainFormStart);
        }

        void MainFormStart(object sender, System.EventArgs e)
        {
            Thread staThread = new Thread((ThreadStart)
                delegate
                {
                    playing = true;
                    voice.Speak(ttr_local, SpeechVoiceSpeakFlags.SVSFDefault | SpeechVoiceSpeakFlags.SVSFlagsAsync);
                    do
                    {
                        do
                        {
                            if (voice.WaitUntilDone(0))
                            {
                                exit_window = true;
                                break;
                            }
                        } while (loop);

                        if (!playing)
                        {
                            voice.Resume();
                            playing = true;
                        }
                        voice.Speak("", SpeechVoiceSpeakFlags.SVSFPurgeBeforeSpeak);
                        if (!exit_window)
                        {
                            voice.Speak(ttr_local.Substring(offset), SpeechVoiceSpeakFlags.SVSFDefault | SpeechVoiceSpeakFlags.SVSFlagsAsync);
                            loop = true;
                        }
                    } while (!exit_window);

                    System.Windows.Forms.Application.Exit();
                });
            staThread.Start();
        }

        public void UbiquitousKeyDown(object sender, Keys KeyCode, bool ctrl)
        {
            if (KeyCode == Keys.Escape)
            {
                voice.Pause();
                exit_window = true;
                loop = false;
                Close();
                return;
            }
            if (KeyCode == Keys.Left)
            {
                offset = Math.Max(offset + voice.Status.InputSentencePosition - 70, 0);
                loop = false;
                return;
            }
            if (KeyCode == Keys.Right)
            {
                offset = Math.Min(offset + voice.Status.InputSentencePosition + 70, ttr_local.Length - 1 );
                loop = false;
                return;
            }
            if (KeyCode == Keys.Space)
            {
                if (playing)
                {
                    voice.Pause();
                    playing = false;
                }
                else
                {
                    voice.Resume();
                    playing = true;
                }
            }
        }
    
    }
}