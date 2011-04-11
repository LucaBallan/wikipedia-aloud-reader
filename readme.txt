//
// TTS_Interface: software for voice assisted text input and menu selection 
// WikiReader:    software to read aloud wikipedia pages.
//
//
//    Copyright (C) 2011 Luca Ballan (ballanlu@gmail.com) 
//                       http://www.inf.ethz.ch/personal/lballan/
//
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



---------------
Run the code:
---------------

1) Go in the bin directory
2) Edit config.xml:
       - set your Wikipedia username and password inside the <connection_data> tag
       - set the TTS voice inside the <var id="voice"> tag. To list all the voices 
         installed on your computer, go to "Control panel" -> "Text to Speech".
3) Run TTS_Interface.exe 
   or 
   Run TTS_Interface.exe "language" "specific_voice_name"
   


Note: To simplify the keyboard usage for blind people, all the keys are remapped according to 
      what it is specified in language.xml. In the English configuration, all the letters of 
      the alphabet are mapped column major on the keyboard, i.e., on a querty keyboard
      "q" corresponds to "a"
      "a" corresponds to "b"
      "z" corresponds to "c"
      "w" corresponds to "d"
      "s" corresponds to "e"
      ... and so on ...
      (see language.xml)



-----------------
Compile the code:
-----------------

1) Compile both WikiReader and TTS_Interface in Release mode
2) Place in the same directory WikiReader.exe, TTS_Interface.exe, rules.xml, language.xml 
   and config.xml






---------------
config.xml:
---------------

Contains all the necessary information to connect to a wiki server (user, password, server name), and
the information regarding the selected language and the selected voice (see control panel for a list 
of the installed voices on your system). Microsoft Anna and Microsoft Sam are the default English voices 
installed on a standard XP/Vista installation. For other languages you may have to install some third party 
TTS software, like for instance "Loquendo". Some necessary translations/rules must also be specified in this 
file for languages different than English. See code for details.



---------------
language.xml:
---------------

Contains all the necessary translations used in the code and also the keyboard remapping layout.



---------------
rules.xml:
---------------

Additional rules (expressed as regular expressions) to convert the written text in a readable text. See
the code for more information.


