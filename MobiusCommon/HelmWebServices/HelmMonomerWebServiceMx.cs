using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace Mobius.Helm
{
	/// <summary>
	/// Mobius wrapper for HelmWebServiceClient built by NSwagStudio on Oct-5-2019
	/// from URL: http://[server]:8080/HELM2MonomerService//swagger.json
	/// This wrapper class provides additionaly method for the service that
	/// make it easier to use the service in the Mobius context.
	/// </summary>

	public class HelmMonomerWebServiceMx
	{
		public HelmMonomerWebService WSC = null; // the wrapped HelmWebServiceClient
		public HttpClient HttpClient = null; // associated HttpClient

		public static string HelmServiceBaseUrl = "http://[server]:8080/HELM2MonomerService/rest";

		public HelmMonomerWebServiceMx()
		{
			HttpClientHandler handler = new HttpClientHandler { Credentials = System.Net.CredentialCache.DefaultNetworkCredentials };
			HttpClient = new HttpClient(handler);
			WSC = new HelmMonomerWebService(HelmServiceBaseUrl, HttpClient);
			return;
		}

		public HelmMonomerDetail GetHelmMonomerDetail(string polymertype, string symbol)
		{
			string json = GetHelmMonomerDetailJson(polymertype, symbol);

			var settings = new JsonSerializerSettings();
			settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;

			HelmMonomerDetail monomerDetail = GetHelmMonomerDetailFromJson(json);
			return monomerDetail;
		}

		public string GetHelmMonomerDetailJson(string polymertype, string symbol)
		{
			Task<string> task = WSC.MonomerDetailAsync(polymertype, symbol);

			task.Wait();

			string json = task.Result;
			return json;
		}

		public HelmMonomerDetail GetHelmMonomerDetailFromJson(string json)
		{
			var settings = new JsonSerializerSettings();
			settings.MetadataPropertyHandling = MetadataPropertyHandling.Ignore;

			HelmMonomerDetail monomerDetail = JsonConvert.DeserializeObject<HelmMonomerDetail>(json, settings);
			return monomerDetail;
		}
	}
}
