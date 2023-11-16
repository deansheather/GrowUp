using GrowUp.Data;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace GrowUp.Services
{
    internal class CustomSerializationBinder : ISerializationBinder
    {
        internal static Dictionary<string, Type> CustomTypes = new() {
            // These keys can never be changed without breaking old config
            // files.
            ["GrowUpTargetCharacterName"] = typeof(Target.CharacterName),
            ["GrowUpTargetCharacterName"] = typeof(Target.CharacterName),
        };

#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
        public Type? BindToType(string assemblyName, string typeName) {
            if (CustomTypes.ContainsKey(typeName)) {
                return CustomTypes[typeName];
            }

            // null means "don't include type info in the JSON"
            return null;
        }
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

        public void BindToName(Type serializedType, out string assemblyName, out string typeName) {
            assemblyName = serializedType.Assembly.FullName ?? "";
            typeName = serializedType.FullName!;

            foreach (var (key, value) in CustomTypes) {
                if (value == serializedType) {
                    typeName = key;
                    break;
                }
            }
        }
    }

    internal class ConfigurationService
    {
        internal static Config Config { get; private set; } = null!;

        internal static void Load() {
            // Dalamud doesn't load JSON containing abstract class lists
            // properly, so manually load using a custom SerializationBinder.
            var configFile = DalamudServices.PluginInterface.ConfigFile;
            try {
                var text = File.ReadAllText(configFile.FullName);
                Config = JsonConvert.DeserializeObject<Config>(text, new JsonSerializerSettings {
                    TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                    TypeNameHandling = TypeNameHandling.Auto,
                    SerializationBinder = new CustomSerializationBinder(),
                })!;
                if (Config == null) {
                    throw new Exception("Deserialized config was null.");
                }
            } catch (FileNotFoundException) {
                Config = new Config();
            }
        }

        internal static void Save() {
            // And custom saving logic to match.
            var configFile = DalamudServices.PluginInterface.ConfigFile;
            var text = JsonConvert.SerializeObject(Config, new JsonSerializerSettings {
                TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Simple,
                TypeNameHandling = TypeNameHandling.Auto,
                SerializationBinder = new CustomSerializationBinder(),
                Formatting = Formatting.Indented,
            });

            File.WriteAllText(configFile.FullName, text);
        }
    }
}
