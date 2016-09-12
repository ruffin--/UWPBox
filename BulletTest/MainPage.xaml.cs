// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

using org.rufwork.UI.widgets;
using org.rufwork.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace BulletTest
{
    /// <summary>base
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        //public BulletBox txt = new BulletBox();
        public UWPBox txt;

        public MainPage()
        {
            this.txt = new UWPBox();

            this.InitializeComponent();
            this.SetupGrid();
        }

        public void SetupGrid()
        {
            ColumnDefinition col1 = new ColumnDefinition();
            RowDefinition row1 = new RowDefinition();
            RowDefinition row2 = new RowDefinition();

            col1.Width = new GridLength(1, GridUnitType.Star);
            row1.Height = new GridLength(30, GridUnitType.Pixel);
            row2.Height = new GridLength(1, GridUnitType.Star);

            MyGrid.ColumnDefinitions.Add(col1);
            MyGrid.RowDefinitions.Add(row1);
            MyGrid.RowDefinitions.Add(row2);

            Grid.SetRow(txt, 1);
            Grid.SetColumn(txt, 0);

            Button cmdGo = new Button();
            cmdGo.Content = "Go";
            cmdGo.Click += CmdGo_Click;
            Grid.SetRow(cmdGo, 0);
            Grid.SetColumn(cmdGo, 0);

            MyGrid.Children.Add(cmdGo);
            MyGrid.Children.Add(txt);

            this.txt.Text = @"Call me Ishmael. Some years ago--never mind how long precisely--having
little or no money in my purse, and nothing particular to interest me on
shore, I thought I would sail about a little and see the watery part of
the world. It is a way I have of driving off the spleen and regulating
the circulation. Whenever I find myself growing grim about the mouth;
whenever it is a damp, drizzly November in my soul; whenever I find
myself involuntarily pausing before coffin warehouses, and bringing up
the rear of every funeral I meet; and especially whenever my hypos get
such an upper hand of me, that it requires a strong moral principle to
prevent me from deliberately stepping into the street, and methodically
knocking people's hats off--then, I account it high time to get to sea
as soon as I can. This is my substitute for pistol and ball. With a
philosophical flourish Cato throws himself upon his sword; I quietly
take to the ship. There is nothing surprising in this. If they but knew
it, almost all men in their degree, some time or other, cherish very
nearly the same feelings towards the ocean with me.";

        }

        private void CmdGo_Click(object sender, RoutedEventArgs e)
        {
            //this.txt.Focus(FocusState.Programmatic);
            //this.txt.SelectionStart = 1;
            //this.txt.SelectionLength = 8;

            CurrentLineViewModel line = this.txt.GetCurrentLine();

            System.Diagnostics.Debug.WriteLine(string.Format(@"#{0}#
#{1}#", line.leading, line.trailing));

            this.txt.Focus(FocusState.Programmatic);
            this.txt.SelectCurrentLine();
        }
    }
}
