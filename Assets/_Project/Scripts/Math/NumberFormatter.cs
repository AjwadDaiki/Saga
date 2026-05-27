using System;
using System.Globalization;
using BreakInfinity;

namespace Saga.Math
{
    /// <summary>
    /// Idle-game style number formatter.
    /// 1234 → "1.23K", 1.5e9 → "1.50B", 1.5e15 → "1.50aa", 1.5e78 → "1.50aab", ...
    /// Falls back to scientific notation past zz (1.5e80+ish) just in case.
    /// </summary>
    public static class NumberFormatter
    {
        // Magnitude index = floor(log10(value) / 3).
        // 0=units, 1=K, 2=M, 3=B, 4=T, 5=aa, 6=bb, ... 17=mm, ...
        private static readonly string[] BaseSuffixes =
        {
            "", "K", "M", "B", "T"
        };

        private const int AlphabeticBase = 5; // index of "aa"
        private const string AlphabetLowercase = "abcdefghijklmnopqrstuvwxyz";

        /// <summary>
        /// Format using fixed-point with the idle suffix convention.
        /// </summary>
        public static string Format(BigDouble value, int decimals = 2)
        {
            var sign = value.Sign();
            if (sign == 0)
            {
                return "0";
            }

            // Handle negatives by flipping sign on output; not expected for currencies but safe.
            var isNegative = sign < 0;
            var abs = isNegative ? -value : value;

            // Under 1000 — no suffix, integer display.
            if (abs < 1000)
            {
                var raw = abs.ToDouble();
                var s = raw.ToString("F0", CultureInfo.InvariantCulture);
                return isNegative ? "-" + s : s;
            }

            // Compute magnitude block (each block = 10^3). Log10 already returns double.
            var log10 = BigDouble.Log10(abs);
            var magnitude = (int)System.Math.Floor(log10 / 3.0);

            // Scaled mantissa for display: abs / 10^(magnitude*3)
            var scaled = abs / BigDouble.Pow(10, magnitude * 3);

            var suffix = GetSuffix(magnitude);
            var fmt = "F" + System.Math.Max(0, decimals).ToString(CultureInfo.InvariantCulture);
            var body = scaled.ToDouble().ToString(fmt, CultureInfo.InvariantCulture);

            return isNegative ? "-" + body + suffix : body + suffix;
        }

        /// <summary>Convenience overload for doubles (rare; primarily for debug tools).</summary>
        public static string Format(double value, int decimals = 2)
        {
            return Format(new BigDouble(value), decimals);
        }

        /// <summary>
        /// Returns the suffix for a given magnitude block (units of 10^3).
        /// 0..4 → "", K, M, B, T
        /// 5..30 → aa, bb, cc, ..., zz
        /// 31..   → ee+exp scientific fallback (signals overflow of alpha range).
        /// </summary>
        public static string GetSuffix(int magnitude)
        {
            if (magnitude < 0) return string.Empty;
            if (magnitude < BaseSuffixes.Length) return BaseSuffixes[magnitude];

            var alphaIndex = magnitude - AlphabeticBase;
            if (alphaIndex >= 0 && alphaIndex < AlphabetLowercase.Length)
            {
                var c = AlphabetLowercase[alphaIndex];
                return new string(c, 2);
            }

            // Past zz — out of designed range; scientific fallback.
            return "e" + (magnitude * 3).ToString(CultureInfo.InvariantCulture);
        }
    }
}
