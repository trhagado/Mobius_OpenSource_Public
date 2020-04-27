using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Mobius.ComOps;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.IdentityModel;


namespace Mobius.Services.Util
{
	public class ServiceHostUtil
	{
		private static System.ServiceModel.EndpointIdentity _serviceEndpointIdentity;

		public static string HostName
		{
			get { return _serviceHostName; }
		}
		private static string _serviceHostName;

		public static int BasePort
		{
			get { return _serviceBasePort; }
		}
		private static int _serviceBasePort;

		/// <summary>
		/// Basic constructor
		/// </summary>

		private ServiceHostUtil()
		{
			return;
		}

		/// <summary>
		/// Setup endpoints
		/// </summary>

		static ServiceHostUtil()
		{
			_serviceHostName = ServicesIniFile.Read("ServiceHost", "localhost");
			_serviceBasePort = int.Parse(ServicesIniFile.Read("ServiceBasePort", "7700"));
			string serviceIdentityType = ServicesIniFile.Read("ServiceIdentityType", "dns");
			string serviceIdentity = ServicesIniFile.Read("ServiceIdentity", "localhost");

			if (serviceIdentityType == "dns")
			{
				_serviceEndpointIdentity =
						new System.ServiceModel.DnsEndpointIdentity(serviceIdentity);
			}
			else if (serviceIdentityType == "upn")
			{
				_serviceEndpointIdentity =
						new System.ServiceModel.UpnEndpointIdentity(serviceIdentity);
			}
			else if (serviceIdentityType == "spn")
			{
				_serviceEndpointIdentity =
						new System.ServiceModel.SpnEndpointIdentity(serviceIdentity);
			}
			else
			{
				throw new InvalidOperationException("Only identity types \"dns\", \"upn\", and \"spn\" (with a corresponding valid ServiceIdentity) are supported.");
			}
		}

		/// <summary>
		/// ApplyEndpointAddressAndIdentityConfig
		/// </summary>
		/// <param name="serviceHost"></param>

		public static void ApplyEndpointAddressAndIdentityConfig(ServiceHost serviceHost)
		{
			Uri serviceUri = null;

			for (int i = 0; i < serviceHost.Description.Endpoints.Count; i++)
			{
				ServiceEndpoint serviceEndpoint = serviceHost.Description.Endpoints[i];
				EndpointAddress originalAddress = serviceEndpoint.Address;

				// Net.pipe addresses are provided by the Nomenclator

				if (originalAddress.Uri.Scheme == "net.pipe")
				{
					serviceUri = originalAddress.Uri;
				}

				// Get the Uri info for the address using the information from the user32-style config
				// port offset (from the base port) depends on the endpoint type...
				//
				// Default base port: 7700
				// Offsets:
				//  0 - http (and mex)
				//  1 - net.tcp
				//  2 - (not used)
				//  3 - (not used)
				//  4 - https
				//  5 - native Mobius Client

				else
				{
					int portOffset = (originalAddress.Uri.Scheme == "net.tcp") ? 1 : 0; //+1 for non-native net.tcp; 0 for http (and mex)

					if (originalAddress.Uri.Scheme == "https")
					{
						portOffset = 4;
					}

					if (serviceEndpoint.Contract.ContractType.Name == "INativeSession") //could really find a better way to determine this...
						portOffset = 5; // the native session host port offset is +5 (changed from 2 to 5, 7002 is already used on the QA server)

					serviceUri = new Uri(originalAddress.Uri.Scheme + "://" + _serviceHostName + ":" + (_serviceBasePort + portOffset) +
						originalAddress.Uri.LocalPath);
				}



				// Apply the update(s) -- Uri and, if present, the identity

				serviceEndpoint.Address = new EndpointAddress(
						serviceUri,
						((originalAddress.Identity == null) ? null : _serviceEndpointIdentity));

				//DebugLog.Message("ServiceHost endpoint " + (i + 1) + ": " + serviceEndpoint.Address + " " + serviceEndpoint.Name);
			}
		}
	}
}
