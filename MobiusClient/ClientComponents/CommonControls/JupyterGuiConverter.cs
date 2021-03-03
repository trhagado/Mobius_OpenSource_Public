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
using Dx = DevExpress.XtraEditors;
using DevExpress.XtraEditors.Controls;
using DevExpress.XtraTab;
using DevExpress.Utils;

namespace Mobius.ClientComponents
{
	public class JupyterGuiConverter
	{
		string AddControlsCode = "";

		static HashSet<string> ControlsLogged = new HashSet<string>();
		HashSet<string> RadioGroups = new HashSet<string>(); // names of each of the radio button groups seen 
		StreamWriter log;

		bool IncludeMenuStubs = false;
		public static bool ConversionsEnabled = DebugMx.False; // set to true to generate UI migration code

		/// <summary>
		/// Convert a Windows/DevExpress Form or UserControl to a set of ControlMx-based controls
		/// that can be later converted into a set of Jupyter widgets, Blazor, etc components for rendering.
		/// </summary>
		/// <param name="cc"></param>

		public void ConvertFormOrUserControl(
			Control cc,
			bool includeMenuStubs = false)
		{
			string eventCode = ""; // temp accumulator
			string varDefStmt = "", varConstructorStmt = "", eventStubFrag = "";
			string inlineStyleProps = "", groupName = "", varInitClause = "";
			string template = "";

			int dy = 0;

			if (!ConversionsEnabled) return;

			IncludeMenuStubs = includeMenuStubs;

			if (WinFormsUtil.ControlMxConverter == null) // allow calls to converter from ComOps
				WinFormsUtil.ControlMxConverter = new ControlMxConverterDelegate(ConvertFormOrUserControl);

			/////////////////////////////////////////////////////////////////////////////
			// Build the HTML and plug into the template
			/////////////////////////////////////////////////////////////////////////////

			string html = "", varDefCode = "", varConstructorCode = ""; // strings for global building of html and code sections
			AddControlsCode = "";

			Type containerType = cc.GetType();
			string containerTypeName = containerType.Name;
			containerTypeName = Lex.Replace(containerTypeName, "MessageBoxMx2", "MessageBoxMx");
			string ctlFullName = containerType.FullName;
			//if (ControlsLogged.Contains(ctlFullName)) return;
			ControlsLogged.Add(ctlFullName);

			log = new StreamWriter(@"C:\MobiusJupyter\GeneratedGuiComponents\" + containerTypeName + ".txt");

			RadioGroups = new HashSet<string>();

			html = $"def {containerTypeName} ():\r\n"; // class name

			/////////////////////////////////////////////////////////////////////////////
			// Process a top level Form or XtraForm (i.e. DialogBox)
			/////////////////////////////////////////////////////////////////////////////

			if (typeof(Form).IsAssignableFrom(containerType)) // Form or XtraForm?
			{
				Form f = cc as Form;
				FormMetricsInfo fmi = WindowsHelper.GetFormMetrics(f);

				DialogBoxMx dbMx = new DialogBoxMx();

				inlineStyleProps = BuildBoundingBoxStyleProps(null, f, 0);

				// Constructor initialization code for this class 

				varConstructorCode = $@"
		WindowTitle = '{f.Text}';
		ShowIcon = {f.ShowIcon};
		ImageName = ''; 
		FormBorderStyle = FormBorderStyle.{f.FormBorderStyle};
		MinimizeBox = {f.MinimizeBox};
		MaximizeBox = {f.MaximizeBox};

		StyleProps = new CssPropsMx('{inlineStyleProps}'); 

		";

				varConstructorCode = varConstructorCode.Replace('\'', '"'); // fix syntax details
				varConstructorCode = varConstructorCode.Replace("True", "true");
				varConstructorCode = varConstructorCode.Replace("False", "false");

				template = DialogBoxClassTemplate;

				// Insert this forms controls into a DialogBoxContainer Form ContentPanel and then convert 
				// that dialog so that the window header controls are included in the conversion as well

				DialogBoxContainer f2 = new DialogBoxContainer();
				f2.Width = f.Width;
				f2.Height = f.Height + f2.DialogBoxHeaderPanel.Height;
				FormMetricsInfo fmi2 = WindowsHelper.GetFormMetrics(f2);

				List<Control> cl = new List<Control>(); // copy ctls to list
				foreach (Control c in f.Controls)
					cl.Add(c);

				f.Controls.Clear(); // remove from original form

				foreach (Control c in cl) // copy from list to new form
					f2.ContentPanel.Controls.Add(c);

				ConvertContainedControls(f2, 0, ref html, ref varDefCode, ref varConstructorCode);

				f2.ContentPanel.Controls.Clear(); // remove from new form

				foreach (Control c in cl) // put back in original form
					f.Controls.Add(c);

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

			else if (cc is UserControl || cc is XtraUserControl)
			{
				html = "";

				varDefCode = "";

				ConvertContainedControls(cc, 0, ref html, ref varDefCode, ref varConstructorCode);

				template = UserControlClassTemplate;
			}

			else throw new Exception("Can't convert control of type: " + cc?.GetType()?.Name);

			// Fill in the template for the Form or UserControl and write out the class code

			string csCode = Lex.Replace(template, "<className>", containerTypeName); // get template and substitute proper class name

			csCode = Lex.Replace(csCode, "<componentDeclarations>", varDefCode);

			csCode = Lex.Replace(csCode, "<componentInitialization>", varConstructorCode);

			csCode = Lex.Replace(csCode, "<addControls>", AddControlsCode);

			log.Close();

			StreamWriter sw = new StreamWriter(@"C:\MobiusJupyter\GeneratedGuiComponents\" + containerTypeName + ".design.cs");
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
			ref string varConstructorCode)
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

				ConvertControl(pc, c, dy, ref html, ref varDefCode, ref varConstructorCode); // convert the control and surround with a positioning div

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
			ref string varConstructorCode)
		{
			string eventCode = ""; // temp accumulator
			string ctlDeclAndInitStmt = "", ctlStyleStmts = "", eventStubFrag = "", oldSyncfusionStmt = "";
			string inlineStyleProps = "", groupName = "", ctlInitClause = "", imageName = "";
			int dy2 = 0;

			CssClassesMx cssClasses = new CssClassesMx("defaults-mx control-div-mx");

			string tag = c.Tag as string; // get any custom class assignments
			if (Lex.Contains(tag, "class=") || Lex.Contains(tag, "class =")) // crude
			{
				string classes = Lex.SubstringBetween(tag, "\"", "\"");
				cssClasses.AddClass(classes);
			}

			CssPropsMx cssProps = new CssPropsMx();

			Type cType = c.GetType();
			string cText = c.Text;
			if (Lex.StartsWith(cText, "&")) // remove shortcut indicators within control text (e.g. menu items, buttons, ...)
				cText = cText.Substring(1);

			cText = HttpUtility.HtmlEncode(c.Text);

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

				ctlInitClause = BuildCtlInitClause(c);
				ctlDeclAndInitStmt += $"\tpublic {typeName} {c.Name} = new {typeName}(){ctlInitClause};\r\n"; // declare and set basic props

				inlineStyleProps = BuildBoundingBoxStyleProps(pc, c, dy);

				if (uc.BorderStyle != BorderStyle.None)
					inlineStyleProps += " border: 1px solid #acacac; background-color: #ffffff; ";

				ctlStyleStmts = $"\t{c.Name}.StyleProps = new CssPropsMx(\"{inlineStyleProps}\");\r\n"; // set bounding box in initialize method
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

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				gb.Height -= 8;
				gb.Top += 8;

				ctlInitClause = BuildCtlInitClause(c,
					"Text", gb.Text, bmx.Text);

				ctlDeclAndInitStmt += $"\tpublic GroupBoxMx {c.Name} = new GroupBoxMx(){ctlInitClause};\r\n"; // declare and set basic props

				oldSyncfusionStmt +=
					"<GroupBox class='groupbox-mx'>\r\n";

				dy2 = 4; // move contained controls down a bit
				ConvertContainedControls(c, dy2, ref oldSyncfusionStmt, ref ctlDeclAndInitStmt, ref ctlStyleStmts);

				oldSyncfusionStmt += "</fieldset>\r\n";

				//div.Close(ref htmlFrag);
			}

			/*************************/
			/*** Panel / XtraPanel ***/
			/*************************/

			else if (cType == typeof(Panel) || cType == typeof(PanelControl) || cType == typeof(XtraPanel))
			{
				PanelControlMx cMx = new PanelControlMx();
				ctlDeclAndInitStmt = BuildDeclAndInitStmt(c, cMx);

				ctlStyleStmts += BuildInlineStylePropsStmt(pc, c, dy); // inline CSS
				ctlStyleStmts += BuildClassesStmt(c, cssClasses); // CSS class assignments

				ConvertContainedControls(c, dy2, ref oldSyncfusionStmt, ref ctlDeclAndInitStmt, ref ctlStyleStmts);
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

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				//tab.Height -= 8;
				//tab.Top += 8;

				oldSyncfusionStmt +=
					@"<SfTab class='tab-control-mx' Height='100%' Width='100%'>
							<TabItems>" + "\r\n";

				ctlInitClause = BuildCtlInitClause(c);
				ctlDeclAndInitStmt += $"\tpublic TabMx {c.Name} = new TabMx(){ctlInitClause};\r\n";

				dy2 = 0; // adjust position of contained controls
				ConvertContainedControls(c, dy2, ref oldSyncfusionStmt, ref ctlDeclAndInitStmt, ref ctlStyleStmts);

				oldSyncfusionStmt +=
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

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);
				//tab.Height -= 8;
				//tab.Top += 8;

				oldSyncfusionStmt +=
					$@"
					 <TabItem>
              <ChildContent>
                  <TabHeader Text='{tp.Text}'></TabHeader>
              </ChildContent>
              <ContentTemplate>
						";

				ctlInitClause = BuildCtlInitClause(c);
				ctlDeclAndInitStmt += $"\tpublic TabPageMx {c.Name} = new TabPageMx(){ctlInitClause};\r\n";

				dy2 = 0; // adjust position of contained controls
				ConvertContainedControls(c, dy2, ref oldSyncfusionStmt, ref ctlDeclAndInitStmt, ref ctlStyleStmts);

				oldSyncfusionStmt +=
					@"</ContentTemplate>
						</TabItem>\r\n";

				//div.Close(ref htmlFrag);
			}

			////////////////////////////////////////////////////////////////////////////////
			// WinForms/DX builtin controls (non-grouping)
			////////////////////////////////////////////////////////////////////////////////

			/////////////////////////////////////////////////////////////////////////////////
			// Dx.LabelControl
			/////////////////////////////////////////////////////////////////////////////////


			else if (cType == typeof(Dx.LabelControl)) // DX LabelControl
			{
				Dx.LabelControl lc = c as Dx.LabelControl;
				TextOptions to = lc.Appearance.TextOptions;

				LabelControlMx lcMx = new LabelControlMx();

				ctlDeclAndInitStmt = BuildDeclAndInitStmt(c, lcMx, "Text", lc.Text, "", "LineVisible", lc.LineVisible, lcMx.LineVisible);

				if (!lc.LineVisible) // it not a line, check to see if a border is desired
				{
					bool showBorder = lc.BorderStyle != BorderStyles.NoBorder;
					if (showBorder && lc.Appearance.Options.UseBorderColor && lc.Appearance.BorderColor == Color.Transparent)
						showBorder = false; // hiding via transparency
					if (showBorder) cssClasses.AddClass("border-mx");
				}

				ctlStyleStmts += BuildInlineStylePropsStmt(pc, c, dy, inlineStyleProps);

				ctlStyleStmts += BuildClassesStmt(c, cssClasses);

#if false // oldish, but works
				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				LabelControl lc = c as LabelControl;
				TextOptions to = lc.Appearance.TextOptions;

				if (lc.LineVisible)
				{
					if (Lex.IsUndefined(lc.Text))
						oldSyncfusionStmt += "<hr class='hr-mobius'>\r\n";

					else // horizontal rule with text overlaid using utility css
						oldSyncfusionStmt += $"<hr data-content='{lc.Text}' class=' class='hr-mobius hr-text'>\r\n";
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
					 *   LabelControlMx Message = new LabelControlMx() { Text = "Message...", DivStyle = new CssPropsMx("position: absolute; display: 
					 *   flex; align-items: center; 
					 *   overflow: hidden; text-overflow: ellipsis; [OR] overflow-x: scroll; overflow-y: scroll; 
					 *   left: 56px; top: 34px; width: calc(100% - 74px); height: calc(100% - 68px);  
					 *   border: 0px solid #acacac; background-color: #eeeeee; ") };
					 * 
					 */

					oldSyncfusionStmt += $"<span {spanStyle}>@{c.Name}.Text</span>\r\n";
				}

				ctlInitClause = BuildCtlInitClause(c, "Text", lc.Text, "");
				ctlDefStmt += $"\tpublic LabelControlMx {c.Name} = new LabelControlMx(){ctlInitClause};\r\n";

				//div.Close(ref htmlFrag);
#endif
			}

			////////////////////////////////////////////////////////////////////////////////
			// WinForms.Label
			////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(WinForms.Label)) // Simpler Windows.Forms Label
			{
				Label l = c as Label;
				LabelControlMx lcMx = new LabelControlMx();

				if (Lex.IsUndefined(l.Text)) return; // ignore if no text

				ctlDeclAndInitStmt = BuildDeclAndInitStmt(c, lcMx, "Text", l.Text, "");

				if (l.BorderStyle != BorderStyle.None)
					inlineStyleProps += " border: 1px solid #acacac; background-color: #eeeeee; ";

				ctlStyleStmts += BuildInlineStylePropsStmt(pc, c, dy, inlineStyleProps); // inline CSS

				ctlStyleStmts += BuildClassesStmt(c, cssClasses); // CSS class assignments
			}

			////////////////////////////////////////////////////////////////////////////////
			// WinForms.PictureBox
			////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(WinForms.PictureBox)) // Simple image
			{
				PictureBox pb = c as PictureBox;
				PictureBoxMx pbMx = new PictureBoxMx();

				imageName = pb?.Tag as string;
				ctlDeclAndInitStmt = BuildDeclAndInitStmt(c, pbMx, "ImageName", imageName, "");

				if (pb.BorderStyle != BorderStyle.None)
					inlineStyleProps += " border: 1px solid #acacac; background-color: #eeeeee; ";

				ctlStyleStmts += BuildInlineStylePropsStmt(pc, c, dy, inlineStyleProps); // inline CSS

				ctlStyleStmts += BuildClassesStmt(c, cssClasses); // CSS class assignments
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

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				oldSyncfusionStmt += $@"<SfDropDownButton CssClass='button-mx' 
				  @ref='{c.Name}.Button' Content='@{c.Name}.Text 
					@onclick = '{c.Name}_Click' >
				 </SfDropDownButton>";

				//<DropDownMenuItems>
				//  <DropDownMenuItem Text='Loading...'></DropDownMenuItem>
				//</DropDownMenuItems>

				ctlInitClause = BuildCtlInitClause(c, "Text", c.Text, "");
				ctlDeclAndInitStmt += $"\tpublic DropDownButtonMx {c.Name} = new DropDownButtonMx(){ctlInitClause};\r\n";

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckEdit (CheckBox and RadioButton)
			/////////////////////////////////////////////////////////////////////////////////


			else if (cType == typeof(CheckEdit))
			{
				if (dy == 0)
					dy = -4; // move up a bit

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				CheckEdit ce = c as CheckEdit;
				CheckBoxStyle style = ce.Properties.CheckBoxOptions.Style;
				if (style == CheckBoxStyle.Default)
				{
					if (ce.Properties.RadioGroupIndex >= 0)
						style = CheckBoxStyle.Radio;
					else style = CheckBoxStyle.CheckBox;
				}

				cText = c.Text.Trim();

				/////////////////////////////////////////////////////////////////////////////////
				// CheckBox
				/////////////////////////////////////////////////////////////////////////////////

				if (style == CheckBoxStyle.CheckBox)
				{ 
					CheckBoxMx cbMx = new CheckBoxMx();

					ctlDeclAndInitStmt = BuildDeclAndInitStmt(c, cbMx,
						"Text", cText, cbMx.Text,
						"Checked", ce.Checked, cbMx.Checked);

					ctlStyleStmts += BuildInlineStylePropsStmt(pc, c, dy); // inline CSS

					ctlStyleStmts += BuildClassesStmt(c, cssClasses); // CSS class assignments
				}

				/////////////////////////////////////////////////////////////////////////////////
				// CheckEdit (CheckBox and RadioButton)
				/////////////////////////////////////////////////////////////////////////////////
				
				else // assume RadioButton
				{
					RadioButtonMx rbMx = new RadioButtonMx();

					groupName = "RadioGroup" + ce.Properties.RadioGroupIndex;

					if (!RadioGroups.Contains(groupName)) // add var to identify the current radio button for the group
					{
						RadioGroups.Add(groupName);
					}

					ctlDeclAndInitStmt = BuildDeclAndInitStmt(c, rbMx,
						"Text", cText, rbMx.Text,
						"Checked", ce.Checked, rbMx.Checked,
						"GroupName", groupName, rbMx.GroupName);

					ctlStyleStmts += BuildInlineStylePropsStmt(pc, c, dy); // inline CSS

					ctlStyleStmts += BuildClassesStmt(c, cssClasses); // CSS class assignments
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

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				CheckButton cb = c as CheckButton;

				groupName = "CheckButtonGroup" + cb.GroupIndex;
				string iconName = cb.Name;
				iconName = Lex.Replace(iconName, "Va", ""); // hack for TextAlignment Dialog
				iconName = Lex.Replace(iconName, "Ha", "");

				if (!Lex.StartsWith(iconName, "Align")) iconName = "Align" + iconName; // make name match SF icon names
				if (!Lex.EndsWith(iconName, "IconMx")) iconName += "IconMx";

				oldSyncfusionStmt += $@"<SfButton @ref ='{cb.Name}.Button' @bind-CssClass='{cb.Name}.CssClass' IconCss = '{iconName}' 
					Content='{cText}' OnClick='{cb.Name}_Click' />" + "\r\n";

				groupName = "CheckButtonGroupValue" + (cb.GroupIndex);
				if (!RadioGroups.Contains(groupName)) // add var to identify the current radio button for the group
				{
					ctlDeclAndInitStmt += $"static CheckedValueContainer {groupName} = new CheckedValueContainer();\r\n";
					RadioGroups.Add(groupName);
				}

				ctlInitClause = BuildCtlInitClause(c,
					"Text", cText, "",
					"CVC", Nas(groupName), "");
				ctlDeclAndInitStmt += $"\tpublic CheckButtonMx {cb.Name} = new CheckButtonMx(){ctlInitClause};\r\n";

				//div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// SimpleButton
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(DevExpress.XtraEditors.SimpleButton))
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


				//{ Text = "Yes", ImageName = "", StyleProps = new CssPropsMx("position: absolute; display: flex; align-items: center; right: 264px; bottom: 8px; width: 76px; height: 23px; ") };

				SimpleButton b = c as SimpleButton;

				ButtonMx bmx = new ButtonMx();
				bool isPrimary = false;

				dy2 = dy;
				if ((b.Anchor & WinForms.AnchorStyles.Top) != 0)
					dy2 -= 3;

				//else if ((b.Anchor & AnchorStyles.Bottom) != 0)
				//	dy2 += 3;

				bool ultraFlat = b.LookAndFeel.Style == DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat;
				if (!ultraFlat)
					cssClasses.AddClass("button-mx");
				else 
					cssClasses.AddClass("transparent-button-mx");

				//if (b.Name == "CloseWindowButton")
				//{
				//	if (b.LookAndFeel.Style == DevExpress.LookAndFeel.LookAndFeelStyle.UltraFlat)
				//		cssClasses.AddClass("borderstyle-none-mx");

				//	if (b.Appearance.BackColor == Color.Transparent)
				//		cssClasses.AddClass("backcolororderstyle-none-mx");
				//}

				imageName = "";
				if (Lex.IsDefined(b.ImageOptions.ImageUri)) // check url first
					imageName = b.ImageOptions.ImageUri;

				else imageName = (b.Tag as string); // check tag 2nd

				if (Lex.IsDefined(imageName))
				{
					if (!Lex.EndsWith(imageName, "IconMx"))
						imageName += "IconMx";

					cssClasses.AddClass(imageName); 
				}

				if (Lex.Eq(c.Name, "BasicOpBut")) cText = ""; // hack to remove this CriteriaDialog button's text


				if (Lex.Eq(cText, "OK") || Lex.Eq(cText, "Yes")) isPrimary = true;

				ctlDeclAndInitStmt = BuildDeclAndInitStmt(c, bmx,
					"Text", cText, bmx.Text,
					"ImageName", imageName, bmx.ImageName, // need this along the adding the image name to the classes list
					"IsPrimary", isPrimary, bmx.IsPrimary);

				ctlStyleStmts += BuildClassesStmt(c, cssClasses);
				ctlStyleStmts += BuildInlineStylePropsStmt(pc, c, dy);

				//ctlStylePropsStmt = $"\t{c.Name}.StyleProps = new CssPropsMx(\"{inlineStyleProps}\");\r\n"; // set bounding box in initialize method


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

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				oldSyncfusionStmt += $@"<SfListView CssClass='listview-mx' TValue='ListItemMx' ShowCheckBox='true' Width='100%' Height='100%'
            @ref='@{c.Name}.SfListView' DataSource='@{c.Name}.Items'>
          <ListViewFieldSettings TValue='ListItemMx' Text='Text' Id='Id' IsChecked='IsChecked'> </ListViewFieldSettings>
          <ListViewEvents TValue='ListItemMx' Clicked='{c.Name}_Clicked'>
          </ListViewEvents>
        </SfListView>" + "\r\n";

				ctlInitClause = BuildCtlInitClause(c);
				ctlDeclAndInitStmt += $"\tpublic CheckedListMx {c.Name} = new CheckedListMx(){ctlInitClause};\r\n";

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

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				oldSyncfusionStmt += $@"<SfComboBox TValue='string' TItem='ListItemMx' Enabled='@{c.Name}.Enabled'
					@ref='{c.Name}.ComboBox' Placeholder='@{c.Name}.InitialText' DataSource='@{c.Name}.Items'>
						<ComboBoxFieldSettings Text='Text' IconCss='IconCss' Value='Value'></ComboBoxFieldSettings>
						<ComboBoxEvents TValue='string' TItem='ListItemMx' Focus='{c.Name}_Focus' ValueChange='{c.Name}_ValueChange'></ComboBoxEvents> 
					</SfComboBox> " + "\r\n";

				ctlInitClause = BuildCtlInitClause(c);
				ctlDeclAndInitStmt += $"\tpublic ComboBoxMx {c.Name} = new ComboBoxMx(){ctlInitClause};\r\n";

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
				TextEdit te = c as TextEdit;
				TextBoxMx tbMx = new TextBoxMx();

				bool editable = !te.Properties.ReadOnly;
				bool readOnly = editable ? false : true;

				MemoEdit me = c as MemoEdit; // MemoEdit is a subclass of textedit
				bool multiline = me != null ? true : false;

				bool enabled = te.Enabled ? true : false;

				ctlDeclAndInitStmt = BuildDeclAndInitStmt(c, tbMx,
					"Text", te.Text, tbMx.Text,
					"PlaceHolder", "", tbMx.PlaceHolder,
					//"FloatLabelType", "", tbMx.FloatLabelType,
					"ReadOnly", readOnly, tbMx.ReadOnly,
					"Multiline", multiline, tbMx.MultiLine,
					"Enabled", enabled, tbMx.Enabled);

				//cssClasses.AddClass("border-mx"); // add css classses as needed

				ctlStyleStmts += BuildInlineStylePropsStmt(pc, c, dy, inlineStyleProps);

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);

				ctlStyleStmts += BuildClassesStmt(c, cssClasses);

#if false


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
#endif
			}

			/////////////////////////////////////////////////////////////////////////////////
			// Artificial DivMx control so the div for an empty container control gets created
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == null) //typeof(DivMx)) // "null" control
			{
				ctlInitClause = BuildCtlInitClause(c);
				ctlDeclAndInitStmt += $"\tpublic DivMx {c.Name} = new DivMx(){ctlInitClause};\r\n";

				ctlStyleStmts = BuildInlineStylePropsStmt(pc, c, dy);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// Unrecognized
			/////////////////////////////////////////////////////////////////////////////////

			else // unrecognized
			{
				oldSyncfusionStmt += "<div> Unrecognized Control: " + c.GetType().Name + " " + c.Name;

				if (Lex.IsDefined(cText)) // insert the element content
					oldSyncfusionStmt += " Text=\"" + cText + '"';
				oldSyncfusionStmt += " </div>\r\n";
			}

			// Integrate the control code into the code passed by the caller

			varDefCode += ctlDeclAndInitStmt;

			varConstructorCode += ctlStyleStmts;

			eventCode += eventStubFrag;

			BuildAddChildControlsStmt(pc, c);

			html += oldSyncfusionStmt; // get rid of eventually

			return;
		}

		/// <summary>
		/// Build the statement to  create the control and set properties other than CSS propertis
		/// </summary>
		/// <param name="c"></param>
		/// <param name="pa"></param>
		/// <returns></returns>

		string BuildDeclAndInitStmt(
			Control c,
			ControlMx cmx,
			params object[] pa)
		{
			string ctlInitClause = BuildCtlInitClause(c, pa);
			string cmxTypeName = cmx.GetType().Name;

			string ctlDefStmt = $"\tpublic {cmxTypeName} {c.Name} = new {cmxTypeName}(){ctlInitClause};\r\n";
			return ctlDefStmt;
		}

		/// <summary>
		/// Build a string that can be used to initialize a new instance of a component
		/// </summary>
		/// <param name="c"></param>
		/// <param name="pa">An array of of triples (var-name, desired-value, default-value) </param>
		/// <returns></returns>
		string BuildCtlInitClause(Control c, params object[] pa)
		{
			AssertMx.IsTrue(pa.Length % 3 == 0, $"Invalid number of parameters: {pa.Length}");
			string args = "", propName = "", propVal = "", defaultVal = "";

			Lex.AppendToList(ref args, $"Name = \"{c.Name}\"");

			if (!c.Enabled)
				Lex.AppendToList(ref args, "Enabled = false");

			//if (!c.Visible) // may not be correct at this point
			//	Lex.AppendItemToStringList(ref args, "Visible = false");

			//if (c.Tag != null)
			//	Lex.AppendToList(ref args, $"Tag = \"{c.Tag.ToString()}\"");

			for (int i1 = 0; i1 < pa.Length; i1 += 3)
			{
				propVal = defaultVal = "";
				propName = pa[i1] as string;
				if (Lex.IsUndefined(propName)) continue;

				object propValObj = pa[i1 + 1];
				if (propValObj != null)
					propVal = propValObj.ToString();

				object defaultValObj = pa[i1 + 2];
				if (defaultValObj != null)
					defaultVal = defaultValObj.ToString();

				if (propVal == defaultVal) continue; // don't need to set if same as default

				if (propValObj is string) // quote string constants
					propVal = Lex.AddDoubleQuotes(propValObj as string);

				else if (propValObj is bool)
					propVal = propVal.ToLower();

				Lex.AppendToList(ref args, propName + " = " + propVal);
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
			return new NasClass($"new CssPropsMx({Lex.AddDoubleQuotesIfNeeded(cssStyle)})");
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
		/// Build a statement to initialize the set of classes associated with a control
		/// </summary>
		/// <param name="c"></param>
		/// <param name="classes"></param>
		/// <returns></returns>

		public string BuildClassesStmt(
			Control c,
			CssClassesMx classes)
		{
			string classList = classes.Format();
			if (Lex.IsUndefined(classList)) return "";

			string stmt = $"\t{c.Name}.StyleClasses = new CssClassesMx(\"{classList}\");\r\n";
			return stmt;
		}

		/// <summary>
		/// Build the statement to set the CssStyleMx inlint props to define the bounding box for the HTML element
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="c"></param>
		/// <param name="dy"></param>
		/// <returns></returns>

		public string BuildInlineStylePropsStmt(
			Control pc,
			Control c,
			int dy,
			string additionalStyleAttributes = null)
		{
			string inlineStyleProps = BuildBoundingBoxStyleProps(pc, c, dy, additionalStyleAttributes);
			string varConstructorStmt = $"\t{c.Name}.StyleProps = new CssPropsMx(\"{inlineStyleProps}\");\r\n";
			return varConstructorStmt;
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
			string additionalStyleAttributes = null)
		{
			//Type pcType = pc.GetType();
			//if (pc is Form || pc is XtraForm) // Form or XtraForm?
			//	dy += 32; // add pixel height of WinForms window header to div top/bottom to adjust for move to HTML

			bool isTopLevelDialogBoxContainer = (c.Controls.Count > 0 && // in control a container directly under a Form or XtraForm parent?
				pc != null && typeof(Form).IsAssignableFrom(pc.GetType()));

			int cTop = c.Top + dy; // move top and bottom down to correct from WinForms to HTML
			int cBottom = c.Bottom + dy;

			string classes = "control-div-mx font-mx defaults-mx";
			string styleProps = "position: absolute; display:flex; align-items:center; ";
			string code = ""; // nothing yet

			if (c.Dock == WinForms.DockStyle.Fill) // if Dock defined the set width/height to 100% (i.e. Fill only for now)
				styleProps+= $"width: 100%; height: 100%; ";

			else // use Anchor settings
			{
				string widthHeight = "";
				bool anchoredLeft = ((c.Anchor & WinForms.AnchorStyles.Left) != 0);
				bool anchoredRight = ((c.Anchor & WinForms.AnchorStyles.Right) != 0);

				if (anchoredLeft)
				{
					styleProps += $"left: {c.Left}px; ";
					if (!anchoredRight)
						widthHeight += $"width: {c.Width}px; ";

					else // if left & right defined then set width as a calc()
					{
						if (isTopLevelDialogBoxContainer)
							widthHeight += $"width: 100%; ";
						else
							widthHeight += $"width: calc(100% - {c.Left + (pc.Width - c.Right)}px); ";
					}
				}

				else if (anchoredRight) // set right and width
				{
					styleProps += $"right: {(pc.Width - c.Right)}px; ";
					widthHeight += $"width: {c.Width}px; ";
				}

				bool anchorTop = ((c.Anchor & WinForms.AnchorStyles.Top) != 0);
				bool anchorBottom = ((c.Anchor & WinForms.AnchorStyles.Bottom) != 0);

				// Note: for buttons it looks like they could move up one px & be 2px taller

				int cHeight = c.Height; // + 2; // make a bit taller?

				if (anchorTop)
				{
					styleProps += $"top: {cTop}px; ";
					if (!anchorBottom)
						widthHeight += $"height: {cHeight}px; ";

					else // if top & bottom defined then set height as a calc()
					{
						if (isTopLevelDialogBoxContainer) // adjustment to DialogBoxContainer.ContentPanel height to account for "artificial" addition of HeaderPanel 
							widthHeight += $"height: calc(100% - {cTop}px); ";
						else
							widthHeight += $"height: calc(100% - {cTop + (pc.Height - cBottom)}px); ";
					}
				}

				else if (anchorBottom) // set bottom and height
				{
					styleProps += $"bottom: {pc.Height - cBottom}px; ";
					widthHeight += $"height: {cHeight}px; ";
				}

				styleProps += widthHeight; // put width height after location
			}


			if (Lex.IsDefined(additionalStyleAttributes))
				styleProps += additionalStyleAttributes;

			// Fixup to proper css form, e.g.: (height = "100px",) => (height: 100px;)

			//styleProps = Lex.Replace(styleProps, " = ", ": ");
			//styleProps = Lex.Replace(styleProps, ",", ";");
			//styleProps = Lex.Replace(styleProps, "'", "");

			//if (!Lex.EndsWith(styleProps.Trim(), ";"))
			//	styleProps += ";";

			return styleProps;
		}

		/// <summary>
		/// Build statement to add control as a child to the parent
		/// e.g.: this.Controls.Add(this.Message);
		/// </summary>
		/// <param name="pc"></param>
		/// <param name="c"></param>
		void BuildAddChildControlsStmt(Control pc, Control c)
		{
			if (pc != null)
			{
				string txt = "";

				if (pc != null && !(pc is UserControl || pc is XtraUserControl || pc is Form || pc is XtraForm))
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
