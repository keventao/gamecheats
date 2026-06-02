using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using MelonLoader;

namespace ClanfolkCheats.Util
{
    public class ModConfig
    {
        private readonly string _path;
        private Dictionary<string, JsonElement> _data = new();

        public ModConfig(string filePath)
        {
            _path = filePath;
            Load();
        }

        public string GetString(string key, string fallback = "")
        {
            if (_data.TryGetValue(key, out var e) && e.ValueKind == JsonValueKind.String)
                return e.GetString() ?? fallback;
            return fallback;
        }

        public int GetInt(string key, int fallback = 0)
        {
            if (_data.TryGetValue(key, out var e) && e.ValueKind == JsonValueKind.Number && e.TryGetInt32(out var v))
                return v;
            return fallback;
        }

        public float GetFloat(string key, float fallback = 0f)
        {
            if (_data.TryGetValue(key, out var e) && e.ValueKind == JsonValueKind.Number && e.TryGetSingle(out var v))
                return v;
            return fallback;
        }

        public bool GetBool(string key, bool fallback = false)
        {
            if (_data.TryGetValue(key, out var e) && (e.ValueKind == JsonValueKind.True || e.ValueKind == JsonValueKind.False))
                return e.GetBoolean();
            return fallback;
        }

        public void Set(string key, string value) => Put(key, value);
        public void Set(string key, int value) => Put(key, value);
        public void Set(string key, float value) => Put(key, value);
        public void Set(string key, bool value) => Put(key, value);

        public void Save()
        {
            try
            {
                var dir = Path.GetDirectoryName(_path);
                if (dir != null) Directory.CreateDirectory(dir);
                var json = JsonSerializer.Serialize(_data, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_path, json);
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[ModConfig] Save failed: {ex.Message}");
            }
        }

        private void Load()
        {
            try
            {
                if (File.Exists(_path))
                {
                    var json = File.ReadAllText(_path);
                    _data = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new();
                }
            }
            catch (Exception ex)
            {
                MelonLogger.Warning($"[ModConfig] Load failed: {ex.Message}");
                _data = new();
            }
        }

        private void Put(string key, object value)
        {
            var json = JsonSerializer.Serialize(value);
            _data[key] = JsonSerializer.Deserialize<JsonElement>(json);
        }
    }
}
