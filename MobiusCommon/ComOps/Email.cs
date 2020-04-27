using System.Collections.Generic;
using System.Text;
using System.Net.Mail;
using DevExpress.XtraEditors;
using java.lang;
using Exception = System.Exception;
using Thread = System.Threading.Thread;

namespace Mobius.ComOps
{
	public class Email
	{
		/// <summary>
		/// SendNotification is a bare-bones email facility.  It is intended to be used
		/// to provide notification when unattended scripts complete successfully.
		/// </summary>
		/// <param name="args">String containing a comma-delimited list of email recipients, a space, and then the body of the mail</param>
		/// <returns></returns>

		public static string SendNotification(
						string args)
		{
			int firstSpace = args.IndexOf(" ");
			if (firstSpace < 0 || (firstSpace + 1) >= args.Length) return "";

			string emailSender = "MobiusNotifier@notification.service." + System.Environment.MachineName;
			string emailRecipients = args.Substring(0, args.IndexOf(" "));
			string msg = args.Substring(args.IndexOf(" ") + 1);
			string emailSubject = "MobiusNotification " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");

			if (Send(emailSender, emailRecipients, emailSubject, msg))
				return "Email notification routed for delivery to " + emailRecipients + ".";
			else return "";
		}

		/// <summary>
		/// Send a critical event notification to Mobius admin(s)
		/// </summary>
		/// <param name="emailSubject"></param>
		/// <param name="msg"></param>
		/// <returns></returns>

		public static bool SendCriticalEventNotificationToMobiusAdmin(
			string emailSubject,
			string msg)
		{
			string admins = "<adminEmailAddressList>"; // generalize later
			return Send(null, admins, emailSubject, msg);
		}

		/// <summary>
		/// Send a basic simple email
		/// </summary>
		/// <param name="emailSender"></param>
		/// <param name="emailRecipients"></param>
		/// <param name="msg"></param>

		public static bool Send(
			string emailSender,
			string emailRecipients,
			string emailSubject,
			string msg)
		{
			try
			{
				MailAddress from;
				if (emailSender != null) from = new MailAddress(emailSender);
				else from = new MailAddress("<adminEmailAddressList>", UmlautMobius.String);
				MailAddress to = new MailAddress(emailRecipients);
				MailMessage mailMessage = new MailMessage(from, to);

				mailMessage.Subject = emailSubject;
				mailMessage.Body = msg;

				Send(mailMessage);
			}

			catch (Exception ex)
			{
				throw new Exception("Send E-mail failed: " + ex.Message, ex);
			}

			return true;
		}

		/// <summary>
		/// Set MailTo from list
		/// </summary>
		/// <param name="mail"></param>
		/// <param name="mailTo"></param>

		public static void SetMultipleMailTo(
				MailMessage mail,
				string mailTo)
		{
			mailTo = mailTo.Replace("\r", "");
			mailTo = mailTo.Replace("\n", ",");
			mailTo = mailTo.Replace(";", ",");
			string[] sa = mailTo.Split(',');
			foreach (string s in sa)
			{
				string s2 = s.Trim();
				if (s2.Length < 3) continue; // skip if too short to be a valid e-mail
				if (s2.IndexOf("@") < 0 || s2.ToLower().EndsWith("@<webSiteAddress>"))
				{ // hack to allow Lotus Notes style addresses to be used
					int i1 = s2.IndexOf("/");
					if (i1 >= 0) s2 = s2.Substring(0, i1);
					string[] sa2 = s2.Split(' '); // separate parts of names
					if (sa2.Length == 3) s2 = sa2[2] + "_" + sa2[0] + "_" + sa2[1];
					else if (sa2.Length == 2) s2 = sa2[1] + "_" + sa2[0];
					s2 += "@<webSiteAddress>";
				}
				mail.To.Add(s2);
			}
			return;
		}

		/// <summary>
		/// Send mail object
		/// </summary>
		/// <param name="mail"></param>

		public static void Send(
				MailMessage mail)
		{
			string mailServerName = "[server]";
			//string mailServerName = "[server]";

			using (SmtpClient mailClient = new SmtpClient())
			{

				if (ServicesIniFile.IniFile != null)
					mailServerName = ServicesIniFile.Read("SmtpServer");
				mailClient.Host = mailServerName;
				mailClient.UseDefaultCredentials = true;
				// mailClient.DeliveryMethod = SmtpDeliveryMethod.PickupDirectoryFromIis;

				// It appears that the SMPTP server is rebooting and is unavailable at 1:01am every morning.
				// We will pause on a failure and try again up to three times.
				int attempt = 0;
				while (attempt < 3)
				{
					try
					{
						attempt++;
						mailClient.Send(mail);
						break;
					}
					catch (SmtpException)
					{
						if (attempt > 2)
						{
							DebugLog.Message("Handled Error: Attempt " + attempt + " of Email.Send failed.  Email not sent.");
							throw;
						}
						DebugLog.Message("Mobius Warning: Attempt " + attempt + " of SmtpClient.Send failed.  Re-attempting email send...");
						Thread.Sleep(10000); // wait 10 seconds
					}
				}
			}

		}
	}
}
