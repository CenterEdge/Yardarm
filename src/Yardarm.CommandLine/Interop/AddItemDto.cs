using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

#nullable enable

namespace Yardarm.CommandLine.Interop
{
    // Note: DataContract attributes are used for deserialization within legacy framework MSBuild

    [DataContract]
    internal class AddItemDto
    {
        [DataMember(Name = "itemType")]
        public string? ItemType { get; set; }

        [DataMember(Name = "targetFramework")]
        public string? TargetFramework { get; set; }

        [DataMember(Name = "identity")]
        public string? Identity { get; set; }

        [DataMember(Name = "metadata")]
        public Dictionary<string, string>? Metadata { get; set; }
    }
}
