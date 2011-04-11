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
using System.Windows.Forms;
using SpeechLib;


namespace TTS_Interface
{
    /// <summary>
    /// SpeechIO : BasicIO
    /// Voice assisted input/output
    /// </summary>
    public class SpeechIO : BasicIO
    {
        private SpVoice voice;
        private int default_voice_rate;

        /// <summary>Default constructor.</summary>
        public SpeechIO(String language, String voice_name, int voice_rate) : base(language)
        {
            default_voice_rate = voice_rate;
            voice = new SpVoice();
            voice.Rate = default_voice_rate;
            
            if (voice_name.Length != 0)
            {
                ISpeechObjectTokens v = voice.GetVoices("Name = " + voice_name, "");
                if (v.Count == 0)
                    Out("Voice unavailable. Using the default voice.");
                else
                    voice.Voice = v.Item(0);
            }
        }

        /// <summary>
        /// Output a string.
        /// </summary>
        new public void Out(String str)
        {
            Out(str, str, default_voice_rate, null);
        }

        /// <summary>
        /// Output a string.
        /// </summary>
        public void Out(String str_w, String str_s)
        {
            Out(str_w, str_s, default_voice_rate, null);
        }

        
        /// <summary>
        /// Output a string.
        /// </summary>
        new public void Out(String str, params object[] args)
        {
            Out(str, str, default_voice_rate, args);
        }


        /// <summary>
        /// Output a string.
        /// </summary>
        public void Out(String str_w, String str_s, params object[] args)
        {
            Out(str_w, str_s, default_voice_rate, args);
        }

        /// <summary>
        /// Output a string.
        /// </summary>
        public void Out(String str_w, String str_s, int voice_rate, params object[] args)
        {
            
            if (str_w.Length != 0) base.Out(str_w, args);
            if (str_s.Length != 0)
            {
                voice.Rate = voice_rate;
                if (args == null)
                {
                    voice.Speak(translate(str_s), SpeechVoiceSpeakFlags.SVSFDefault);
                }
                else
                {
                    voice.Speak(String.Format(translate(str_s), args), SpeechVoiceSpeakFlags.SVSFDefault);
                }
                voice.Rate = default_voice_rate;
            }
        }

        /// <summary>
        /// Spell a character.
        /// </summary>
        public void SpellChar(char c)
        {
            if (!isCharacterReadable(c)) return;

            String carname = c.ToString();
            switch (c)
            {
                case ',': carname = "comma"; break;
                case '.': carname = "full stop"; break;
                case '-': carname = "dash"; break;
                case '+': carname = "plus"; break;
                case '*': carname = "asterisk"; break;
                case '<': carname = "less than"; break;
                case '\\':carname = "slash"; break;
                case '?': carname = "question mark"; break;
                case '!': carname = "exclamation mark"; break;
                case '_': carname = "underscore"; break;
                case ':': carname = "colon"; break;
                case ';': carname = "semicolon"; break;
                case '>': carname = "greater than"; break;
                case '\'':carname = "apostrophe"; break;
                case '^': carname = "caret"; break;
                case '(': carname = "open bracket"; break;
                case ')': carname = "close bracket"; break;
                case ' ': carname = "space"; break;
            }
            Out(c.ToString(), carname);
        }

        /// <summary>
        /// Assisted Input
        /// </summary>
        public String GetText(String intro)
        {
            IO.ForceFocus();
            ClearOutputBuffer();
            ClearInputBuffer();
            Out(intro);
            Out(Environment.NewLine, "Press escape once you finished. Press enter to listen what you already wrote.");
            ClearOutputBuffer();

            KeyInput cki;
            String Text = "";
            String Word = "";
            
            
            do {
                // input
                cki = GetChar();
                if (!cki.isValid)
                {
                    IO.DisableForceFocus();
                    return "";
                }
                char c = cki.ASCII;

                // Ensure focus
                IO.SetFocus();

                // use character
                if (isCharacterReadable(c)) {
                    if (isCharacterSeparator(c)) {
                        voice.Rate = 4;
                        voice.Speak(Word, SpeechVoiceSpeakFlags.SVSFNLPSpeakPunc | SpeechVoiceSpeakFlags.SVSFDefault);
                        voice.Rate = default_voice_rate;

                        SpellChar(c);
                        Text += Word + c;
                        Word = "";
                    } else {
                        SpellChar(c);
                        Word += c;
                    }
                }
                
                switch (cki.Key) {
                    case System.Windows.Forms.Keys.Delete:
                        ClearOutputBuffer();
                        Text = "";
                        Word = "";
                        Out("", "Text deleted.");
                        break;
                    case System.Windows.Forms.Keys.Enter: 
                        voice.Speak(Text + Word, SpeechVoiceSpeakFlags.SVSFNLPSpeakPunc | SpeechVoiceSpeakFlags.SVSFDefault); 
                        break;
                    case System.Windows.Forms.Keys.Back:
                        if (Text.Length + Word.Length != 0) 
                        {
                            if (Word.Length != 0)
                            {
                                Word = Word.Substring(0, Word.Length - 1);
                            }
                            else
                            {
                                Text = Text.Substring(0, Text.Length - 1);
                            }
                            ClearOutputBuffer();
                            OutNT(Text);
                            OutNT(Word);
                        }
                        voice.Speak("backspace", SpeechVoiceSpeakFlags.SVSFDefault);
                        break;
                };
            } while (cki.Key != System.Windows.Forms.Keys.Escape);

            Text += Word;
            Word = "";
            voice.Speak(Text, SpeechVoiceSpeakFlags.SVSFNLPSpeakPunc | SpeechVoiceSpeakFlags.SVSFDefault);

            IO.DisableForceFocus();
            return Text;
        }


        /// <summary>Check if carattere is readable</summary>
        private bool isCharacterReadable(char c)
        {
            return ((Char.IsDigit(c)) || (Char.IsLetter(c)) || (isCharacterSeparator(c)));
        }

        /// <summary>Check if carattere is a separator</summary>
        private bool isCharacterSeparator(char c)
        {
            return ((Char.IsSeparator(c)) || (Char.IsPunctuation(c)));
        }

    }
}
