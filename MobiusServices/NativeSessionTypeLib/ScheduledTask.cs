using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization;


namespace Mobius.Services.Native
{
    [DataContract(Namespace = "http://server/MobiusServices/Native/v1.0")]
    [KnownType(typeof(string))]
    [KnownType(typeof(Mobius.Services.Types.Node))]
    [KnownType(typeof(Dictionary<string, Mobius.Services.Types.Node>))]
    public class ScheduledTask
    {
        [DataMember] public int JobId = -1;
        [DataMember] public ScheduledTaskTypes TaskType = ScheduledTaskTypes.Unknown;
        [DataMember] public ScheduledTaskStatus Status = ScheduledTaskStatus.Unknown;
        [DataMember] public DateTime SubmissionDateTime = DateTime.Now;
        [DataMember] public TimeSpan SuggestedTimeBetweenPolls = TimeSpan.FromSeconds(5.0);
        [DataMember] public string CommandArg;
        [DataMember] public object Result;

        public ScheduledTask()
        {
        }

        public ScheduledTask(ScheduledTaskTypes taskType, string commandArg)
        {
            TaskType = taskType;
            CommandArg = commandArg;
        }
    }
    
    [DataContract(Namespace = "http://server/MobiusServices/Native/v1.0")]
    public enum ScheduledTaskTypes
    {
        [EnumMember] Unknown,
        [EnumMember] ExecuteCommand,
				[EnumMember] LoadMetadata,
        [EnumMember] UpdateMetaTableStatistics,
    }

    [DataContract(Namespace = "http://server/MobiusServices/Native/v1.0")]
    public enum ScheduledTaskStatus
    {
        [EnumMember] Unknown,
        [EnumMember] Running,
        [EnumMember] Succeeded,
        [EnumMember] Failed,
    }
}
