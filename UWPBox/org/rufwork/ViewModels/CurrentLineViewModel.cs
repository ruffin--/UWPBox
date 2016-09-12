using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace org.rufwork.ViewModels
{
    public class CurrentLineViewModel
    {
        public string leading = "";
        public string trailing = "";
        //public string lineEnding = "";
        //public int lineStart = int.MinValue;

        public string fullLineWithoutEnding
        {
            get {
                return leading + trailing;
            }
        }
    }
}
