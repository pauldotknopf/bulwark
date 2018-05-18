﻿using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bulwark.Integration.GitLab.Api
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MergeStatus
    {
        [EnumMember(Value="cannot_be_merged")]
        CannotBeMerged,
        [EnumMember(Value="can_be_merged")]
        CanBeMerged,
        [EnumMember(Value="unchecked")]
        Unchecked
    }
}