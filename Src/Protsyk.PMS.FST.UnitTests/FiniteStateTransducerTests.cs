using System;
using Xunit;

namespace Protsyk.PMS.FST.UnitTests
{
    public class FiniteStateTransducerTests
    {
        [Fact]
        public void AcceptanceTest()
        {
            var inputs = new string[]{
                "Albert Schweitzer Ziekenhuis. Locatie Amstelwijck Heliport",
                "Amsterdam Airfield",
                "Amsterdam Airport",
                "Amsterdam Airport Schiphol",
                "Amsterdam Heliport",
                "Chafei Amsei Airport",
                "New Amsterdam Airport",
                "Schwarzheide/Schipkau Airport"
            };

            var outputs = new int[] {
                43711,
                23465,
                41198,
                2513,
                43207,
                5873,
                41521,
                29065
            };

            var fst = new FSTBuilder<int>(FSTVarIntOutput.Instance).FromList(inputs, outputs);
            Verify(fst, inputs, outputs);

            var fst1 = FST<int>.FromBytes(fst.GetBytes(), FSTVarIntOutput.Instance);
            Verify(fst1, inputs, outputs);
 
            var fst2 = FST<int>.FromBytesCompressed(fst.GetBytesCompressed(), FSTVarIntOutput.Instance);
            Verify(fst2, inputs, outputs);
        }

        [Fact]
        public void WildcardMatchingTest()
        {
            var inputs = new string[]{
                "Albert Schweitzer Ziekenhuis. Locatie Amstelwijck Heliport",
                "Amsterdam Airfield",
                "Amsterdam Airport",
                "Amsterdam Airport Schiphol",
                "Amsterdam Heliport",
                "Chafei Amsei Airport",
                "New Amsterdam Airport",
                "Schwarzheide/Schipkau Airport"
            };

            var outputs = new int[] {
                43711,
                23465,
                41198,
                2513,
                43207,
                5873,
                41521,
                29065
            };

            var fst = new FSTBuilder<int>(FSTVarIntOutput.Instance).FromList(inputs, outputs);
            Verify(fst, inputs, outputs);

            var expectedTerms = new string[]
            {
                "Amsterdam Airport Schiphol",
                "Schwarzheide/Schipkau Airport"
            };

            var expectedOutputs = new int[]
            {
                2513,
                29065
            };

            var expectedIndex = 0;

            Assert.Equal(expectedOutputs.Length, expectedTerms.Length);

            foreach (var term in fst.Match(new WildcardMatcher("*Schip*", 255)))
            {
                Assert.Equal(expectedTerms[expectedIndex], term);
                Assert.True(fst.TryMatch(term, out int value));
                Assert.Equal(expectedOutputs[expectedIndex], value);
                ++expectedIndex;
            }

            Assert.Equal(expectedOutputs.Length, expectedIndex);
        }


         private void Verify<T>(FST<T> fst, string[] inputs, T[] outputs)
         {
             for (int i=0; i<inputs.Length; ++i)
             {
                 Assert.True(fst.TryMatch(inputs[i], out var v));
                 Assert.Equal(outputs[i], v);
             }
         }
    }
}
