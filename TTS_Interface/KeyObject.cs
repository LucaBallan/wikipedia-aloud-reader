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
using System.Windows.Forms;
using System.Text;

namespace TTS_Interface
{
    /// <summary>KeyObject</summary>
    public class KeyObject
    {
        private bool isvalid_;
        private KeyEventArgs code_;
        private char ASCII_;

        /// <summary></summary>
        public KeyObject() {
            isvalid_ = false;
            ASCII_ = (char)0;
            code_ = null;
        }
        
        /// <summary></summary>
        public KeyObject(KeyEventArgs code,char ASCII)
        {
            isvalid_ = true;
            ASCII_ = ASCII;
            code_ = code;
        }

        /// <summary></summary>
        public bool isvalid() 
        {
            return isvalid_;
        }

        /// <summary></summary>
        public char ASCII()
        {
            return ASCII_;
        }

        /// <summary></summary>
        public KeyEventArgs code()
        {
            return code_;
        }
    }
}
