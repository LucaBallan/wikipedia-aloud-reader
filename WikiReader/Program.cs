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
using System.IO;
using System.IO.Compression;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.XPath;
using System.Web;
using DotNetWikiBot;
using System.Net;
using System.Threading;

namespace WikiReader
{
    class Program
    {
        static String RecursiveReplace(String input, String pattern, String replacement, RegexOptions options, int min_it, int max_it)
        {
            int i = 0;
            for (; i < min_it; i++)
                input = Regex.Replace(input, pattern, replacement, options);
            for (; i < max_it; i++)
            {
                if (!(Regex.Match(input, pattern).Success)) break;
                input = Regex.Replace(input, pattern, replacement, options);
            }
            return input;
        }
        static String GetVal(String text, String param)
        {
            Match val = Regex.Match(text, @"\|(\s*)" + param + @"(\s*)=(?<desc>.[^\|]*)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            if (val.Success) {
                String str = val.Groups[3].ToString();
                str = Regex.Replace(str, @"[\n\r]", "", RegexOptions.None);
                return str;
            }
            return "";
        }
        static String KillSpaces(String text)
        {
            text = Regex.Replace(text, @"[\n\r]", " ", RegexOptions.None);
            text = Regex.Replace(text, @"[ ]{2,}", " ", RegexOptions.None);
            text = Regex.Replace(text, @" (?<caract>[,\.])", "${caract}", RegexOptions.None);
            text = Regex.Replace(text, @"\s+$", "", RegexOptions.None);
            text = Regex.Replace(text, @"^\s+", "", RegexOptions.None);
            return text;
        }
        static String GetBioText(String BioText)
        {
            String name = GetVal(BioText, Bot.Msg("Title")) + GetVal(BioText, Bot.Msg("Name")) + GetVal(BioText, Bot.Msg("Surname")) + GetVal(BioText, Bot.Msg("PostSurname"));
            String born = GetVal(BioText, Bot.Msg("BirthPlace")) + "," + GetVal(BioText, Bot.Msg("BirthDate")) + GetVal(BioText, Bot.Msg("BirthYear"));
            String died = GetVal(BioText, Bot.Msg("DeathPlace")) + "," + GetVal(BioText, Bot.Msg("DeathDate")) + GetVal(BioText, Bot.Msg("DeathYear"));
            String borndied = born;
            died = KillSpaces(died);
            if (died.CompareTo(",") != 0) borndied += ", " + died;
            
            bool genderMale = (KillSpaces(GetVal(BioText, Bot.Msg("Gender"))).ToLower().CompareTo("m")==0);
            String what = GetVal(BioText, Bot.Msg("Activity"));
            String what_other = GetVal(BioText, Bot.Msg("OtherActivities"));
            if (what_other.Length != 0) {
                what += ", " + GetVal(BioText, Bot.Msg("Activity") + "2");
                what += ", " + what_other;
            }
            else
            {
                for (int i = 2; i < 5; i++)
                {
                    String ex = GetVal(BioText, Bot.Msg("Activity") + i);
                    if (ex.Length == 0) break;
                    String ex2 = GetVal(BioText, Bot.Msg("Activity") + (i + 1));
                    if (ex2.Length == 0) what += " " + Bot.Msg("and") + " " + ex;
                    else what += ", " + ex;
                }
            }
            String tobe = (genderMale ? Bot.Msg("is aM") : Bot.Msg("is aF"));
            tobe = tobe.Substring(0,tobe.Length-1);
            String tt = name + " (" + borndied + ") " + tobe + " " + what + GetVal(BioText, Bot.Msg("Nationality")) + GetVal(BioText, Bot.Msg("PostNationality")) + ".";
            tt = KillSpaces(tt);
            return tt;
        }
        static public String GoogleSearch(String text_to_search, String language)
        {
            String pageURL = "http://www.google.com/search?hl=" + language + "&q=" + HttpUtility.UrlEncode(text_to_search) + "&as_sitesearch=" + language + ".wikipedia.org" + "&btnI="+Bot.Msg("I%27m+Feeling+Lucky");
            HttpWebRequest webReq = (HttpWebRequest)WebRequest.Create(pageURL);
            webReq.Proxy.Credentials = CredentialCache.DefaultCredentials;
            webReq.UseDefaultCredentials = true;
            webReq.ContentType = Bot.webContentType;
            webReq.UserAgent = Bot.botVer;
            webReq.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip,deflate");
            HttpWebResponse webResp = null;
            try
            {
                webResp = (HttpWebResponse)webReq.GetResponse();
            }
            catch (Exception)
            {
                return "";
            }
            String respStr = webResp.ResponseUri.ToString();
                Stream Answer = webResp.GetResponseStream();
                StreamReader _Answer = new StreamReader(Answer);
                _Answer.ReadToEnd();
                _Answer.Close();
                webResp.Close();

            Match status = Regex.Match(respStr, ".wikipedia.org/wiki/", RegexOptions.IgnoreCase);
            if (!status.Success) return "";
            respStr = respStr.Substring(status.Index + status.Length);
            return respStr;
        }

