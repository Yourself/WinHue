using System;
using System.Runtime.CompilerServices;
using WinHue.Framework;

namespace WinHue.Core
{
    internal sealed class InputDescriptor
    {
        public InputDescriptor(Type type)
        {
            ValidateType(type);
            Type = type;
        }

        public IInput CreateInstance()
        {
            return (IInput)Activator.CreateInstance(Type)!;
        }

        public Type Type { get; }

        private static void ValidateType(Type type, [CallerArgumentExpression("type")] string? exp = null)
        {
            if (!type.IsAssignableTo(typeof(IInput)))
            {
                throw new ArgumentException("Type must implement IInput", exp);
            }
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new ArgumentException("Inputs must have parameterless constructor", exp);
            }
        }
    }
}
