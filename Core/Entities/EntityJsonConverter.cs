﻿using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Core.Entities
{

/// <summary>
/// Converts Entities to Json
/// </summary>
public class EntityJsonConverter : JsonConverter
{
    private EntityJsonConverter() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static EntityJsonConverter Instance { get; } = new();

    /// <inheritdoc />
    public override void WriteJson(
        JsonWriter writer,
        object? entityObject,
        JsonSerializer serializer)
    {
        if (entityObject is not Entity entity)
            return;

        var dictionary = CreateDictionary(entity);

        serializer.Serialize(writer, dictionary);

        static Dictionary<string, object?> CreateDictionary(Entity entity)
        {
            var dictionary = new Dictionary<string, object?>();

            foreach (var entityProperty in entity)
            {
                var value = GetObject(entityProperty.BestValue);
                dictionary.Add(entityProperty.Name, value);
            }

            return dictionary;

            static object? GetObject(EntityValue ev)
            {
                return ev switch
                {
                    EntityValue.NestedEntity nestedEntity => CreateDictionary(nestedEntity.Value),
                    EntityValue.EnumerationValue enumerationValue => enumerationValue.Value.Value,
                    EntityValue.NestedList list => list.Value.Select(GetObject).ToList(),
                    _ => ev.ObjectValue
                };
            }
        }
    }

    /// <inheritdoc />
    public override object ReadJson(
        JsonReader reader,
        Type objectType,
        object? existingValue,
        JsonSerializer serializer)
    {
        var objectDict = serializer.Deserialize<Dictionary<string, object>>(reader);

        var entity =
            Entity.Create(objectDict!.Select(x => (new EntityPropertyKey(x.Key), x.Value))!);

        return entity;
    }

    /// <inheritdoc />
    public override bool CanConvert(Type objectType) => objectType == typeof(Entity);
}

}
