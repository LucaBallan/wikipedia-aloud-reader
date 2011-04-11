//
// found in http://www.blackwasp.co.uk/RomanToNumber.aspx

using System;
using System.Collections;
using System.Text.RegularExpressions;

namespace WikiReader
{
    static class RomanToNumberConvertor
    {
        private enum RomanDigit
        {
            I = 1,
            V = 5,
            X = 10,
            L = 50,
            C = 100,
            D = 500,
            M = 1000
        }



        /// <summary>Converts a Roman numerals value 
        /// into an integer</summary>
        static public int RomanToNumber(String roman)
        {
            // Rule 7
            roman = roman.ToUpper().Trim();
            if (roman == "N") return 0;

            // Rule 4
            if (roman.Split('V').Length > 2 ||
                roman.Split('L').Length > 2 ||
                roman.Split('D').Length > 2)
                throw new ArgumentException("Rule 4");

            // Rule 1
            int count = 1;
            char last = 'Z';
            foreach (char numeral in roman)
            {
                // Valid character?
                if ("IVXLCDM".IndexOf(numeral) == -1)
                    throw new ArgumentException("Invalid numeral");

                // Duplicate?
                if (numeral == last)
                {
                    count++;
                    if (count == 4)
                        throw new ArgumentException("Rule 1");
                }
                else
                {
                    count = 1;
                    last = numeral;
                }
            }

            // Create an ArrayList containing the values
            int ptr = 0;
            ArrayList values = new ArrayList();
            int maxDigit = 1000;
            while (ptr < roman.Length)
            {
                // Base value of digit
                char numeral = roman[ptr];
                int digit = (int)Enum.Parse(typeof(RomanDigit), numeral.ToString());

                // Rule 3
                if (digit > maxDigit)
                    throw new ArgumentException("Rule 3");

                // Next digit
                int nextDigit = 0;
                if (ptr < roman.Length - 1)
                {
                    char nextNumeral = roman[ptr + 1];
                    nextDigit = (int)Enum.Parse(typeof(RomanDigit), nextNumeral.ToString());

                    if (nextDigit > digit)
                    {
                        if ("IXC".IndexOf(numeral) == -1 ||
                            nextDigit > (digit * 10) ||
                            roman.Split(numeral).Length > 3)
                            throw new ArgumentException("Rule 3");

                        maxDigit = digit - 1;
                        digit = nextDigit - digit;
                        ptr++;
                    }
                }

                values.Add(digit);

                // Next digit
                ptr++;
            }

            // Rule 5
            for (int i = 0; i < values.Count - 1; i++)
                if ((int)values[i] < (int)values[i + 1])
                    throw new ArgumentException("Rule 5");

            // Rule 2
            int total = 0;
            foreach (int digit in values)
                total += digit;

            return total;
        }

        /// <summary>Parse an entire text converting Roman 
        /// numbers into arabic ones.
        /// NOTE: To speed up the procedure eliminate before
        /// double spaces.</summary>
        static public String Convert(String text, String language)
        {
            
            //Match RomanNumbers = Regex.Match(text, @"(?<=\s)(?![\s,\.\)])M{0,4}(CM|CD|D?C{0,3})(XC|XL|L?X{0,3})(IX|IV|V?I{0,3})(?=\W)", RegexOptions.Singleline);
			Match RomanNumbers = Regex.Match(text, @"(?<=\s)(?![\s,\.\)])(X{0,3})(IX|IV|V?I{0,3})(?=\W)", RegexOptions.Singleline);

            int offset = 0;
            while (RomanNumbers.Success)
            {
                if (RomanNumbers.Length == 0)
                {
                    RomanNumbers = RomanNumbers.NextMatch();
                    continue;
                }
                String RomanNumber = text.Substring(RomanNumbers.Index + offset, RomanNumbers.Length);
                if ((RomanNumbers.Length == 1) && (RomanNumber.EndsWith(" ")))
                {
                    RomanNumbers = RomanNumbers.NextMatch();
                    continue;
                }

                String BeforeRomanNumber = text.Substring(0, RomanNumbers.Index + offset);
                String AfterRomanNumber  = text.Substring(RomanNumbers.Index + RomanNumbers.Length + offset);

                if (RomanNumbers.Length == 1) 
                {
                    if ((AfterRomanNumber.StartsWith("'")) || (AfterRomanNumber.StartsWith("`")))
                    {
                        RomanNumbers = RomanNumbers.NextMatch();
                        continue;
                    }
                    if (RomanNumber.EndsWith("I"))
                    {
                        // ambiguous
                        RomanNumbers = RomanNumbers.NextMatch();
                        continue;
                    }
                }
                /*
                if ((RomanNumbers.Length == 1) && (AfterRomanNumber.StartsWith(".")) && ( (RomanNumber.EndsWith("M")) || (RomanNumber.EndsWith("L")) || (RomanNumber.EndsWith("C")) || (RomanNumber.EndsWith("D"))  ) )
                {
                    RomanNumbers = RomanNumbers.NextMatch();
                    continue;
                }*/


                try
                {
                    int origina_len = RomanNumber.Length;
                    RomanNumber = RomanToNumber(RomanNumber).ToString();
                    if (language == "en")
                    {
                        if (RomanNumber.EndsWith("1"))
                        {
                            RomanNumber += "st";
                        }
                        else
                        {
                            if (RomanNumber.EndsWith("2"))
                            {
                                RomanNumber += "nd";
                            }
                            else
                            {
                                if (RomanNumber.EndsWith("3"))
                                {
                                    RomanNumber += "rd";
                                }
                                else
                                {
                                    RomanNumber += "th";
                                }
                            }
                        }
                    }
                    if (language == "it")
                    {
                        RomanNumber += "°";
                    }
                    offset += RomanNumber.Length - origina_len;
                }
                catch (Exception)
                {
                }
                text = BeforeRomanNumber + RomanNumber + AfterRomanNumber;
                RomanNumbers = RomanNumbers.NextMatch();
            }
            return text;
        }
    }

}