        static String username = null;
        static String password = null;
        static String wikiserver = null;
        static String voice_name = null;

        static bool BotReadConfig(String language)
        {
            String id;
            if (!File.Exists("config.xml")) return false;
            using (XmlReader reader = XmlReader.Create("config.xml"))
            {
                if (!reader.ReadToFollowing("connection_data")) return false;
                if (!reader.ReadToDescendant("var")) return false;
                
                do {
                    id = reader["id"];
                    if (id.CompareTo("username") == 0) username = reader.ReadString();
                    if (id.CompareTo("password") == 0) password = reader.ReadString();
                } while (reader.ReadToNextSibling("var"));
                if ((username == null) || (password == null)) return false;
            }
            using (XmlReader reader = XmlReader.Create("config.xml"))
            {
                if (!reader.ReadToFollowing("wiki_voice")) return false;
                if (!reader.ReadToDescendant(language)) return false;
                if (!reader.ReadToDescendant("var")) return false;

                do {
                    id = reader["id"];
                    if (id.CompareTo("wiki") == 0) wikiserver = reader.ReadString();
                    if (id.CompareTo("voice") == 0) voice_name = reader.ReadString();
                } while (reader.ReadToNextSibling("var"));
                if ((wikiserver == null) || (voice_name == null)) return false;
            }
            return true;
        }
        

