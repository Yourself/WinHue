using System;

namespace WinHue.Framework
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CustomPageAttribute : Attribute
    {
        public CustomPageAttribute(Type pageType)
        {
            PageType = pageType;
        }

        public Type PageType { get; }
    }
}
