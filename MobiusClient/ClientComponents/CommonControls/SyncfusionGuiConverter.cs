using System;
using System.IO;
using System.Drawing;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
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

namespace Mobius.ComOps
{
	public class SyncfusionGuiConverter
	{
		static HashSet<string> ControlsLogged = new HashSet<string>();
		HashSet<string> RadioGroups = new HashSet<string>(); // names of each of the radio button groups seen 
		StreamWriter log;

		bool IncludeMenuStubs = false;
		public static bool Active = DebugMx.True; // set to true to generate Razor code

		/// <summary>
		/// Convert a Windows/DevExpress Form or UserControl to a Blazor .razor Component file
		/// </summary>
		/// <param name="pc"></param>

		public void ToRazor(
			Control pc,
			bool includeMenuStubs = false)
		{
			string s;

			if (!Active) return;

			IncludeMenuStubs = includeMenuStubs;

			//////////////////////////////////////////////
			// Build the HTML and plug into the template
			//////////////////////////////////////////////

			string html = "", code = "", eventStubs = ""; // strings for global building of html and code sections

			Type t = pc.GetType();
			string ctlFullName = t.FullName;
			//if (ControlsLogged.Contains(ctlFullName)) return;
			ControlsLogged.Add(ctlFullName);

			log = new StreamWriter(@"C:\MobiusWebClient\GeneratedGuiComponents\" + t.Name + ".txt");

			RadioGroups = new HashSet<string>();


			// ======================================================
			// Process a top level Form or XtraForm (i.e. DialogBox)
			// ======================================================

			if (typeof(Form).IsAssignableFrom(pc.GetType())) // Form or XtraForm?
			{
				Form f = pc as Form;

				int height = f.Height; // add size of WinForms header to get correct html height
				int width = f.Width;
				string headerText = f.Text;
				string enableResize = (f.FormBorderStyle == FormBorderStyle.Sizable) ? "true" : "false";

				// Wrap with SfDialog definition 
				// Note that the width of the header div = calc(100% - 32px) so that the close button will work if present

				html = $@"<SfDialog Target='#target' Height='{height}px' Width='{width}px' IsModal='true' ShowCloseIcon='true' AllowDragging='true' EnableResize='{enableResize}' 
					@ref ='SfDialog' @bind-Visible='DialogVisible' CssClass='dialogboxmx'>
				  <DialogTemplates>
						<Header>
							<div class='control-div-mx font-mx defaults-mx' style='display: inline-flex; position: absolute; align-items: center; width: calc(100% - 32px); height: 20px; font-size: 13px; border: 0px;'>@HeaderText</div>
						</Header>
					<Content>
				 ";
				
				// Define variables for code section

				code = $@"

		/******************************* File links *********************************/
		public static {f.Name} RazorFile => {f.Name}.CsFile; 
		/****************************************************************************/

		public static {f.Name} Instance {{ get; set; }} 
		public SfDialog SfDialog {{ get; set; }} 
		public string HeaderText = ""{headerText}"";" + @"
		public static bool IncludeInRenderTree { get; set; } = true; 
		public bool DialogVisible { get; set; } = false; 
		public DialogResult DialogResult { get; set; } = DialogResult.None;
		public bool RenderingEnabled = true;
		protected override bool ShouldRender() { return RenderingEnabled; }" + "\r\n\r\n";

				eventStubs = EventStubs;

				if (includeMenuStubs)
				{
					code += @"
		public SfContextMenu<MenuItem> ContextMenu; // the context menu to show
		public List<MenuItem> MenuItems = // MenuItems data source for ContextMenu
			new List<MenuItem> { new MenuItem { Text = ""Loading..."" } };" + "\r\n";
				}

				ConvertContainedControls(pc, 0, ref html, ref code, ref eventStubs);

				html += // Finish up the SfDialog component
					$@"</Content>
					</DialogTemplates>
					<DialogEvents Opened='@DialogOpened' Closed='@DialogClosed'></DialogEvents>
				</SfDialog>" + "\r\n";


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

			// ======================================================
			// Process a UserControl or XtraUserControl
			// ======================================================

			else if (pc is UserControl || pc is XtraUserControl)
			{
				html =
				 @"<!-- Cascade this instance of the component down to all lower components -->

					<CascadingValue Value='@this'>" + "\r\n";

				code = $@"

		/******************************* File links *********************************/
		public static {pc.Name} RazorFile => {pc.Name}.CsFile; 
		/****************************************************************************/" + "\r\n\r\n";


				ConvertContainedControls(pc, 0, ref html, ref code, ref eventStubs);

				html += " </CascadingValue>"; // finish up the HTML part of the component definition

			}

			else throw new Exception("Can't convert control of type: " + pc?.GetType()?.Name);

			string razor = Lex.Replace(RazorTemplate, "<html>", html);

			razor = Lex.Replace(razor, "<code>", code);

			razor = Lex.Replace(razor, "<eventStubs>", eventStubs);

			log.Close();

			StreamWriter sw = new StreamWriter(@"C:\MobiusWebClient\GeneratedGuiComponents\" + t.Name + ".razor");
			sw.Write(razor);
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
			ref string code,
			ref string eventStubs)
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
				DivSfMx c = new DivSfMx();
				c.Location = new Point(0, 0);
				c.Size = new Size(1, 1);
				c.Dock = DockStyle.Fill;
				c.Name = pc.GetType().Name;
				cl.Add(c);
			}

			foreach (Control c in cl)
			{
				//if (!cc.Visible) continue;

				Type t = c.GetType();

				// Log basic info

				log.WriteLine(string.Format(format, t.Name, c.Name,
					c.Left, c.Top, c.Width, c.Height,
					c.Anchor, c.Dock, c.Text, t.FullName));

				ConvertControl(pc, c, dy, ref html, ref code, ref eventStubs); // convert the control and surround with a positioning div

			} // child control list

			log.WriteLine("End of Parent Type:\t" + pcType.Name + "\t, Parent Name:\t" + pc.Name + "\t, Full Type Name:\t" + pcType.FullName);

			return;
		}

		void ConvertControl(
			Control pc,
			Control c,
			int dy,
			ref string html,
			ref string code,
			ref string eventStub)
		{
			string htmlFrag = "", codeFrag = "", eventStubFrag = "", divStyle = "", groupName = "", initArgs = "";
			int dy2 = 0;

			Type cType = c.GetType();
			string cText = c.Text;
			if (Lex.StartsWith(cText, "&")) // remove shortcut indicators within control text (e.g. menu items, buttons, ...)
				cText = cText.Substring(1);

			cText = HttpUtility.HtmlEncode(c.Text);

			DivSfMx div = new DivSfMx(); // the div to surround and position the control

			////////////////////////////////////////////////////////////////////////////////
			// For user controls write reference to this file and then the control and 
			// its contents to a new file
			////////////////////////////////////////////////////////////////////////////////

			if (c is UserControl || c is XtraUserControl)
			{
				
				UserControl uc = c as UserControl;

				string ctlClassName = cType.Name;

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle, ""); // build the div

				if (uc.BorderStyle != BorderStyle.None)
					divStyle += " border: 1px solid #acacac; background-color: #ffffff; ";

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"{ctlClassName} {c.Name} = new {ctlClassName}(){initArgs};\r\n";

				//htmlFrag += $"</ {ctlClassName}>\r\n"; // not needed yet, closed above

				div.Close(ref htmlFrag);
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

				gb.Height += 8;
				gb.Top -= 8;

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle); // build the div

				gb.Height -= 8;
				gb.Top += 8;

				htmlFrag +=
					"<GroupBox class='groupbox-mx'>\r\n";

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"GroupBoxMx {c.Name} = new GroupBoxMx({initArgs});\r\n";

				dy2 = 4; // move contained controls down a bit
				ConvertContainedControls(c, dy2, ref htmlFrag, ref codeFrag, ref eventStubFrag);

				htmlFrag += "</fieldset>\r\n";

				div.Close(ref htmlFrag);
			}

