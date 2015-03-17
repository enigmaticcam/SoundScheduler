using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Engine {
    public class Genetic {
        public float IsSolved {
            get { return -1; }
        }

        private int _chromosoneCount = 100;
        private Random _random;
        private int _bitLength;
        private int _bitCount;
        private FitnessFunction _fitness;
        private int _chromosoneLength;
        private char[][] _chromosones;
        private char[][] _newChromosones;
        private int[][] _chromosonesAsInt;
        private float[] _ranks;
        private float[] _roulette;
        private char[][] _chromosonePair;
        private Dictionary<char, char> _mutateRef;
        private bool _stop;
        private int _solutionIndex;
        private char[] _bestSoFarChar;
        private float _bestSoFarScore;
        private bool _doLogging = true;
        private HashSet<string> _newChromosonesIndex;
        private int _seed;

        public int[] Begin(int bitLength, int bitCount) {
            return Begin(bitLength, bitCount, -1, RandomFitness);
        }

        public int[] Begin(int bitLength, int bitCount, int seed) {
            return Begin(bitLength, bitCount, seed, RandomFitness);
        }

        public int[] Begin(int bitLength, int bitCount, Func<int[], float> fitness) {
            return Begin(bitLength, bitCount, -1, fitness);
        }

        public int[] Begin(int bitLength, int bitCount, int seed, Func<int[], float> fitness) {
            _bitLength = bitLength;
            _bitCount = bitCount;
            _fitness = new FitnessFunction(fitness);
            _seed = seed;
            Core();
            return ChromosoneToInt(_chromosones[_solutionIndex]);
        }

        private void Core() {
            SetChromosoneLength();
            Instantiate();
            GenerateRandomChromosones();
            BeginGenetics();
        }

        private void Instantiate() {
            GenerateSeed();
            _newChromosonesIndex = new HashSet<string>();
            _ranks = new float[_chromosoneCount];
            _roulette = new float[_chromosoneCount];
            _newChromosones = new char[_chromosoneCount][];
            _chromosonePair = new char[2][];
            _chromosonePair[0] = new char[_chromosoneLength * _bitLength];
            _chromosonePair[1] = new char[_chromosoneLength * _bitLength];
            for (int i = 0; i < _chromosoneCount; i++) {
                _newChromosones[i] = new char[_chromosoneLength * _bitLength];
            }
            _chromosonesAsInt = new int[_chromosoneCount][];
            for (int i = 0; i < _chromosoneCount; i++) {
                _chromosonesAsInt[i] = new int[_bitLength];
            }
            _mutateRef = new Dictionary<char, char>();
            _mutateRef.Add('0', '1');
            _mutateRef.Add('1', '0');
        }

        private void GenerateSeed() {
            if (_seed != -1) {
                _random = new Random(_seed);
            } else {
                _random = new Random();
            }
        }

        private void SetChromosoneLength() {
            int length = 0;
            bool found = false;
            do {
                double powerByTwo = Math.Pow(2, length);
                if (powerByTwo > _bitCount) {
                    string bits = Convert.ToString(Convert.ToInt32(powerByTwo), 2);
                    _chromosoneLength = bits.Length - 1;
                    found = true;
                }
                length++;
            } while (!found);
        }

        private void GenerateRandomChromosones() {
            _chromosones = new char[_chromosoneCount][];
            for (int chromosoneIndex = 0; chromosoneIndex < _chromosoneCount; chromosoneIndex++) {
                _chromosones[chromosoneIndex] = new char[_chromosoneLength * _bitLength];
                for (int bitIndex = 0; bitIndex < _bitLength; bitIndex++) {
                    int bitValue = _random.Next(0, _bitCount);
                    char[] bits = Convert.ToString(bitValue, 2).PadLeft(_chromosoneLength, '0').ToCharArray();
                    ReplaceInChar(ref _chromosones[chromosoneIndex], bits, bitIndex * _chromosoneLength);
                }
            }
        }

        private void BeginGenetics() {
            do {
                RankChromosonesInRoulette();
                if (_stop) {
                    break;
                } else {
                    GenerateNewChromosones();
                    int difference = ReferenceCheck();
                    CopyNewChromosonesToCurrentChromosones();
                }
            } while (true);
        }

        private void RankChromosonesInRoulette() {
            float sum = 0;
            for (int index = 0; index < _chromosoneCount; index++) {
                _chromosonesAsInt[index] = ChromosoneToInt(_chromosones[index]);
                float score = _fitness.GetFitness(_chromosonesAsInt[index]);
                if (score == this.IsSolved) {
                    _stop = true;
                    _solutionIndex = index;
                }
                if (score > _bestSoFarScore) {
                    _bestSoFarScore = score;
                    _bestSoFarChar = (char[])_chromosones[index].Clone();
                }
                _ranks[index] = score;
                sum += score;
            }
            GetRoulette(sum);
        }

        private void GetRoulette(float totalFitnessSum) {
            for (int index = 0; index < _chromosoneCount; index++) {
                _roulette[index] = _ranks[index] / totalFitnessSum;
            }
        }

        private void GenerateNewChromosones() {
            int newChromosoneCount = 0;
            _newChromosonesIndex.Clear();
            do {
                PerformCopyAndMaybeCrossover();
                MutateChromosonePair();
                string chromosone = OutputToOneLine(_chromosonePair[0]);
                if (!_newChromosonesIndex.Contains(chromosone)) {
                    CopyChromosonePairToNewChromosone(0, newChromosoneCount);
                    ++newChromosoneCount;
                    _newChromosonesIndex.Add(chromosone);
                }
            } while (newChromosoneCount < _chromosoneCount);
        }

        private void PerformCopyAndMaybeCrossover() {
            int swapIndex = int.MaxValue;
            int chromosone1 = GetRandomFromRoulette();
            int chromosone2 = GetRandomFromRoulette();
            swapIndex = _random.Next(0, _chromosoneLength * _bitLength);
            for (int index = 0; index < _chromosoneLength * _bitLength; index++) {
                if (index > swapIndex) {
                    _chromosonePair[0][index] = _chromosones[chromosone2][index];
                } else {
                    _chromosonePair[0][index] = _chromosones[chromosone1][index];
                }
            }
        }

        private void MutateChromosonePair() {
            for (int pairIndex = 0; pairIndex < 1; pairIndex++) {
                for (int chromosoneIndex = 0; chromosoneIndex < _bitLength * _chromosoneLength; chromosoneIndex++) {
                    int canMutate = _random.Next(0, 1001);
                    if (canMutate == 1000) {
                        _chromosonePair[pairIndex][chromosoneIndex] = _mutateRef[_chromosonePair[pairIndex][chromosoneIndex]];
                    }
                }
            }
        }

        private int GetRandomFromRoulette() {
            double threshold = _random.NextDouble();
            double sum = 0;
            int index = 0;
            do {
                sum += _roulette[index];
                if (sum >= threshold) {
                    return index;
                }
                index++;
            } while (index < _chromosoneCount - 1);
            return index;
        }

        private void ReplaceInChar(ref char[] source, char[] target, int index) {
            for (int i = 0; i < target.Length; i++) {
                source[index + i] = target[i];
            }
        }

        private float RandomFitness(int[] chromosone) {
            float fitness = _random.Next(1, 101);
            if (fitness == 101) {
                return this.IsSolved;
            } else {
                return 1 / fitness;
            }
        }

        private int[] ChromosoneToInt(char[] chromosone) {
            int[] chromosoneAsInt = new int[_bitLength];
            for (int chromosoneIndex = 0; chromosoneIndex < _bitLength; chromosoneIndex++) {
                char[] chrome = new char[_chromosoneLength];
                for (int i = 0; i < _chromosoneLength; i++) {
                    chrome[i] = chromosone[chromosoneIndex * _chromosoneLength + i];
                }
                chromosoneAsInt[chromosoneIndex] = Convert.ToInt32(new string(chrome), 2);
            }
            return chromosoneAsInt;
        }

        private bool IsChromosoneValid(char[] chromosone) {
            int[] chromosoneAsInt = ChromosoneToInt(chromosone);
            for (int index = 0; index < _bitLength; index++) {
                if (chromosoneAsInt[index] > _bitCount) {
                    return false;
                }
            }
            return true;
        }

        private void CopyNewChromosonesToCurrentChromosones() {
            for (int i = 0; i <= _chromosones.GetUpperBound(0); i++) {
                char[] innerValues = _chromosones[i];
                for (int j = 0; j <= innerValues.GetUpperBound(0); j++) {
                    _chromosones[i][j] = _newChromosones[i][j];
                }
            }
        }

        private void CopyChromosonePairToNewChromosone(int pairIndex, int newChromosoneIndex) {
            for (int i = 0; i <= _newChromosones[newChromosoneIndex].GetUpperBound(0); i++) {
                _newChromosones[newChromosoneIndex][i] = _chromosonePair[pairIndex][i];
            }
        }

        private int ReferenceCheck() {
            int count = 0;
            for (int i = 0; i <= _chromosones.GetUpperBound(0); i++) {
                char[] innerValues = _chromosones[i];
                for (int j = 0; j <= innerValues.GetUpperBound(0); j++) {
                    if (_chromosones[i][j] != _newChromosones[i][j]) {
                        count++;
                    }
                }
            }
            return count;
        }

        private string OutputToOneLine(char[] someChar) {
            return new string(someChar);
        }

        public class FitnessFunction {
            private Func<int[], float> _fitness;

            public float GetFitness(int[] chromosone) {
                return _fitness(chromosone);
            }

            public FitnessFunction(Func<int[], float> fitness) {
                _fitness = fitness;
            }
        }
    }
}