        static public void PrintHelp()
        {
            System.Console.WriteLine("Usage: WikiReader [options]");
            System.Console.WriteLine("\n");
            System.Console.WriteLine("         -e entry    read the entry \"entry\" on Wikipedia");
            System.Console.WriteLine("         -s keyword  search for the keyword \"keyword\" and read");
            System.Console.WriteLine("                     the corresponding page.");
            System.Console.WriteLine("         -l language use the specified language, e.g. en, it...");
            System.Console.WriteLine("\n");
        }
        static public String kill(String text, String pattern)
        {
            text = RecursiveReplace(text, @"\{\{[\s]*" + pattern + @"[^\|\{\}]*\|(?<desc>.[^\|\{\}]*)\}\}", "", RegexOptions.Singleline | RegexOptions.IgnoreCase, 1, 1);
            return text;
        }
        static void Main(string[] args)
        {
            Site site = null;
            Page p = null;
            String text = "";
            String language = "en";
            String text_to_search = "";
            int search_method = 0;
            int cmd_index=0;

            while (cmd_index<args.Length) {
                if (args[cmd_index].ToLower().CompareTo("-e") == 0)
                {
                    cmd_index++;
                    if (cmd_index>=args.Length) {
                        PrintHelp();
                        return;
                    }
                    text_to_search = args[cmd_index];
                    search_method = 0;
                    cmd_index++;
                    continue;
                }
                if (args[cmd_index].ToLower().CompareTo("-s") == 0)
                {
                    cmd_index++;
                    if (cmd_index >= args.Length)
                    {
                        PrintHelp();
                        return;
                    }
                    text_to_search = args[cmd_index];
                    search_method = 1;
                    cmd_index++;
                    continue;
                }
                if (args[cmd_index].ToLower().CompareTo("-l") == 0)
                {
                    cmd_index++;
                    if (cmd_index >= args.Length)
                    {
                        PrintHelp();
                        return;
                    }
                    language = args[cmd_index].Substring(0,2).ToLower();
                    cmd_index++;
                    continue;
                }
                cmd_index++;
            }
            if (text_to_search.Length == 0)
            {
                PrintHelp();
                return;
            }

            if (!BotReadConfig(language))
            {
                System.Console.WriteLine("File \"config.xml\" is not in the right format or it does not exist.");
                return;
            }
            if (language != "en")
                if (!Bot.LoadLocalizedMessages(language)) 
                    System.Console.WriteLine("Selected language different from english but \"language.xml\" does not exists or it is corrupted or it does not contain the desitered language.");
            Bot.LoadLanguageSpecificRules(language);

            try
            {
                site = new Site(wikiserver, username, password);
            }
            catch (Exception)
            {
                System.Console.WriteLine(Bot.Msg("Connection error."));
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new WikiReader(Bot.Msg("Connection error."),voice_name));
                return;
            }
            if (search_method == 1)
            {
                text_to_search = GoogleSearch(text_to_search, language);
                if (text_to_search.Length == 0)
                {
                    System.Console.WriteLine(Bot.Msg("Page not found."));
                    Application.EnableVisualStyles();
                    Application.SetCompatibleTextRenderingDefault(false);
                    Application.Run(new WikiReader(Bot.Msg("Page not found."), voice_name));
                    return;
                }
                text_to_search = HttpUtility.UrlDecode(text_to_search).Replace("_"," ");
                search_method = 0;
            }
            if (search_method == 0) {
                p = new Page(site, text_to_search);

                if (!p.getCorrectTitle())
                {
                    text = Bot.Msg("Page not found.");
                }
                else
                {
                    p.LoadEx();
                    if (!p.Exists())
                    {
                        if (p.getCorrectTitle()) p.LoadEx();
                        if (!p.Exists())
                        {
                            if (p.getCorrectTitle()) p.LoadEx();
                            if (!p.Exists())
                            {
                                if (p.getCorrectTitle()) p.LoadEx();   // Try to search four times
                            }
                            if (!p.Exists())
                            {
                                // Desperate attempts
                                p.title = System.Threading.Thread.CurrentThread.CurrentCulture.TextInfo.ToTitleCase(p.title.ToLower());
                                p.LoadEx();
                                if (!p.Exists())
                                {
                                    p.title = p.title.ToLower();
                                    p.LoadEx();
                                    if (!p.Exists())
                                    {
                                        p.title = p.title.ToUpper();
                                        p.LoadEx();
                                        if (!p.Exists())
                                        {
                                            string s = p.title.ToLower();
                                            p.title = char.ToUpper(s[0]) + s.Substring(1);
                                        }
                                    }
                                }
                            }

                        }
                    }
                }

                if (!p.Exists())
                {
                    text = Bot.Msg("Page not found.");
                }
                else
                {
                    if (p.IsDisambig())
                    {
                    }
                    p.RemoveInterWikiLinks();
                    text = p.text;
                    String BioS = "";
                    int rep;

                    // [[...]]:
                    // [[desc]] -> desc
                    text = RecursiveReplace(text, @"\[\[(?<desc>(?!File:).[^\|\[\]]*)\]\]", "${desc}", RegexOptions.Singleline, 2, 6);
                    // [[text|desc]] -> desc
                    text = RecursiveReplace(text, @"\[\[(?<text>(?!File:).[^\|\[\]]*)\|(?<desc>.[^\|\[\]]*)\]\]", "${desc}", RegexOptions.Singleline, 1, 3);
                    // [link text] -> text
                    text = RecursiveReplace(text, @"(?<!\[)\[(?<link>.[^\[\]\s]*)\s(?<desc>.[^\[\]]*)\](?!\])", "${desc}", RegexOptions.Singleline, 1, 1);
                    // Repeat
                    text = RecursiveReplace(text, @"\[\[(?<desc>(?!File:).[^\|\[\]]*)\]\]", "${desc}", RegexOptions.Singleline, 0, 6);
                    text = RecursiveReplace(text, @"\[\[(?<text>(?!File:).[^\|\[\]]*)\|(?<desc>.[^\|\[\]]*)\]\]", "${desc}", RegexOptions.Singleline, 0, 6);
                    // [[File:...|...|...]] or any [[..|..|..|]] multi value (including the single and the dobule)
                    text = RecursiveReplace(text, @"\[\[(?<desc>File:.[^\[\]]*)\]\]", "", RegexOptions.Singleline, 1, 1);
                    // Kill any remaining [[..|..|..|]] multi value or single
                    text = RecursiveReplace(text, @"\[\[(?<desc>.[^\[\]]*)\]\]", "", RegexOptions.Singleline, 1, 1);


                    // {{*|desc}}
                    // kill {{Main|desc}}
                    // kill {{See also|desc}}
                    // kill {{pp-semi|desc}}
                    // kill {{About|desc}}
                    text = kill(text, @"About");
                    text = kill(text, @"pp-semi");
                    text = kill(text, @"Main");
                    text = kill(text, @"See\salso");
                    text = kill(text, @"Citation\sneeded");
                    text = kill(text, @"commons\scategory");
                    text = kill(text, @"reflist");
                    text = kill(text, @"Refbegin");
                    

                    // {{text|desc}} -> desc
                    text = RecursiveReplace(text, @"\{\{(?<text>.[^\|\{\}]*)\|(?<desc>.[^\|\{\}]*)\}\}", "${desc}", RegexOptions.Singleline, 1, 3);
                    // Kill any remaining {{..|..|..|}} multi value or single except for Bio
                    text = RecursiveReplace(text, @"\{\{(?<desc>(?!Bio).[^\{\}]*)\}\}", "", RegexOptions.Singleline, 1, 2);
                    // Parse Bio
                    Match Bio = Regex.Match(text, @"\{\{[\s]*Bio[^\{\}]*\}\}", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (Bio.Success)
                    {
                        BioS = text.Substring(Bio.Index + ("{{Bio".Length), Bio.Length - 2 - ("{{Bio".Length));
                        BioS = GetBioText(BioS);
                        text = BioS + Environment.NewLine + text;
                    }
                    // Kill any remaining {{..|..|..|}} multi value or single
                    text = RecursiveReplace(text, @"\{\{(?<desc>.[^\{\}]*)\}\}", "", RegexOptions.Singleline, 1, 2);
                    // {{{text}}} -> text
                    text = RecursiveReplace(text, @"\{\{\{(?<desc>.[^\{\}]*)\}\}\}", "${desc}", RegexOptions.Singleline, 1, 1);
                    // Tables
                    text = RecursiveReplace(text, @"\{\|(?<desc>.[^\{\}]*)\|\}", Bot.Msg("Missing table."), RegexOptions.Singleline, 1, 2);
                    // Repeat
                    text = RecursiveReplace(text, @"\{\{(?<text>.[^\|\{\}]*)\|(?<desc>.[^\|\{\}]*)\}\}", "${desc}", RegexOptions.Singleline, 1, 3);
                    text = RecursiveReplace(text, @"\{\{(?<desc>.[^\{\}]*)\}\}", "", RegexOptions.Singleline, 1, 2);
                    text = RecursiveReplace(text, @"\{\{\{(?<desc>.[^\{\}]*)\}\}\}", "${desc}", RegexOptions.Singleline, 1, 1);


                    // Get rid of bold, italic, bold+italic and improper nestings
                    text = Regex.Replace(text, "''''(?<text>.[^']*)''''", "${text}", RegexOptions.Singleline);
                    text = Regex.Replace(text, "'''(?<text>.[^']*)'''", "${text}", RegexOptions.Singleline);
                    text = Regex.Replace(text, "''(?<text>.[^']*)''", "${text}", RegexOptions.Singleline);
                    text = Regex.Replace(text, "'''(?<text>.[^']*)'''", "${text}", RegexOptions.Singleline);
                    text = Regex.Replace(text, "''''(?<text>.[^']*)''''", "${text}", RegexOptions.Singleline);

                    // ==text== -> Sezione text
                    String SectionStr=Bot.Msg("Section ${text}:");
                    String SubSectionStr = Bot.Msg("Subsection ${text}:");
                    text = Regex.Replace(text, @"^(\s*)======(?<text>.[^=\n\r]*)======", SubSectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)=====(?<text>.[^=\n\r]*)=====", SubSectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)====(?<text>.[^=\n\r]*)====", SubSectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)===(?<text>.[^=\n\r]*)===", SubSectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)==(?<text>.[^=\n\r]*)==", SectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)=(?<text>.[^=\n\r]*)=", SectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)======(?<text>.[^=\n\r]*)", SubSectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)=====(?<text>.[^=\n\r]*)", SubSectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)====(?<text>.[^=\n\r]*)", SubSectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)===(?<text>.[^=\n\r]*)", SubSectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)==(?<text>.[^=\n\r]*)", SectionStr, RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)=(?<text>.[^=\n\r]*)", SectionStr, RegexOptions.Multiline);

                    // Kill ---- 
                    text = Regex.Replace(text, @"----(-)*", Environment.NewLine, RegexOptions.Singleline);

                    // Kill lists
                    text = Regex.Replace(text, @"^(\s*)\*+", "", RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*)#+", "", RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*):+", "", RegexOptions.Multiline);
                    text = Regex.Replace(text, @"^(\s*);+", "", RegexOptions.Multiline);

                    // Kill HTML 
                    // <br /> <br> -> \n
                    text = Regex.Replace(text, @"<br(\s*)>", "", RegexOptions.IgnoreCase);
                    text = Regex.Replace(text, @"<br(\s*)/>", "", RegexOptions.IgnoreCase);
                    // Kill any single operator
                    text = Regex.Replace(text, @"<[^<>]*/>", "", RegexOptions.Singleline);
                    for (rep = 0; rep < 4; rep++)
                    {
                        // Kill ref
                        text = Regex.Replace(text, @"<ref[^<>]*>[^<>]*</ref[^<>]*>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                        // Kill comments
                        text = Regex.Replace(text, @"<!--[^<>]*-->", "", RegexOptions.Singleline);
                    }
                    // <..> text </..> -> text
                    text = RecursiveReplace(text, @"<([A-Z][A-Z0-9]*)\b[^>]*>(?<text>.[^<>]*)</\1>", "${text}", RegexOptions.Singleline | RegexOptions.IgnoreCase, 1, 10);
                    // Kill any remaining HTML TAG
                    text = Regex.Replace(text, @"<[^<>]*>", "", RegexOptions.Singleline);
                    // &nbsp; -> " "
                    text = Regex.Replace(text, @"&nbsp;", " ", RegexOptions.Singleline);

                    // Kill double space/newlines
                    text = Regex.Replace(text, @"[ ]{2,}", " ", RegexOptions.None);
                    text = Regex.Replace(text, @" (?<caract>[,\.\n\r])", "${caract}", RegexOptions.None);
                    text = Regex.Replace(text, @"[\n]{2,}", "\n", RegexOptions.None);
                    text = Regex.Replace(text, @"[\r]{2,}", "\r", RegexOptions.None);
                    //text = Regex.Replace(text, @"\n\r", "\r\n", RegexOptions.None);   // adjust irregular CRLF sequences
                    text = RecursiveReplace(text, @"\r\n\r\n", "\r\n", RegexOptions.None, 1, 3);

                    // Stop at references
                    Match end = Regex.Match(text, @"<references(\s*)/>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    if (end.Success) text = text.Substring(0, end.Index);

                    // Kill the space at the beginning
                    text = Regex.Replace(text, @"^(\s*)", "", RegexOptions.Singleline);
                    text = p.title + "." + Environment.NewLine + Environment.NewLine + text;


                    // Roman numbers
                    text = RomanToNumberConvertor.Convert(text, language);

                    // Localized rules
                    SortedDictionary<string, string>.Enumerator T = Bot.rules.GetEnumerator();
                    while (T.MoveNext())
                    {
                        text = Regex.Replace(text, T.Current.Key, T.Current.Value, RegexOptions.Singleline);
                    } 
                }
            }

            System.Console.WriteLine(text);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new WikiReader(text, voice_name));
        }
    }
}
