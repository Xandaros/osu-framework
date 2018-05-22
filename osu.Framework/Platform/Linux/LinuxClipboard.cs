// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

// using System.Windows.Forms;
using System;
using System.Runtime.InteropServices;

using static osu.Framework.Platform.Linux.Native.X11;

namespace osu.Framework.Platform.Linux
{
    public class LinuxClipboard : Clipboard
    {
        public override string GetText()
        {
            IntPtr display = XOpenDisplay(IntPtr.Zero);
            if (display == IntPtr.Zero)
            {
                return string.Empty;
            }

            int screen = XDefaultScreen(display);
            ulong root = XRootWindow(display, screen);
            ulong sel = XInternAtom(display, "CLIPBOARD", false);
            ulong utf8 = XInternAtom(display, "UTF8_STRING", false);

            ulong owner = XGetSelectionOwner(display, sel);
            if (owner == 0)
            {
                return string.Empty;
            }

            ulong targetWindow = XCreateSimpleWindow(display, root, -10, -10, 1, 1, 0, 0, 0);
            XSelectInput(display, targetWindow, (ulong)SelectionNotify);

            ulong targetProp = XInternAtom(display, "CLIPBOARD", false);
            XConvertSelection(display, sel, utf8, targetProp, targetWindow, CurrentTime);

            XEvent xEvent = new XEvent();
            while (true)
            {
                xEvent.getEvent(display);
                if (xEvent.xevent_.type == SelectionNotify)
                {
                    XSelectionEvent ev = (XSelectionEvent)Marshal.PtrToStructure(xEvent.handle, typeof(XSelectionEvent));
                    if (ev.property == 0)
                    {
                        return string.Empty;
                    }
                    string ret = GetUTF8Property(display, targetWindow, targetProp);
                    XDestroyWindow(display, targetWindow);
                    return ret;
                }
            }

            // return System.Windows.Forms.Clipboard.GetText(TextDataFormat.UnicodeText);
        }

        public override void SetText(string selectedText)
        {
            //Clipboard.SetText(selectedText);

            //This works within osu but will hang any application you try to paste to afterwards until osu is closed.
            //Likely requires the use of X libraries directly to fix
        }
    }
}
