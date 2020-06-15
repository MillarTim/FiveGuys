using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
namespace CSS.Connector.FileProcessors.Vanguard
{
    public static class CobolUtility
    {
		readonly static string codeCharsPositive = "{ABCDEFGHI";
		readonly static string codeCharsNegative = "}JKLMNOPQR";

		public static string ConvertFromCOBOLSignedNumberToDouble1(string quantity, int decimalPlaces)
        {
            // COBOL Signed Number Converter
            string codeChar;
            double convertedNumber;
            string numberSign = string.Empty;
            int length = quantity.Trim().Length;
            string signedNumber = length > 0 ? quantity.ToUpper().Trim().Substring(length - 1) : "0";

            //ClrHelper.ArgumentIsNotEmpty(signedNumber, "Signed Number");

            codeChar = signedNumber.Substring(signedNumber.Length - 1);
           
            if (codeCharsPositive.IndexOf(codeChar) > -1)
            {
                // For Positive Numbers, the last digit is converted from
                // "{ABCDEFGHI" to "0123456789" respectively.

                signedNumber = (signedNumber.Substring(0, signedNumber.Length - 1) + codeCharsPositive.IndexOf(codeChar));
                convertedNumber = Convert.ToDouble(signedNumber);
            }
            else if (codeCharsNegative.IndexOf(codeChar) > -1)
            {
                // For Negative Numbers, the last digit is converted from
                // "}JKLMNOPQR" to "0123456789" respectively.

                signedNumber = signedNumber.Substring(0, signedNumber.Length - 1) + codeCharsNegative.IndexOf(codeChar);
                convertedNumber = Convert.ToDouble(signedNumber);
                numberSign = "-";
            }
            else
                // Not a COBOL Signed Number, but still apply decimal places
                convertedNumber = Convert.ToDouble(signedNumber);

            if (decimalPlaces > 0)
                convertedNumber = (convertedNumber / (10 ^ decimalPlaces));

            return RemoveLastCharacter(quantity) + convertedNumber.ToString(CultureInfo.CurrentCulture) + numberSign.ToString(CultureInfo.CurrentCulture);
        }

		// ref to indicate
		internal static string FormatString(string stringToFormat, string formattingString)
		{
			string outputString = stringToFormat;
			string[] formattingStrings = formattingString.Split('~');
			// formattingStrings should have 3 or 4 components. 3 for dates & 4 for decimalls
			int formattingStringCount = formattingStrings.Count();
			if (formattingStringCount != 3 && formattingStringCount != 4) throw new System.Data.DataException(string.Format("Formatting string ({0}) is invalid.  It must be tilde delimited with three or four components.  See code for more details.", formattingString));

			if (!Regex.IsMatch(outputString, formattingStrings[0]))
				throw new System.Data.DataException(string.Format("String to format ({0}) does not match expected format ({1}).", stringToFormat, formattingString));

			outputString = Regex.Replace(outputString, formattingStrings[0], formattingStrings[1]);

			// IBM binary coded decimal where last digit defines least significant digit and sign of the entire number
			if (formattingStrings[2] == "EBCDIC_BCD")
			{
				string rightMostDigit = outputString[outputString.Length - 1].ToString();
				outputString = outputString.Substring(0, outputString.Length - 1);
				int index = codeCharsNegative.IndexOf(rightMostDigit);
				if (index > -1)
				{
					outputString = "-" + outputString + index.ToString();
				}
				else
					outputString += codeCharsPositive.IndexOf(rightMostDigit).ToString();
				formattingStrings[2] = "Decimal"; // Once data has been fixed, it's a normal decimal
			}

			if (formattingStringCount == 4)
			{
				TypeCode typeCode = (TypeCode)Enum.Parse(typeof(TypeCode), formattingStrings[2], true);
				outputString = string.Format(formattingStrings[3], Convert.ChangeType(outputString, typeCode));
			}

			/*
			if (formattingStrings.Count() > 2)
			{
				string inputRegex = formattingStrings[0];
			}

			string[] formattingStrings = concatenatedFormattingString.Split('~');
			if (formattingStrings.Length != 2 && formattingStrings.Length != 4) return "*" + fieldValue;
			string Stage1FormatValue = Regex.Replace(fieldValue, formattingStrings[0], formattingStrings[1]);
			if (formattingStrings.Length == 2) return Stage1FormatValue;

			// To use string.format to format the value, determine what data type is being converted
			TypeCode typeCode = (TypeCode)Enum.Parse(typeof(TypeCode), formattingStrings[2], true);
			// Then using the format string (4th item
			string Stage2FormatValue = string.Format(
				formattingStrings[3],
				Convert.ChangeType(
					Stage1FormatValue,
					typeCode));

			return Stage2FormatValue;
			*/
			return outputString;
		}

		internal static string RemoveLastCharacter(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;
            else
                return value.TrimEnd(value[value.Length - 1]);
        }
        internal static string InsertImpliedDecimal(string input,int decimalPlaces)
        {
            return input.Substring(0, input.Length - decimalPlaces) + "." + input.Substring(input.Length - decimalPlaces, decimalPlaces);
        }
    }
}
