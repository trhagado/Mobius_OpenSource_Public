using Mobius.Data;
using Mobius.ComOps;
using Mobius.ClientComponents;
using Mobius.ServiceFacade;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Mobius.ClientComponents
{
	/// <summary>
	/// Security commands
	/// </summary>
	
	public class SecurityUtil
	{

		static DictionaryMx UserNames; // dictionary of Mobius user names

		public SecurityUtil()
		{
		}

		/// <summary>
		/// Authenticate a user
		/// </summary>
		/// <returns></returns>

		public static UserInfo AuthenticateUser ()
		{
			UserInfo uInf;
			if (SS.I.Attended)
			{
				string val = SS.I.IniFile.Read("TrustedAuthentication");
				if (Lex.Eq(val,"True"))
				{
					uInf = AuthenticateUserTrusted();
					if (uInf != null) return uInf;
				}

				// Prompt for username/password

				string initialUserName = System.Environment.UserName;
				return AuthenticateUserNontrusted(initialUserName);
			}

			else // console mode, use current account
			{
				uInf = new UserInfo();
				if (!String.IsNullOrEmpty(SS.I.UserName))
				{ // if userName supplied on command line use it
					uInf.UserName = SS.I.UserName;
				}
				else // just get from operating system
				{
					uInf.UserDomainName = System.Environment.UserDomainName;
					uInf.UserName = System.Environment.UserName;
				}
				Security.SetUser(uInf.UserName);
				return uInf;
			}
		}

		/// <summary>
		/// Do trusted user authentication
		/// </summary>
		/// <returns></returns>
		
		public static UserInfo AuthenticateUserTrusted()
		{
			throw new NotImplementedException();
#if false
			UserInfo uInf = new UserInfo();

			string publicKey, privateKey;
			XlEncrypt.GetKeys(out publicKey, out privateKey);
			string encryptedName = Client.Get("Mobius.Client.UIMisc.GetEncryptedDomainUsername(" + 
				Lex.Dq(publicKey) + ")"); 
				
			string decrypted = XlEncrypt.Decrypt(encryptedName,privateKey);
			string [] sa = decrypted.Split('\\');

			uInf.UserDomainName = sa[0].ToUpper();
			uInf.UserName = sa[1].ToUpper();

			Security.SetUser(uInf.UserName);
			return uInf;
#endif
		}

		/// <summary>
		/// Prompt user for account name & password
		/// </summary>
		/// <param name="initialUserName"></param>
		/// <returns></returns>

		public static UserInfo AuthenticateUserNontrusted(
			string initialUserName)
		{
			UserInfo uInf = Logon.Show(initialUserName);
			return uInf;
		}

/// <summary>
/// Get a sorted list of all user names
/// </summary>
/// <returns></returns>

		public static List<string> GetAllUsers()
		{
			DictionaryMx users = DictionaryMx.Get("UserName");
			if (users == null) users = new DictionaryMx();

			List<string> names = new List<string>();
			for (int i1 = 0; i1 < users.Words.Count; i1++)
			{
				string name = SecurityUtil.GetPersonNameReversed(users.Words[i1]);
				if (!Lex.IsNullOrEmpty(name)) names.Add(name);
			}

			names.Sort();
			return names;
		}

/// <summary>
/// Get an internal user name from an external name
/// </summary>
/// <param name="externalUserName"></param>
/// <returns></returns>

		public static string GetInternalUserName(
			string externalUserName)
		{
			DictionaryMx users = DictionaryMx.Get("UserName");
			if (users == null) users = new DictionaryMx();

			for (int i1 = 0; i1 < users.Words.Count; i1++)
			{
				string userId = users.Words[i1];
				string name = SecurityUtil.GetPersonNameReversed(userId);
				if (Lex.Eq(userId, externalUserName) || Lex.Eq(name, externalUserName)) return userId;
			}

			return null;
		}

		/// <summary>
		/// Get the full person name, "John Q Public"
		/// </summary>
		/// <param name="userName"></param>
		/// <returns>Full person name</returns>

		public static string GetPersonName(
			string userName)
		{
			string userInfo = GetUserInfo(userName);
			if (String.IsNullOrEmpty(userInfo)) return userName;

			UserInfo ui = UserInfo.Deserialize(userInfo);
			return ui.FullName;
		}

/// <summary>
/// Get the full person name with the last name first, "Public, John Q"
/// </summary>
/// <param name="userName"></param>
/// <returns>Full person name</returns>
 
		public static string GetPersonNameReversed(
			string userName)
		{
			string userInfo = GetUserInfo(userName);
			if (String.IsNullOrEmpty(userInfo)) return userName;

			UserInfo ui = UserInfo.Deserialize(userInfo);
			return ui.FullNameReversed;
		}

		/// <summary>
		/// Get short person name, e.g. "J. Smith"
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static string GetShortPersonName(string userName)
		{
			string userInfo = GetUserInfo(userName);
			if (String.IsNullOrEmpty(userInfo)) return userName;

			UserInfo ui = UserInfo.Deserialize(userInfo);
			return ui.ShortName;
		}
	
		/// <summary>
		/// Get short person name, e.g. "Smith, J."
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static string GetShortPersonNameReversed(string userName)
		{
			if (Lex.IsNullOrEmpty(userName)) return "";
			if (!SS.I.UsePersonNameInPlaceOfUserName)
				return userName;

			string userInfo = GetUserInfo(userName);
			if (String.IsNullOrEmpty(userInfo)) return userName;
			UserInfo ui = UserInfo.Deserialize(userInfo);
			return ui.ShortNameReversed;
		}

/// <summary>
/// Get user information string
/// </summary>
/// <param name="userName"></param>
/// <returns></returns>

		static string GetUserInfo(string userName)
		{
			if (UserNames == null)
			{
				DictionaryFactory df = new DictionaryFactory();

				UserNames = DictionaryMx.Get("UserName");
				if (UserNames == null) UserNames = new DictionaryMx();
			}

			userName = UserInfo.NormalizeUserName(userName);
			string userInfo = UserNames.LookupDefinition(userName);
			return userInfo;
		}

		/// <summary>
		/// Process a command line to grant a user a privilege
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>
		public static string GrantPrivilege(
			string commandLine)
		{
			if (!Security.IsAdministrator(SS.I.UserName))
				return "You must be an administrator to grant privileges";

			Lex lex = new Lex();
			lex.OpenString(commandLine);
			string priv = lex.Get();
			string userName = lex.Get();
			if (Lex.Eq(userName, "to"))
				userName = lex.Get();
			if (priv == "" || userName == "") return "Syntax: GRANT privilege TO userid";
			if (!PrivilegesMx.IsValidPrivilegeName(priv)) return priv + " is not a valid privilege";
			if (!UserExists(userName)) return "User " + userName + " doesn't exist";
			Security.GrantPrivilege(userName, priv);
			return "Privilege granted";
		}

		/// <summary>
		/// Process command line to revoke a user privilege
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>
		public static string RevokePrivilege(
			string commandLine)
		{
			if (!Security.IsAdministrator(SS.I.UserName))
				return "You must be an administrator to revoke privileges";

			Lex lex = new Lex();
			lex.OpenString(commandLine);

			string priv = lex.Get();
			string userName = lex.Get();
			if (Lex.Eq(userName, "from"))
				userName = lex.Get();
			if (priv == "" || userName == "") return "Syntax: REVOKE privilege FROM userid";
			if (!PrivilegesMx.IsValidPrivilegeName(priv)) return priv + " is not a valid privilege";
			if (!UserExists(userName)) return "User " + userName + " doesn't exist";
			if (!Security.HasPrivilege(userName, priv)) return "User " + userName + " doesn't have this privilege";
			Security.RevokePrivilege(userName, priv);
			return "Privilege revoked";
		}

		/// <summary>
		/// See if a user exists
		/// </summary>
		/// <param name="userName"></param>
		/// <returns></returns>

		public static bool UserExists(
			string userName)
		{
			if (Security.GetUserInfo(userName) != null) return true;
			else return false;
		}

		/// <summary>
		/// Process command line to create a user
		/// </summary>
		/// <param name="commandLine"></param>
		/// <returns></returns>

		public static string CreateUser(
			string commandLine)
		{
			string tok, msg;

			Lex lex = new Lex();
			lex.OpenString(commandLine);

			if (!Security.IsAdministrator(SS.I.UserName))
				return "You must be a Mobius administrator to create users";

			string userName = lex.GetUpper(); // get user name
			bool interactive = Lex.IsNullOrEmpty(userName);

			while (true)
			{
				UserInfo ui = new UserInfo();

				int i1 = userName.IndexOf(@"\");
				if (i1 < 0) i1 = userName.IndexOf(@"/"); // try forward slash
				if (i1 < 0) // domain not specified, default is AM
				{
					ui.UserName = userName;
				}
				else if (i1 > 0)
				{
					ui.UserDomainName = userName.Substring(0, i1);
					ui.UserName = userName.Substring(i1 + 1);
				}

				if (interactive) // prompt
				{
					DialogResult dr = CreateUserDialog.ShowDialog(ui, "Create User");
					if (dr == DialogResult.Cancel) return "";
				}

				else
				{ // Syntax: CREATE USER [domain\]userid firstname [mi] lastname [emailAddress] [company] [site] [department]
					ui.FirstName = Lex.CapitalizeFirstLetters(Lex.RemoveAllQuotes(lex.Get()));
					tok = Lex.CapitalizeFirstLetters(Lex.RemoveAllQuotes(lex.Get()));
					if (tok.Length == 1) ui.MiddleInitial = tok;
					else lex.Backup();
					ui.LastName = Lex.CapitalizeFirstLetters(Lex.RemoveAllQuotes(lex.Get()));
					ui.EmailAddress = Lex.CapitalizeFirstLetters(Lex.RemoveAllQuotes(lex.Get()));
					ui.Company = Lex.CapitalizeFirstLetters(Lex.RemoveAllQuotes(lex.Get()));
					ui.Site = Lex.CapitalizeFirstLetters(Lex.RemoveAllQuotes(lex.Get()));
					ui.Department = Lex.CapitalizeFirstLetters(Lex.RemoveAllQuotes(lex.Get()));
				}

				UserInfo existingUi = Security.ReadUserInfo(ui.UserName);

				try
				{
					Security.CreateUser(ui);
					if (existingUi == null) msg = "User successfully created";
					else msg = "User information updated";
					msg += "\n\n" +
					"User: " + ui.UserName + "\n" +
					"Domain: " + ui.UserDomainName + "\n" +
					"First Name: " + ui.FirstName + "\n" +
					"Middle Initial: " + ui.MiddleInitial + "\n" +
					"Last Name: " + ui.LastName;
				}
				catch (Exception ex) { msg = "User creation failed: " + ex.Message; }

				if (!interactive) return msg;

				MessageBoxMx.Show(msg);
				userName = "";
			}
		}

/// <summary>
/// Update user
/// </summary>
/// <param name="commandLine"></param>
/// <returns></returns>

		public static string UpdateUser(
			string commandLine)
		{
			string msg;

			Lex lex = new Lex();
			lex.OpenString(commandLine);

			if (!Security.IsAdministrator(SS.I.UserName))
				return "You must be a Mobius administrator to update users";

			string userName = lex.GetUpper(); // get user name
			bool interactive = Lex.IsNullOrEmpty(userName);

			while (true)
			{
				if (interactive)
				{
					userName = InputBoxMx.Show("Enter the Username of the user to update:", "Update User", userName);
					if (Lex.IsNullOrEmpty(userName)) return "";
				}

				userName = userName.ToUpper();
				UserInfo ui = Security.ReadUserInfo(userName);

				if (ui == null)
				{
					msg = "User doesn't exist: " + userName;
					if (!interactive) return msg;
					MessageBoxMx.ShowError(msg);
					continue;
				}

				DialogResult dr = CreateUserDialog.ShowDialog(ui, "Update User");
				if (dr == DialogResult.Cancel) return "";

				try 
				{ 
					Security.CreateUser(ui);
					msg = "User information updated";
				}
				catch (Exception ex) { msg = "User update failed: " + ex.Message; }

				if (!interactive) return msg;

				MessageBoxMx.Show(msg);
				userName = "";
				continue;
			}
		}

/// <summary>
/// Disable user
/// </summary>
/// <param name="commandLine"></param>
/// <returns></returns>

		public static string DisableUser(
			string commandLine)
		{
			string tok, msg;

			UserInfo ui = new UserInfo();
			Lex lex = new Lex();
			lex.OpenString(commandLine);
			string userName = lex.Get();
			bool interactive = Lex.IsNullOrEmpty(userName);

			while (true)
			{
				if (interactive)
				{
					userName = InputBoxMx.Show("Enter the Username of the user to disable:", "Disable User", userName);
					if (Lex.IsNullOrEmpty(userName)) return "";
				}

				userName = userName.ToUpper();
				if (Security.ReadUserInfo(userName) == null)
				{
					msg = "User doesn't exist: " + userName;
					if (!interactive) return msg;
					MessageBoxMx.ShowError(msg);
					continue;
				}

				try
				{
				Security.RevokePrivilege(userName, "Logon");
				msg = "User disabled: " + userName;
				}
				catch (Exception ex) {  msg = "Disable user failed for: " + userName; }
				if (!interactive) return msg;

				MessageBoxMx.Show(msg);
				userName = "";
				continue;
			}
		}

/// <summary>
/// Delete all user account information and user objects
/// </summary>
/// <param name="commandLine"></param>
/// <returns></returns>

		public static string DeleteUser(
			string commandLine)
		{
			string tok, msg;

			Lex lex = new Lex();
			lex.OpenString(commandLine);
			string userName = lex.Get();
			bool interactive = Lex.IsNullOrEmpty(userName);

			while (true)
			{
				if (interactive)
				{
					userName = InputBoxMx.Show("Enter the Username of the user to delete:", "Delete User", userName);
					if (Lex.IsNullOrEmpty(userName)) return "";
				}

				userName = userName.ToUpper();
				UserInfo ui = Security.ReadUserInfo(userName);
				if (ui == null)
				{
					msg = "User doesn't exist: " + userName;
					if (!interactive) return msg;
					MessageBoxMx.ShowError(msg);
					continue;
				}

				msg = "Are you sure you want to delete user: " + userName + " (" + ui.FullName + ")";
				DialogResult dr = MessageBoxMx.Show(msg, "Delete User", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
				if (dr != DialogResult.Yes)
				{
					if (!interactive) return "";
					else continue;
				}

				bool result = Security.DeleteUser(userName);
				if (result == true) msg = "User deleted: " + userName;
				else msg = "Delete user failed for: " + userName;
				if (!interactive) return msg;

				MessageBoxMx.Show(msg);
				userName = "";
				continue;
			}
		}

  }
}
