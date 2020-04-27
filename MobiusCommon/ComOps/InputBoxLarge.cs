using DevExpress.XtraEditors;

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mobius.ComOps
{
	/// <summary>
	/// Summary description for PromptBox.
	/// </summary>
	public partial class InputBoxLarge : XtraForm
	{
		static InputBoxLarge Instance;

		/// <summary>
		/// Required designer variable.
		/// </summary>
		//private System.ComponentModel.Container components = null;

		public InputBoxLarge()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			Instance = this;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>

		//protected override void Dispose(bool disposing)
		//{
		//	if (disposing)
		//	{
		//		if (components != null)
		//		{
		//			components.Dispose();
		//		}
		//	}
		//	base.Dispose(disposing);
		//}


		/// <summary>
		/// Get a line of input from the user
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="title"></param>
		/// <returns></returns>

		public static string Show(
			string prompt,
			string title)
		{
			return Show(prompt, title, "", -1, -1);
		}

		public static string Show(
			string prompt,
			string title,
			int width,
			int height)
		{
			return Show(prompt, title, "", width, height);
		}

		public static string Show(
			string prompt,
			string title,
			string defaultText)
		{
			return Show(prompt, title, defaultText, -1, -1);
		}

		/// <summary>
		/// Get a line of input from the user
		/// </summary>
		/// <param name="prompt"></param>
		/// <param name="title"></param>
		/// <param name="defaultText"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns>New input or null if cancelled</returns>

		public static string Show(
			string prompt,
			string title,
			string defaultText,
			int width,
			int height)
		{
			string txt;
			if (Instance == null)		Instance = new InputBoxLarge();

			if (width > 0) Instance.Width = width;
			if (height > 0) Instance.Height = height;

			Instance.Prompt.Text = prompt;

			Instance.Text = title;
			Instance.Input.Text = defaultText;

			DialogResult dr = Instance.ShowDialog(Form.ActiveForm); // (SessionManager.ActiveForm);
			if (dr == DialogResult.Cancel) return null;
			else return Instance.Input.Text;
		}

		private void InputBox_Activated(object sender, System.EventArgs e)
		{
			Input.SelectAll();
			Input.Focus();
		}

		private void OK_Click(object sender, EventArgs e)
		{

		}

		private void Cancel_Click(object sender, EventArgs e)
		{

		}
	}
}
