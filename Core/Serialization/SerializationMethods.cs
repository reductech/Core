﻿using System;
using System.Collections;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Reductech.EDR.Core.Entities;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Core.Serialization
{
    /// <summary>
    /// Serializes primitive types
    /// </summary>
    public static class SerializationMethods
    {
        /// <summary>
        /// Serialize a constant freezable step. Used in short form serialization.
        /// </summary>
        public static Result<string> TrySerializeConstant(ConstantFreezableStep cfp) => TrySerializeShortForm(cfp.Value);

        /// <summary>
        /// Serialize a value.
        /// </summary>
        private static Result<string> TrySerializeShortForm(object value)
        {
            if (value is string s)
                return TrySerializeShortFormString(s);

            if (value is IEnumerable enumerable)
            {

                var r = enumerable.OfType<object>().Select(TrySerializeShortForm)
                    .Combine()
                    .Map(x=> string.Join(", ", x))
                    .Map(x=> $"[{x}]");

                return r;
            }

            if (value is Entity entity)
                return entity.TrySerializeShortForm();

            if(value is Stream)
                return Result.Failure<string>("Streams cannot be serialized in short form");

            if (value is EntityStream)
                return Result.Failure<string>("EntityStreams cannot be serialized in short form");


            return TrySerializeSimple(value);
        }


        /// <summary>
        /// Tries to serialize a string in the short form.
        /// </summary>
        public static Result<string> TrySerializeShortFormString(string s)
        {
            var newS = $"'{Escape(s)}'";
            if (!newS.Contains('\n'))
                return newS;
            return Result.Failure<string>("Strings containing newline characters cannot be serialized in short form");
        }

        /// <summary>
        /// Try to serialize a simple object.
        /// </summary>
        public static Result<string> TrySerializeSimple(object value)
        {
            if (value is Enum)
                return value.GetType().Name + "." + value;
            if (value is bool b)
                return b.ToString();
            if (value is int i)
                return i.ToString();
            if (value is double d)
                return d.ToString("G17", CultureInfo.InvariantCulture);


            return Result.Failure<string>($"Could not serialize {value.GetType().Name}");
        }


        private static object ConvertToSerializableType(Entity entity)
        {
            var shortFormEntity = entity.TrySerializeShortForm();
            if (shortFormEntity.IsSuccess)
                return shortFormEntity.Value;

            return entity.ToSimpleObject();
        }


        private static object ConvertToSerializableType1(object value)
        {
            if (value is string s)
                return new YamlString(s);

            if (value is Entity entity)
                return ConvertToSerializableType(entity);

            if (value is EntityStream entityStream)
            {
                var task = entityStream.TryGetResultsAsync(CancellationToken.None);
                task.Wait();

                var entities = task
                    .Result //This should work, but maybe not in some environments
                    .Map(x => x.Select(ConvertToSerializableType).ToList());

                if (entities.IsFailure)
                    throw new SerializationException(entities.Error);

                return entities.Value;
            }


            if (value is Stream stream)
            {
                var streamString = StreamToString(stream, Encoding.UTF8);
                return new YamlString(streamString); //This will be a string - convert it to a stream
            }

            if (value is IEnumerable enumerable)
            {
                var list = enumerable.OfType<object>().Select(ConvertToSerializableType1).ToList();
                return list;
            }


            var r = TrySerializeSimple(value);

            if (r.IsSuccess)
                return r.Value;

            throw new SerializationException(r.Error);//This is unexpected
        }

        internal static object ConvertToSerializableType(ConstantFreezableStep cfp) => ConvertToSerializableType1(cfp.Value);

        /// <summary>
        /// Escape single quotes
        /// </summary>
        public static string Escape(string s) => s.Replace("'", "''");

        /// <summary>
        /// Converts a stream to a string with the given encoding.
        /// </summary>
        public static string StreamToString(Stream stream, Encoding encoding)
        {
            stream.Position = 0;
            using StreamReader reader = new StreamReader(stream, encoding);
            return reader.ReadToEnd();
        }

    }
}
