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
            var hashCode = Math.Abs(GetDeterministicHashCode(value));
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

        int GetDeterministicHashCode(string str) {
            unchecked {
                int hash1 = (5381 << 16) + 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length; i += 2) {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1)
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}