using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Bulwark.Integration.GitLab.Api.Types
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum MergeRequestState
    {
        [EnumMember(Value="opened")]
        Opened,
        [EnumMember(Value="closed")]
        Closed,
        [EnumMember(Value="merged")]
        Merged
    }
}