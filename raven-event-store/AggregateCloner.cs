using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Raven.EventStore;

internal static class AggregateCloner
{
    private static readonly JsonSerializerSettings Settings = new()
    {
        ContractResolver = new NonPublicMemberContractResolver(),
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
    };

    internal static Aggregate Clone(Aggregate aggregate)
    {
        var json = JsonConvert.SerializeObject(aggregate, aggregate.GetType(), Settings);
        return (Aggregate)JsonConvert.DeserializeObject(json, aggregate.GetType(), Settings);
    }
}

internal class NonPublicMemberContractResolver : DefaultContractResolver
{
    protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
    {
        var jsonProperties = new List<JsonProperty>();

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            var jsonProp = CreateProperty(prop, memberSerialization);
            jsonProp.Readable = true;
            jsonProp.Writable = true;
            jsonProperties.Add(jsonProp);
        }

        return jsonProperties;
    }
}