			/*************************/
			/*** Panel / XtraPanel ***/
			/*************************/

			else if (cType == typeof(Panel) || cType == typeof(PanelControl) || cType == typeof(XtraPanel))
			{
				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle);

				htmlFrag += ""; // todo...

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"PanelControlMx {c.Name} = new PanelControlMx(){initArgs};\r\n";

				ConvertContainedControls(c, dy2, ref htmlFrag, ref codeFrag, ref eventStubFrag);

				htmlFrag += ""; // todo...

				div.Close(ref htmlFrag);
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

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle); // build the div

				//tab.Height -= 8;
				//tab.Top += 8;

				htmlFrag +=
					@"<SfTab class='tab-control-mx' Height='100%' Width='100%'>
							<TabItems>" + "\r\n";

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"TabMx {c.Name} = new TabMx(){initArgs};\r\n";

				dy2 = 0; // adjust position of contained controls
				ConvertContainedControls(c, dy2, ref htmlFrag, ref codeFrag, ref eventStubFrag);

				htmlFrag +=
					@"</TabItems>
			</SfTab>\r\n";

				div.Close(ref htmlFrag);
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

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle); // build the div

				//tab.Height -= 8;
				//tab.Top += 8;

				htmlFrag +=
					$@"
					 <TabItem>
              <ChildContent>
                  <TabHeader Text='{tp.Text}'></TabHeader>
              </ChildContent>
              <ContentTemplate>
						";

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"TabPageMx {c.Name} = new TabPageMx(){initArgs};\r\n";

				dy2 = 0; // adjust position of contained controls
				ConvertContainedControls(c, dy2, ref htmlFrag, ref codeFrag, ref eventStubFrag);

				htmlFrag +=
					@"</ContentTemplate>
						</TabItem>\r\n";

				div.Close(ref htmlFrag);
			}

			////////////////////////////////////////////////////////////////////////////////
			// WinForms/DX builtin controls (non-grouping)
			////////////////////////////////////////////////////////////////////////////////

			/////////////////////////////////////////////////////////////////////////////////
			// LabelControl
			/////////////////////////////////////////////////////////////////////////////////


			else if (cType == typeof(LabelControl)) // DX LabelControl
			{
				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle);

				LabelControl lc = c as LabelControl;
				TextOptions to = lc.Appearance.TextOptions;

				if (lc.LineVisible)
				{
					if (Lex.IsUndefined(lc.Text))
						htmlFrag += "<hr class='hr-mobius'>\r\n";

					else // horizontal rule with text overlaid using utility css
						htmlFrag += $"<hr data-content='{lc.Text}' class=' class='hr-mobius hr-text'>\r\n";
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

					htmlFrag += $"<span {spanStyle}>@{c.Name}.Text</span>\r\n";
				}

				initArgs = BuildInitArgs("Text", lc.Text, "DivStyle", NewCss(divStyle));
				codeFrag += $"LabelControlMx {c.Name} = new LabelControlMx(){initArgs};\r\n";

				div.Close(ref htmlFrag);
			}

			else if (cType == typeof(System.Windows.Forms.Label)) // Simpler Windows.Forms Label
			{
				Label l = c as Label;

				if (Lex.IsUndefined(l.Text)) return; // ignore if no text

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle);

				if (l.BorderStyle != BorderStyle.None)
					divStyle += " border: 1px solid #acacac; background-color: #eeeeee; ";

				htmlFrag += $"<span>@{c.Name}.Text</span>\r\n";

				initArgs = BuildInitArgs("Text", l.Text, "DivStyle", NewCss(divStyle));
				codeFrag += $"LabelControlMx {c.Name} = new LabelControlMx(){initArgs};\r\n";

				div.Close(ref htmlFrag);
			}

			else if (cType == typeof(System.Windows.Forms.PictureBox)) // Simple image
			{
				PictureBox pb = c as PictureBox;

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle);

				if (pb.BorderStyle != BorderStyle.None)
					divStyle += " border: 1px solid #acacac; background-color: #eeeeee; ";

				htmlFrag += $"<img src='@{c.Name}.ImageName' width='{c.Width}' height='{c.Height}' />\r\n";

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"PictureBoxMx {c.Name} = new PictureBoxMx(){initArgs};\r\n";

				div.Close(ref htmlFrag);
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

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle);

				htmlFrag += $@"<SfDropDownButton CssClass='button-mx' 
				  @ref='{c.Name}.Button' Content='@{c.Name}.Text 
					@onclick = '{c.Name}_Click' >
				 </SfDropDownButton>";

				//<DropDownMenuItems>
				//  <DropDownMenuItem Text='Loading...'></DropDownMenuItem>
				//</DropDownMenuItems>

				initArgs = BuildInitArgs("Text", c.Text, "DivStyle", NewCss(divStyle));
				codeFrag += $"DropDownButtonMx {c.Name} = new DropDownButtonMx(){initArgs};\r\n";

				div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckEdit (CheckBox and RadioButton)
			/////////////////////////////////////////////////////////////////////////////////


			else if (cType == typeof(CheckEdit))
			{
				if (dy == 0)
					dy = -4; // move up a bit

				div.Build(pc, c, dy + 2, ref htmlFrag, ref codeFrag, out divStyle);

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
					htmlFrag += $@"<SfCheckBox CssClass='font-mx defaults-mx' Label='@{c.Name}.Text' Name='{c.Name}'   
						@ref ='{c.Name}.Button' @bind-Checked='{c.Name}.Checked' @onchange = '{c.Name}_CheckedChanged' />" + "\r\n";

					initArgs = BuildInitArgs("Text", cText, "DivStyle", NewCss(divStyle));
					codeFrag += $"CheckBoxMx {c.Name} = new CheckBoxMx(){initArgs};\r\n";
				}

				else // assume RadioButton
				{

					groupName = "RadioGroupValue" + ce.Properties.RadioGroupIndex;
					htmlFrag += $@"<SfRadioButton CssClass='font-mx defaults-mx' Label='@{c.Name}.Text' Name='{groupName}' Value='{ce.Name}'
						@ref ='{c.Name}.Button' @bind-Checked='{groupName}.CheckedValue' @onchange = '{c.Name}_CheckedChanged' />" + "\r\n";

					if (!RadioGroups.Contains(groupName)) // add var to identify the current radio button for the group
					{
						codeFrag += $"static CheckedValueContainer {groupName} = new CheckedValueContainer();\r\n";
						RadioGroups.Add(groupName);
					}

					initArgs = BuildInitArgs("Text", cText, "CVC", Nas(groupName), "DivStyle", NewCss(divStyle));
					codeFrag += $"RadioButtonMx {c.Name} = new RadioButtonMx(){initArgs};\r\n";
				}

				eventStubFrag += $@"

				/// <summary>
				/// {c.Name}_CheckedChanged
				/// </summary>
				 
				private void {c.Name}_CheckedChanged()
					{{
						return;
					}}" + "\r\n";

				div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckButton - Like a RadioButton but with an image et al in place of a checkmark
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(CheckButton))
			{
				// CheckButtons and their groups are handled in the same way as RadioButtons
				// with the value for checked comparison being the ID of the SfButton.

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle);

				CheckButton cb = c as CheckButton;

				groupName = "CheckButtonGroup" + cb.GroupIndex;
				string iconName = cb.Name;
				iconName = Lex.Replace(iconName, "Va", ""); // hack for TextAlignment Dialog
				iconName = Lex.Replace(iconName, "Ha", "");

				if (!Lex.StartsWith(iconName, "Align")) iconName = "Align" + iconName; // make name match SF icon names
				if (!Lex.EndsWith(iconName, "IconMx")) iconName += "IconMx";

				htmlFrag += $@"<SfButton @ref ='{cb.Name}.Button' @bind-CssClass='{cb.Name}.CssClass' IconCss = '{iconName}' 
					Content='{cText}' OnClick='{cb.Name}_Click' />" + "\r\n";

				groupName = "CheckButtonGroupValue" + (cb.GroupIndex);
				if (!RadioGroups.Contains(groupName)) // add var to identify the current radio button for the group
				{
					codeFrag += $"static CheckedValueContainer {groupName} = new CheckedValueContainer();\r\n";
					RadioGroups.Add(groupName);
				}

				initArgs = BuildInitArgs("Text", cText, "CVC", Nas(groupName), "DivStyle", NewCss(divStyle));
				codeFrag += $"CheckButtonMx {cb.Name} = new CheckButtonMx(){initArgs};\r\n";

				div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// SimpleButton
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(SimpleButton))
			{
				// Example: <SfButton CssClass="e-flat" IsToggle="true" IsPrimary="true" 
				//            Content="@Content" IconCss="@IconCss" @ref="ToggleButton" @onclick="OnToggleClick"></SfButton>

				SimpleButton b = c as SimpleButton;
				dy2 = dy;
				if ((b.Anchor & AnchorStyles.Top) != 0)
					dy2 -= 3;

				//else if ((b.Anchor & AnchorStyles.Bottom) != 0)
				//	dy2 += 3;

				div.Build(pc, c, dy2, ref htmlFrag, ref codeFrag, out divStyle);

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

				htmlFrag += $"<SfButton CssClass='{cssClass}' Content='@{c.Name}.Text' ";

				if (Lex.IsDefined(imageName))
					htmlFrag += $" IconCss = '@{c.Name}.ImageName' ";

				if (Lex.Eq(cText, "OK") || Lex.Eq(cText, "Yes"))
					htmlFrag += " IsPrimary = 'true'";

				//string handler = WindowsHelper.GetEventHandlerName(c, "Click"); // doesn't work
				//if (Lex.IsDefined(handler)) ...
				htmlFrag += $"@ref='{c.Name}.Button' @onclick = '{c.Name}_Click' />\r\n";

				initArgs = BuildInitArgs("Text", cText, "ImageName", imageName, "DivStyle", NewCss(divStyle));
				codeFrag += $"ButtonMx {c.Name} = new ButtonMx(){initArgs};\r\n";

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

				div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// CheckedListBoxControl
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(CheckedListBoxControl))
			{
				CheckedListBoxControl db = c as CheckedListBoxControl;

				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle);

				htmlFrag += $@"<SfListView CssClass='listview-mx' TValue='ListItemMx' ShowCheckBox='true' Width='100%' Height='100%'
            @ref='@{c.Name}.SfListView' DataSource='@{c.Name}.Items'>
          <ListViewFieldSettings TValue='ListItemMx' Text='Text' Id='Id' IsChecked='IsChecked'> </ListViewFieldSettings>
          <ListViewEvents TValue='ListItemMx' Clicked='{c.Name}_Clicked'>
          </ListViewEvents>
        </SfListView>" + "\r\n";

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"CheckedListMx {c.Name} = new CheckedListMx(){initArgs};\r\n";

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

				div.Close(ref htmlFrag);
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

				div.Build(pc, c, dy - 2, ref htmlFrag, ref codeFrag, out divStyle);

				htmlFrag += $@"<SfComboBox TValue='string' TItem='ListItemMx' Enabled='@{c.Name}.Enabled'
					@ref='{c.Name}.ComboBox' Placeholder='@{c.Name}.InitialText' DataSource='@{c.Name}.Items'>
						<ComboBoxFieldSettings Text='Text' IconCss='IconCss' Value='Value'></ComboBoxFieldSettings>
						<ComboBoxEvents TValue='string' TItem='ListItemMx' Focus='{c.Name}_Focus' ValueChange='{c.Name}_ValueChange'></ComboBoxEvents> 
					</SfComboBox> " + "\r\n";

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"ComboBoxMx {c.Name} = new ComboBoxMx(){initArgs};\r\n";

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

				div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// TextEdit / MemoEdit
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(TextEdit) || cType == typeof(MemoEdit))
			{
				div.Build(pc, c, dy - 2, ref htmlFrag, ref codeFrag, out divStyle); // move the textbox up a few pixels to align better with other controls

				// Example: <SfTextBox @bind-Value="@TextBoxValue" @ref="@SfTextBox" Type="InputType.Text" Placeholder="@InitialText" />

				TextEdit te = c as TextEdit; 
				bool editable = !te.Properties.ReadOnly;
				string readOnly = editable ? "false" : "true";

				MemoEdit me = c as MemoEdit; // MemoEdit is a subclass of textedit
				string multiline = me != null ? "true" : "false"; 

				string enabled = te.Enabled ? "true" : "false";

				htmlFrag += $@"<SfTextBox CssClass='e-small sftextbox-mx defaults-mx'  Type='InputType.Text' 
					@ref='{c.Name}.SfTextBox' @bind-Value='{c.Name}.Text' Readonly='{readOnly}' Multiline='{multiline}' Enabled ='{enabled}'";

				if (editable)
					htmlFrag += $" Input='{c.Name}_Input' Focus='{c.Name}_Focus' ";
					
				htmlFrag += "/>\r\n";

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"TextBoxMx {c.Name} = new TextBoxMx(){initArgs};\r\n";


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

				div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// Artificial DivSfMx control so the div for an empty container control gets created
			/////////////////////////////////////////////////////////////////////////////////

			else if (cType == typeof(DivSfMx)) // "null" control
			{
				div.Build(pc, c, dy, ref htmlFrag, ref codeFrag, out divStyle);

				initArgs = BuildInitArgs("DivStyle", NewCss(divStyle));
				codeFrag += $"DivSfMx {c.Name} = new DivSfMx(){initArgs};\r\n";

				div.Close(ref htmlFrag);
			}

			/////////////////////////////////////////////////////////////////////////////////
			// Unrecognized
			/////////////////////////////////////////////////////////////////////////////////

			else // unrecognized
			{
				htmlFrag += "<div> Unrecognized Control: " + c.GetType().Name + " " + c.Name;

				if (Lex.IsDefined(cText)) // insert the element content
					htmlFrag += " Text=\"" + cText + '"';
				htmlFrag += " </div>\r\n";
			}

			html += htmlFrag;
			code += codeFrag;
			eventStub += eventStubFrag;

			return;
		}

		string BuildInitArgs(params object[] pa)
		{
			string args = "", propName = "", propVal = "";
			for (int i1 = 0; i1 < pa.Length; i1 += 2)
			{
				propName = pa[i1] as string;
				if (Lex.IsUndefined(propName)) continue;

				if (Lex.IsDefined(args)) args += ", ";

				object valObj = pa[i1 + 1];
				if (valObj == null)
					propVal = "null";

				else if (valObj is string) // quote string constants
					propVal = Lex.AddDoubleQuotes(valObj as string);

				else propVal = valObj.ToString(); // variable or expression

				args += propName + " = " + propVal;
			}

			args = "{" + args + "}";
			return args;
		}



		/****************************************/
					/*** Template for building Razor file ***/
					/****************************************/

					static string RazorTemplate = @"
