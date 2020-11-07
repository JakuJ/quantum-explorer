using System;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Explorer.Utilities
{
    /// <summary>Contains helper methods for rendering Razor components.</summary>
    public static class Rendering
    {
        /// <summary>Renders an instance of a component onto the page.</summary>
        /// <param name="instance">Instance of some component.</param>
        /// <returns>The <see cref="RenderFragment"/> corresponding to the rendered component.</returns>
        public static RenderFragment RenderContent(ComponentBase instance)
        {
            FieldInfo? fragmentField = GetPrivateField(instance.GetType(), "_renderFragment");

            return fragmentField?.GetValue(instance) as RenderFragment ??
                   throw new ArgumentException("Could not find a render fragment in the argument to RenderContent");
        }

        // https://stackoverflow.com/a/48551735/66988
        private static FieldInfo? GetPrivateField(Type? t, string name)
        {
            const BindingFlags bf = BindingFlags.Instance |
                                    BindingFlags.NonPublic |
                                    BindingFlags.DeclaredOnly;

            FieldInfo? fi;
            while ((fi = t!.GetField(name, bf)) == null && (t = t.BaseType) != null) { }

            return fi;
        }
    }
}
