using System;
using System.Runtime.CompilerServices;
using WinHue.Framework;

namespace WinHue.Core
{
    internal sealed class OutputDescriptor
    {
        public OutputDescriptor(Type type)
        {
            ValidateType(type);
            Type = type;
        }

        public IOutput CreateInstance()
        {
            return (IOutput)Activator.CreateInstance(Type)!;
        }

        public Type Type { get; }

        private static void ValidateType(Type type, [CallerArgumentExpression("type")] string? exp = null)
        {
            if (!type.IsAssignableTo(typeof(IOutput)))
            {
                throw new ArgumentException("Type must implement IOutput", exp);
            }
        }
    }
}
