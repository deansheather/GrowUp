using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace GrowUp.Data
{
    [Serializable]
    public class Config : IPluginConfiguration
    {
        public int Version { get; set; } = 0;

        public List<Rule> Rules = new();
    }
}
