using System;
using System.Collections.Generic;
using WinHue.Framework;

namespace WinHue.Core
{
    internal sealed class ConfigurationDescriptor
    {
        public ConfigurationDescriptor(Type type)
        {
            var configurableProperties = new List<ConfigurablePropertyDescriptor>();
            foreach (var property in type.GetProperties())
            {
                if (property.IsDefined(typeof(ConfigurableAttribute), false))
                {
                    configurableProperties.Add(new(property));
                }
            }

            Properties = configurableProperties.ToArray();
        }

        public IEnumerable<ConfigurablePropertyDescriptor> Properties { get; }
    }
}
