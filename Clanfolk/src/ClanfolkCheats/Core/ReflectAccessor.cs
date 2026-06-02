using System;
using System.Collections.Generic;
using System.Reflection;

namespace ClanfolkCheats.Core
{
    public static class ReflectAccessor
    {
        private const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private static readonly Dictionary<(Type, string), PropertyInfo?> _props = new();
        private static readonly Dictionary<(Type, string), FieldInfo?> _fields = new();

        public static bool TryGet(object? instance, string member, out object? value)
        {
            value = null;
            if (instance == null) return false;
            var t = instance.GetType();
            var p = GetProp(t, member);
            if (p != null && p.CanRead) { value = p.GetValue(instance); return true; }
            var f = GetField(t, member);
            if (f != null) { value = f.GetValue(instance); return true; }
            return false;
        }

        public static bool TrySet(object? instance, string member, object? value)
        {
            if (instance == null) return false;
            var t = instance.GetType();
            var p = GetProp(t, member);
            if (p != null && p.CanWrite) { p.SetValue(instance, Coerce(value, p.PropertyType)); return true; }
            var f = GetField(t, member);
            if (f != null) { f.SetValue(instance, Coerce(value, f.FieldType)); return true; }
            return false;
        }

        public static long GetInt64(object? instance, string member, long fallback = 0)
            => TryGet(instance, member, out var v) && v != null ? Convert.ToInt64(v) : fallback;

        public static void SetInt64(object? instance, string member, long value)
            => TrySet(instance, member, value);

        public static float GetSingle(object? instance, string member, float fallback = 0)
            => TryGet(instance, member, out var v) && v != null ? Convert.ToSingle(v) : fallback;

        public static void SetSingle(object? instance, string member, float value)
            => TrySet(instance, member, value);

        public static int GetInt32(object? instance, string member, int fallback = 0)
            => TryGet(instance, member, out var v) && v != null ? Convert.ToInt32(v) : fallback;

        public static void SetInt32(object? instance, string member, int value)
            => TrySet(instance, member, value);

        private static object? Coerce(object? value, Type target)
        {
            if (value == null) return null;
            if (target.IsInstanceOfType(value)) return value;
            try { if (value is IConvertible) return Convert.ChangeType(value, Nullable.GetUnderlyingType(target) ?? target); }
            catch { }
            return value;
        }

        private static PropertyInfo? GetProp(Type t, string name)
        {
            var key = (t, name);
            if (!_props.TryGetValue(key, out var p)) { p = t.GetProperty(name, Flags); _props[key] = p; }
            return p;
        }

        private static FieldInfo? GetField(Type t, string name)
        {
            var key = (t, name);
            if (!_fields.TryGetValue(key, out var f)) { f = t.GetField(name, Flags); _fields[key] = f; }
            return f;
        }
    }
}
