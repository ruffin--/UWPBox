﻿// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rufwork.Convenience
{
    // I have most of these in RufworkExtensions (https://github.com/ruffin--/RufworkExtensions),
    // but I'm going to embed here (with stodgily appended underscores to method names) to make
    // this package easier to reuse.
    public static class StringExtensions
    {
        private static Regex _regexStartDigitsPeriod = new Regex(@"^\d+\. ");
        private static Regex _regexNoContentOL = new Regex(@"^\s*[0-9]+\. $");

        public static string NormalizeNewlineToCarriageReturn_(this string str)
        {
            str = str.Replace("\r\n", "\r");
            str = str.Replace("\n", "\r");
            return str;
        }

        // For some reason, the PCL wasn't supporting TakeWhile, so I skipped on this LINQ heavy solution:
        // string toPrepend = string.Concat(value.TakeWhile(c => c.Equals(' '))); // ...
        public static Tuple<string, string> PullLeadingAndTrailingSpaces_(this string value)
        {
            // I don't normally return like this. Sorry.
            if (string.IsNullOrEmpty(value))
                return new Tuple<string, string>(string.Empty, string.Empty);

            if (string.IsNullOrWhiteSpace(value))
                return new Tuple<string, string>(value, string.Empty);

            int prependCount = 0;
            int appendCount = 0;

            int i = 0;
            while (value[i++].Equals(' '))
                prependCount++;

            i = value.Length - 1;
            while (value[i--].Equals(' '))
                appendCount++;

            return new Tuple<string, string>(new string(' ', prependCount), new string(' ', appendCount));
        }

        // Note that this doesn't work with Hebrew characters with vowels,
        // apparently (though you could argue it kind of does, iiuc)
        // See stackoverflow.com/questions/15029238
        public static string ReverseString_(this string str)
        {
            if (null == str)
                return null;

            char[] aReverseMe = str.ToCharArray();
            Array.Reverse(aReverseMe);
            return new string(aReverseMe);
        }

        public static string Slice_(this string str, int intSlice)
        {
            string ret = str;

            if (0 == intSlice)
            {
                ret = str;
            }
            else if (intSlice > 0)
            {
                ret = str.Substring(0, intSlice);
            }
            else
            {
                // Remember that intSlice is negative here
                ret = str.Remove(str.Length + intSlice);
            }
            return ret;
        }

        public static QuackResult QuacksLikeOrderedList_(this string fullLineWithoutEnding, int tabLengthInSpaces)
        {

            QuackResult result = new QuackResult();
            var match = _regexStartDigitsPeriod.Match(fullLineWithoutEnding.TrimStart());

            int spaceCount = fullLineWithoutEnding.TakeWhile(c => c.Equals(' ')).Count();
            result.QuacksLikeAnOL = 0 == spaceCount % tabLengthInSpaces
                && match.Success;

            if (match.Success)
            {
                result.HasContent = !_regexNoContentOL.IsMatch(fullLineWithoutEnding);
                result.ListOrdinal = int.Parse(match.Value.Trim().Trim('.'));
                result.MatchLength = match.Length;
                result.MatchStart = match.Index + spaceCount;
            }

            return result;
        }

        public static bool CouldBeUrl_(this string str)
        {
            //return str.StartsWith("http") && str.Contains("://");
            return Uri.IsWellFormedUriString(str, UriKind.Absolute);
        }

        public static string Splice_(this string str, string toInsert, int spliceLoc)
        {
            string ret = str;
            if (!string.IsNullOrEmpty(str) && str.Length >= spliceLoc && spliceLoc > -1)
            {
                ret = str.Substring(0, spliceLoc) + toInsert + str.Substring(spliceLoc);
            }
            return ret;
        }

        // No underscore here. It's a stand-in for ErrHand.LogMsg.
        public static void LogMsg(this string strMsg)
        {
            System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ": " + strMsg);
        }

    }

    public class QuackResult
    {
        public bool QuacksLikeAnOL = false;
        public bool HasContent = false;
        public int ListOrdinal = -1;
        public int MatchLength = -1;
        public int MatchStart = -1;

        public override string ToString()
        {
            return "Quacks? " + this.QuacksLikeAnOL
                + " :: HasContent? " + this.HasContent
                + " :: ListOrdinal: " + this.ListOrdinal
                + " :: MatchLength: " + this.MatchLength
                + " :: MatchStart: " + this.MatchStart;
        }
    }
}
