namespace WinHue.Input
{
    internal sealed class AudioDevice
    {
        public AudioDevice(string id, string name, int index)
        {
            ID = id;
            Name = name;
            Index = index;
        }

        public string ID { get; set; }
        public string Name { get; set; }
        public int Index { get; set; }
    }
}
