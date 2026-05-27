using BreakInfinity;
using NUnit.Framework;
using Saga.Math;

namespace Saga.Tests
{
    [TestFixture]
    public class NumberFormatterTests
    {
        [Test]
        public void Zero_Returns_Plain_Zero()
        {
            Assert.AreEqual("0", NumberFormatter.Format(new BigDouble(0)));
        }

        [TestCase(1.0, "1")]
        [TestCase(42.0, "42")]
        [TestCase(999.0, "999")]
        public void Under_Thousand_Has_No_Suffix(double value, string expected)
        {
            Assert.AreEqual(expected, NumberFormatter.Format(new BigDouble(value)));
        }

        [TestCase(1000.0, "1.00K")]
        [TestCase(1234.0, "1.23K")]
        [TestCase(12345.0, "12.34K")]
        [TestCase(999999.0, "999.99K")]
        public void Thousands_Use_K_Suffix(double value, string expected)
        {
            Assert.AreEqual(expected, NumberFormatter.Format(new BigDouble(value)));
        }

        [TestCase(1_000_000.0, "1.00M")]
        [TestCase(5_500_000.0, "5.50M")]
        public void Millions_Use_M_Suffix(double value, string expected)
        {
            Assert.AreEqual(expected, NumberFormatter.Format(new BigDouble(value)));
        }

        [TestCase(1e9, "1.00B")]
        [TestCase(1e12, "1.00T")]
        public void Billions_And_Trillions_Use_B_And_T(double value, string expected)
        {
            Assert.AreEqual(expected, NumberFormatter.Format(new BigDouble(value)));
        }

        [Test]
        public void Quadrillions_Switch_To_aa_Suffix()
        {
            // 1e15 = 1 quadrillion → "1.00aa"
            Assert.AreEqual("1.00aa", NumberFormatter.Format(new BigDouble(1e15)));
        }

        [Test]
        public void Quintillions_Use_bb_Suffix()
        {
            // 1e18 → "1.00bb"
            Assert.AreEqual("1.00bb", NumberFormatter.Format(new BigDouble(1e18)));
        }

        [Test]
        public void Sextillions_Use_cc_Suffix()
        {
            Assert.AreEqual("1.00cc", NumberFormatter.Format(new BigDouble(1e21)));
        }

        [Test]
        public void Magnitude_30_Use_zz_Suffix()
        {
            // BaseSuffix range covers 0..4 (units..T), then aa=5, bb=6, ..., zz=30.
            // 10^(3*30) = 1e90 → "zz"
            var value = BigDouble.Pow(10, 90);
            Assert.AreEqual("1.00zz", NumberFormatter.Format(value));
        }

        [Test]
        public void Beyond_zz_Falls_Back_To_Scientific()
        {
            // 10^(3*31) = 1e93 → fallback "e93"
            var value = BigDouble.Pow(10, 93);
            Assert.AreEqual("1.00e93", NumberFormatter.Format(value));
        }

        [Test]
        public void Negative_Values_Prepend_Minus()
        {
            Assert.AreEqual("-1.23K", NumberFormatter.Format(new BigDouble(-1234)));
        }

        [Test]
        public void Decimals_Parameter_Controls_Precision()
        {
            Assert.AreEqual("1.235K", NumberFormatter.Format(new BigDouble(1234.5), decimals: 3));
            Assert.AreEqual("1K", NumberFormatter.Format(new BigDouble(1234.5), decimals: 0));
        }

        [Test]
        public void GetSuffix_Maps_Magnitudes_Correctly()
        {
            Assert.AreEqual("", NumberFormatter.GetSuffix(0));
            Assert.AreEqual("K", NumberFormatter.GetSuffix(1));
            Assert.AreEqual("M", NumberFormatter.GetSuffix(2));
            Assert.AreEqual("B", NumberFormatter.GetSuffix(3));
            Assert.AreEqual("T", NumberFormatter.GetSuffix(4));
            Assert.AreEqual("aa", NumberFormatter.GetSuffix(5));
            Assert.AreEqual("bb", NumberFormatter.GetSuffix(6));
            Assert.AreEqual("zz", NumberFormatter.GetSuffix(30));
        }
    }
}
