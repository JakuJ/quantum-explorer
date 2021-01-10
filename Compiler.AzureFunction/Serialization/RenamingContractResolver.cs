using System;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Compiler.AzureFunction.Serialization
{
    /// <inheritdoc/>
    /// <summary>
    /// Manages property name resolution in <see cref="Complex"/> and <see cref="ValueTuple{T,T}"/> types
    /// during JSON serialization.
    /// </summary>
    internal class RenamingContractResolver : DefaultContractResolver
    {
        public static readonly RenamingContractResolver Instance = new();

        private RenamingContractResolver() =>
            PropertyMappings = new Dictionary<Type, Dictionary<string, string>>
            {
                {
                    typeof(Complex),
                    new Dictionary<string, string>
                    {
                        { "Real", "R" },
                        { "Imaginary", "I" },
                    }
                },
                {
                    typeof(ValueTuple<int, Complex>),
                    new Dictionary<string, string>
                    {
                        { "Item1", "F" },
                        { "Item2", "S" },
                    }
                },
            };

        private Dictionary<Type, Dictionary<string, string>> PropertyMappings { get; }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = base.CreateProperty(member, memberSerialization);

            if (member.DeclaringType != null
             && PropertyMappings.TryGetValue(member.DeclaringType, out var dict)
             && dict.TryGetValue(member.Name, out var jsonName))
            {
                property.PropertyName = jsonName;
            }

            if (property is { PropertyName: "Magnitude" or "Phase" })
            {
                property.ShouldSerialize = _ => false;
            }

            return property;
        }
    }
}