@using Mobius.ComOps
@using WebShell

@using Syncfusion.Blazor.Popups
@using Syncfusion.Blazor.Layouts
@using Syncfusion.Blazor.Navigations
@using Syncfusion.Blazor.Inputs
@using Syncfusion.Blazor.Buttons
@using Syncfusion.Blazor.SplitButtons
@using Syncfusion.Blazor.DropDowns
@using Syncfusion.Blazor.Lists
@using Syncfusion.Blazor.Grids;

@using Newtonsoft.Json

@using System.Reflection
@using Microsoft.AspNetCore.Components

@inject IJSRuntime JsRuntime;

@namespace Mobius.ClientComponents
@********************************@


@********@
@* HTML *@
@********@

<html>

@********@
@*Code *@
@********@

@code{
	<code>
}

@*******@
@*CSS *@
@*******@


@***************@
@* Event stubs *@
@***************@

@*
<eventStubs>
*@
";


		static string EventStubs = @"

		/*************************************************/
		/*** Basic SfDialog overrides and Click events ***/
		/*************************************************/

		protected override void OnInitialized()
    {
			Instance = this;
			base.OnInitialized();
			return;
		}

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			return;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			return;
		}

		protected override void OnAfterRender(bool firstRender)
		{
			base.OnAfterRender(firstRender);
		}

		/// <summary>
    /// Dialog Opened
    /// </summary>
    /// <returns></returns>

		private async Task DialogOpened(Syncfusion.Blazor.Popups.OpenEventArgs args)
    {
			args.PreventFocus = true;
			// await initialComponentToFocusOn.FocusIn();
      return;
    }

    /// <summary>
    /// DialogCLosed
    /// </summary>
 
		private void DialogClosed(Syncfusion.Blazor.Popups.CloseEventArgs args)
    {
      if (Lex.Ne(args?.ClosedBy, ""User Action""))
        DialogResult = DialogResult.Cancel;

      return;
    }

