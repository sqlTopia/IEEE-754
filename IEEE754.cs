using System.Data.SqlTypes;
using System;

public partial class Database
{
        [Microsoft.SqlServer.Server.SqlFunction]
        public static SqlString IEEE754(byte[] input)
        {
                string bits = "";
                string sign;
                string exponent;
                string fraction;

                switch (input.Length)
                {
                        case 2:
                                bits = bitstring(input);
                                
                                sign = bits.Substring(0, 1);

                                exponent = bits.Substring(1, 5);
                                exponent = alignbits(exponent, "1");
                                exponent = (Convert.ToInt32(exponent) - 15).ToString();
                                exponent = powerof2(Convert.ToInt32(exponent));

                                fraction = bits.Substring(6, 10);
                                fraction = alignbits(fraction, "0.5");

                                break;
                        case 4:
                                bits = bitstring(input);

                                sign = bits.Substring(0, 1);

                                exponent = bits.Substring(1, 8);
                                exponent = alignbits(exponent, "1");
                                exponent = (Convert.ToInt32(exponent) - 127).ToString();
                                exponent = powerof2(Convert.ToInt32(exponent));

                                fraction = bits.Substring(9, 23);
                                fraction = alignbits(fraction, "0.5");

                                break;
                        case 8:
                                bits = bitstring(input);

                                sign = bits.Substring(0, 1);

                                exponent = bits.Substring(1, 11);
                                exponent = alignbits(exponent, "1");
                                exponent = (Convert.ToInt32(exponent) - 1023).ToString();
                                exponent = powerof2(Convert.ToInt32(exponent));

                                fraction = bits.Substring(12, 52);
                                fraction = alignbits(fraction, "0.5");

                                break;
                        case 16:
                                bits = bitstring(input);
                                
                                sign = bits.Substring(0, 1);

                                exponent = bits.Substring(1, 15);
                                exponent = alignbits(exponent, "1");
                                exponent = (Convert.ToInt32(exponent) - 16383).ToString();
                                exponent = powerof2(Convert.ToInt32(exponent));

                                fraction = bits.Substring(16, 112);
                                fraction = alignbits(fraction, "0.5");

                                break;
                        case 32:
                                bits = bitstring(input);
                                
                                sign = bits.Substring(0, 1);

                                exponent = bits.Substring(1, 19);
                                exponent = alignbits(exponent, "1");
                                exponent = (Convert.ToInt32(exponent) - 262143).ToString();
                                exponent = powerof2(Convert.ToInt32(exponent));

                                fraction = bits.Substring(20, 236);
                                fraction = alignbits(fraction, "0.5");

                                break;
                        default:
                                return SqlString.Null;
                }

                fraction = stringsum(fraction, "1");
                exponent = stringmultiply(exponent, fraction);

                if (sign=="1")
                {
                        exponent = "-" + exponent;
                }

                return new SqlString(exponent);
        }
        static string bitstring(byte[] input)
        {
                string result = "";

                for (int index = 0; index < input.Length; index++)
                {
                        result += Convert.ToString(input[index], 2).PadLeft(8, '0');
                }

                return result;
        }
        static string alignbits(string bitmap, string input)
        {
                string result;

                if (input.StartsWith("0."))
                {
                        result = "0.0";

                        for (int index = 0; index < bitmap.Length; index++)
                        {
                                if (bitmap.Substring(index, 1) == "1")
                                {
                                        result = stringsum(result, input);
                                }

                                input = stringdivide(input);
                        }
                }
                else
                {
                        result = "0";

                        for (int index = bitmap.Length - 1; index >= 0; --index)
                        {
                                if (bitmap.Substring(index, 1) == "1")
                                {
                                        result = stringsum(result, input);
                                }

                                input = stringmultiply(input, "2");
                        }
                }

                return result;
        }
        static string powerof2(int exponent)
        {
                if (exponent == 0)
                {
                        return "1";
                }

                string intermediate;
                string result;
                byte memory;
                byte digit;

                if (exponent >= 1 && exponent <= 262143)
                {
                        result = "2";

                        for (int index = 2; index <= exponent; index++)
                        {
                                result = stringmultiply(result, "2");
                        }

                        return result;
                }

                if (exponent >= -262142 && exponent <= -1)
                {
                        result = "0.5";

                        for (int index = -2; index >= exponent; --index)
                        {
                                result = stringdivide(result);
                        }

                        return result;
                }

                return "";
        }
        static string stringmultiply(string factor1, string factor2)
        {
                factor1 = cleanup(factor1);
                factor2 = cleanup(factor2);

                int digits = 0;
                
                if (factor1.IndexOf('.') >= 0)
                {
                        digits = digits + factor1.Length - factor1.IndexOf('.') - 1;
                }

                factor1 = factor1.TrimStart('0');
                factor1 = factor1.TrimStart('.');
                factor1 = factor1.TrimStart('0');
                factor1 = factor1.Replace(".", "");

                if (factor2.IndexOf('.') >= 0)
                {
                        digits = digits + factor2.Length - factor2.IndexOf('.') - 1;
                }

                factor2 = factor2.TrimStart('0');
                factor2 = factor2.TrimStart('.');
                factor2 = factor2.TrimStart('0');
                factor2 = factor2.Replace(".", "");

                string result = "";
                int index = -1;
                int memory;
                string intermediate;

                for (int position2 = factor2.Length - 1; position2 >= 0; --position2)
                {
                        memory = 0;

                        index++;
                        intermediate = new String('0', index);

                        for (int position1 = factor1.Length - 1; position1 >= 0; --position1)
                        {
                                memory += (int)(Char.GetNumericValue(factor2, position2)) * (int)(Char.GetNumericValue(factor1, position1));

                                intermediate = (memory % 10).ToString() + intermediate;
                                memory /= 10;
                        }

                        if (memory >= 1)
                        {
                                intermediate = memory.ToString() + intermediate;
                        }

                        result = stringsum(result, intermediate);
                }

                if (result.Length <= digits)
                {
                        result = "0." + result.PadLeft(digits, '0');
                }
                else if (digits >= 1)
                {
                        result = result.Substring(0, result.Length - digits) + "." + result.Substring(result.Length - digits, digits);
                }

                return cleanup(result);
        }
        static string stringsum(string term1, string term2)
        {
                term1 = cleanup(term1);
                term2 = cleanup(term2);

                // Split terms in integer and decimal parts
                int position1 = term1.IndexOf('.');
                int position2 = term2.IndexOf('.');

                string t1 = "";
                string t2 = "";

                string integerpart = "";
                string decimalpart = "";

                int memory = 0;

                // Take care of decimal parts
                if (position1 >= 0 && position1 < term1.Length - 1)
                {
                        t1 = term1.Substring(position1 + 1, term1.Length - position1 - 1);
                        t1.TrimEnd('0');
                }

                if (position2 >= 0 && position2 < term2.Length - 1)
                {
                        t2 = term2.Substring(position2 + 1, term2.Length - position2 - 1);
                        t2.TrimEnd('0');
                }

                if (t1 == "" && t2 == "")
                {
                        decimalpart = "";
                }
                else if (t1 == "")
                {
                        decimalpart = "." + t2;
                }
                else if (t2 == "")
                {
                        decimalpart = "." + t1;
                }
                else
                {
                        int index = Math.Max(t1.Length - 1, t2.Length - 1);

                        while (index >= 0)
                        {
                                if (index < t1.Length && index < t2.Length)
                                {
                                        memory += (int)(Char.GetNumericValue(t1, index)) + (int)(Char.GetNumericValue(t2, index));
                                }
                                else if (index < t1.Length)
                                {
                                        memory += (int)(Char.GetNumericValue(t1, index));
                                }
                                else
                                {
                                        memory += (int)(Char.GetNumericValue(t2, index));
                                }

                                if (memory >= 10)
                                {
                                        decimalpart = (memory - 10).ToString() + decimalpart;
                                        memory = 1;
                                }
                                else
                                {
                                        decimalpart = memory.ToString() + decimalpart;
                                        memory = 0;
                                }

                                --index;
                        }

                        decimalpart = decimalpart.TrimEnd('0');

                        if (decimalpart != "")
                        {
                                decimalpart = "." + decimalpart;
                        }
                }

                // Take care of integer parts
                if (position1 == 0)
                {
                        t1 = "0";
                }
                else if (position1 == -1)
                {
                        t1 = term1.TrimStart('0');
                }
                else
                {
                        t1 = term1.Substring(0, position1).TrimStart('0');
                }

                if (t1 == "")
                {
                        t1 = "0";
                }

                if (position2 == 0)
                {
                        t2 = "0";
                }
                else if (position2 == -1)
                {
                        t2 = term2.TrimStart('0');
                }
                else
                {
                        t2 = term2.Substring(0, position2).TrimStart('0');
                }

                if (t2 == "")
                {
                        t2 = "0";
                }

                if (t1 == "0" && t2 == "0")
                {
                        integerpart = memory.ToString();
                }
                else
                { 
                        int index1 = t1.Length - 1;
                        int index2 = t2.Length - 1;

                        while (index1 >= 0 || index2 >= 0)
                        {
                                if (index1 >= 0 && index2 >= 0)
                                {
                                        memory += (int)(Char.GetNumericValue(t1, index1)) + (int)(Char.GetNumericValue(t2, index2));
                                }
                                else if (index1 >= 0)
                                {
                                        memory += (int)(Char.GetNumericValue(t1, index1));
                                }
                                else
                                {
                                        memory += (int)(Char.GetNumericValue(t2, index2));
                                }

                                if (memory >= 10)
                                {
                                        integerpart = (memory - 10).ToString() + integerpart;
                                        memory = 1;
                                }
                                else
                                {
                                        integerpart = memory.ToString() + integerpart;
                                        memory = 0;
                                }

                                --index1;
                                --index2;
                        }

                        if (memory == 1)
                        {
                                integerpart = "1" + integerpart;
                        }
                }

                return cleanup(integerpart + decimalpart);
        }
        static string stringdivide(string input)
        {
                string result = "0.";
                int memory = 0;
                int digit;

                for (int position = 2; position < input.Length; position++)
                {
                        memory = 10 * memory + (int)(Char.GetNumericValue(input, position));
                        digit = memory / 2;
                        result += digit.ToString();
                        memory -= 2 * digit;
                }

                return result + "5";
        }
        static string cleanup(string input)
        {
                input = input.TrimStart('0');           // Delete leading zeros

                if (input.IndexOf('.') >= 0)
                {
                        input = input.TrimEnd('0');     // Delete trailing zeros
                        input = input.TrimEnd('.');     // Delete trailing decimal separator
                }

                if (input == "" || input.StartsWith("."))
                {
                        input = "0" + input;
                }

                return input;
        }
}
