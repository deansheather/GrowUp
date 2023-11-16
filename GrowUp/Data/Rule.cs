using System;

namespace GrowUp.Data
{
    [Serializable]
    public class Rule
    {
        public string ID = Guid.NewGuid().ToString();
        public bool Enabled = true;
        public Target? Target = default;
        public ObjectProperties Properties = new();
    }

    [Serializable]
    public struct ObjectProperties
    {
        // TODO: allow values to be "set", "reset", or "inherit" modes
        public float Scale { get; set; } = 1.0f;

        public ObjectProperties() { }
    }
}
