using System;
using System.Runtime.CompilerServices;
using WinHue.Framework;

namespace WinHue.Core
{
    internal class InputDescriptor
    {
        public InputDescriptor(Type type)
        {
            ValidateType(type);
            Type = type;
        }

        public Type Type { get; }

        private static void ValidateType(Type type, [CallerArgumentExpression("type")] string? exp = null)
        {
            if (!type.IsAssignableTo(typeof(IInput)))
            {
                throw new ArgumentException("Type must implement IInput", exp);
            }
        }
    }
}
