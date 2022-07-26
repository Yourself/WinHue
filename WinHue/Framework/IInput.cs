namespace WinHue.Framework
{
    /// <summary>
    /// Represents an input data source that can be utilized by visualizers.
    /// </summary>
    internal interface IInput
    {
        /// <summary>
        /// Gets whether this input is always active. If false, requires an active consumer.
        /// </summary>
        bool AlwaysActive { get; }

        /// <summary>
        /// Activates this input.
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivates this input.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Updates the input.
        /// </summary>
        void Update();
    }
}