/****************************/
/*** SfContextMenu events ***/
/****************************/

		public void BeforeMenuOpen(BeforeOpenCloseMenuEventArgs<MenuItem> args) // called once for menu with children
		{
			//DebugLog.Message(args.ParentItem != null ? args.ParentItem?.Text : ""null"");
			return;
		}

		public void MenuOpened(OpenCloseMenuEventArgs<MenuItem> args) // called once for menu with children
		{
			//DebugLog.Message(args.ParentItem != null ? args.ParentItem?.Text : ""null"");
			return;
		}

		public void BeforeMenuClosed(BeforeOpenCloseMenuEventArgs<MenuItem> args) // called once for menu with children
		{
			//DebugLog.Message(args.ParentItem?.Text);
			return;
		}

		public void MenuClosed(OpenCloseMenuEventArgs<MenuItem> args) // called once for menu with children
		{
			//DebugLog.Message(args.ParentItem?.Text);
			return;
		}

		public void MenuItemRender(MenuEventArgs<MenuItem> args) // called once per item
		{
			return;
		}

		public void MenuItemSelected(MenuEventArgs<MenuItem> args)
		{
			//DebugLog.Message(args?.Item?.Text != null ? args.Item.Text : ""null"");
			return;
		}

/*********************************/
/*** Component-specific events ***/
/*********************************/" + 
			
			"\r\n\r\n";


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
	}

	/// <summary>
	/// "Not a string" object
	/// </summary>

	internal class Nas
{
	object Value;

