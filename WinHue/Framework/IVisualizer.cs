namespace WinHue.Framework
{
    /// <summary>
    /// A visualizer processes data from inputs and uses that to driveone or more outputs.
    /// </summary>
    internal interface IVisualizer
    {
        /// <summary>
        /// Gets the priority of this visualizer. The visualizers with the highest non-zero priority will run.
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Called to update this visualizer's state, consuming input data and driving output data.
        /// </summary>
        void Update();
    }
}
