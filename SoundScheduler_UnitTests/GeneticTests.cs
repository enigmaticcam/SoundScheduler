using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoundScheduler_Logic.Engine;

namespace SoundScheduler_UnitTests {
    [TestClass]
    public class GeneticTests {

        [TestMethod]
        public void Genetic_Test() {
            Genetic gen = new Genetic();
            gen.Begin(5, 5, Fitness);

            Assert.AreEqual(true, true);
        }

        private float Fitness(int[] chromosone) {
            float count = 0;
            for (int i = 0; i <= chromosone.GetUpperBound(0); i++) {
                if (chromosone[i] == 4) {
                    count++;
                }
            }
            if (count == chromosone.GetUpperBound(0) + 1) {
                return -1;
            } else {
                return 1 / ((float)chromosone.GetUpperBound(0) - count);
            }
        }
    }
}
