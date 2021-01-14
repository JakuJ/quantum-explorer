using System;
using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Compiler.AzureFunction.Serialization
{
    /// <inheritdoc cref="JsonConverter"/>
    /// <summary>
    /// Provides the ability to deserialize <see cref="T:System.Numerics.Complex" /> objects
    /// from JSON objects that contain "R" and "I" keys.
    /// </summary>
    internal class ComplexConverter : JsonConverter
    {
        public static readonly ComplexConverter Instance = new();

        private ComplexConverter() { }

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType) => objectType == typeof(Complex);

        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            // Load the JSON for the Complex into a JObject
            JObject jo = JObject.Load(reader);

            // Read the properties which will be used as constructor parameters
            double real = jo["R"]?.Value<double>() ?? default;
            double imaginary = jo["I"]?.Value<double>() ?? default;

            // Construct the Complex object using the non-default constructor
            return new Complex(real, imaginary);
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            => throw new NotImplementedException();
    }
}
