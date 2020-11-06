using System;
using System.Reflection;
using Microsoft.AspNetCore.Components;

namespace Explorer.Utilities
{
    public static class Rendering
    {
        public static RenderFragment RenderContent(ComponentBase instance)
        {
            var fragmentField = GetPrivateField(instance.GetType(), "_renderFragment");

            var value = (RenderFragment)fragmentField.GetValue(instance);

            return value;
        }

        //https://stackoverflow.com/a/48551735/66988
        private static FieldInfo GetPrivateField(Type t, string name)
        {
            const BindingFlags bf = BindingFlags.Instance |
                                    BindingFlags.NonPublic |
                                    BindingFlags.DeclaredOnly;

            FieldInfo fi;
            while ((fi = t.GetField(name, bf)) == null && (t = t.BaseType) != null) ;

            return fi;
        }
    }
}
