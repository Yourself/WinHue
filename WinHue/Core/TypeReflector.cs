using System.Collections.Generic;
using System.Reflection;
using WinHue.Framework;

namespace WinHue.Core
{
    internal class TypeReflector
    {
        public TypeReflector()
        {
            mAssembly = Assembly.GetExecutingAssembly();

            foreach (var type in mAssembly.GetTypes())
            {
                if (type.IsAssignableTo(typeof(IInput)))
                {
                    mInputs.Add(new(type));
                }
                else if (type.IsAssignableTo(typeof(IOutput)))
                {
                    mOutputs.Add(new(type));
                }
                else if (type.IsAssignableTo(typeof(IVisualizer)))
                {
                    mVisualizers.Add(new(type));
                }
            }
        }

        private readonly List<InputDescriptor> mInputs = new();
        private readonly List<OutputDescriptor> mOutputs = new();
        private readonly List<VisualizerDescriptor> mVisualizers = new();

        private readonly Assembly mAssembly;
    }
}
