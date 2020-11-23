﻿using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Serialization;

namespace Reductech.EDR.Core.Entities
{
    /// <summary>
    /// A piece of data.
    /// </summary>
    public sealed class Entity : IEnumerable<KeyValuePair<string, EntityValue>>
    {
        private readonly IReadOnlyDictionary<string, EntityValue> _fields;




        /// <summary>
        /// Create a new record.
        /// </summary>
        public Entity(params KeyValuePair<string, EntityValue>[] fields) : this(fields.AsEnumerable()) {}

        /// <summary>
        /// Create a new record.
        /// </summary>
        public Entity(IEnumerable<KeyValuePair<string, EntityValue>> fields) => _fields = new Dictionary<string, EntityValue>(fields);

        /// <summary>
        /// Create a new entity
        /// </summary>
        public static Entity Create(IEnumerable<KeyValuePair<string, object>> fields)
        {
            var fieldEntities = fields
                .Select(x => new KeyValuePair<string, EntityValue>(x.Key, EntityValue.Create(x .Value.ToString())));

            return new Entity(fieldEntities);
        }

        /// <summary>
        /// Gets the names of different fields on this object.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetFieldNames() => _fields.Select(x => x.Key);

        /// <summary>
        /// Creates a copy of this with the new fields added or updated.
        /// </summary>
        /// <param name="newFields"></param>
        /// <returns></returns>
        public Entity WithFields(IReadOnlyCollection<KeyValuePair<string, EntityValue>> newFields)
        {
            if (!newFields.Any())
                return this;

            var newDict = newFields.Concat(_fields).GroupBy(x => x.Key, x => x.Value)
                .ToDictionary(x => x.Key, x => x.First());

            return new Entity(newDict);
        }

        /// <summary>
        /// Gets the values of a particular field.
        /// </summary>
        public EntityValue this[string key] => _fields[key];

        /// <inheritdoc />
        public IEnumerator<KeyValuePair<string, EntityValue>> GetEnumerator() => _fields.GetEnumerator();

        /// <inheritdoc />
        public override string ToString() => AsString();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


        /// <summary>
        /// Serialize this record.
        /// </summary>
        /// <returns></returns>
        public Result<string> TrySerializeShortForm()
        {
            var sb = new StringBuilder();

            sb.Append("(");

            var results = new List<Result<string>>();

            foreach (var (key, value) in _fields)
            {
                value.Value.Switch(_=>{},
                    singleValue=>
                    {
                        var r = SerializationMethods.TrySerializeShortFormString(singleValue.Original)
                            .Map(v => $"{key} = {v}");
                        results.Add(r);
                    },
                    multiValue=>
                    {
                        var r = SerializationMethods.TrySerializeSimpleList(multiValue.Select(x => x.Original))
                            .Map(v => $"{key} = {v}");
                        results.Add(r);
                    });
            }

            var result = results.Combine();

            if (result.IsFailure)
                return result.ConvertFailure<string>();

            sb.AppendJoin(",", result.Value);

            sb.Append(")");

            return sb.ToString();
        }

        /// <summary>
        /// Convert this entity to an object that can be serialized
        /// </summary>
        /// <returns></returns>
        public object ToSimpleObject()
        {
            IDictionary<string, object> expandoObject = new ExpandoObject();

            foreach (var (key, value) in _fields)
            {
                value.Value.Switch(_=>{},
                    v=> expandoObject[key] = v,
                    l => expandoObject[key] = l  );
            }

            return expandoObject;
        }

        /// <summary>
        /// Converts this record into a string.
        /// </summary>
        public string AsString()
        {
            var result = string.Join(", ",
                _fields.Select(field => $"{field.Key}: {field.Value}"));

            return result;
        }

    }
}
