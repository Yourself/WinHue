using System;
using System.Reflection;
using System.Xml.Serialization;

namespace WinHue.Core
{
    internal sealed class ConfigurablePropertyDescriptor
    {
        public ConfigurablePropertyDescriptor(PropertyInfo prop)
        {
            mProp = prop;
            Serializer = new(prop.PropertyType);
        }

        public string Name => mProp.Name;
        public Type Type => mProp.PropertyType;
        public XmlSerializer Serializer { get; }

        private readonly PropertyInfo mProp;
    }
}
