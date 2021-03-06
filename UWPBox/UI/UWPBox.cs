﻿// =========================== LICENSE ===============================
// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.
// ======================== EO LICENSE ===============================

using Rufwork.Convenience;
using Rufwork.UI.ViewModels;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Rufwork.UI
{
    public class UWPBox : TextBox
    {
        public static string Tab = "    ";
        public static char[] ac0D0A = { '\r', '\n' };


        // Events
        public delegate void SearchWrappedHandler();
        public event SearchWrappedHandler SearchWrapped;

        // There are strange things afoot at the Circle K when it comes to SelectedText
        // and NewLines as of 20160202. See here:
        // http://stackoverflow.com/questions/35138047/textbox-text-substringtextbox-selectionstart-doesnt-work-because-selectedtext
        // The fix here isn't horribly efficient, but since it's only going to happen on user
        // selection, we actually have some time to do these string manipulations in live time.
        // That is, this isn't happening 1000s of times on a background thread...

        public UWPBox() : base()
        {
            this.Name = "UWPBox: " + DateTime.Now.ToString("hh:mm ss");
            this.FontFamily = new FontFamily("Consolas");
            //this.AcceptsTab = true;   // <<< Does not exist in UWPland.
            this.AcceptsReturn = true;
            this.TextWrapping = TextWrapping.Wrap;

            //this.MobyWrapped();
            //this.BulletText();

            ScrollViewer.SetVerticalScrollBarVisibility(this, ScrollBarVisibility.Auto);
            ScrollViewer.SetHorizontalScrollBarVisibility(this, ScrollBarVisibility.Auto);

            // We've got some wacky TextBox behaviors to normalize on KeyDown,
            // the Control-I behavior, and the Tab-selects-a-new-control behavior.
            this.KeyDown += this.KeyDownHandler;
            this.KeyUp += this.KeyUpHandler;
            this.Paste += UWPBox_Paste;
        }

        //=======================================
        #region UWP TextBox kludges
        //=======================================
        // There are situations where we could not detect wackiness (we'd have to change selections
        // and sniff in those cases, which might be useful at some future point, like the wackiness
        // stops in UWP TextBoxes),but if we ever *do* find it, we don't need to check again. It'll
        // always be wacky.
        private bool? _kludgeFound = null;

        public bool KludgeItUp
        {
            get
            {
                bool kludgeIt = false;
                if (_kludgeFound.HasValue)
                {
                    return _kludgeFound.Value;
                }
                else
                {
                    switch (Environment.NewLine)
                    {
                        case "\r\n":
                            //if (this.Text.Contains('\r'))
                            // TODO: Note that we're not doing \n. I think that's okay, unless we see it in the wild.
                            if (Regex.IsMatch(this.SelectedText, "\r(?!\n)"))   // "negative lookahead"
                            {
                                kludgeIt = true;
                            }
                            else if (this.Text.Contains(Environment.NewLine))
                            {
                                // This will solve it once and for all.
                                int tempSelectionStart = this.SelectionStart;
                                int tempSelectionLength = this.SelectionLength;

                                this.SelectionStart = 0;
                                this.SelectionLength = this.Text.Length;

                                kludgeIt = !this.Text.Length.Equals(this.SelectionLength);
                                _kludgeFound = kludgeIt;

                                // Put things back where you found them.
                                this.SelectionStart = tempSelectionStart;
                                this.SelectionLength = tempSelectionLength;
                            }
                            else
                            {
                                string strSelectionByLengths = this.Text.Substring(this.SelectionStart, this.SelectionLength);
                                if (!strSelectionByLengths.Equals(this.SelectedText))
                                {
                                    kludgeIt = true;
                                }
                            }
                            break;

                        case "\n":
                        case "\r":
                            kludgeIt = false;
                            break;

                        default:
                            throw new Exception("Newline value not captured in UWPBox[1]: ("
                                + Environment.NewLine.Length + ") "
                                + Environment.NewLine.Replace('\n', 'N').Replace('\r', 'R'));
                    }
                    if (kludgeIt) _kludgeFound = true;
                }

                return kludgeIt;
            }
        }

        /// <summary>
        /// A SelectionStart that meshes with the plain Text property.
        /// </summary>
        public int SelectionStart2_ForText
        {
            get
            {
                int ret = this.SelectionStart;

                switch (Environment.NewLine)
                {
                    case "\r\n":
                        if (KludgeItUp)
                        {
                            string strWorking = this.Text.Substring(0, this.SelectionStart);
                            int lastStop = this.SelectionStart;
                            int intNLcount = 0;

                            while (strWorking.Contains("\r\n") || strWorking.Equals("\r"))
                            {
                                int nlCountFromLoop = Regex.Matches(strWorking, "\r\n").Count;
                                intNLcount += nlCountFromLoop;
                                strWorking = strWorking.Replace("\r\n", "") + this.Text.Substring(lastStop, nlCountFromLoop);

                                lastStop = lastStop + nlCountFromLoop;

                                // Peek ahead and see if this line ends with a \r\n, which it
                                // would've missed if we're in wacky land (which the _kludgeItUp
                                // check said that we are).
                                if (
                                    strWorking.EndsWith("\r") && !strWorking.Contains("\r\n")
                                    && this.Text.Length > lastStop
                                    && this.Text[lastStop].Equals('\n')
                                )   {
                                    intNLcount++;
                                    // TODO: There has to be a better way to do this. The probablem is that if "\r\n" is the
                                    // end of the selection, we get stuck in a loop where we find one more "\r\n" match, then we
                                    // remove it, then we add "\r" back when we grab the this.Text.Substring(lastStop, nlCountFromLoop)
                                    // string and add it back, as nlCountFromLoop expected to add another non-newline character.
                                    break;
                                }
                            }

                            ret = this.SelectionStart + intNLcount;
                        }
                        break;

                    case "\r":
                    case "\n":
                        ret = this.SelectionStart;
                        break;

                    default:
                        throw new Exception("Newline value not captured in UWPBox[2]: ("
                            + Environment.NewLine.Length + ") "
                            + Environment.NewLine.Replace('\n', 'N').Replace('\r', 'R'));
                }

                return ret;
            }
        }

        public int TextIndexToTextBoxLoc(int textIndex)
        {
            int ret = textIndex;

            switch (Environment.NewLine)
            {
                case "\r\n":
                    if (KludgeItUp)
                    {
                        string strWorking = this.Text.Substring(0, textIndex);
                        int intNLcount = Regex.Matches(strWorking, "\r\n").Count;
                        ret = textIndex - intNLcount;
                    }
                    break;

                case "\r":
                case "\n":
                    // We should be fine, right? One for one.
                    break;

                default:
                    throw new Exception("Newline value not captured in UWPBox[3]: ("
                        + Environment.NewLine.Length + ") "
                        + Environment.NewLine.Replace('\n', 'N').Replace('\r', 'R'));
            }

            return ret;
        }

        /// <summary>
        /// A SelectionLength that meshes with the plain Text property.
        /// </summary>
        public int SelectionLength2_ForText
        {
            get
            {
                int ret = this.SelectionLength;

                switch (Environment.NewLine)
                {
                    case "\r\n":
                        if (KludgeItUp)
                        {
                            // NOTE: Not tracking \n by itself here either, since we haven't seen that in the wild.
                            ret = this.SelectionLength + Regex.Matches(this.SelectedText, "\r(?!\n)").Count;
                        }
                        break;

                    case "\r":
                    case "\n":
                        ret = this.SelectionLength;
                        break;

                    default:
                        throw new Exception("Newline value not captured in UWPBox[3]: ("
                            + Environment.NewLine.Length + ") "
                            + Environment.NewLine.Replace('\n', 'N').Replace('\r', 'R'));
                }

                return ret;
            }
        }

        public string SelectionLineEnding
        {
            get
            {
                return KludgeItUp ? "\r" : Environment.NewLine;
            }
        }
        //=======================================
        #endregion UWP TextBox kludges
        //=======================================

        //=============================================
        #region methods that could live anywhere
        //=============================================

        // Dialog Wrapper
        public static async Task<int> ShowDialog(string strMsg, string strTitle = "", bool bShowCancel = true)
        {
            var dialog = new MessageDialog(strMsg);
            if (!string.IsNullOrWhiteSpace(strTitle))
            {
                dialog.Title = strTitle;
            }

            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            if (bShowCancel)
                dialog.Commands.Add(new UICommand { Label = "Cancel", Id = 1 });

            var res = await dialog.ShowAsync();
            return (int)res.Id;
        }
        public async void ShowErrorDialog(Exception e, string strLocation)
        {
            var errMsg = "An error occurred in a textbox:\n\n" + e.Message + "\n\n" + strLocation;
            errMsg.LogMsg();
            var dialog = new MessageDialog(errMsg);
            dialog.Title = "Issue Tracker for UWPBox";
            dialog.Commands.Add(new UICommand { Label = "Ok", Id = 0 });
            var res = await dialog.ShowAsync();

        }
        // EO Dialog Wrapper


        public CurrentLineViewModel GetCurrentLine(int intNumLinesPreviousToCurrent = 0, string strContents = null)
        {
            return this.GetCurrentLine(intNumLinesPreviousToCurrent, strContents ?? this.Text, this.SelectionStart2_ForText);
        }

        public CurrentLineViewModel GetCurrentLine(int intNumLinesPreviousToCurrent, string strContents, int intSelectionPoint)
        {
            CurrentLineViewModel vm = new CurrentLineViewModel();

            bool noValueExists = false;
            string lineLead = string.Empty;
            string lineEnd = string.Empty;

            string strLeading = strContents.Substring(0, intSelectionPoint).NormalizeNewlineToCarriageReturn_();

            // Note: Loop is only executed if we're going back one or more lines from the current.
            for (int i = 0; i < intNumLinesPreviousToCurrent; i++)
            {
                if (strLeading.Contains('\r'))
                {
                    intSelectionPoint = strLeading.LastIndexOf('\r');
                    if (intSelectionPoint < 0)
                        intSelectionPoint = 0;

                    strLeading = strLeading.Substring(0, intSelectionPoint);
                }
                else
                {
                    noValueExists = true;
                    break;
                }
            }

            if (!noValueExists)
            {
                if (strLeading.Contains('\r'))
                {
                    int intCutPoint = strLeading.LastIndexOf('\r') + 1;
                    lineLead = strLeading.Substring(intCutPoint);
                }
                else
                {
                    lineLead = strLeading;
                }

                if (0 == intNumLinesPreviousToCurrent)
                {
                    // Then we need to get the balance of the line, since the current line is what's before
                    // the cursor or selection's start to what's after, up until a new line.
                    string strTrailing = strContents.Substring(intSelectionPoint).NormalizeNewlineToCarriageReturn_();
                    if (strTrailing.Contains('\r'))
                    {
                        int intCutPoint = strTrailing.IndexOf('\r');
                        lineEnd = strTrailing.Substring(0, intCutPoint);
                    }
                    else
                    {
                        lineEnd = strTrailing;
                    }
                    vm.leading = lineLead;
                    vm.trailing = lineEnd;
                }
                else
                {
                    vm.leading = lineLead;
                    vm.trailing = string.Empty;
                }
            }

            return vm;
        }

        public void SelectCurrentLine()
        {
            CurrentLineViewModel vm = this.GetCurrentLine();

            this.SelectionStart -= vm.leading.Length;
            this.SelectionLength = vm.fullLineWithoutEnding.Length;
        }

        public int CountNewlinesPreceedingSelection()
        {
            string leading = this.Text.Substring(0, this.SelectionStart2_ForText);
            string preceeding0A0Ds = string.Concat(leading.Reverse().TakeWhile(c => ac0D0A.Contains(c)).Reverse());
            return preceeding0A0Ds.NormalizeNewlineToCarriageReturn_().Length;
        }

        // This seems slow for some reason. Sure would be great if MS
        // would fix TextBox.
        public int CountNewlinesFollowingSelection()
        {
            int originalSelectionLength = this.SelectionLength;

            string trailing0D0As = string.Empty;
            bool bContinue = true;

            while (bContinue)
            {
                int beforeLength = this.SelectedText.Length;
                this.SelectionLength++;
                string cachedSelection = this.SelectedText;

                // We increased the selection, so the selection length
                // should be longer. If that's the case, continue.
                //
                // If beforeLength is the same as after we increase the
                // selection, then we should be at the end of the string.
                // Stop looking for line endings.
                // If beforeLength is GREATER, then all bets are off. ;^)
                // But quit looking then too.
                if (beforeLength < cachedSelection.Length)
                {
                    char c = cachedSelection[cachedSelection.Length - 1]; // BAM

                    if (this.SelectionLength.Equals(cachedSelection.Length)
                        && ac0D0A.Contains(c))
                    {
                        trailing0D0As += c;
                    }
                    else
                    {
                        bContinue = false;
                    }
                } else {
                    // If this selection didn't grow, we're probably at the end of the text. No more 0D0As.
                    bContinue = false;
                }
            }

            trailing0D0As = trailing0D0As.NormalizeNewlineToCarriageReturn_();
            this.SelectionLength = originalSelectionLength;

            return trailing0D0As.Length;
        }
        //=============================================
        #endregion methods that could live anywhere
        //=============================================


        protected override void OnLostFocus(RoutedEventArgs e)
        {
            bool isUpArroPressed = Window.Current.CoreWindow.GetKeyState(VirtualKey.Up).HasFlag(CoreVirtualKeyStates.Down);

            if (isUpArroPressed)
            {
                this.Focus(FocusState.Programmatic);
            }
            else
            {
                base.OnLostFocus(e);
            }
        }

        private void UWPBox_Paste(object sender, TextControlPasteEventArgs e)
        {
            //"Native paste action ignored and transferred to KeyUp".LogMsg();

            // This isn't accessible from here, so we're going to have to rip out all pastes,
            // and then reimplement in KeyUp for V.
            //bool isShiftDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);

            e.Handled = true;
        }


        public async Task<bool> FindNext(string toFind, StringComparison comparisonType, bool wrapped = false)
        {
            var foundLoc = await this.FoundNextLoc(toFind, comparisonType, wrapped);

            return foundLoc > -1;
        }

        public async Task<int> FoundNextLoc(string toFind, StringComparison comparisonType, bool wrapped = false)
        {
            int foundLoc = this.Text.IndexOf(toFind, this.SelectionStart2_ForText + this.SelectedText.Length, comparisonType);
            //$"foundLoc: {foundLoc} -- searchstring: {toFind}".LogMsg();

            if (foundLoc >= 0)
            {
                this.SelectionStart = this.TextIndexToTextBoxLoc(foundLoc);
                this.SelectionLength = toFind.NormalizeNewlineToCarriageReturn_().Length;

                //$"selectionstart: {this.SelectionStart} -- selectionlength {this.SelectionLength}".LogMsg();
            }
            else if (!wrapped)
            {
                SearchWrapped?.Invoke();

                var selectionStart = this.SelectionStart;
                var selectionLength = this.SelectionLength;

                this.SelectionStart = 0;
                this.SelectionLength = 0;

                foundLoc = await this.FoundNextLoc(toFind, comparisonType, true);

                // Just as convenience, put the selection back if the term's not found.
                // Otherwise we lose our selection and cursor loc and have the cursor at 0
                // after the failed wrapping search concludes.
                if (foundLoc.Equals(-1))
                {
                    this.SelectionStart = selectionStart;
                    this.SelectionLength = selectionLength;
                }
            }

            return foundLoc;
        }

        //====================================================
        #region KeyDown related
        //====================================================
        public virtual void KeyDownHandler(object sender, KeyRoutedEventArgs e)
        {
            bool isCtrlDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
            bool isShiftDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
            bool isAltDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.LeftMenu).HasFlag(CoreVirtualKeyStates.Down)
                || Window.Current.CoreWindow.GetKeyState(VirtualKey.RightMenu).HasFlag(CoreVirtualKeyStates.Down);

            try
            {
                switch (e.OriginalKey)
                {
                    case VirtualKey.Enter:
                        "Enter from main keydownhandler".LogMsg();
                        break;

                    case VirtualKey.I:
                        // First, kill the default "Ctrl-I inserts a tab" action.
                        if (isCtrlDown)
                        {
                            e.Handled = true;
                            this.HandleCtrlI(); // Just in case we want to do something different with Ctrl-I
                        }
                        break;

                    // Reimplementing Paste
                    case VirtualKey.V:
                        if (isCtrlDown || isAltDown)
                            this.HandlePaste(isCtrlDown, isShiftDown, isAltDown);
                        break;

                    // Replace default "tab means tab to next control" action.
                    case VirtualKey.Tab:
                        this.HandleTabPress(isCtrlDown, isShiftDown, isAltDown);
                        e.Handled = true;
                        break;
                }

                this.HandleKeyDown(sender, e, isCtrlDown, isShiftDown, isAltDown);
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog(ex, "UWPBox.KeyDownHandler");
            }
        }

        public virtual void HandleKeyDown(object sender, KeyRoutedEventArgs e, bool isCtrlDown, bool isShiftDown, bool isAltDown)
        {
            this.BaseHandleKeyDown(sender, e, isCtrlDown, isShiftDown, isAltDown);
        }

        public void BaseHandleKeyDown(object sender, KeyRoutedEventArgs e, bool isCtrlDown, bool isShiftDown, bool isAltDown)
        {
            try
            {
                if (isCtrlDown && e.OriginalKey != VirtualKey.Control)
                {
                    ("KeyDown with control from UWPBox BaseHandleKeys (HandleKeyDown not overridden): " + e.OriginalKey.ToString()).LogMsg();
                }

                switch (e.OriginalKey)
                {
                    case VirtualKey.M:
                        if (isShiftDown && isCtrlDown)
                        {
                            this.BulletText();
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog(ex, "Key down handler, textbox");
            }
        }

        public virtual async void HandlePaste(bool isCtrlDown, bool isShiftDown, bool isAltDown)
        {
            if (isCtrlDown)
            {
                try
                {
                    string strClipboard = await ClipboardConvenience.ClipboardAsText();
                    this.SelectedText = strClipboard;   // TODO: Be more careful about things that don't have text.

                    this.SelectionStart += this.SelectionLength;
                    this.SelectionLength = 0;
                }
                catch (Exception exPaste)
                {
                    this.ShowErrorDialog(exPaste, "Your clipboard does not appear to contain pasteable text.");
                }
            }
        }

        public virtual void HandleCtrlI()
        {
            "Ctrl-I pressed.".LogMsg();
        }

        public string DeleteATabWorthOfLeadingSpaces(string value)
        {
            string ret = value;

            if (value.StartsWith("\t") && value.Length > 1)
            {
                ret = value.Substring(1);
            }
            else
            {
                int intTabSize = UWPBox.Tab.Length;
                Tuple<string, string> tupe = value.PullLeadingAndTrailingSpaces_();

                if (tupe.Item1.Length >= intTabSize)
                {
                    ret = value.Substring(intTabSize);
                }
                else if (tupe.Item1.Length > 0)
                {
                    ret = value.Substring(tupe.Item1.Length);
                }
            }

            return ret;
        }

        public virtual void HandleTabPress(bool isCtrlDown, bool isShiftDown, bool isAltDown)
        {
            // First deal with tabs without shifts or selections; the easiest case.
            if (0 == this.SelectionLength && !isShiftDown)
            {
                this.SelectedText = UWPBox.Tab;
                this.SelectionStart += UWPBox.Tab.Length;
                this.SelectionLength = 0;
            }
            else
            {
                int intLengthAdded = this.ExpandSelectionToPrevNL();

                // Shift down means unindent a tab
                if (isShiftDown)
                {
                    StringBuilder builder = new StringBuilder();
                    string[] astrLines = this.SelectedText.NormalizeNewlineToCarriageReturn_().Split('\r');

                    // TODO: We could have an edge case where there's a line sans newlines and the shift-tab
                    // is only supposed to take off spaces *up until the selection start*.

                    foreach (string line in astrLines)
                    {
                        builder.Append(this.DeleteATabWorthOfLeadingSpaces(line) + this.SelectionLineEnding);
                    }

                    // Remove last newline.
                    builder.Remove(builder.Length - this.SelectionLineEnding.Length, this.SelectionLineEnding.Length);

                    this.SelectedText = builder.ToString();
                    // Now make up for the fact that we'll be "unsetting" the selection by the
                    // amount of spaces that we had to extend it to get to the start of the first line.
                    // That is, we potentially removed some spaces from that line, and if we subtract back
                    // out *all* that we extended, our selected length would be too short by the number of
                    // chars we just took off the front when we "detabbed".
                    int intNumSpacesDeletedFromFirstLine = Math.Min(astrLines[0].PullLeadingAndTrailingSpaces_().Item1.Length, UWPBox.Tab.Length);
                    intLengthAdded = Math.Max(intLengthAdded - intNumSpacesDeletedFromFirstLine, 0);
                }
                else
                {
                    string strNewLinesRemoved = this.SelectedText;
                    string strSuffix = new string(strNewLinesRemoved.ReverseString_().TakeWhile(ch => UWPBox.ac0D0A.Contains(ch)).ToArray());
                    strNewLinesRemoved = strNewLinesRemoved.Slice_(-1 * strSuffix.Length);

                    this.SelectedText = UWPBox.Tab + strNewLinesRemoved.Replace(this.SelectionLineEnding, this.SelectionLineEnding + UWPBox.Tab) + strSuffix;
                    this.SelectionLength -= Tab.Length;     // TODO: Or should the selection start with the newly added leading tab?
                    this.SelectionStart += Tab.Length;      // Ditto
                }

                this.SelectionLength -= intLengthAdded;
                this.SelectionStart += intLengthAdded;
            }
        }

        public int ExpandSelectionToPrevNL()
        {
            // Else if there is a selection (or we're removing tabs), expand the
            // selection back to the start of the first line that's selected.
            char c = ' ';
            string strExtra = string.Empty;
            string strCurrentText = this.Text;

            int intStringOffset = 0;
            int intSelStart = this.SelectionStart2_ForText;

            // If we're already at the start of a line, there's no extra work to do.
            // Insert the first tab right before the selection. If we're removing, we
            // can't, since we're already at the start of the line.
            if (0 != intSelStart && !UWPBox.ac0D0A.Contains(strCurrentText[intSelStart - 1]))
            {
                do
                {
                    intStringOffset++;
                    c = strCurrentText[intSelStart - intStringOffset];

                    //("Char offset " + intStringOffset + ": " + (int)c + " :: " + c.ToString()).LogMsg();

                    if (!ac0D0A.Contains(c))
                    {
                        strExtra = c + strExtra;
                    }
                } while (intStringOffset < intSelStart && !ac0D0A.Contains(c));

                // If we're not at the start of the string/text area's text,
                // and pull back to the prior safe character. If it's the start of the text,
                // we can keep the whole thing.
                if (ac0D0A.Contains(c)) intStringOffset--;
                ("#" + strExtra + "#").LogMsg();

                this.SelectionStart -= intStringOffset;
                this.SelectionLength += intStringOffset;
            }


            return intStringOffset;
        }

        public int ExpandSelectionToNextNL()
        {
            int trailingLength = 0;
            if (!(this.SelectionLength > 0 && ac0D0A.Contains(this.SelectedText[this.SelectedText.Length - 1])))
            {
                string allTrailing = this.Text.Substring(this.SelectionLength2_ForText + this.SelectionStart2_ForText);
                trailingLength = allTrailing.TakeWhile(c => !ac0D0A.Contains(c)).Count();

                this.SelectionLength += trailingLength;
            }
            return trailingLength;
        }
        //====================================================
        #endregion KeyDown related
        //====================================================

        //====================================================
        #region KeyUp related
        //====================================================
        private void KeyUpHandler(object sender, KeyRoutedEventArgs e)
        {
            try
            {
                bool isCtrlDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control).HasFlag(CoreVirtualKeyStates.Down);
                bool isShiftDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.Shift).HasFlag(CoreVirtualKeyStates.Down);
                bool isAltDown = Window.Current.CoreWindow.GetKeyState(VirtualKey.LeftMenu).HasFlag(CoreVirtualKeyStates.Down)
                    || Window.Current.CoreWindow.GetKeyState(VirtualKey.RightMenu).HasFlag(CoreVirtualKeyStates.Down);

                this.HandleKeyUp(sender, e, isCtrlDown, isShiftDown, isAltDown);
            }
            catch (Exception ex)
            {
                this.ShowErrorDialog(ex, "Textbox keyuphandler");
            }
        }

        public virtual void HandleKeyUp(object sender, KeyRoutedEventArgs e, bool isCtrlDown, bool isShiftDown, bool isAltDown)
        {
            this.BaseHandleKeyUp(sender, e, isCtrlDown, isShiftDown, isAltDown);
        }

        public void BaseHandleKeyUp(object sender, KeyRoutedEventArgs e, bool isCtrlDown, bool isShiftDown, bool isAltDown)
        {
            //"BaseHandleKeyUp/HandleKeyUp not overridden override".LogMsg();
        }
        //====================================================
        #endregion KeyUp related
        //====================================================

        //=========================================
        #region textbox text manipulation methods
        //=========================================
        // TODO: Unit tests
        /// <summary>
        /// Pulls characters from before and after the text currently selected in the textbox and drops
        /// them into a Tuple of two string items.
        /// </summary>
        /// <param name="intNumChars">Default is 20. How many characters to pull before and after the currently selected text.</param>
        /// <returns>Returns a tuple of two strings with preceeding chars in Item1 and trailing chars in Item2.</returns>
        public Tuple<string, string> GetTextSurroundingSelection(int intNumChars = 20)
        {
            string ret = string.Empty;
            string trailingText = string.Empty;
            string leadingText = string.Empty;

            int intStart = this.SelectionStart2_ForText - intNumChars;
            int intLength = intNumChars;

            if (this.SelectionStart2_ForText > this.Text.Length)
            {
                throw new Exception("Text length was shorter than selection start. This shouldn't happen.\n"
                    + "Selection start: " + this.SelectionStart2_ForText + "\n"
                    + "Content length: " + this.Text.Length);
            }
            else
            {
                if (intStart < 0)
                {
                    intLength = this.SelectionStart2_ForText;   // If the length requested is too long, give them only to the selection start.
                    intStart = 0;
                }

                leadingText = this.Text.Substring(intStart, intLength);

                // Now the same check, but for the trailing text.
                intStart = this.SelectionStart2_ForText + this.SelectionLength2_ForText;
                intLength = intNumChars;

                int intEndOfSelection = this.SelectionStart2_ForText + this.SelectionLength2_ForText;

                if (this.Text.Length < intStart + intNumChars)
                {
                    intLength = this.Text.Length - intEndOfSelection;
                }

                // If everything looks good, take the substring for trailing text/Item2.
                if (intLength > 0 && intStart >= intEndOfSelection)
                {
                    trailingText = this.Text.Substring(intStart, intLength);
                }
            }

            return new Tuple<string, string>(leadingText, trailingText);
        }
        //=========================================
        #endregion textbox text manipulation methods
        //=========================================


        //=========================================
        #region debug methods
        //=========================================
        public void BulletText()
        {
            this.Text = @"* test 1
    * test1.1
    * test1.2
        * test 1.2.1
        * test 1.2.2
    * test1.3
* test 2
    * test2.1
* test 3";
        }

        public async void MobyWrapped()
        {
            int result = await UWPBox.ShowDialog("Erase what's in this window and replace with testing Markdown? Really?", "Pick Cancel");

            if (result.Equals(0))
            {
                this.Text = @"###Call me Ishmael.

![Jumping Ahead](https://upload.wikimedia.org/wikipedia/commons/7/7b/Moby_Dick_p510_illustration.jpg)

**Some years ago--*never mind how long precisely*--having little or no money in my purse**, and nothing particular to interest me on shore, I thought I would `sail` about a little and see the watery part of the world. It is a way I have of driving off

    the spleen and regulating the circulation.
    Whenever I find myself growing grim about the
    mouth; whenever it is a damp, drizzly November in

> my soul; whenever I find myself involuntarily pausing
> before coffin warehouses, and bringing up the rear of
> every funeral I meet;

|First| Second     |Third     |Fourth|
|-----|:----------:|----------|-----:|
|and  | especially |whenever  |my    |
hypos | get        |such      |an    |
|upper| hand       |of        |me,
that  | it         |requires  | a

strong moral principle to prevent me from deliberately stepping into the street, and

[Linkige](http://www.rufwork.com)

methodically knocking people's hats off--then, I account it high time to get to

> sea as soon as I can. This is my substitute for
> pistol and ball. With a philosophical flourish
> Cato throws himself upon his sword; I quietly
> take to the ship.

There is nothing surprising in this. If they but knew it, almost all men in their degree, some time or other, cherish very nearly the same feelings towards the ocean with me.

+------------------------------+----------------+------------+----------------------------+
| Icon                         | Keystrokes     | Action     | Notes                      |
+==============================+================+============+============================+
| ![Save As                    | Ctrl-Shift-S   | Save As    | Defaults to .md            |
| ](http://rufwork.com/sa.png) |                |            |                            |
+------------------------------+----------------+------------+----------------------------+
| ![Export                     | Ctrl-E         | Export     | Check your CSS Settings    |
| ](http://rufwork.com/e.png)  |                |            | (by clicking the Settings  |
|                                                              button) to set whether     |
|                                                              you'd like the exported    |
|                                                              HTML to include the CSS    |
|                                                              used in the preview window.|
+------------------------------+----------------+------------+----------------------------+
| ![Find                       | Ctrl-F         | Find       | Opens the Find & Replace   |
| ](http://rufwork.com/f.png)  |                |            | dialog.                    |
|                                                            |                            |
|                                                            | * F3 to Find again.        |
|                                                            | * F4 to Replace any text   |
|                                                            |   currently found, then    |
|                                                            |   Find again.              |
|                                                            | * F8 to Replace the found  |
|                                                            |   text with the replace    |
|                                                            |   text throughout (occurs  |
|                                                            |   immediately).            |
+------------------------------+----------------+------------+----------------------------+

---

###Bootstrap

<style>.container > div > div{background-color:orange;border: 1px dashed;}</style>

+[bs]-------------------------------------------------------------+
| Row 1 Line 1 Col 1, | Row 1 Line 1 Col 2, | Row 1 Line 1 Col 3, |
| Row 1 Line 2 Col 1, | Row 1 Line 2 Col 2, | Row 1 Line 2 Col 3, |
| Row 1 Line 3 Col 1  | Row 1 Line 3 Col 2  | Row 1 Line 3 Col 3  |
+-----------------------------------------------------------------+
| Row 2, 1st of 2 cells, line 1 | Row 2, 2nd of 2 cells           |
| Row 2, 1st of 2 cells, line 2 | Row 2, 2nd of 2 cells           |
+-----------------------------------------------------------------+";
            }
        }
        //=========================================
        #endregion debug methods
        //=========================================
    }
}
