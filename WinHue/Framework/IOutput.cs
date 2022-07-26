namespace WinHue.Framework
{
    internal interface IOutput
    {
        /// <summary>
        /// Activates this output.
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivates this output.
        /// </summary>
        void Deactivate();

        /// <summary>
        /// Updates the state of the output.
        /// </summary>
        void Update();
    }
}
