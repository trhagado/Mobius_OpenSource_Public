using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Types
{
	[DataContract(Namespace = "http://server/MobiusServices/v1.0")]
	public enum MetaBrokerType
	{
		[EnumMember] Unknown = 0, // unknown type
		[EnumMember] Generic = 1, // basic generic broker
		[EnumMember] Assay = 2, // generic assay metabroker
		[EnumMember] Annotation = 4, // annotation table data
		[EnumMember] CalcField = 5, // calculated field

		[EnumMember] Pivot = 11, // generic pivot broker
		[EnumMember] MultiTable = 12, // metatable based on multiple underlying tables
		[EnumMember] CalcTable = 13, // Table containing multiple calculated fields
		[EnumMember] TargetAssay = 14, // retrieves and summarizes data by target and assay
		[EnumMember] UnpivotedAssay = 15, // search unpivoted assay results across databases
		[EnumMember] RgroupDecomp = 16, // r-group decomposition table
		[EnumMember] SpotfireLink = 17, // Link to Spotfire analysis

		[EnumMember] NoSql = 18, // broker that doesn't depend on SQL (exclusively) for data search and retrieval
		[EnumMember] EmbeddedData = 18, // (replaced by NoSQL, remove when PRD metadata updated)

		[EnumMember] MaxBrokerType = 18
	}
}
