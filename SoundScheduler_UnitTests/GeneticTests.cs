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
            get { return 40; }
        }

        [TestMethod]
        public void Genetic_Test() {
            Genetic gen = new Genetic();
            gen.Begin(this.BitLength, this.BitCount, Fitness);

            Assert.AreEqual(true, true);
        }

        private float Fitness(int[] chromosone) {
            float count = 0;
            for (int i = 0; i <= chromosone.GetUpperBound(0); i++) {
                if (chromosone[i] == this.BitCount - 1) {
                    count++;
                }
            }
            //if (count == chromosone.GetUpperBound(0) + 1) {
            //    return -1;
            //} else {
            //    float divideBy = (float)chromosone.GetUpperBound(0) - count + 1;
            //    if (divideBy == 0) {
            //        throw new Exception("this shouldn't happen dude");
            //    }
            //    return 1 / divideBy;
            //}
            if (count == chromosone.GetUpperBound(0) + 1) {
                return -1;
            } else {
                return count;
            }
        }
    }
}
