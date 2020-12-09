using Mobius.ComOps;

using DevExpress.XtraEditors;
using DevExpress.XtraTab;
using DevExpress.XtraEditors.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Data;
using System.IO;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
  public partial class MessageBoxMx : XtraForm
  {
    public delegate DialogResult ShowDelegate(string message);

    public static bool UseDevExpressMessageBox = true;

    public MessageBoxMx()
    {
      try
      {
        InitializeComponent();
      }
      catch (Exception ex) // seems to sometimes occur when exiting WIN 10
      {
        string msg = ex.Message;
      }
    }

    public static void InvokeShow(string message)
    {
      Form activeForm = SessionManager.ActiveForm;
      if (activeForm != null)
      {
        activeForm.Invoke(new ShowDelegate(Show), message);
      }
      else
      {
        //try to invoke show under the ShellForm
        SessionManager.Instance.ShellForm.Invoke(new ShowDelegate(Show), message);
      }
      return;
    }

    public static DialogResult Show(
      string messageText)
    {
      return Show(messageText, UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Information); // MessageBoxIcon.None);
    }

    public static DialogResult ShowInformation(
      string messageText)
    {
      return Show(messageText, UmlautMobius.String, MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    public static DialogResult ShowError(
      string messageText)
    {
      return Show(messageText, UmlautMobius.String + " Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
    }

    public static DialogResult ShowUnexpectedException(
      Exception ex)
    {
      return ShowError("Unexpected error: " + DebugLog.FormatExceptionMessage(ex));
    }

    public static DialogResult Show(
      string messageText,
      string titleCaption)
    {
      return Show(messageText, titleCaption, MessageBoxButtons.OK, MessageBoxIcon.None);
    }

    /// <summary>
    /// Show - Simple
    /// </summary>
    /// <param name="messageText"></param>
    /// <param name="titleCaption"></param>
    /// <param name="buttons"></param>
    /// <returns></returns>

    public static DialogResult Show(
      string messageText,
      string titleCaption,
      MessageBoxButtons buttons)
    {
      return Show(messageText, titleCaption, buttons, MessageBoxIcon.None);
    }

    /// <summary>
    /// Show with custom size
    /// </summary>
    /// <param name="messageText"></param>
    /// <param name="titleCaption"></param>
    /// <param name="buttons"></param>
    /// <param name="icon"></param>
    /// <returns></returns>

    public static DialogResult ShowWithCustomSize(
      string messageText,
      string titleCaption,
      MessageBoxButtons buttons,
      MessageBoxIcon icon,
      int width,
      int height)
    {
      int dri;
      DialogResult dr;

      if (buttons == MessageBoxButtons.OK)
      {
        dri = MessageBoxMx.ShowWithCustomButtons(messageText, titleCaption, "OK", null, null, null, icon, width, height);
        return DialogResult.OK;
      }

      else if (buttons == MessageBoxButtons.OKCancel)
      {
        dri = MessageBoxMx.ShowWithCustomButtons(messageText, titleCaption, "OK", "Cancel", null, null, icon, width, height);
        if (dri == 1) return DialogResult.OK;
        else return DialogResult.Cancel;
      }


      else if (buttons == MessageBoxButtons.AbortRetryIgnore)
      {
        dri = MessageBoxMx.ShowWithCustomButtons(messageText, titleCaption, "Abort", "Retry", "Ignore", null, icon, width, height);
        if (dri == 1) return DialogResult.Abort;
        else if (dri == 2) return DialogResult.Retry;
        else return DialogResult.Ignore;
      }

      else if (buttons == MessageBoxButtons.YesNoCancel)
      {
        dri = MessageBoxMx.ShowWithCustomButtons(messageText, titleCaption, "Yes", "No", "Cancel", null, icon, width, height);
        if (dri == 1) return DialogResult.Yes;
        else if (dri == 2) return DialogResult.No;
        else return DialogResult.Cancel;
      }

      else if (buttons == MessageBoxButtons.YesNo)
      {
        dri = MessageBoxMx.ShowWithCustomButtons(messageText, titleCaption, "Yes", "No", null, null, icon, width, height);
        if (dri == 1) return DialogResult.Yes;
        else return DialogResult.No;
      }

      else if (buttons == MessageBoxButtons.RetryCancel)
      {
        dri = MessageBoxMx.ShowWithCustomButtons(messageText, titleCaption, "Retry", "Cancel", null, null, icon, width, height);
        if (dri == 1) return DialogResult.Retry;
        else return DialogResult.Cancel;
      }

      else
      {
        dr = XtraMessageBox.Show(SessionManager.ActiveForm, messageText, titleCaption, buttons, icon);
        return dr;
      }
    }

    /// <summary>
    /// Show with up to 4 custom buttons
    /// </summary>
    /// <param name="message"></param>
    /// <param name="caption"></param>
    /// <param name="button1"></param>
    /// <param name="button2"></param>
    /// <param name="button3"></param>
    /// <param name="button4"></param>
    /// <param name="icon"></param>
    /// <returns>Value of 1 - 4 corresponding to button pressed </returns>

    public static int ShowWithCustomButtons(
      string message,
      string caption,
      string button1,
      string button2,
      string button3,
      string button4,
      MessageBoxIcon icon,
      int width = -1,
      int height = -1)
    {
      if (!SS.I.Attended)
      {
        ClientLog.Message("Show: " + message);
        return (int)DialogResult.OK;
      }

      Progress.Hide(); // hide any progress message

      MessageBoxMx mb = new MessageBoxMx();

      if (SyncfusionConverter.Active)
      {
        MessageBoxMx2 mb2 = new MessageBoxMx2();
        new SyncfusionConverter().ToRazor(mb2);
      }

      int rc = mb.ShowInstance(message, caption, button1, button2, button3, button4, icon, width, height);
      return rc;
    }

    int ShowInstance(
      string message,
      string caption,
      string button1,
      string button2,
      string button3,
      string button4,
      MessageBoxIcon icon,
      int width = -1,
      int height = -1)
    {
      IWin32Window owner = SessionManager.ActiveForm;
      DialogResult dr;

      try
      {

        // Use DevExpress message box

        if (Message == null)
        {
          XtraMessageBox.SmartTextWrap = true;

          XtraMessageBoxArgs mba = new XtraMessageBoxArgs();
          mba.Owner = Owner;
          mba.Text = message;
          mba.Caption = caption;
          mba.Buttons = new DialogResult[] { DialogResult.OK };
          mba.AllowHtmlText = DevExpress.Utils.DefaultBoolean.False;
          dr = XtraMessageBox.Show(mba);
          return (int)dr;
        }

        if (width > 0) // size specified
        {
          Width = width;
          Height = height;
        }

        Text = caption;
        Message.Text = message;
        int definedCount = 0;
        definedCount += SetupButton(Button1, button1);
        definedCount += SetupButton(Button2, button2);
        definedCount += SetupButton(Button3, button3);
        definedCount += SetupButton(Button4, button4);

        int shift = (Button2.Left - Button1.Left) * (4 - definedCount);
        Button1.Left += shift;
        Button2.Left += shift;
        Button3.Left += shift;
        Button4.Left += shift;

        if (message.Contains("</") || message.Contains("/>") || Lex.Contains(message, "<br>"))
        { // display HTML prompt
          Message.Visible = false;
          HtmlMessage.Visible = true;
          string htmlFile = TempFile.GetTempFileName("html");
          StreamWriter sw = new StreamWriter(htmlFile);
          int backColor = ((BackColor.R * 256 + BackColor.G) * 256) + BackColor.B;
          string hexColor = String.Format("#{0:X}", backColor);
          if (!Lex.Contains(message, "<body")) // set basic style for page
            sw.Write("<body " +
              " topmargin='0' leftmargin='0' marginwidth='0' marginheight='0' hspace='0' vspace='0' " +
              " style=\"font-size:8.5pt;font-family:'Tahoma';background-color:" + hexColor + "\">");

          else // just try to set matching background color
            message = Lex.Replace(message, "background-color: #FFFFFF", "background-color: " + hexColor);

          message = message.Replace("\n", "<br>");
          message = message.Replace("\r", "");

          sw.Write(message);
          if (!Lex.Contains(message, "<body"))
            sw.Write("</body>");
          sw.Close();
          HtmlMessage.Navigate(htmlFile);
        }

        else // display simple label prompt
        {
          HtmlMessage.Visible = false;
          Message.Visible = true;
          Message.BackColor = Message.Parent.BackColor;
          if (Lex.Contains(message, "<br>")) // replace "<br>" with newlines
            message = message.Replace("<br>", "\n");

          message = Lex.AdjustEndOfLineCharacters(message, "\n"); // convert any new line chars to standard char
          Message.Lines = message.Split('\n');

          if (Message.Lines.Length <= 4)
          {
            Message.BorderStyle = BorderStyles.NoBorder;
            Message.Properties.ScrollBars = ScrollBars.None;
          }
          else
          {
            Message.BorderStyle = BorderStyles.Default;
            Message.Properties.ScrollBars = ScrollBars.Vertical;
          }

          //Message.Text = prompt;
        }

        SetIconImageIndex(IconImage, icon); // set the proper icon

        owner = SessionManager.ActiveForm;
        //owner = null; // debug (switching to another app and then back to the main form does not bring up this modal dialog)
        dr = ShowDialog(owner);
        return (int)dr;
      }

      catch (Exception ex)
      {
        string errorMsg = "Show: " + message + "\r\n" + new StackTrace(true);
        ClientLog.Message(errorMsg);
        DebugLog.Message(errorMsg);
        return (int)DialogResult.OK;
      }

    }

    /// <summary>
    /// SetIconImageIndex
    /// </summary>
    /// <param name="iconImage"></param>
    /// <param name="icon"></param>
    public static void SetIconImageIndex(
      Label iconImage,
      MessageBoxIcon icon)
    {
      switch (icon) // set the proper icon
      {
        case MessageBoxIcon.None:
          iconImage.Visible = false;
          break;

        case MessageBoxIcon.Error: // hand & stop 
          iconImage.ImageIndex = 0;
          break;

        case MessageBoxIcon.Question:
          iconImage.ImageIndex = 1;
          break;

        case MessageBoxIcon.Exclamation: // warning
          iconImage.ImageIndex = 2;
          break;

        case MessageBoxIcon.Information: // asterisk
          iconImage.ImageIndex = 3;
          break;

        default:
          iconImage.Visible = false;
          break;

      }
    }

    int SetupButton(
        SimpleButton button,
        string text)
    {
      button.Text = text;
      if (!Lex.IsNullOrEmpty(text))
      {
        if (Lex.Eq(text, "OK") || Lex.Eq(text, "Yes")) AcceptButton = button;
        else if (Lex.Eq(text, "Cancel")) CancelButton = button;
        return 1;
      }
      else
      {
        button.Visible = false;
        return 0;
      }
    }

    /// <summary>
    /// Show a message box and return result
    /// </summary>
    /// <param name="messageText"></param>
    /// <param name="titleCaption"></param>
    /// <param name="buttons"></param>
    /// <param name="icon"></param>
    /// <returns></returns>
    /// 
    public static DialogResult Show(
        string messageText,
        string titleCaption,
        MessageBoxButtons buttons,
        MessageBoxIcon icon,
        int width = -1,
        int height = -1)
    {
      DialogResult dr = DialogResult.OK;

      if (!Lex.IsNullOrEmpty(ScriptLog.FileName))
        ScriptLog.Message("> " + messageText);

      if (!SS.I.Attended)
      {
        ClientLog.Message("Show: " + messageText);
        return DialogResult.OK;
      }

      else if (SS.I.QueryTestMode)
      {
        QueryTest.LogMessage("Show: " + messageText);
        return DialogResult.OK;
      }

      //DebugLog.Message(messageText += "\r\n" + new StackTrace(true)); // debug where message is called from"

      Progress.Hide(); // hide any progress message

      if (Lex.Contains(messageText, "<br>")) // replace "<br>" with newlines
        messageText = messageText.Replace("<br>", "\r\n");

      if (Lex.CountLines(messageText) > 5)
      {
        if (width <= 0) width = 650;
        if (height <= 0) height = 400;
        dr = ShowWithCustomSize(messageText, titleCaption, buttons, icon, width, height);
        return dr;
      }

      if (messageText.Length > 72 && !messageText.Contains("\r") && !messageText.Contains("\n"))
        messageText = WrapText(messageText, 6000);
      //icon = MessageBoxIcon.Information;
      dr = XtraMessageBox.Show(messageText, titleCaption, buttons, icon);
      return dr;
    }

    private void MessageBoxEx_Activated(object sender, EventArgs e)
    {
      Button1.Focus(); // put initial focus on Yes button
    }

    private void Button1_Click(object sender, EventArgs e)
    {
      DialogResult = (DialogResult)1;
    }

    private void Button2_Click(object sender, EventArgs e)
    {
      DialogResult = (DialogResult)2;
    }

    private void Button3_Click(object sender, EventArgs e)
    {
      DialogResult = (DialogResult)3;
    }

    private void Button4_Click(object sender, EventArgs e)
    {
      DialogResult = (DialogResult)4;
    }

    /// <summary>
    /// Wrap text into specified width in pixels
    /// </summary>
    /// <param name="text">Text to wrap</param>
    /// <param name="wrapWidth">In pixels</param>
    /// <returns></returns>

    public static string WrapText(
      string text,
      int wrapWidth)
    {
      string s, txt;
      StringBuilder sb = new StringBuilder();
      char c;
      int p, l, width, lineCount, i1;
      GraphicsMx graphics = new GraphicsMx();
      graphics.SetFont("Arial", 9);

      s = text.Replace('\t', ' '); // convert tabs to spaces

      p = 0;
      lineCount = 0;
      while (true)
      {
        width = 0;
        for (l = 0; p + l < s.Length; l++)
        {
          c = s[p + l];
          if (c == '\n' || c == '\r') break;
          width += graphics.CharWidth(c);
        }

        if (width <= wrapWidth) goto LocatedBreak;

        // Backscan looking for a space, dash or comma break character

        while (true)
        {
          for (l = l - 1; l > 0; l--)
          {
            c = s[p + l - 1];
            if (c == ' ' || c == '-' || c == ',') break;
          }
          if (l == 0) break; // break out if no luck

          width = 0;
          int p2 = p + l - 1;
          if (p2 >= s.Length)
          {
            ClientLog.Message("p2 too big: " + p + " " + p2 + " " + s); // debug info
            p2 = s.Length - 1;
          }
          for (i1 = p; i1 <= p2; i1++)
            width += graphics.CharWidth(s[i1]);
          if (width <= wrapWidth) goto LocatedBreak;
        }

        // break within a word

        l = 0;
        width = 0;
        while (true)
        {
          i1 = graphics.CharWidth(s[p + l]);
          if (l > 0 && width + i1 > wrapWidth) break;
          width += i1;
          l++;
        }

      LocatedBreak:

        i1 = 0; // left justified
        txt = s.Substring(p, l);
        p += l;
        while (p < s.Length - 1 && (s[p] == '\n' || s[p] == '\r')) p++;

        if (lineCount > 0) sb.Append("\r\n");
        sb.Append(txt);
        lineCount++;

        if (p >= s.Length)
          break;

      } // end of text loop

      return sb.ToString();
    }

    /// <summary>
    /// Resized
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>

    private void MessageBoxMx_SizeChanged(object sender, EventArgs e)
    {
      return;
    }

  }

  public class ErrorMessageBox
  {
    new public static DialogResult Show(string text)
    {
      return XtraMessageBox.Show(text, UmlautMobius.String + " Error");
    }

    new public static DialogResult Show(IWin32Window owner, string text)
    {
      return XtraMessageBox.Show(owner, text, UmlautMobius.String + " Error");
    }
  }


}