// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;

namespace Rufwork.Convenience
{
    public static class ClipboardConvenience
    {
        /// <summary>
        /// Will return a Microsoft formatted HTML snippet if the clipboard contains it, otherwise
        /// plain text.
        ///
        /// Passing returnFragmentOnly as false, the default, will return "full" clipboard contents
        /// in "raw" format:
        /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/ms649015(v=vs.85).aspx
        ///
        /// You may also want to see this convenience wrapper that parses html fragments into view models:
        /// https://github.com/ruffin--/HtmlFragmentHelper
        ///
        /// Passing returnHtmlClipboardRaw as true will only return the HTML code from the selected fragment
        /// without any clipboard metadata or HTML header information. See `returnFragmentOnly` comments
        /// for more details.
        ///
        /// If clipboard isn't in HTML format, this will attempt to return the clipboard's contents as text.
        /// </summary>
        /// <param name="returnFragmentOnly">If false, the clipboard will be returned as a  full
        /// Microsoft html fragment be returned. True will return only the html fragment between
        /// "&lt;!--StartFragment-->" and &lt;!--EndFragment--> tags without metadata or any html
        /// outside of those two tags.</param>
        /// <returns>Returns ONE of
        /// 1.) Full html Microsoft clipboard as text,
        /// 2.) The html fragment of the clipboard as text, or
        /// 3.) Non-html format clipboard contents as plain text.</returns>
        public static async Task<string> ClipboardAsHtml(bool returnFragmentOnly = false)
        {
            string ret = string.Empty;
            DataPackageView dataPackageView = Clipboard.GetContent();

            if (dataPackageView.Contains(StandardDataFormats.Html))
            {
                ret = await Clipboard.GetContent().GetHtmlFormatAsync();

                if (returnFragmentOnly)
                {
                    string delimiterStartAfter = "<!--StartFragment-->";
                    string delimiterEndBefore = "<!--EndFragment-->";

                    if (-1 < ret.IndexOf(delimiterStartAfter))
                    {
                        ret = ret.Substring(ret.IndexOf(delimiterStartAfter) + delimiterStartAfter.Length);
                        if (-1 < ret.IndexOf(delimiterEndBefore))
                        {
                            ret = ret.Substring(0, ret.IndexOf(delimiterEndBefore));
                        }
                        else
                        {
                            ret = string.Empty;  // No luck, Ending not after Start; go back to nothing.
                        }
                    }
                }
            }
            else if (dataPackageView.Contains(StandardDataFormats.Text))
            {
                ret = await Clipboard.GetContent().GetTextAsync();
            }

            return ret;
        }

        public static async Task<string> ClipboardAsText()
        {
            return await Clipboard.GetContent().GetTextAsync();
        }
    }
}
