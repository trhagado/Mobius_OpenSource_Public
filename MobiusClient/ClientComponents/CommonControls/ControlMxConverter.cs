using Mobius.ComOps;
using Mobius.BaseControls;

using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using WinForms = System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Threading;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Management;
using System.Web;
using Microsoft.Win32;

using DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using DevExpress.Utils;

namespace Mobius.ClientComponents
{
	public class ControlMxConverter
	{
		string AddControlsCode = "";

		static HashSet<string> ControlsLogged = new HashSet<string>();
		HashSet<string> RadioGroups = new HashSet<string>(); // names of each of the radio button groups seen 
		StreamWriter log;

		bool IncludeMenuStubs = false;
		public static bool Active = DebugMx.True; // set to true to generate Razor code

		/// <summary>
		/// Convert a Windows/DevExpress Form or UserControl to a set of ControlMx-based controls
		/// that can be later converted into a set of Jupyter widgets, Blazor, etc components for rendering.
		/// </summary>
		/// <param name="pc"></param>

		public void Convert(
			Control pc,
			bool includeMenuStubs = false)
		{
			string eventCode = ""; // temp accumulator
			string cssClasses = "", varDefStmt = "", boundingBoxStmt = "", eventStubFrag = "";
			string inlineStyleProps = "", groupName = "", initArgs = "";
			string template = "", s;

			int dy = 0;

			if (!Active) return;

			IncludeMenuStubs = includeMenuStubs;

			if (WinFormsUtil.ControlMxConverter == null) // allow calls to converter from ComOps
				WinFormsUtil.ControlMxConverter = new ControlMxConverterDelegate(Convert);

			/////////////////////////////////////////////////////////////////////////////
			// Build the HTML and plug into the template
			/////////////////////////////////////////////////////////////////////////////

			string html = "", varDefCode = "", initCode = ""; // strings for global building of html and code sections
			AddControlsCode = "";

			Type t = pc.GetType();
			string ctlFullName = t.FullName;
			//if (ControlsLogged.Contains(ctlFullName)) return;
			ControlsLogged.Add(ctlFullName);

			log = new StreamWriter(@"c:\downloads\MobiusJupyterTemplates\" + t.Name + ".txt");

			RadioGroups = new HashSet<string>();

			html = $"def {t.Name} ():\r\n"; // class name

			/////////////////////////////////////////////////////////////////////////////
			// Process a top level Form or XtraForm (i.e. DialogBox)
			/////////////////////////////////////////////////////////////////////////////

			if (typeof(Form).IsAssignableFrom(t)) // Form or XtraForm?
			{
				Form f = pc as Form;
				DialogBoxMx dbMx = new DialogBoxMx();

// Insert this form into a DialogBoxContainer and then convert that container
// so that the window header controls are included in the conversion as well

				DialogBoxContainer f2 = new DialogBoxContainer();
				f2.ShowIcon = f.ShowIcon;
				f2.FormBorderStyle = f.FormBorderStyle;
				f2.Text = f.Text;
				f2.MinimizeBox = f.MinimizeBox;
				f2.MaximizeBox = f.MaximizeBox;
				f2.Width = f.Width; // needs adjusting for delta in header size
				f2.Height = f.Height; // ditto

				if (f2.ShowIcon)
					f2.WindowIcon.Name = "Mobius16x16.png";

				foreach (Control c in f.Controls)
				{
					f2.ContentPanel.Controls.Add(c);
				}

				int height = f.Height; // add size of WinForms header to get correct html height
				int width = f.Width;
				string headerText = f.Text;
				string enableResize = (f.FormBorderStyle == WinForms.FormBorderStyle.Sizable) ? "true" : "false";

				initArgs = BuildInitArgs(f2,
					"TitleTextText", f2.Text, dbMx.TitleText,
					"ImageName", f2.WindowIcon.Name, dbMx.ImageName,
					"FormBorderStyle", f2.FormBorderStyle, dbMx.FormBorderStyle,
					"MinimizeBox", f2.MinimizeBox, dbMx.MinimizeBox,
					"MaximizeBox", f2.MaximizeBox, dbMx.MaximizeBox);

				varDefStmt += $"\tpublic DialogBoxMx {f.Name} = new DialogBoxMx(){initArgs};\r\n"; // declare and set basic props

				// Define variables for code section

				varDefCode = varDefStmt;

				initCode = "";

				template = DialogBoxClassTemplate;

				boundingBoxStmt = BuildBoundingBoxStmt(null, f2, dy);

				ConvertContainedControls(pc, 0, ref html, ref varDefCode, ref initCode);

				html += // Finish up the Dialog component
					$@"";


				if (includeMenuStubs)
				{
					html += @"
				 <div id='ContextMenuDivId' class='ContextMenuDiv' style='width: 100%;'
			     oncontextmenu='return false;'>
						<SfContextMenu TValue='@MenuItem' @ref='@ContextMenu' Target='.ContextMenuDiv' Items='@MenuItems'>
							<MenuEvents TValue='@MenuItem'
													OnOpen='@BeforeMenuOpen' OnClose='@BeforeMenuClosed' Opened='@MenuOpened' Closed='@MenuClosed'
													OnItemRender='@MenuItemRender' ItemSelected='@MenuItemSelected' />
						</SfContextMenu>
					</div>" + "\r\n";
				}
			}

			/////////////////////////////////////////////////////////////////////////////
			// Process a UserControl or XtraUserControl
			/////////////////////////////////////////////////////////////////////////////

			else if (pc is UserControl || pc is XtraUserControl)
			{
				html = "";

				varDefCode = "";

				ConvertContainedControls(pc, 0, ref html, ref varDefCode, ref initCode);

				template = UserControlClassTemplate;
			}

			else throw new Exception("Can't convert control of type: " + pc?.GetType()?.Name);

			// Fill in the template for the Form or UserControl and write out the class code

			string csCode = Lex.Replace(template, "<className>", t.Name); // get template and substitute proper class name

			csCode = Lex.Replace(csCode, "<componentDeclarations>", varDefCode);

			csCode = Lex.Replace(csCode, "<componentInitialization>", initCode);

			csCode = Lex.Replace(csCode, "<addControls>", AddControlsCode);

			log.Close();

			StreamWriter sw = new StreamWriter(@"c:\downloads\MobiusJupyterTemplates\" + t.Name + ".design.cs");
			sw.Write(csCode);
			sw.Close();
			return;
		}

		/// <summary>
		/// Convert a list of controls for a control container which can be any of the following:
		///   UserControl, XtraUserControl
		///   WinForms/Dx predefined container (e.g. GroupBox, Panel...)
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="sw"></param>
		/// <param name="customDivStyling"></param>
		/// <param name="header"></param>
		/// <param name="footer"></param>
		/// <param name="html"></param>

		void ConvertContainedControls(
			Control pc,
			int dy,
			ref string html,
			ref string varDefCode,
			ref string initCode)
		{
			Type pcType = pc.GetType();

			log.WriteLine("");
			log.WriteLine("Parent Type:\t" + pcType.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + pcType.FullName);

			string format = "{0,-26}\t{1,-26}\t{2,5}\t{3,5}\t{4,5}\t{5,6}\t{6,-26}\t{7,-6}\t{8,-14}\t{9,-32}";
			log.WriteLine(string.Format(format, "Type", "Name", "Left", "Top", "Width", "Height", "Anchor", "Dock", "Text", "Full Type Name"));
			log.WriteLine(string.Format(format, "--------------------------", "--------------------------", "----", "----", "-----", "------", "--------------------------", "----", "--------------", "--------------------------"));

			//if (Lex.IsDefined(header)) // wrap in header if defined
			//	html += header;

			//else // otherwise wrap in a div
			//{
			//	html += // wrap control in div with class name matching the Winforms/Dx control class name
			//		$@"<div class='font-mx defaults-mx' class='{pcType.Name}' style='position: relative; width: 100%; height: 100%; border: 1px solid orange; {customDivStyling} '>" + "\r\n";
			//}

			List<Control> cl = new List<Control>();

			foreach (Control c in pc.Controls) // sort controls by position
			{
				int ci = 0;
				Point p1 = c.Location;
				for (ci = 0; ci < cl.Count; ci++)
				{
					Point p2 = cl[ci].Location;
					if (p1.Y < p2.Y) break;
					if (p1.X < p2.X) break;
				}

				cl.Insert(ci, c);
			}

			if (cl.Count == 0) // create a "null" control and add to the list so the div for the container gets created
			{
				return; // throw new NotImplementedException();

				//DivMx c = new DivMx();
				//c.Location = new Point(0, 0);
				//c.Size = new Size(1, 1);
				//c.Dock = DockStyle.Fill;
				//c.Name = pc.GetType().Name;
				//cl.Add(c);
			}

			foreach (Control c in cl)
			{
				//if (!cc.Visible) continue;

				Type t = c.GetType();

				// Log basic info

				log.WriteLine(string.Format(format, t.Name, c.Name,
					c.Left, c.Top, c.Width, c.Height,
					c.Anchor, c.Dock, c.Text, t.FullName));

				ConvertControl(pc, c, dy, ref html, ref varDefCode, ref initCode); // convert the control and surround with a positioning div

			} // child control list

			log.WriteLine("End of Parent Type:\t" + pcType.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + pcType.FullName);

			return;
		}

		void ConvertControl(
			Control pc,
			Control c,
			int dy,
			ref string html,
			ref string varDefCode,
			ref string initCode)
		{
			string eventCode = ""; // temp accumulator
			string cssClasses = "", varDefStmt = "", boundingBoxStmt = "", eventStubFrag = "";
			string inlineStyleProps = "", groupName = "", initArgs = "";
			int dy2 = 0;

			Type cType = c.GetType();
			string cText = c.Text;
			if (Lex.StartsWith(cText, "&")) // remove shortcut indicators within control text (e.g. menu items, buttons, ...)
				cText = cText.Substring(1);

			cText = HttpUtility.HtmlEncode(c.Text);

			//DivMx div = new DivMx(); // the div to surround and position the control

			////////////////////////////////////////////////////////////////////////////////
			// For user controls write reference to this file and then the control and 
			// its contents to a new file
			////////////////////////////////////////////////////////////////////////////////

			if (c is UserControl || c is XtraUserControl)
			{

				UserControl uc = c as UserControl;
				UserControlMx ucMx = new UserControlMx();

				string typeName = c.GetType().Name;
				string ctlClassName = cType.Name;

				UserControlMx bmx = new UserControlMx() { };

				initArgs = BuildInitArgs(c);
				varDefStmt += $"\tpublic {typeName} {c.Name} = new {typeName}(){initArgs};\r\n"; // declare and set basic props

				inlineStyleProps = BuildBoundingBoxStyleProps(pc, c, dy);

				if (uc.BorderStyle != BorderStyle.None)
					inlineStyleProps += " border: 1px solid #acacac; background-color: #ffffff; ";

				boundingBoxStmt = $"\t{c.Name}.StyleProps = new CssStyleMx(\"{inlineStyleProps}\");\r\n"; // set bounding box in initialize method
			}

			////////////////////////////////////////////////////////////////////////////////
			// For WinForms/DX container controls write reference the control and subcontrols
			// to this file.
			////////////////////////////////////////////////////////////////////////////////

			/****************/
			/*** GroupBox ***/
			/****************/

			else if (cType == typeof(GroupBox))
			{
				GroupBox gb = c as GroupBox;
				GroupBoxMx bmx = new GroupBoxMx();

				gb.Height += 8;
				gb.Top -= 8;

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				gb.Height -= 8;
				gb.Top += 8;

				initArgs = BuildInitArgs(c,
					"Text", gb.Text, bmx.Text);

				varDefStmt += $"\tpublic GroupBoxMx {c.Name} = new GroupBoxMx(){initArgs};\r\n"; // declare and set basic props

				cssClasses +=
					"<GroupBox class='groupbox-mx'>\r\n";

				dy2 = 4; // move contained controls down a bit
				ConvertContainedControls(c, dy2, ref cssClasses, ref varDefStmt, ref eventStubFrag);

				cssClasses += "</fieldset>\r\n";

				//div.Close(ref htmlFrag);
			}

			/*************************/
			/*** Panel / XtraPanel ***/
			/*************************/

			else if (cType == typeof(Panel) || cType == typeof(PanelControl) || cType == typeof(XtraPanel))
			{
				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				cssClasses += ""; // todo...

				initArgs = BuildInitArgs(c);
				varDefStmt += $"\tpublic PanelControlMx {c.Name} = new PanelControlMx(){initArgs};\r\n";

				ConvertContainedControls(c, dy2, ref cssClasses, ref varDefStmt, ref eventStubFrag);

				cssClasses += ""; // todo...

				//div.Close(ref htmlFrag);
			}

			/***********************/
			/*** XtraTab Control ***/
			/***********************/

			else if (cType == typeof(XtraTabControl))
			{

				/* Example:
					 <SfTab ID="QueriesControlTab" Height="32px" Width="100%"
									 CssClass="tab-workaround"
									 ShowCloseButton="true"
									 LoadOn="ContentLoad.Demand"
									 @ref="Tabs"
									 SelectedItemChanged="Tab_SelectedItemChanged">

							<TabEvents Created="Tab_Created"
												 Adding="Tab_Adding"
												 Added="Tab_Added"
												 Removing="Tab_Removing"
												 Removed="Tab_Removed"
												 Selecting="Tab_Selecting"
												 Selected="Tab_Selected"
												 Destroyed="Tab_Destroyed">
							</TabEvents>

							<TabItems>
								<TabItem>
									<ChildContent>
										<TabHeader Text="Query 1" IconCss="QueryIconMx"></TabHeader>
									</ChildContent>
							</TabItem>
						</TabItems>

					</SfTab>

				 */


				XtraTabControl tab = c as XtraTabControl;

				//tab.Height += 8;
				//tab.Top -= 8;

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				//tab.Height -= 8;
				//tab.Top += 8;

				cssClasses +=
					@"<SfTab class='tab-control-mx' Height='100%' Width='100%'>
							<TabItems>" + "\r\n";

				initArgs = BuildInitArgs(c);
				varDefStmt += $"\tpublic TabMx {c.Name} = new TabMx(){initArgs};\r\n";

				dy2 = 0; // adjust position of contained controls
				ConvertContainedControls(c, dy2, ref cssClasses, ref varDefStmt, ref eventStubFrag);

				cssClasses +=
					@"</TabItems>
			</SfTab>\r\n";

				//div.Close(ref htmlFrag);
			}

			/*******************/
			/*** XtraTabPage ***/
			/*******************/

			else if (cType == typeof(XtraTabPage))
			{

				/* Example:
				 * 
					 <TabItem>
              <ChildContent>
                  <TabHeader Text='Rome'></TabHeader>
              </ChildContent>
              <ContentTemplate>
                  <div id='div1' ...>
                  </div>
									...	                       																			
                  <div id='divN' ...>
                  </div>
              </ContentTemplate>
          </TabItem>

				 */

				XtraTabPage tp = c as XtraTabPage;

				//tab.Height += 8;
				//tab.Top -= 8;

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);
				//tab.Height -= 8;
				//tab.Top += 8;

				cssClasses +=
					$@"
					 <TabItem>
              <ChildContent>
                  <TabHeader Text='{tp.Text}'></TabHeader>
              </ChildContent>
              <ContentTemplate>
						";

				initArgs = BuildInitArgs(c);
				varDefStmt += $"\tpublic TabPageMx {c.Name} = new TabPageMx(){initArgs};\r\n";

				dy2 = 0; // adjust position of contained controls
				ConvertContainedControls(c, dy2, ref cssClasses, ref varDefStmt, ref eventStubFrag);

				cssClasses +=
					@"</ContentTemplate>
						</TabItem>\r\n";

				//div.Close(ref htmlFrag);
			}

			////////////////////////////////////////////////////////////////////////////////
			// WinForms/DX builtin controls (non-grouping)
			////////////////////////////////////////////////////////////////////////////////

			/////////////////////////////////////////////////////////////////////////////////
			// LabelControl
			/////////////////////////////////////////////////////////////////////////////////


			else if (cType == typeof(LabelControl)) // DX LabelControl
			{
				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				LabelControl lc = c as LabelControl;
				TextOptions to = lc.Appearance.TextOptions;

				if (lc.LineVisible)
				{
					if (Lex.IsUndefined(lc.Text))
						cssClasses += "<hr class='hr-mobius'>\r\n";

					else // horizontal rule with text overlaid using utility css
						cssClasses += $"<hr data-content='{lc.Text}' class=' class='hr-mobius hr-text'>\r\n";
				}

				else if (Lex.IsDefined(lc.Text) || to.WordWrap == WordWrap.Wrap)
				{
					//if (lc.BorderStyle != BorderStyles.NoBorder && lc.BorderStyle != BorderStyles.Default) // add border?
					//	divStyle += " border: 1px solid #acacac; background-color: #eeeeee; ";

					string spanStyle = ""; // default centers vertically 
																 //if (to.WordWrap == WordWrap.Wrap) 
																 //	spanStyle = " style='height: 100%; padding: 0px 2px 0px 2px;'"; // align with top of containing div and provide a bit of side padding

					/* 
					 * For the MessageBoxMx where messages can overflow the div in both x and y:
					 * <div @ref='Message.DivRef' class='control-div-mx font-mx defaults-mx' style='@Message?.DivStyle?.StyleString'>
					 *   <span style='padding: 0px 2px 0px 2px;'>@Message.Text</span>
					 * </div>
					 * 
					 * 
					 *   LabelControlMx Message = new LabelControlMx() { Text = "Message...", DivStyle = new CssStyleMx("position: absolute; display: 
					 *   flex; align-items: center; 
					 *   overflow: hidden; text-overflow: ellipsis; [OR] overflow-x: scroll; overflow-y: scroll; 
					 *   left: 56px; top: 34px; width: calc(100% - 74px); height: calc(100% - 68px);  
					 *   border: 0px solid #acacac; background-color: #eeeeee; ") };
					 * 
					 */

					cssClasses += $"<span {spanStyle}>@{c.Name}.Text</span>\r\n";
				}

				initArgs = BuildInitArgs(c, "Text", lc.Text, "");
				varDefStmt += $"\tpublic LabelControlMx {c.Name} = new LabelControlMx(){initArgs};\r\n";

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				//div.Close(ref htmlFrag);
			}

			else if (cType == typeof(WinForms.Label)) // Simpler Windows.Forms Label
			{
				Label l = c as Label;

				if (Lex.IsUndefined(l.Text)) return; // ignore if no text

				inlineStyleProps = BuildBoundingBoxStyleProps(pc, c, dy);

				if (l.BorderStyle != BorderStyle.None)
					inlineStyleProps += " border: 1px solid #acacac; background-color: #eeeeee; ";

				boundingBoxStmt = $"\t{c.Name}.StyleProps = new CssStyleMx(\"{inlineStyleProps}\");\r\n"; // set bounding box in initialize method

				cssClasses += $"<span>@{c.Name}.Text</span>\r\n";

				initArgs = BuildInitArgs(c, "Text", l.Text, "");
				varDefStmt += $"\tpublic LabelControlMx {c.Name} = new LabelControlMx(){initArgs};\r\n";

				//div.Close(ref htmlFrag);
			}

			else if (cType == typeof(WinForms.PictureBox)) // Simple image
			{
				PictureBox pb = c as PictureBox;

				inlineStyleProps = BuildBoundingBoxStyleProps(pc, c, dy);

				if (pb.BorderStyle != BorderStyle.None)
					inlineStyleProps += " border: 1px solid #acacac; background-color: #eeeeee; ";

				boundingBoxStmt = $"\t{c.Name}.StyleProps = new CssStyleMx(\"{inlineStyleProps}\");\r\n"; // set bounding box in initialize method

				cssClasses += $"<img src='@{c.Name}.ImageName' width='{c.Width}' height='{c.Height}' />\r\n";

				initArgs = BuildInitArgs(c);
				varDefStmt += $"\tpublic PictureBoxMx {c.Name} = new PictureBoxMx(){initArgs};\r\n";

			}

			/////////////////////////////////////////////////////////////////////////////////
			// DropdownButton - button with dropdown menu attached
			// Example:
			//  <SfDropDownButton IconCss="e-ddb-icons e-profile">
			//    <DropDownMenuItems>
			//      <DropDownMenuItem Text="User Settings" IconCss="e-ddb-icons e-settings"></DropDownMenuItem>
			//      <DropDownMenuItem Text="Log Out" IconCss="e-ddb-icons e-logout"></DropDownMenuItem>
			//    </DropDownMenuItems>
			// </SfDropDownButton>
			//
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(DropDownButton))
			{
				DropDownButton db = c as DropDownButton;

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				cssClasses += $@"<SfDropDownButton CssClass='button-mx' 
				  @ref='{c.Name}.Button' Content='@{c.Name}.Text 
					@onclick = '{c.Name}_Click' >
				 </SfDropDownButton>";

				//<DropDownMenuItems>
				//  <DropDownMenuItem Text='Loading...'></DropDownMenuItem>
				//</DropDownMenuItems>

				initArgs = BuildInitArgs(c, "Text", c.Text, "");
				varDefStmt += $"\tpublic DropDownButtonMx {c.Name} = new DropDownButtonMx(){initArgs};\r\n";

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckEdit (CheckBox and RadioButton)
			/////////////////////////////////////////////////////////////////////////////////


			else if (cType == typeof(CheckEdit))
			{
				if (dy == 0)
					dy = -4; // move up a bit

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				CheckEdit ce = c as CheckEdit;
				CheckBoxStyle style = ce.Properties.CheckBoxOptions.Style;
				if (style == CheckBoxStyle.Default)
				{
					if (ce.Properties.RadioGroupIndex >= 0)
						style = CheckBoxStyle.Radio;
					else style = CheckBoxStyle.CheckBox;
				}

				cText = c.Text.Trim();

				if (style == CheckBoxStyle.CheckBox)
				{ // CheckBox
					cssClasses += $@"<SfCheckBox CssClass='font-mx defaults-mx' Label='@{c.Name}.Text' Name='{c.Name}'   
						@ref ='{c.Name}.Button' @bind-Checked='{c.Name}.Checked' @onchange = '{c.Name}_CheckedChanged' />" + "\r\n";

					initArgs = BuildInitArgs(c, "Text", cText, "");
					varDefStmt += $"\tpublic CheckBoxMx {c.Name} = new CheckBoxMx(){initArgs};\r\n";
				}

				else // assume RadioButton
				{

					groupName = "RadioGroupValue" + ce.Properties.RadioGroupIndex;
					cssClasses += $@"<SfRadioButton CssClass='font-mx defaults-mx' Label='@{c.Name}.Text' Name='{groupName}' Value='{ce.Name}'
						@ref ='{c.Name}.Button' @bind-Checked='{groupName}.CheckedValue' @onchange = '{c.Name}_CheckedChanged' />" + "\r\n";

					if (!RadioGroups.Contains(groupName)) // add var to identify the current radio button for the group
					{
						varDefStmt += $"static CheckedValueContainer {groupName} = new CheckedValueContainer();\r\n";
						RadioGroups.Add(groupName);
					}

					initArgs = BuildInitArgs(c, 
						"Text", cText, "",
						"CVC", Nas(groupName), "");
					varDefStmt += $"\tpublic RadioButtonMx {c.Name} = new RadioButtonMx(){initArgs};\r\n";
				}

				eventStubFrag += $@"

				/// <summary>
				/// {c.Name}_CheckedChanged
				/// </summary>
				 
				private void {c.Name}_CheckedChanged()
					{{
						return;
					}}" + "\r\n";

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckButton - Like a RadioButton but with an image et al in place of a checkmark
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(CheckButton))
			{
				// CheckButtons and their groups are handled in the same way as RadioButtons
				// with the value for checked comparison being the ID of the SfButton.

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				CheckButton cb = c as CheckButton;

				groupName = "CheckButtonGroup" + cb.GroupIndex;
				string iconName = cb.Name;
				iconName = Lex.Replace(iconName, "Va", ""); // hack for TextAlignment Dialog
				iconName = Lex.Replace(iconName, "Ha", "");

				if (!Lex.StartsWith(iconName, "Align")) iconName = "Align" + iconName; // make name match SF icon names
				if (!Lex.EndsWith(iconName, "IconMx")) iconName += "IconMx";

				cssClasses += $@"<SfButton @ref ='{cb.Name}.Button' @bind-CssClass='{cb.Name}.CssClass' IconCss = '{iconName}' 
					Content='{cText}' OnClick='{cb.Name}_Click' />" + "\r\n";

				groupName = "CheckButtonGroupValue" + (cb.GroupIndex);
				if (!RadioGroups.Contains(groupName)) // add var to identify the current radio button for the group
				{
					varDefStmt += $"static CheckedValueContainer {groupName} = new CheckedValueContainer();\r\n";
					RadioGroups.Add(groupName);
				}

				initArgs = BuildInitArgs(c, 
					"Text", cText, "",
					"CVC", Nas(groupName), "");
				varDefStmt += $"\tpublic CheckButtonMx {cb.Name} = new CheckButtonMx(){initArgs};\r\n";

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// SimpleButton
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(SimpleButton))
			{
				// Example:
				// v.Container(children=[
				//				v.Btn(color='primary', children=[
				//				v.Icon(left=True, children=[
				//						'mdi-email-edit-outline'
				//				]),
				//				'Click me'
				//		])
				//	])


				//{ Text = "Yes", ImageName = "", StyleProps = new CssStyleMx("position: absolute; display: flex; align-items: center; right: 264px; bottom: 8px; width: 76px; height: 23px; ") };

				SimpleButton b = c as SimpleButton;
				bool isPrimary = false;

				dy2 = dy;
				if ((b.Anchor & WinForms.AnchorStyles.Top) != 0)
					dy2 -= 3;

				//else if ((b.Anchor & AnchorStyles.Bottom) != 0)
				//	dy2 += 3;

				string cssClass = "button-mx";

				string imageName = "";
				if (Lex.IsDefined(b.ImageOptions.ImageUri))
				{
					imageName = b.ImageOptions.ImageUri;

					if (Lex.StartsWith(imageName, "&#")) // unicode character?
					{
						cText += " " + imageName;
						imageName = "";
					}

					else // button with icon
					{
						cssClass = "icon-button-mx";

						if (b.Appearance.BackColor == Color.Transparent)
							cssClass = "transparent-icon-button-mx";

					}
				}

				if (Lex.Eq(c.Name, "BasicOpBut")) cText = ""; // hack to remove this CriteriaDialog button's text
				
				cssClasses += cssClass + " ";

				if (Lex.IsDefined(imageName))
					cssClasses += $" IconCss = '@{c.Name}.ImageName' ";

				if (Lex.Eq(cText, "OK") || Lex.Eq(cText, "Yes")) isPrimary = true;

				//string handler = WindowsHelper.GetEventHandlerName(c, "Click"); // doesn't work
				//if (Lex.IsDefined(handler)) ...
				cssClasses += $"@ref='{c.Name}.Button' @onclick = '{c.Name}_Click' />\r\n";

				ControlMx ctlMx = new ControlMx(); // for ref

				ButtonMx bmx = new ButtonMx() {};

				initArgs = BuildInitArgs(c,
					"Text", cText, bmx.Text,
					"ImageName", imageName, bmx.ImageName,
					"IsPrimary", isPrimary, bmx.IsPrimary);
	
				varDefStmt += $"\tpublic ButtonMx {c.Name} = new ButtonMx(){initArgs};\r\n"; // declare and set basic props

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				if (Lex.Eq(b.Text, "OK"))
				{
					eventStubFrag += @"

				/// <summary>
				/// OK_Click
				/// </summary>

				private async Task OK_Click()
				{
					DialogResult = DialogResult.OK;
					await SfDialog.Hide();
					return;
				}" + "\r\n";
				}

				else if (Lex.Eq(b.Text, "Cancel"))
				{
					eventStubFrag += @"

					/// <summary>
					/// Cancel_Click
					/// </summary>

					private async Task Cancel_Click()
				{
					DialogResult = DialogResult.Cancel;
					await SfDialog.Hide();
				}" + "\r\n";
				}

				else
				{
					eventStubFrag += $@"

				/// <summary>
				/// {b.Name}_Click
				/// </summary>
				/// <returns></returns>
				 
					private async Task {c.Name}_Click()
					{{
						await Task.Yield();
						return;
					}}" + "\r\n";
				}

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckedListBoxControl
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(CheckedListBoxControl))
			{
				CheckedListBoxControl db = c as CheckedListBoxControl;

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				cssClasses += $@"<SfListView CssClass='listview-mx' TValue='ListItemMx' ShowCheckBox='true' Width='100%' Height='100%'
            @ref='@{c.Name}.SfListView' DataSource='@{c.Name}.Items'>
          <ListViewFieldSettings TValue='ListItemMx' Text='Text' Id='Id' IsChecked='IsChecked'> </ListViewFieldSettings>
          <ListViewEvents TValue='ListItemMx' Clicked='{c.Name}_Clicked'>
          </ListViewEvents>
        </SfListView>" + "\r\n";

				initArgs = BuildInitArgs(c);
				varDefStmt += $"\tpublic CheckedListMx {c.Name} = new CheckedListMx(){initArgs};\r\n";

				eventStubFrag += $@"

				/// <summary>
				/// {c.Name}_Clicked
				/// </summary>
				/// <returns></returns>
				
				private async Task {c.Name}_Clicked(ClickEventArgs<ListItemMx> args)
				{{
						args.ItemData.IsChecked = args.IsChecked;
						await Task.Yield();
						return;
				 
					}}" + "\r\n";

				//div.Close(ref htmlFrag);
			}


			/////////////////////////////////////////////////////////////////////////////////
			// ComboBoxEdit - Text box with dropdown menu attached
			// Example:
			//  <SfComboBox TValue="string" TItem="Countries" @onkeypress="@KeyPressed" DataSource="@Country">
			//	 <ComboBoxFieldSettings Text = "Name" Value = "Code" ></ ComboBoxFieldSettings >
			//   <ComboBoxEvents TValue='string' ValueChange='control_ValueChange'></ComboBoxEvents> 
			//	</ SfComboBox>
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(ComboBoxEdit))
			{
				ComboBoxEdit cb = c as ComboBoxEdit;

				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				cssClasses += $@"<SfComboBox TValue='string' TItem='ListItemMx' Enabled='@{c.Name}.Enabled'
					@ref='{c.Name}.ComboBox' Placeholder='@{c.Name}.InitialText' DataSource='@{c.Name}.Items'>
						<ComboBoxFieldSettings Text='Text' IconCss='IconCss' Value='Value'></ComboBoxFieldSettings>
						<ComboBoxEvents TValue='string' TItem='ListItemMx' Focus='{c.Name}_Focus' ValueChange='{c.Name}_ValueChange'></ComboBoxEvents> 
					</SfComboBox> " + "\r\n";

				initArgs = BuildInitArgs(c);
				varDefStmt += $"\tpublic ComboBoxMx {c.Name} = new ComboBoxMx(){initArgs};\r\n";

				eventStubFrag += $@"

				/// <summary>
				/// {c.Name}_Focus - Component has received focus event
				/// </summary>
				
				private void {c.Name}_Focus(object args)
				{{
					return;
				}}

				/// <summary>
				/// {c.Name}_ValueChange
				/// </summary>
				/// <returns></returns>
				
				private async Task {c.Name}_ValueChange(ChangeEventArgs<string, ListItemMx> args)
				{{
						// todo...
						await Task.Yield();
						return;
				 
					}}" + "\r\n";

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// TextEdit / MemoEdit
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(TextEdit) || cType == typeof(MemoEdit))
			{
				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				// Example: <SfTextBox @bind-Value="@TextBoxValue" @ref="@SfTextBox" Type="InputType.Text" Placeholder="@InitialText" />

				TextEdit te = c as TextEdit;
				bool editable = !te.Properties.ReadOnly;
				string readOnly = editable ? "false" : "true";

				MemoEdit me = c as MemoEdit; // MemoEdit is a subclass of textedit
				string multiline = me != null ? "true" : "false";

				string enabled = te.Enabled ? "true" : "false";

				cssClasses += $@"<SfTextBox CssClass='e-small sftextbox-mx defaults-mx'  Type='InputType.Text' 
					@ref='{c.Name}.SfTextBox' @bind-Value='{c.Name}.Text' Readonly='{readOnly}' Multiline='{multiline}' Enabled ='{enabled}'";

				if (editable)
					cssClasses += $" Input='{c.Name}_Input' Focus='{c.Name}_Focus' ";

				cssClasses += "/>\r\n";

				initArgs = BuildInitArgs(c, "DivStyle", NewCss(inlineStyleProps));
				varDefStmt += $"\tpublic TextBoxMx {c.Name} = new TextBoxMx(){initArgs};\r\n";


				if (editable)
				{
					eventStubFrag += $@"

					/// <summary>
					/// {c.Name}_Focus - Component has received focus event
					/// </summary>
				
					private void {c.Name}_Focus(FocusInEventArgs args)
					{{
						return;
					}}

					/// <summary>
					/// {c.Name}_Input - Input text has changed event
					/// </summary>

					private void {c.Name}_Input(InputEventArgs args)
					{{
						return;
					}}" + "\r\n";
				}

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// Artificial DivMx control so the div for an empty container control gets created
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == null) //typeof(DivMx)) // "null" control
			{
				boundingBoxStmt = BuildBoundingBoxStmt(pc, c, dy);

				initArgs = BuildInitArgs(c);
				varDefStmt += $"\tpublic DivMx {c.Name} = new DivMx(){initArgs};\r\n";

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// Unrecognized
			/////////////////////////////////////////////////////////////////////////////////

			else // unrecognized
			{
				cssClasses += "<div> Unrecognized Control: " + c.GetType().Name + " " + c.Name;

				if (Lex.IsDefined(cText)) // insert the element content
					cssClasses += " Text=\"" + cText + '"';
				cssClasses += " </div>\r\n";
			}

// Integrate the control code into the top-level code block

			html += cssClasses;
			varDefCode += varDefStmt;
			initCode += boundingBoxStmt;
			eventCode += eventStubFrag;

			BuildAddChildControlStmt(pc, c);

			return;
		}

/// <summary>
/// Build a string that can be used to initialize a new instance of a component
/// </summary>
/// <param name="c"></param>
/// <param name="pa">An array of of triples (var-name, desired-value, default-value) </param>
/// <returns></returns>
		string BuildInitArgs(Control c, params object[] pa)
		{
			AssertMx.IsTrue(pa.Length % 3 == 0, $"Invalid number of parameters: {pa.Length}"); 
			string args = "", propName = "", propVal = "", defaultVal = "";

			Lex.AppendItemToStringList(ref args, $"Name = \"{c.Name}\"");

			if (!c.Enabled)
				Lex.AppendItemToStringList(ref args, "Enabled = false");

			//if (!c.Visible) // may not be correct at this point
			//	Lex.AppendItemToStringList(ref args, "Visible = false");

			if (c.Tag != null)
				Lex.AppendItemToStringList(ref args, $"Tag = \"{c.Tag.ToString()}\"");

			for (int i1 = 0; i1 < pa.Length; i1 += 3)
			{
				propName = pa[i1] as string;
				if (Lex.IsUndefined(propName)) continue;

				object propValObj = pa[i1 + 1];
				if (propValObj == null)
					propVal = "null";
				else propVal = propValObj.ToString();

				object defaultValObj = pa[i1 + 2];
				if (defaultValObj == null)
					defaultVal = "null";
				else defaultVal = defaultValObj.ToString();

				if (propVal == defaultVal) continue; // don't need to set if same as default

				if (propValObj is string) // quote string constants
					propVal = Lex.AddDoubleQuotes(propValObj as string);

				else if (propValObj is bool)
					propVal = propVal.ToLower();

				Lex.AppendItemToStringList(ref args, propName + " = " + propVal);
			}

			//if (Lex.IsDefined(args)) // allow empty braces
			args = "{" + args + "}";

			return args;
		}



		//////////////////////////////////////////////////////////////////////////////
		// Template for building DialogBoxMx class file 
		//////////////////////////////////////////////////////////////////////////////

		static string DialogBoxClassTemplate = @"
using Mobius.ComOps;
using Mobius.Data;
using Mobius.BaseControls;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Text;

namespace Mobius.ClientComponents
{
	public partial class <classname> : DialogBoxMx, IDialogBoxMx
	{

	/******************************* File links *********************************/
	public static <classname> DesignFile => <classname>.CsFile;
	/****************************************************************************/

// *****************************************************************************
// Controls contained in this component
// *****************************************************************************

	<componentDeclarations>

// *****************************************************************************
// Constructor
// *****************************************************************************

	public <classname>()
	{
	<componentInitialization>

	<addControls>
	}	
}
}

";

		//////////////////////////////////////////////////////////////////////////////
		// Template for building UserControlMx class file 
		//////////////////////////////////////////////////////////////////////////////
		
		static string UserControlClassTemplate = @"
using Mobius.ComOps;
using Mobius.Data;
using Mobius.BaseControls;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Text;

namespace Mobius.ClientComponents
{
	public partial class <classname> : UserControlMx
	{

	/******************************* File links *********************************/
	public static <classname> DesignFile => <classname>.CsFile;
	/****************************************************************************/

// *****************************************************************************
// Controls contained in this component
// *****************************************************************************

	<componentDeclarations>

// *****************************************************************************
// Constructor
// *****************************************************************************

	public <classname>()
	{
	<componentInitialization>

	<addControls>
	}	
}
}

";


		static string Children = "";

		static string EventStubs = "";


		NasClass Nas(object v)
		{
			return new NasClass(v);
		}

		/// <summary>
		/// Create a CssStyleMx constructor expression from a style string
		/// </summary>
		/// <param name="cssStyle"></param>
		/// <returns></returns>

		NasClass NewCss(string cssStyle)
		{
			return new NasClass($"new CssStyleMx({Lex.AddDoubleQuotesIfNeeded(cssStyle)})");
		}

		/// <summary>
		/// Not a string object
		/// </summary>
		class NasClass
		{
			object Value;

			public NasClass(object value)
			{
				Value = value;
			}

			public override string ToString()
			{
				return (Value != null ? Value.ToString() : null);
			}
		}

		/// <summary>
		/// DivMx Class
		/// </summary>

		//internal class DivMx : Control
		//{
		public string Class = "font-mx defaults-mx";
		public string Position = "absolute";
		public string Display = "flex";
		public string AlignItems = "center";

		//public string Width = "";
		//public string Height = "";

		//public string Top = "";
		//public string Bottom = "";
		//public string Left = "";
		//public string Right = "";

		public string Border = ""; // "1px solid yellow";

/// <summary>
/// Build the statement to set the CssStyleMx inlint props to define the bounding box for the HTML element
/// </summary>
/// <param name="pc"></param>
/// <param name="c"></param>
/// <param name="dy"></param>
/// <returns></returns>

		public string BuildBoundingBoxStmt(
			Control pc,
			Control c,
			int dy)
		{
			string inlineStyleProps = BuildBoundingBoxStyleProps(pc, c, dy);
			string boundingBoxStmt = $"\t{c.Name}.StyleProps = new CssStyleMx(\"{inlineStyleProps}\");\r\n";
			return boundingBoxStmt;
		}

		/// <summary>
		/// Build the css properties that define the position and size of the 
		/// bounding box that contains the control
		/// </summary>
		/// <param name="pc">Parent control</param>
		/// <param name="c">Control being wrapped in the Div</param>
		/// <returns></returns>

		public string BuildBoundingBoxStyleProps(
			Control pc,
			Control c,
			int dy,
			string styleAttributesToRemove = null,
			string styleAttributesToAdd = null)
		{
			Type pcType = pc.GetType();
			if (pc is Form || pc is XtraForm) // Form or XtraForm?
				dy += 32; // add pixel height of WinForms window header to div top/bottom to adjust for move to HTML

			int cTop = c.Top + dy; // move top and bottom down to correct from WinForms to HTML
			int cBottom = c.Bottom + dy;

			string classes = "control-div-mx font-mx defaults-mx";
			string styleProps = "position = 'absolute', display = 'flex', align-items = 'center'";
			string code = ""; // nothing yet

			if (c.Dock == WinForms.DockStyle.Fill) // if Dock defined the set width/height to 100% (i.e. Fill only for now)
				Lex.AppendItemToStringList(ref styleProps, "width = '100%', height = '100%'");

			else // use Anchor settings
			{
				string widthHeight = "";
				bool anchoredLeft = ((c.Anchor & WinForms.AnchorStyles.Left) != 0);
				bool anchoredRight = ((c.Anchor & WinForms.AnchorStyles.Right) != 0);

				if (anchoredLeft)
				{
					Lex.AppendItemToStringList(ref styleProps, "left = " + c.Left + "px'");
					if (!anchoredRight)
						Lex.AppendItemToStringList(ref widthHeight, "width = " + c.Width + "px'");

					else // if left & right defined then set width as a calc()
						Lex.AppendItemToStringList(ref widthHeight, "width = 'calc(100% - " + (c.Left + (pc.Width - c.Right)) + "px)'");
				}

				else if (anchoredRight) // set right and width
				{
					Lex.AppendItemToStringList(ref styleProps, "right = " + (pc.Width - c.Right) + "px'");
					Lex.AppendItemToStringList(ref widthHeight, "width = " + c.Width + "px'");
				}

				bool anchorTop = ((c.Anchor & WinForms.AnchorStyles.Top) != 0);
				bool anchorBottom = ((c.Anchor & WinForms.AnchorStyles.Bottom) != 0);

				// Note: for buttons it looks like they could move up one px & be 2px taller

				int cHeight = c.Height + 2; // make a bit taller

				if (anchorTop)
				{
					Lex.AppendItemToStringList(ref styleProps, "top = " + cTop + "px'");
					if (!anchorBottom)
						Lex.AppendItemToStringList(ref widthHeight, "height = '" + cHeight + "px'");

					else // if top & bottom defined then set height as a calc()
						Lex.AppendItemToStringList(ref widthHeight, "height = 'calc(100% - " + (cTop + (pc.Height - cBottom)) + "px)'");
				}

				else if (anchorBottom) // set bottom and height
				{
					Lex.AppendItemToStringList(ref styleProps, "bottom = " + (pc.Height - cBottom) + "px");
					Lex.AppendItemToStringList(ref widthHeight, "height = " + cHeight + "px'");
				}

				Lex.AppendItemToStringList(ref styleProps, widthHeight); // put width height after location
			}


			if (Lex.IsDefined(styleAttributesToRemove))
			{
				//style = Lex.Replace(style, styleAttributesToRemove, "");
			}

			if (Lex.IsDefined(styleAttributesToAdd))
			{
				//style = Lex.Replace(style, "style='", "style='" + styleAttributesToAdd + " ");
			}

			return styleProps;
		}

		/// <summary>
		/// Build statement to add control as a child to the parent
		/// e.g.: this.Controls.Add(this.Message);
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="c"></param>
		void BuildAddChildControlStmt(Control pc, Control c)
		{
			if (pc != null)
			{
				string txt = "";

				if (pc != null && !(pc is UserControl || pc is XtraUserControl || pc is Form || pc is XtraForm ))
						txt = pc.Name + ".";

				AddControlsCode += $"\t{txt}ChildControls.Add({c.Name});\r\n";
			}
		}

	/// <summary>
	/// Return string to close the div
	/// </summary>
	/// <returns></returns>

	public void Close(
			ref string htmlFrag)
		{
			htmlFrag += "</div>\r\n";
			return;
		}

	}

}
