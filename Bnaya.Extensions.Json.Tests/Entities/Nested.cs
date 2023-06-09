﻿using System.Collections.Generic;
using System.Linq;

namespace System.Text.Json.Extension.Extensions.Tests
{
    public class Nested : IEquatable<Nested>
    {
        public int Id { get; set; }
        public Dictionary<ConsoleColor, string> Map { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as Nested);
        }

        public bool Equals(Nested other)
        {
            return other != null &&
                   Id == other.Id &&
                   Map.Count == other.Map.Count && other.Map.All(p => Map[p.Key] == p.Value);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Map.Aggregate(0, (a, b) => a ^ b.Key.GetHashCode() ^ b.Value.GetHashCode()));
        }

        public static bool operator ==(Nested left, Nested right)
        {
            return EqualityComparer<Nested>.Default.Equals(left, right);
        }

        public static bool operator !=(Nested left, Nested right)
        {
            return !(left == right);
        }
    }
}
