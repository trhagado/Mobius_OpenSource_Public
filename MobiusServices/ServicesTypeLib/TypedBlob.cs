using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
	[DataContract(Namespace = "http://<server>/MobiusServices/v1.0")]
	public class TypedBlob
	{
		[DataMember] public string Type;
		[DataMember] public byte[] Data;
	}

	[DataContract(Namespace = "http://<server>/MobiusServices/v1.0")]
	public class TypedClob
	{
		[DataMember] public string Type;
		[DataMember] public string Data;
	}

}
