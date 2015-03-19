using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_UnitTests {
    [TestClass]
    public class GeneticTests {

        private int BitCount {
            get { return 8; }
        }

        private int BitLength {
            get { return 10; }
        }

        [TestMethod]
        public void Genetic_Test() {
            for (int i = 0; i <= 1000; i++) {
                Genetic gen = new Genetic();
                gen.Begin(this.BitLength, this.BitCount, i, Fitness);
            }

            Assert.AreEqual(true, true);
        }

        private float Fitness(int[] chromosone, Genetic genetic) {
            float count = 0;
            for (int i = 0; i <= chromosone.GetUpperBound(0); i++) {
                if (chromosone[i] == this.BitCount - 1) {
                    count++;
                }
            }
            if (count == chromosone.GetUpperBound(0) + 1) {
                return genetic.IsSolved;
            } else {
                return count;
            }
        }
    }
}
