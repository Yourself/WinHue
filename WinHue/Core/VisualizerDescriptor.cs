using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using WinHue.Framework;

namespace WinHue.Core
{
    internal sealed class VisualizerDescriptor
    {
        public VisualizerDescriptor(Type type)
        {
            ValidateType(type);

            Dependencies = GetDependencies(type);
            Type = type;
        }

        public IEnumerable<Type> Dependencies { get; }

        public Type Type { get; }

        private static void ValidateType(Type type, [CallerArgumentExpression("type")] string? exp = null)
        {
            if (!type.IsAssignableTo(typeof(IVisualizer)))
            {
                throw new ArgumentException("Type must implement IVisualizer interface", exp);
            }
            if (type.IsAbstract)
            {
                throw new ArgumentException("Type must not be abstract", exp);
            }
        }

        private static IEnumerable<Type> GetDependencies(Type type, [CallerArgumentExpression("type")] string? exp = null)
        {
            var ctors = type.GetConstructors();
            if (ctors.Length != 1)
            {
                throw new ArgumentException("Type must specify exactly one constructor", exp);
            }

            var ctorParams = ctors[0].GetParameters();

            foreach (var param in ctorParams)
            {
                var elemType = param.ParameterType;
                if (elemType.IsAssignableTo(typeof(IInput))) continue;
                if (elemType.IsAssignableTo(typeof(IOutput))) continue;
                throw new ArgumentException($"Constructor parameter {param.ParameterType} {param.Name} must implement IInput or IOutput", exp);
            }

            return ctorParams.Select(param => param.ParameterType).ToArray();
        }
    }
}
