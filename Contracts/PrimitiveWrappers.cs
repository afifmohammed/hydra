﻿using System;
using Newtonsoft.Json;

namespace Hydra.Core
{
    public interface Wrapper<out TValue>
    {
        TValue Value { get; }
    }

    public struct JsonContent : Wrapper<string>
    {
        public JsonContent(object obj)
        {
            Value = JsonConvert.SerializeObject(obj);
        }
        public JsonContent(string value)
        {
            Value = value;
        }
        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if ((obj is JsonContent) == false) return false;

            return Equals((JsonContent)obj);
        }

        public bool Equals(JsonContent other)
        {
            return string.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }

    public struct Version : Wrapper<int>
    {
        public Version(int value)
        {
            Value = value;
        }
        public int Value { get; set; }
    }

    public struct TypeContract : Wrapper<string>
    {
        public TypeContract(Type t)
        {
            Value = t.FriendlyName();
        }

        public TypeContract(object t)
        {
            Value = t.GetType().FriendlyName();
        }

        public string Value { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) return false;
            if ((obj is TypeContract) == false) return false;

            return Equals((TypeContract)obj);
        }

        public bool Equals(TypeContract other)
        {
            return string.Equals(Value, other.Value);
        }

        public override int GetHashCode()
        {
            return Value?.GetHashCode() ?? 0;
        }
    }
}
