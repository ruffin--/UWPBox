// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

using System;

namespace Rufwork.Convenience
{
    internal static class ExceptionExtensions
    {
        public static string Log_(this Exception e, string strLocation, string strAddlInfo = "")
        {
            string ret = $@"{strLocation} #{e.Message}#
    Trace: {e.StackTrace}
    {strAddlInfo}";

            ret.LogMsg();

            return ret;
        }
    }
}
