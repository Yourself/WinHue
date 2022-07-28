using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace WinHue.Utilities
{
    internal static class Reflection
    {
        public static (bool IsCollection, Type Type) GetCollectionElementType(this Type type)
        {
            if (!type.IsAssignableTo(typeof(IEnumerable))) return (false, type);
            if (type.IsArray) return (true, type.GetElementType()!);
            var enumerableInterface = type.GetInterfaces()
                .Where(iface => iface.IsGenericType)
                .FirstOrDefault(iface => iface.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (enumerableInterface == null) return (true, typeof(object));
            return (true, enumerableInterface.GetGenericArguments().First());
        }
    }
}
