//////////////////////////////////////////////////////////////////////////////
// This source code and all associated files and resources are copyrighted by
// the author(s). This source code and all associated files and resources may
// be used as long as they are used according to the terms and conditions set
// forth in The Code Project Open License (CPOL), which may be viewed at
// http://www.blackbeltcoder.com/Legal/Licenses/CPOL.
//
// This code was original published on Black Belt Coder
// (http://www.blackbeltcoder.com).
//
// Copyright (c) 2011 Jonathan Wood
//
// JJL 2022 - Add support for text DisplayStyle

using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ACT_Notes
{
	/// <summary>
	/// Drop-down color picker control that can be placed in a tool strip.
	/// </summary>
	[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip|
		ToolStripItemDesignerAvailability.StatusStrip)]
	[DefaultProperty("Value")]
	public class ColorToolStripDropDownButton : ToolStripDropDownButton
	{
		// Events
		public delegate void ColorPaletteEventHandler(object sender, ColorPickerEventArgs e);
		public new event ColorPaletteEventHandler Click;
		public event ColorPaletteEventHandler SelectionChanged;

		// Properties
		private Color _value = Color.White;

		/// <summary>
		/// Gets or sets the current color value of this instance.
		/// </summary>
		[Description("Specifies the current color value.")]
		[DefaultValue(KnownColor.White)]
		public Color Value
		{
			get { return _value; }
			set { _value = value; Invalidate(); }
		}

		#region ColorPalette Properties

		/// <summary>
		/// Gets or sets the height of each palette item, in pixels.
		/// </summary>
		[Description("Indicates the height of each palette item in pixels.")]
		[DefaultValue(16)]
		public int BoxHeight { get { return _palette.BoxHeight; } set { _palette.BoxHeight = value; } }

		/// <summary>
		/// Gets or sets the width of each palette item, in pixels.
		/// </summary>
		[Description("Indicates the width of each palette item in pixels.")]
		[DefaultValue(16)]
		public int BoxWidth { get { return _palette.BoxWidth; } set { _palette.BoxWidth = value; } }

		/// <summary>
		/// Gets or sets the number of palette items per row.
		/// </summary>
		[Description("Determines the number of palette items per row.")]
		[DefaultValue(8)]
		public int Columns { get { return _palette.Columns; } set { _palette.Columns = value; } }

		/// <summary>
		/// Gets or sets the initial color palette.
		/// </summary>
		[Description("Specifies the control's initial palette.")]
		[DefaultValue(Palette.Basic)]
		public Palette DefaultPalette { get { return _palette.DefaultPalette; } set { _palette.DefaultPalette = value; } }

		/// <summary>
		/// Gets or sets the distance between palette items, in pixels.
		/// </summary>
		[Description("Determines the distance between palette items in pixels.")]
		[DefaultValue(2)]
		public int Margins { get { return _palette.Margins; } set { _palette.Margins = value; } }

		/// <summary>
		/// Gets or sets the maximum number of visible rows. Scrolling is enabled if this
		/// number is less than the total number of rows. Use 0 to display all rows.
		/// </summary>
		[Description("Indicates the maximum number of visible rows. Use 0 to display all rows.")]
		[DefaultValue(0)]
		public int VisibleRows { get { return _palette.VisibleRows; } set { _palette.VisibleRows = value; } }

		#endregion

		#region ColorDropDown Properties

		/// <summary>
		/// Gets or sets whether or not the drop-down palette includes a button that
		/// allows the user to select a color from the color dialog box.
		/// </summary>
		[Description("Determines if the palette displays a button to select additional colors.")]
		[DefaultValue(true)]
		public bool MoreColorsButton { get { return _dropDown.MoreColorsButton; } set { _dropDown.MoreColorsButton = value; } }

		#endregion

		// Private data
		private ColorDropDown _dropDown = new ColorDropDown();
		private ColorPalette _palette;

		// Construction
		public ColorToolStripDropDownButton()
		{
			DropDown = _dropDown;
			_palette = _dropDown.GetColorPaletteControl();

			_palette.Click += new ColorPalette.ColorPaletteEventHandler(_palette_Click);
			_palette.SelectionChanged += new ColorPalette.ColorPaletteEventHandler(_palette_SelectionChanged);
		}

		// Propagate Click event
		void _palette_Click(object sender, ColorPickerEventArgs e)
		{
			Value = e.Value;
			_dropDown.Close(ToolStripDropDownCloseReason.ItemClicked);
			if (Click != null)
				Click(this, e);
		}

		// Propagate SelectionChanged event
		void _palette_SelectionChanged(object sender, ColorPickerEventArgs e)
		{
			if (SelectionChanged != null)
				SelectionChanged(this, e);
		}

		/// <summary>
		/// Returns the dropdown ColorPalette control associated with
		/// this instance.
		/// </summary>
		public ColorPalette GetColorPaletteControl()
		{
			return _dropDown.GetColorPaletteControl();
		}

		// Render control
		protected override void OnPaint(PaintEventArgs e)
		{
			// Render background
			ToolStripItemRenderEventArgs args = new ToolStripItemRenderEventArgs(e.Graphics, this);
			Parent.Renderer.DrawDropDownButtonBackground(args);
			ToolStripArrowRenderEventArgs args2 = new ToolStripArrowRenderEventArgs(e.Graphics, this,
				new Rectangle(3 + 15, 0, Width - (3 + 15), Height),
				Color.FromKnownColor(KnownColor.ControlText), ArrowDirection.Down);
			Parent.Renderer.DrawArrow(args2);

            if (DisplayStyle == ToolStripItemDisplayStyle.Image)
            {
				// Render color box
				Rectangle rect = new Rectangle(3, 3, 15, 15);
				e.Graphics.FillRectangle(new SolidBrush(_value), rect);
				e.Graphics.DrawRectangle(Pens.Black, rect);
			}
			else
			{
				// clear the box
				Rectangle rect = new Rectangle(3, 2, 16, 16);
				e.Graphics.FillRectangle(Brushes.WhiteSmoke, rect);

				// add the text
				e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
				e.Graphics.DrawString(Text, this.Font, new SolidBrush(this.ForeColor), new Rectangle(3, 2, 15, 15));

				// add the color "underline"
				rect = new Rectangle(3, 15, 14, 3);
				e.Graphics.FillRectangle(new SolidBrush(_value), rect);
			}
		}
	}
}
