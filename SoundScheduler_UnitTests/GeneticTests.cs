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

        //[TestMethod]
        public void Genetic_Test() {
            for (int i = 0; i <= 1000; i++) {
                Genetic gen = new Genetic();
                gen.Begin(this.BitLength, this.BitCount, i, Fitness1);
            }

            Assert.AreEqual(true, true);
        }

        //[TestMethod]
        public void Genetic_DoesMaintainImmutableBits() {
            for (int i = 0; i <= 1000; i++) {
                Genetic gen = new Genetic();
                gen.AddImmutableBits(3, 1);
                gen.AddImmutableBits(0, 2);
                gen.AddImmutableBits(7, 3);

                gen.Begin(8, 8, Fitness2);
            }

            Assert.AreEqual(true, true);
        }

        private float Fitness1(int[] chromosone, Genetic genetic) {
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

        private float Fitness2(int[] chromosome, Genetic genetic) {
            float count = 0;
            if (chromosome[3] != 1 || chromosome[0] != 2 || chromosome[7] != 3) {
                throw new Exception("Bad combination found");
            } else {
                for (int i = 0; i <= chromosome.GetUpperBound(0); i++) {
                    if (i != 3 && i != 0 && i != 7) {
                        if (chromosome[i] == i) {
                            count++;
                        }
                    }
                }
            }
            if (count == 5) {
                return genetic.IsSolved;
            } else {
                return count;
            }
        }
    }
}
