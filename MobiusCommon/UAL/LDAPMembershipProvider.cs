using System;
using System.Data;
using System.Configuration;
//using System.Web;
//using System.Web.Security;
//using System.Web.UI;
//using System.Web.UI.WebControls;
//using System.Web.UI.WebControls.WebParts;
//using System.Web.UI.HtmlControls;
using Novell.Directory.Ldap;

/// <summary>
/// Summary description for LDAPMembershipProvider
/// </summary>
public class LDAPMembershipProvider // : MembershipProvider
{
    private string _Name;
    private string _ldap_host;
    private int _ldap_port;

	public LDAPMembershipProvider()
	{
		_ldap_host = "[server]";
		_ldap_port = 389;
	}

	//	public override bool ValidateUser(string username, string password)
	public bool ValidateUser(string username, string password)
	{
		bool retval = this.authenticate(username,password);
		return retval;
	}

	private bool authenticate(string username, string password) 
	{
        
		string userDN = "uid="+username+",ou=account,o=corp,dc=com";

		// Creating an LdapConnection instance 
		LdapConnection ldapConn= new LdapConnection();
		
		//Connect function will create a socket connection to the server
		
		ldapConn.Connect(_ldap_host,_ldap_port);
		
		//Bind function will Bind the user object Credentials to the Server
		try {
			ldapConn.Bind(userDN,password);
		}
		catch (Exception ex) {
			
		}
		
		return ldapConn.Bound;
	}

//	public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		public void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
		{
        try
        {
            _Name = name;
            _ldap_host = (config["ldap_host"] == null) ? "[server]" : config["ldap_host"];
            _ldap_port = (config["ldap_port"] == null) ? 389 : Convert.ToInt32(config["ldap_port"]);
        }
        catch (Exception ex)
        {
            throw ex;
        }
//        base.Initialize(name, config);
    }

#if false

    public override string ApplicationName
    {
        get
        {
            return _Name;
        }
        set
        {
            _Name = value;
        }
    }

    public override bool ChangePassword(string username, string oldPassword, string newPassword)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override bool DeleteUser(string username, bool deleteAllRelatedData)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override bool EnablePasswordReset
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override bool EnablePasswordRetrieval
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override int GetNumberOfUsersOnline()
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override string GetPassword(string username, string answer)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override MembershipUser GetUser(string username, bool userIsOnline)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override string GetUserNameByEmail(string email)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override int MaxInvalidPasswordAttempts
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override int MinRequiredNonAlphanumericCharacters
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override int MinRequiredPasswordLength
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override int PasswordAttemptWindow
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override MembershipPasswordFormat PasswordFormat
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override string PasswordStrengthRegularExpression
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override bool RequiresQuestionAndAnswer
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override bool RequiresUniqueEmail
    {
        get { throw new Exception("The method or operation is not implemented."); }
    }

    public override string ResetPassword(string username, string answer)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override bool UnlockUser(string userName)
    {
        throw new Exception("The method or operation is not implemented.");
    }

    public override void UpdateUser(MembershipUser user)
    {
        throw new Exception("The method or operation is not implemented.");
    }
#endif

}
