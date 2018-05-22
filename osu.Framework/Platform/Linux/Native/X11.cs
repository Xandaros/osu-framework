// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using System;
using System.Runtime.InteropServices;

namespace osu.Framework.Platform.Linux.Native
{
    public unsafe class X11
    {
        public static int SelectionNotify = 31;
        public static ulong CurrentTime = 0L;
        public static ulong AnyPropertyType = 0L;

        [DllImport("libX11.so")]
        public extern static IntPtr XOpenDisplay(IntPtr ptr);
        [DllImport("libX11.so")]
        public extern static int XDefaultScreen(IntPtr display);
        [DllImport("libX11.so")]
        public extern static ulong XRootWindow(IntPtr display, int screen);
        [DllImport("libX11.so")]
        public extern static ulong XInternAtom(IntPtr display, string name, bool only_if_exists);
        [DllImport("libX11.so")]
        public extern static ulong XGetSelectionOwner(IntPtr display, ulong atom);
        [DllImport("libX11.so")]
        public extern static ulong XCreateSimpleWindow(IntPtr display, ulong root, int x, int y, uint width, uint height, uint border_width, ulong border,
                                                       ulong background);
        [DllImport("libX11.so")]
        public extern static int XSelectInput(IntPtr display, ulong window, ulong event_mask);
        [DllImport("libX11.so")]
        public extern static int XConvertSelection(IntPtr display, ulong selection, ulong target, ulong property, ulong window, ulong time);
        [DllImport("libX11.so")]
        public extern static int XNextEvent(IntPtr display, IntPtr destination);
        [DllImport("libX11.so")]
        public extern static int XGetWindowProperty(IntPtr display, ulong window, ulong property, long offset, long length, bool delete, ulong req_type,
                                                    UIntPtr actual_type_return, IntPtr actual_format_return, UIntPtr nitems_return,
                                                    UIntPtr bytes_after_return, IntPtr* prop_return);
        [DllImport("libX11.so")]
        public extern static int XFree(IntPtr data);
        [DllImport("libX11.so")]
        public extern static int XDeleteProperty(IntPtr display, ulong window, ulong property);
        [DllImport("libX11.so")]
        public extern static int XDestroyWindow(IntPtr display, ulong window);

        public static string GetUTF8Property(IntPtr display, ulong window, ulong property)
        {
            ulong type, dul, size, da;
            int di;
            IntPtr data;
            XGetWindowProperty(display, window, property, 0, 0, false, AnyPropertyType, (UIntPtr)(&type), (IntPtr)(&di), (UIntPtr)(&dul), (UIntPtr)(&size), &data);
            XFree(data);

            ulong incr = XInternAtom(display, "INCR", false);
            if (type == incr)
            {
                // Clipboard contents are huge - ignore
                return string.Empty;
            }

            XGetWindowProperty(display, window, property, 0, (long)size, false, AnyPropertyType, (UIntPtr)(&da), (IntPtr)(&di), (UIntPtr)(&dul), (UIntPtr)(&dul), &data);
            string ret2 = Marshal.PtrToStringAuto(data);
            XFree(data);

            return ret2;
        }

        public class XEvent
        {
            public IntPtr handle { get; set; }
            public XEvent_ xevent_ { get; set; }

            public XEvent()
            {
                handle = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(XEvent_)));
            }

            ~XEvent()
            {
                Marshal.FreeHGlobal(handle);
            }

            public int getEvent(IntPtr display)
            {
                int status = XNextEvent(display, handle);
                xevent_ = (XEvent_)Marshal.PtrToStructure(handle, typeof(XEvent_));
                return status;
            }

            [StructLayout(LayoutKind.Sequential, Size = 192)]
            public struct XEvent_
            {
                public int type;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct XSelectionEvent
        {
            public int type;
            public ulong serial;
            public bool send_event;
            public IntPtr display;
            public ulong window;
            public ulong selection;
            public ulong target;
            public ulong property;
            public ulong time;
        }

    }
}