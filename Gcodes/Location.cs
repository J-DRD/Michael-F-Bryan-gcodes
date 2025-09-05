using System;
using System.Collections.Generic;

namespace Gcodes
{
    public struct Location : IEquatable<Location>
    {
        public Location(int byteIndex, int line, int column)
        {
            ByteIndex = byteIndex;
            Line = line;
            Column = column;
        }

        public int ByteIndex { get; }
        public int Line { get; }
        public int Column { get; }

        public override bool Equals(object obj)
        {
            return obj is Location other && Equals(other);
        }

        public bool Equals(Location other)
        {
            return ByteIndex == other.ByteIndex &&
                   Line == other.Line &&
                   Column == other.Column;
        }

        public override int GetHashCode()
        {
            var hashCode = 1862217691;
            hashCode = hashCode * -1521134295 + ByteIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + Line.GetHashCode();
            hashCode = hashCode * -1521134295 + Column.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Location location1, Location location2)
        {
            return location1.Equals(location2);
        }

        public static bool operator !=(Location location1, Location location2)
        {
            return !(location1 == location2);
        }
    }
}
