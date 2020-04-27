using Mobius.Data;
using Mobius.ComOps;

using System;
using System.IO;
using System.Collections;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{

// When initially formating a tuple this buffer contains the lines for
// each of the formated segments for the tuple. When used in the intermediate
// buffer this structure contains all of the lines for a single segment. The
// segments are chained together using Next.

	public class TupleBuffer 
	{
		public string [] Lines; // array of lines for the row
		public int LineCount; // count of lines used for row
		public int LineCountFirst; // count of lines for first table
		public int LineCountOther; // count of lines for other tables
		public TupleBuffer NextSegmentBuffer; // link to lines for next segment (intermediate buffer only)
	}       

// Page buffer class

	public class PageBuffer 
	{
		public string [] Lines; // lines of text of buffer
		public int LineCount; // count of lines filled
		public int Free; // next free line
		public int MaxLine; // index of max line used for repeats prior to this one
		public int PhysicalLineCount; // count of lines output to device
		public int RowCount; // count of data rows in buffer 
		public MoleculeMx Structures; // structures for page (todo: fixup?)
	}

// Fully formatted buffer for display

	public class DisplayBuffer 
	{
		public string [] Lines; // Array of lines in buffer
		public int LineCount; /* # of lines */
		public int Page; /* page for this buffer */
		public DisplayBuffer Previous; /* previous buffer in chain */
		public DisplayBuffer Next; /* next buffer in chain */
	} 

// Worksheet cell info on graphics for structures, graphs and images

	public class CellGraphic 
	{
		public float RowHeight; // row height for cell text and graphics in points
		public int Width, Height; // image size in points
		public int Col, Row;
		public MoleculeMx Molecule = null; // any molecule associated with cell
		public string GraphicsFileName = "";
		public CellStyleMx CellStyle; // style to be applied to cell
	} 
}
