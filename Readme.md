This project hopes to work around some of the nasty issues with the early releases of the UWP TextBox. Some of these issues are described in these StackOverflow questions:

* [TextBox.Text.Substring(TextBox.SelectionStart) doesn't work because SelectedText changes \r\n to \r](http://stackoverflow.com/questions/35138047/textbox-text-substringtextbox-selectionstart-doesnt-work-because-selectedtext)
* [How to stop Control-I from inserting a tab in a UWP TextBox at CoreWindow scope?](http://stackoverflow.com/questions/35045514/how-to-stop-control-i-from-inserting-a-tab-in-a-uwp-textbox-at-corewindow-scope)

> **NOTE:** Let me emphasize that first one. Even if `Environment.NewLine` is `\r\n`, and you have `\r\n` all over `myTextBox.Text`, if you check `myTextBox.SelectedText`, at least in Windows 10 for PCs, *all newlines will be turned into `\r`!!!* This makes for really wacky integrations of changes to `SelectedText` back into the context of `Text`. See [the StackOverflow question](http://stackoverflow.com/questions/35138047/textbox-text-substringtextbox-selectionstart-doesnt-work-because-selectedtext) for a specific example.

This has been reported as a [bug in the .NET Framework](https://connect.microsoft.com/VisualStudio/feedback/details/2967753), but unfortunately got pushed to the Windows Platform team instead of a workaround given for .NET specifically:

> Posted by Paul [MSFT] on 8/16/2016 at 8:56 AM
> Hi,
> 
> We have determined that this issue belongs to the Windows Platform team and we have transferred this issue to their database. Per their request, we will close this issue and they will contact you directly if they need further information.
> 
> To submit feedback directly on Windows, use the Windows Feedback app. For more information, see http://windows.microsoft.com/en-us/windows/preview-updates-feedback-pc.

The only important file from this repository for your usage is going to be `UWPBox.cs`, which can be found in the folder tree `UWPBox\org\rufwork\UI\widgets`. You are welcome, of course, to take that out of its dated Java-style folder tree.

Behaviors that this extension of TextBox changes include:

* The Boolean `KludgeItUp` is `true` if the TextBox is still translating native `\r\n` to simply `\r` in `SelectedText`.
* `SelectionStart2_ForText` which gives you an honest starting point for the selection in `SelectedText`, adjusted for `\r` to `\r\n`.
* `SelectionLength2_ForText` that gives the selection length renormalized for the current `Environment.NewLine`.
* Up arrow on top row of text no longer moves focus to controls above the textbox.
* Ctrl-I no longer inserts a tab.
* Ctrl-V for paste is pushed so that it can be handled after KeyDownHandler fires.
* `Tab` no longer changes focus to the next control, and can be used to un/indent blocks of text.

###LICENSE

    // =========================== LICENSE ===============================
    // This Source Code Form is subject to the terms of the Mozilla Public
    // License, v. 2.0. If a copy of the MPL was not distributed with this
    // file, You can obtain one at http://mozilla.org/MPL/2.0/.
    // ======================== EO LICENSE ===============================
    
As always, use this code at your own risk.