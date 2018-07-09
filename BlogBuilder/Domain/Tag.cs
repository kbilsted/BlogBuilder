using System;

namespace Kbg.BlogBuilder.Domain
{
    public class Tag
    {
        const int MaxValue_ffffff = 16777215;
        public readonly string DisplayText;

        public readonly string HexCodeForValue;

        public readonly string Value;

        public Tag(string value)
        {
            Value = value;
            DisplayText = Value.Replace('_', ' ');
            var hashCode = Math.Abs(value.GetHashCode());
            var color = hashCode % MaxValue_ffffff;
            HexCodeForValue = color.ToString("x").PadRight(6, '0');
        }

        //public override int GetHashCode()
        //{
        //    return Value.GetHashCode();
        //}

        //public override bool Equals(object obj)
        //{
        //    return string.Equals(Value, ((Tag)obj).Value);
        //}

        public override string ToString()
        {
            return Value;
        }
    }
}