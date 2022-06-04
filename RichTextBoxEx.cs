using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ACT_Notes
{
    /// <summary>
    /// RichTextBox extension that provides methods to stop and resume painting the text.
    /// </summary>
    public class RichTextBoxEx : RichTextBox
    {
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, ref Point lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int wMsg, int wParam, IntPtr lParam);

        const int WM_USER = 1024;
        const int WM_SETREDRAW = 11;
        const int EM_GETEVENTMASK = WM_USER + 59;
        const int EM_SETEVENTMASK = WM_USER + 69;
        const int EM_GETSCROLLPOS = WM_USER + 221;
        const int EM_SETSCROLLPOS = WM_USER + 222;

        private Point _ScrollPoint;
        private bool _Painting = true;
        private IntPtr _EventMask;
        private int _SuspendIndex = 0;
        private int _SuspendLength = 0;

        public void SuspendPainting()
        {
            if (_Painting)
            {
                _SuspendIndex = SelectionStart;
                _SuspendLength = SelectionLength;
                SendMessage(Handle, EM_GETSCROLLPOS, 0, ref _ScrollPoint);
                SendMessage(Handle, WM_SETREDRAW, 0, IntPtr.Zero);
                _EventMask = SendMessage(Handle, EM_GETEVENTMASK, 0, IntPtr.Zero);
                _Painting = false;
            }
        }

        public void ResumePainting()
        {
            if (!_Painting)
            {
                Select(_SuspendIndex, _SuspendLength);
                SendMessage(Handle, EM_SETSCROLLPOS, 0, ref _ScrollPoint);
                SendMessage(Handle, EM_SETEVENTMASK, 0, _EventMask);
                SendMessage(Handle, WM_SETREDRAW, 1, IntPtr.Zero);
                _Painting = true;
                Invalidate();
            }

        }
    }
}
