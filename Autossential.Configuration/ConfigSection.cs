using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Autossential.Configuration.Activities")]

namespace Autossential.Configuration
{
    public sealed class ConfigSection : IEnumerator, IEnumerable<KeyValuePair<string, object>>
    {
        internal ConfigSection(string yamlPath) : this(RootName, Deserialize(File.ReadAllText(yamlPath)))
        {
        }

        internal ConfigSection(string name, Dictionary<object, object> config)
        {
            if (!IsSetupDone())
                Setup(".", '/');

            Name = name;

            foreach (var item in config)
            {
                var key = item.Key.ToString();

                if (item.Value is Dictionary<object, object> section)
                {
                    _settings.Add(key, new ConfigSection(key, section)
                    {
                        Parent = this
                    });
                }
                else
                {
                    _settings.Add(key, item.Value);
                }
            }
        }

        internal static string RootName { get; set; }

        internal static char SectionDelimiter { get; set; }

        internal static Dictionary<object, object> Deserialize(string yamlContent)
        {
            var deserializer = new YamlDotNet.Serialization.Deserializer();
            return deserializer.Deserialize<Dictionary<object, object>>(yamlContent);
        }

        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

        public ConfigSection Parent { get; private set; }

        public static void Setup(string rootName, char sectionDelimiter)
        {
            if (IsSetupDone()) throw new InvalidOperationException("ConfigSection can be setup only once and before the first object instantiation.");
            if (string.IsNullOrWhiteSpace(rootName)) throw new ArgumentException("Argument cannot be null, empty or consisting only of white-space characters.", nameof(RootName));
            if (sectionDelimiter == default(char)) throw new ArgumentException("Argument must have a valid value", nameof(sectionDelimiter));

            RootName = rootName;
            SectionDelimiter = sectionDelimiter;
        }

        public bool MoveNext() => GetEnumerator().MoveNext();

        public void Reset() => GetEnumerator().Reset();

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => _settings.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private static bool IsSetupDone() => !string.IsNullOrWhiteSpace(RootName) && SectionDelimiter != default(char);

        public object Current => GetEnumerator().Current;

        public string Name { get; }

        public string FullName()
        {
            var list = new List<string>() { Name };
            var section = this;
            while (section.Parent != null)
            {
                list.Add(section.Parent.Name);
                section = section.Parent;
            }
            list.Reverse();
            return string.Join(SectionDelimiter.ToString(), list.ToArray());
        }

        public object this[string keyPath]
        {
            get
            {
                ResolveKeyPath(keyPath, out ConfigSection section, out string key);
                return section._settings[key];
            }
            set
            {
                ResolveKeyPath(keyPath, out ConfigSection section, out string key);
                section._settings[key] = value;
            }
        }

        private void ResolveKeyPath(string keyPath, out ConfigSection section, out string key)
        {
            var index = keyPath.LastIndexOf(SectionDelimiter);
            if (index == -1)
            {
                section = this;
                key = keyPath;
                return;
            }

            section = Section(keyPath.Substring(0, index));
            key = keyPath.Substring(index + 1);
        }

        public ConfigSection Section(string keyPath)
        {
            if (_cache.ContainsKey(keyPath))
                return _cache[keyPath];

            var section = this;
            foreach (var key in keyPath.Split(SectionDelimiter))
            {
                if (!section._settings.ContainsKey(key))
                    return null;

                section = (ConfigSection)section._settings[key];
            }

            _cache[keyPath] = section;
            return section;
        }

        public ConfigSection Section(string keyPath, bool asCopy)
        {
            var section = Section(keyPath);
            if (section == null)
                return null;

            return asCopy ? section.Copy() : section;
        }

        public ConfigSection Copy()
        {
            return new ConfigSection(Name, _settings.ToDictionary(p => (object)p.Key, p => p.Value));
        }

        private readonly Dictionary<string, ConfigSection> _cache = new Dictionary<string, ConfigSection>(StringComparer.OrdinalIgnoreCase);

        public bool HasSection(string keyPath) => Section(keyPath) != null;

        public bool HasKey(string keyPath)
        {
            ResolveKeyPath(keyPath, out ConfigSection section, out string key);
            return section._settings.ContainsKey(key);
        }

        internal void Merge(ConfigSection other)
        {
            foreach (var item in other)
            {
                if (_settings.ContainsKey(item.Key)
                    && _settings[item.Key] is ConfigSection mainSection
                    && item.Value is ConfigSection otherSection)
                {
                    mainSection.Merge(otherSection);
                    continue;
                }

                _settings[item.Key] = item.Value;
            }
        }
    }
}