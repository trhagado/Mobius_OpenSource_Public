using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views;
using DevExpress.XtraGrid.Views.Base;
using DevExpress.XtraGrid.Views.Grid;
using DevExpress.XtraGrid.Views.BandedGrid;
//using DevExpress.XtraGrid.Views.Card;
//using DevExpress.XtraGrid.Views.Layout;
using DevExpress.XtraGrid.Columns;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraEditors.Repository;
//using DevExpress.XtraGrid.Views.Base.ViewInfo;
//using DevExpress.XtraGrid.Views.Grid.ViewInfo;
//using DevExpress.XtraGrid.Views.BandedGrid.ViewInfo;
//using DevExpress.XtraGrid.Views.Card.ViewInfo;
//using DevExpress.XtraGrid.Views.Layout.ViewInfo;
using DevExpress.Data;
using DevExpress.Utils;
using DevExpress.XtraGrid.Drawing;
using DevExpress.XtraGrid.Registrator;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// MoleculeBandedGridView - Support pixel scrolling of banded grid view  
	/// i.e. Have both Pixel Scrolling and Row Auto Height enabled. 
	/// </summary>

	// Developer Express Code Central Example:
	// How to create a GridView descendant class and register it for design-time use
	// 
	// This is an example of a custom GridView and a custom control that inherits the
	// DevExpress.XtraGrid.GridControl. Make sure to build the project prior to opening
	// Form1 in the designer. Please refer to the http://www.devexpress.com/scid=A859
	// Knowledge Base article for the additional information.
	// 
	// You can find sample updates and versions for different programming languages here:
	// http://www.devexpress.com/example=E900

/// <summary>
/// Extended GridView
/// </summary>

	public class MoleculeGridView : GridView
	{
		public MoleculeGridView()	: this(null)
		{
			return;
		}

		public MoleculeGridView(GridControl grid)
			: base(grid)
		{
			return; // custom initialization code
		}

		protected override string ViewName
		{
			get { return "MoleculeGridView"; }
		}

		protected override bool IsAllowPixelScrollingAutoRowHeight
		{
			get { return true; }
		}

		protected override bool IsAllowPixelScrollingByDefault
		{
			get { return true; }
		}

		protected override bool IsAllowPixelScrollingPreview
		{
			get { return true; }
		}
	}

	/// <summary>
	/// MoleculeGridViewInfoRegistrator
	/// </summary>

	public class MoleculeGridViewInfoRegistrator : BandedGridInfoRegistrator
	{
		public override string ViewName { get { return "MoleculeGridView"; } }

		public override BaseView CreateView(GridControl grid)
		{
			return new MoleculeGridView(grid as GridControl);
		}
	}

	/// <summary>
	/// Extended BandedGridView
	/// </summary>

	public class MoleculeBandedGridView : BandedGridView
	{
		public MoleculeBandedGridView()	: this(null)
		{
			return;
		}

		public MoleculeBandedGridView(GridControl grid)	: base(grid)
		{
			return; // custom initialization code
		}

		protected override string ViewName
		{
			get { return "MoleculeBandedGridView"; }
		}

		protected override bool IsAllowPixelScrollingAutoRowHeight
		{
			get {	return true; }
		}

		protected override bool IsAllowPixelScrollingByDefault
		{
			get	{	return true; }
		}

		protected override bool IsAllowPixelScrollingPreview
		{
			get {	return true; }
		}
	}

	/// <summary>
	/// MoleculeBandedGridViewInfoRegistrator
	/// </summary>

	public class MoleculeBandedGridViewInfoRegistrator : BandedGridInfoRegistrator
	{
		public override string ViewName { get { return "MoleculeBandedGridView"; } }

		public override BaseView CreateView(GridControl grid)
		{
			return new MoleculeBandedGridView(grid as GridControl);
		}
	}

}