	public Nas (object value)
		{
		Value = value;
		}
}


	/// <summary>
	/// DivSfMx Class
	/// </summary>

	internal class DivSfMx : Control
	{
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
		/// Build a Div to wrap the specified control
		/// </summary>
		/// <param name="pc">Parent control</param>
		/// <param name="c">Control being wrapped in the Div</param>
		/// <returns></returns>

		public void Build(
			Control pc,
			Control c,
			int dy,
			ref string htmlFrag,
			ref string codeFrag,
			out string divStyle,
			string styleAttributesToRemove = null,
			string styleAttributesToAdd = null)
		{
			Type pcType = pc.GetType();
			if (pc is Form || pc is XtraForm) // Form or XtraForm?
				dy += 32; // add pixel height of WinForms window header to div top/bottom to adjust for move to HTML

			int cTop = c.Top + dy; // move top and bottom down to correct from WinForms to HTML
			int cBottom = c.Bottom + dy;

			string div = $"<div @ref='{c.Name}.DivRef' class='control-div-mx font-mx defaults-mx' style='@{c.Name}?.DivStyle?.StyleString'";
			string code = ""; // nothing yet
			string style = "position: absolute; display: flex; align-items: center; ";

			if (c.Dock == DockStyle.Fill) // if Dock defined the set width/height to 100% (i.e. Fill only for now)
				style += "width: 100%; height: 100%; ";

			else // use Anchor settings
			{
				string widthHeight = "";
				bool left = ((c.Anchor & AnchorStyles.Left) != 0);
				bool right = ((c.Anchor & AnchorStyles.Right) != 0);

				if (left)
				{
					style += "left: " + c.Left + "px; ";
					if (!right)
						widthHeight += "width: " + c.Width + "px; ";

					else // if left & right defined then set width as a calc()
						widthHeight += "width: calc(100% - " + (c.Left + (pc.Width - c.Right)) + "px); ";
				}

				else if (right) // set right and width
				{
					style += "right: " + (pc.Width - c.Right) + "px; ";
					widthHeight += "width: " + c.Width + "px; ";
				}

				bool anchorTop = ((c.Anchor & AnchorStyles.Top) != 0);
				bool anchorBottom = ((c.Anchor & AnchorStyles.Bottom) != 0);

				// Note: for buttons it looks like they could move up one px & be 2px taller

				int cHeight = c.Height + 2; // make a bit taller

				if (anchorTop)
				{
					style += "top: " + cTop + "px; ";
					if (!anchorBottom)
						widthHeight += "height: " + cHeight + "px; ";

					else // if top & bottom defined then set height as a calc()
						widthHeight += "height: calc(100% - " + (cTop + (pc.Height - cBottom)) + "px); ";
				}

				else if (anchorBottom) // set bottom and height
				{
					style += "bottom: " + (pc.Height - cBottom) + "px; ";
					widthHeight += "height: " + cHeight + "px; ";
				}

				style += widthHeight; // put width height after location
			}


			div += " >\r\n"; // finish up div tag

			if (Lex.IsDefined(styleAttributesToRemove))
			{
				style = Lex.Replace(style, styleAttributesToRemove, "");
			}

			if (Lex.IsDefined(styleAttributesToAdd))
			{
				style = Lex.Replace(style, "style='", "style='" + styleAttributesToAdd + " ");
			}

			htmlFrag += div;

			divStyle = style;
			return;
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
