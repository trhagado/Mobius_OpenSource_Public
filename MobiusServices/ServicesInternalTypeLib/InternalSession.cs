using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Mobius.Services.Types.Internal
{

/// <summary>
/// InternalSession - Session information held within the service
/// </summary>

	[DataContract(Namespace = "Mobius.Services.Types.Internal")]
	[Serializable]
	//    [KnownType(typeof(Mobius.Services.Types.Session))]
	public class InternalSession
	{
		[DataMember]
		public int Id;
		[DataMember]
		public string UserId; // domain\userName
		[DataMember]
		public Dictionary<SessionParameterName, string> SessionParameters = 
			new Dictionary<SessionParameterName, string>();
		[DataMember]
		public DateTime CreationDT;
		[DataMember]
		public DateTime ExpirationDT;
		[DataMember]
		public DateTime LastFreshenDT;
		[DataMember]
		public string CallerIPAddress;
		[DataMember]
		public bool IsNativeSession; // if true the native process services will be called directly rather than through the host services

/// <summary>
/// Convert a Types.InternalSession to a Types.Session
/// </summary>
/// <param name="session"></param>
/// <returns></returns>

		public static explicit operator Mobius.Services.Types.Session(InternalSession session)
		{
			if (session == null) return null;

			Types.Session publicSession =
				new Mobius.Services.Types.Session
				{
					Id = session.Id,
					UserId = session.UserId,
					SessionParameters = session.SessionParameters,
					CreationDT = session.CreationDT,
					ExpirationDT = session.ExpirationDT,
				};

			if (!publicSession.SessionParameters.ContainsKey(SessionParameterName.IsNativeSession))
			  publicSession.SessionParameters.Add(SessionParameterName.IsNativeSession, session.IsNativeSession.ToString());

			return publicSession;
		}

/// <summary>
/// Access the native process id from SessionParameters[NativeSessionLine]
/// </summary>

		public int ProcessId
		{
			get
			{
				int processId = -1;
				Dictionary<SessionParameterName, string> sps = SessionParameters;
				SessionParameterName nsl = SessionParameterName.NativeSessionLine;
				if (sps.ContainsKey(nsl)) int.TryParse(sps[nsl], out processId);
				return processId;
			}

			set
			{
				Dictionary<SessionParameterName, string> sps = SessionParameters;
				SessionParameterName nsl = SessionParameterName.NativeSessionLine;
				sps[nsl] = value.ToString();
			}
		}

/// <summary>
/// ToString
/// </summary>
/// <returns></returns>

		public override string ToString()
		{
			string txt =
					"Session: " + Id +
					", Created: " + CreationDT.ToString("yyyy-MM-dd HH:mm:ss") +
					", Last Freshen: " + LastFreshenDT.ToString("yyyy-MM-dd HH:mm:ss") +
					", Good till: " + ExpirationDT.ToString("yyyy-MM-dd HH:mm:ss") +
					", UserName: " + UserId +
					", IP Address: " + CallerIPAddress +
					", IsNativeSession: " + IsNativeSession;

			if (SessionParameters != null)
			{
				foreach (SessionParameterName spn in SessionParameters.Keys)
					txt += ", Parameter: " + spn + " = " + SessionParameters[spn];
			}

			return txt;
		}

	}
}
