using System.Text.RegularExpressions;

namespace org.rufwork.ViewModels
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
