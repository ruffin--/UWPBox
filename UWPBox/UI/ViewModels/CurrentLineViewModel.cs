﻿// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

namespace Rufwork.UI.ViewModels
{
    public class CurrentLineViewModel
    {
        public string leading = "";
        public string trailing = "";

        public string fullLineWithoutEnding
        {
            get {
                return leading + trailing;
            }
        }
    }
}
