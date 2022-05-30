using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ACT_Notes
{
	public class FontDialogEx : FontDialog
	{
		//Extended Font Dialog to allow setting the dialog’s location

		//Windows Message Constants
		private const Int32 WM_INITDIALOG = 0x0110;

		//uFlag Constants
		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_SHOWWINDOW = 0x0040;
		private const uint SWP_NOZORDER = 0x0004;
		private const uint UFLAGS = SWP_NOSIZE | SWP_NOZORDER | SWP_SHOWWINDOW;

		//Windows Handle Constants
		static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
		static readonly IntPtr HWND_TOP = new IntPtr(0);
		static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

		//Module vars
		private int _x;
		private int _y;

		//WinAPI definitions
		[DllImport("user32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

		public FontDialogEx() { }

		//Constructor including window location
		public FontDialogEx(int X, int Y)
		{
			_x = X;
			_y = Y;
		}

		//Hook into Windows Messages
		protected override IntPtr HookProc(IntPtr hWnd, int msg, IntPtr wparam, IntPtr lparam)
		{
			IntPtr returnValue = base.HookProc(hWnd, msg, wparam, lparam);
			if (msg == WM_INITDIALOG)
			{
				SetWindowPos(hWnd, HWND_TOP, _x, _y, 0, 0, UFLAGS);
			}
			return returnValue;
		}
	}
}
