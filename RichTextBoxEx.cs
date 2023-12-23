using System;
using System.ComponentModel;
using System.Diagnostics;
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

        #region Numbered List Support

        // derived from https://github.com/tringelberg/RichTextBoxExtensions

        const int EM_SETPARAFORMAT = WM_USER + 71;
        const int EM_GETPARAFORMAT = WM_USER + 61;

        int _indentTwips = 15;

        [DllImport("User32.dll")]
        public static extern int SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public enum RichEditNumberingOption
        {
            Bullet = 1,         // PFN_BULLET
            Arabic,             // PFN_ARABIC
            LowercaseLetters,   // PFN_LCLETTER
            UppercaseLetters,   // PFN_UCLETTER
            LowercaseRoman,     // PFN_LCROMAN
            UppercaseRoman      // PFN_UCROMAN    
        }
        public enum RichEditNumberingStyle
        {
            ParanthesesRight = 0x00,        // PFNS_PAREN
            ParanthesesLeftRight = 0x100,   // PFNS_PARENS
            Period = 0x200,                 // PFNS_PERIOD
            Plain = 0x300,                  // PFNS_PLAIN
            NoNumber = 0x400                // PFNS_NONUMBER
        }

        [StructLayout(LayoutKind.Sequential)]
        internal class PARAFORMAT2
        {
            public uint cbSize;
            public uint dwMask;
            public ushort wNumbering;
            public ushort wReserved;
            public int dxStartIndent;
            public int dxRightIndent;
            public int dxOffset;
            public ushort wAlignment;
            public short cTabCount;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] rgxTabs;

            public int dySpaceBefore;
            public int dySpaceAfter;
            public int dyLineSpacing;
            public short sStyle;
            public byte bLineSpacingRule;
            public byte bOutlineLevel;
            public ushort wShadingWeight;
            public ushort wShadingStyle;
            public ushort wNumberingStart;
            public ushort wNumberingStyle;
            public ushort wNumberingTab;
            public ushort wBorderSpace;
            public ushort wBorderWidth;
            public ushort wBorders;
        };

        public class RichTextBoxInfo
        {
            public bool IsListingEnabled { get; set; }
            public RichEditNumberingOption NumberingOption { get; set; }
            public RichEditNumberingStyle NumberingStyle { get; set; }
            public int IntendInTwips { get; set; }
        }

        internal class RichEdit
        {
            // Masks.
            public const int PFM_STARTINDENT = 0x00000001;
            public const int PFM_OFFSET = 0x00000004;
            public const int PFM_ALIGNMENT = 0x00000008;
            public const int PFM_NUMBERING = 0x00000020;
            public const int PFM_NUMBERINGSTYLE = 0x00002000;
            public const int PFM_NUMBERINGSTART = 0x00008000;

            // Alignment.
            public const int PFA_CENTER = 3;

            // Numbering options.
            public const int PFN_ARABIC = 2;

            // Numbering styles.

            public const int PFNS_PERIOD = 0x200;

            // Styles.
            public const int PFNS_NEWNUMBER = 0x00008000;
        }

        #endregion Numbered List support

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

        /// <summary>
        /// Property similar to SelectionBullet except it sets/resets the numbered list.
        /// </summary>
        [Description("Numbered List"), Category("Behavior")]
        public bool SelectionNumbered {
            get {
                RichTextBoxInfo info = GetListingInfo();
                return info.IsListingEnabled;
            }

            set { 
                if (value)
                    EnableListing();
                else
                    DisableListing();
            } 
        }

        /// <summary>
        /// The numbered list changes format based on indentation using this step size.
        /// </summary>
        [Description("Numbered List Indent"), Category("Behavior")]
        public int IndentTwips { get { return _indentTwips; } set { _indentTwips = value; } }

        /// <summary>
        /// Enables numbering listing mode.
        /// </summary>
        /// <param name="option">The kind of numbering (Arabic, Bullet, etc...)</param>
        /// <param name="style">The style for the numbering (Parantheses, Period, etc...)\</param>
        /// <param name="indentInTwips">The indentation in twips.</param>
        public void EnableListing()
        {
            RichEditNumberingOption option = RichEditNumberingOption.Arabic;
            RichEditNumberingStyle style = RichEditNumberingStyle.Period;

            switch ((SelectionIndent / _indentTwips) % 5)
            {
                case 0:
                    option = RichEditNumberingOption.Arabic;
                    style = RichEditNumberingStyle.Period;
                    break;
                case 1:
                    option = RichEditNumberingOption.UppercaseLetters;
                    style = RichEditNumberingStyle.ParanthesesRight;
                    break;
                case 2:
                    option = RichEditNumberingOption.LowercaseLetters;
                    style = RichEditNumberingStyle.ParanthesesRight;
                    break;
                case 3:
                    option = RichEditNumberingOption.UppercaseRoman;
                    style = RichEditNumberingStyle.Period;
                    break;
                case 4:
                    option = RichEditNumberingOption.LowercaseRoman;
                    style = RichEditNumberingStyle.Period;
                    break;
            }

            PARAFORMAT2 pfm = new PARAFORMAT2()
            {
                //dwMask = RichEdit.PFM_STARTINDENT | RichEdit.PFM_NUMBERINGSTYLE | RichEdit.PFM_NUMBERINGSTART | RichEdit.PFM_NUMBERING | RichEdit.PFM_OFFSET,
                dwMask = RichEdit.PFM_NUMBERINGSTYLE | RichEdit.PFM_NUMBERINGSTART | RichEdit.PFM_NUMBERING | RichEdit.PFM_OFFSET,
                wNumberingStart = 1,
                wNumbering = (ushort)option,
                wNumberingStyle = (ushort)style,
                //dxStartIndent = SelectionIndent
            };

            SendParagraphFormatMessage(EM_SETPARAFORMAT, pfm, out _);
        }

        /// <summary>
        /// Disables numbering listing mode.
        /// </summary>
        public void DisableListing()
        {
            PARAFORMAT2 pfm = new PARAFORMAT2()
            {
                dwMask = RichEdit.PFM_STARTINDENT | RichEdit.PFM_NUMBERINGSTYLE | RichEdit.PFM_NUMBERINGSTART | RichEdit.PFM_NUMBERING | RichEdit.PFM_OFFSET,
                wNumberingStart = 1,
                wNumbering = 0,
                wNumberingStyle = 0,
                dxStartIndent = 0
            };

            SendParagraphFormatMessage(EM_SETPARAFORMAT, pfm, out _);
        }

        /// <summary>
        /// Retrieves listing information about the current selection.
        /// </summary>
        /// <returns>A RichTextBoxInfo-Instance</returns>
        public RichTextBoxInfo GetListingInfo()
        {
            PARAFORMAT2 pfm = new PARAFORMAT2();

            pfm.cbSize = (uint)Marshal.SizeOf(typeof(PARAFORMAT2));
            SendParagraphFormatMessage(EM_GETPARAFORMAT, pfm, out PARAFORMAT2 outPfm);

            return new RichTextBoxInfo
            {
                IsListingEnabled = outPfm.wNumbering > (ushort)RichEditNumberingOption.Bullet,
                NumberingOption = (RichEditNumberingOption)outPfm.wNumbering,
                NumberingStyle = (RichEditNumberingStyle)outPfm.wNumberingStyle,
                IntendInTwips = outPfm.dxStartIndent
            };
        }

        private void SendParagraphFormatMessage(uint msg, PARAFORMAT2 inPfm, out PARAFORMAT2 outPfm)
        {
            int structSize = Marshal.SizeOf(inPfm);
            IntPtr structPtr = Marshal.AllocCoTaskMem(structSize);

            inPfm.cbSize = (uint)structSize;

            Marshal.StructureToPtr(inPfm, structPtr, false);
            SendMessage(this.Handle, (uint)msg, IntPtr.Zero, structPtr);

            outPfm = msg == EM_GETPARAFORMAT ? Marshal.PtrToStructure<PARAFORMAT2>(structPtr) : default;

            Debug.WriteLine(outPfm?.dwMask);
            Marshal.FreeCoTaskMem(structPtr);
        }
    }

}
