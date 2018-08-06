
# TTS_Interface and WikiReader

TTS_Interface :  software for voice assisted text input and menu selection 
WikiReader    :  software to read aloud wikipedia pages.



---------------
Run the code:
---------------

1) Go in the bin directory
2) Edit config.xml:
    - set your Wikipedia username and password inside the <connection_data> tag
    - set the TTS voice inside the <var id="voice"> tag. 
    - To list all the voices installed on your computer, go to "Control panel" -> "Text to Speech".
3) Run TTS_Interface.exe 
   or 
   Run TTS_Interface.exe "language" "specific_voice_name"
   

<BR>
Note: To simplify the keyboard usage for blind people, all the keys are remapped according to 
      what it is specified in language.xml. In the English configuration, all the letters of 
      the alphabet are mapped column major on the keyboard, i.e., on a querty keyboard<BR>
      "q" corresponds to "a"<BR>
      "a" corresponds to "b"<BR>
      "z" corresponds to "c"<BR>
      "w" corresponds to "d"<BR>
      "s" corresponds to "e"<BR>
      ... and so on ...<BR>
      (see language.xml)<BR>
<BR>


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


