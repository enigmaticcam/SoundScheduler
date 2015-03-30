﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Engine {
    public class Genetic {
        public float IsSolved {
            get { return -1; }
        }

        private int _generationCount;
        public int GenerationCount {
            get { return _generationCount; }
        }

        private int _chromosomeCount = 500;
        private Random _random;
        private int _bitLength;
        private int _bitCount;
        private FitnessFunction _fitness;
        private int _chromosomeLength;
        private char[][] _chromosomes;
        private char[][] _newchromosomes;
        private int[][] _chromosomesAsInt;
        private float[] _ranks;
        private float[] _roulette;
        private char[][] _chromosomePair;
        private Dictionary<char, char> _mutateRef;
        private bool _stop;
        private int _solutionIndex;
        private char[] _bestSoFarChar;
        private float _bestSoFarScore;
        private HashSet<string> _newchromosomesIndex;
        private int _seed;
        private float[] _elitistScore;
        private int[] _elitistIndex;
        private ImmutableBits _immutableBits;
        private List<ImmutableBitsVector> _immutableBitsVectors = new List<ImmutableBitsVector>();

        public void AddImmutableBits(int index, int bitNumber) {
            _immutableBitsVectors.Add(new ImmutableBitsVector(index, bitNumber));
        }

        public int[] Begin(int bitLength, int bitCount) {
            return Begin(bitLength, bitCount, -1, RandomFitness);
        }

        public int[] Begin(int bitLength, int bitCount, int seed) {
            return Begin(bitLength, bitCount, seed, RandomFitness);
        }

        public int[] Begin(int bitLength, int bitCount, Func<int[], Genetic, float> fitness) {
            return Begin(bitLength, bitCount, -1, fitness);
        }

        public int[] Begin(int bitLength, int bitCount, int seed, Func<int[], Genetic, float> fitness) {
            _bitLength = bitLength;
            _bitCount = bitCount;
            _fitness = new FitnessFunction(fitness);
            _seed = seed;
            Core();
            return chromosomeToInt(_chromosomes[_solutionIndex]);
        }

        private void Core() {
            SetchromosomeLength();
            Instantiate();
            GenerateRandomchromosomes();
            BeginGenetics();
        }

        private void Instantiate() {
            GenerateSeed();
            GenerateImmutableBits();
            _newchromosomesIndex = new HashSet<string>();
            _ranks = new float[_chromosomeCount];
            _roulette = new float[_chromosomeCount];
            _newchromosomes = new char[_chromosomeCount][];
            _chromosomePair = new char[2][];
            _chromosomePair[0] = new char[_chromosomeLength * _bitLength];
            _chromosomePair[1] = new char[_chromosomeLength * _bitLength];
            for (int i = 0; i < _chromosomeCount; i++) {
                _newchromosomes[i] = new char[_chromosomeLength * _bitLength];
            }
            _chromosomesAsInt = new int[_chromosomeCount][];
            for (int i = 0; i < _chromosomeCount; i++) {
                _chromosomesAsInt[i] = new int[_bitLength];
            }
            _mutateRef = new Dictionary<char, char>();
            _mutateRef.Add('0', '1');
            _mutateRef.Add('1', '0');
            _elitistScore = new float[(int)(_chromosomeCount * 0.05)];
            _elitistIndex = new int[(int)(_chromosomeCount * 0.05)];
        }

        private void GenerateSeed() {
            if (_seed != -1) {
                _random = new Random(_seed);
            } else {
                _random = new Random();
            }
        }

        private void GenerateImmutableBits() {
            _immutableBits = new ImmutableBits(_chromosomeLength);
            foreach (ImmutableBitsVector vector in _immutableBitsVectors) {
                _immutableBits.AddImmutableBits(vector.Index, vector.BitNumber);
            }
        }

        private void SetchromosomeLength() {
            int length = 0;
            bool found = false;
            do {
                double powerByTwo = Math.Pow(2, length);
                if (powerByTwo > _bitCount) {
                    string bits = Convert.ToString(Convert.ToInt32(powerByTwo), 2);
                    _chromosomeLength = bits.Length - 1;
                    found = true;
                }
                length++;
            } while (!found);
        }

        private void GenerateRandomchromosomes() {
            _chromosomes = new char[_chromosomeCount][];
            for (int chromosomeIndex = 0; chromosomeIndex < _chromosomeCount; chromosomeIndex++) {
                _chromosomes[chromosomeIndex] = new char[_chromosomeLength * _bitLength];
                for (int bitIndex = 0; bitIndex < _bitLength; bitIndex++) {
                    int bitValue = _random.Next(0, _bitCount);
                    char[] bits = Convert.ToString(bitValue, 2).PadLeft(_chromosomeLength, '0').ToCharArray();
                    ReplaceInChar(ref _chromosomes[chromosomeIndex], bits, bitIndex * _chromosomeLength);
                }
                _immutableBits.TrasmuteImmutableBits(_chromosomes[chromosomeIndex]);
            }
        }

        private void BeginGenetics() {
            _generationCount = 0;
            do {
                RankchromosomesInRoulette();
                if (_stop) {
                    break;
                } else {
                    GenerateNewchromosomes();
                    CopyNewchromosomesToCurrentchromosomes();
                }
                ++_generationCount;
            } while (true);
        }

        private void RankchromosomesInRoulette() {
            float sum = 0;
            for (int index = 0; index < _chromosomeCount; index++) {
                _chromosomesAsInt[index] = chromosomeToInt(_chromosomes[index]);
                float score = _fitness.GetFitness(_chromosomesAsInt[index], this);
                if (score == this.IsSolved) {
                    _stop = true;
                    _solutionIndex = index;
                }
                if (score > _bestSoFarScore) {
                    _bestSoFarScore = score;
                    _bestSoFarChar = (char[])_chromosomes[index].Clone();
                }
                _ranks[index] = score;
                sum += score;
            }
            GetRoulette(sum);
        }

        private void GetRoulette(float totalFitnessSum) {
            for (int index = 0; index < _chromosomeCount; index++) {
                _roulette[index] = _ranks[index] / totalFitnessSum;
                AddToElitist(index);
            }
        }

        private void AddToElitist(int index) {
            if (_elitistScore[0] < _roulette[index]) {
                _elitistScore[0] = _roulette[index];
                _elitistIndex[0] = index;
            }
            if (_elitistScore[1] < _elitistScore[0]) {
                int elitistIndex = 1;
                float tempScore = 0;
                int tempIndex = 0;
                while (elitistIndex <= _elitistIndex.GetUpperBound(0) && _elitistScore[elitistIndex] < _elitistScore[elitistIndex - 1]) {
                    tempScore = _elitistScore[elitistIndex];
                    tempIndex = _elitistIndex[elitistIndex];
                    _elitistScore[elitistIndex] = _elitistScore[elitistIndex - 1];
                    _elitistIndex[elitistIndex] = _elitistIndex[elitistIndex - 1];
                    _elitistScore[elitistIndex - 1] = tempScore;
                    _elitistIndex[elitistIndex - 1] = tempIndex;
                    elitistIndex += 1;
                }
            }
        }

        private void GenerateNewchromosomes() {
            _newchromosomesIndex.Clear();
            int newchromosomeCount = CopyElitistToNewChromosomes();
            do {
                PerformCopyAndMaybeCrossover();
                MutatechromosomePair();
                _immutableBits.TrasmuteImmutableBits(_chromosomePair[0]);
                string chromosome = OutputToOneLine(_chromosomePair[0]);
                if (IschromosomeValid(_chromosomePair[0]) && !_newchromosomesIndex.Contains(chromosome)) {
                    int[] chrome = chromosomeToInt(_chromosomePair[0]);
                    CopychromosomePairToNewchromosome(0, newchromosomeCount);
                    ++newchromosomeCount;
                    _newchromosomesIndex.Add(chromosome);
                }
            } while (newchromosomeCount < _chromosomeCount);
        }

        private int CopyElitistToNewChromosomes() {
            for (int elitistIndex = 0; elitistIndex <= _elitistIndex.GetUpperBound(0); elitistIndex++) {
                for (int bitIndex = 0; bitIndex <= _newchromosomes[elitistIndex].GetUpperBound(0); bitIndex++) {
                    _newchromosomes[elitistIndex][bitIndex] = _chromosomes[_elitistIndex[elitistIndex]][bitIndex];
                }
                _newchromosomesIndex.Add(OutputToOneLine(_newchromosomes[elitistIndex]));
            }
            return _elitistIndex.GetUpperBound(0);
        }

        private void PerformCopyAndMaybeCrossover() {
            int swapIndex = int.MaxValue;
            int chromosome1 = GetRandomFromRoulette();
            int chromosome2 = GetRandomFromRoulette();
            swapIndex = _random.Next(0, _chromosomeLength * _bitLength);
            for (int index = 0; index < _chromosomeLength * _bitLength; index++) {
                if (index > swapIndex) {
                    _chromosomePair[0][index] = _chromosomes[chromosome2][index];
                } else {
                    _chromosomePair[0][index] = _chromosomes[chromosome1][index];
                }
            }
        }

        private void MutatechromosomePair() {
            for (int pairIndex = 0; pairIndex < 1; pairIndex++) {
                for (int chromosomeIndex = 0; chromosomeIndex < _bitLength * _chromosomeLength; chromosomeIndex++) {
                    int canMutate = _random.Next(0, 1001);
                    if (canMutate == 1000) {
                        _chromosomePair[pairIndex][chromosomeIndex] = _mutateRef[_chromosomePair[pairIndex][chromosomeIndex]];
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
            } while (index < _chromosomeCount - 1);
            return index;
        }

        private void ReplaceInChar(ref char[] source, char[] target, int index) {
            for (int i = 0; i < target.Length; i++) {
                source[index + i] = target[i];
            }
        }

        private float RandomFitness(int[] chromosome, Genetic genetic) {
            float fitness = _random.Next(1, 101);
            if (fitness == 101) {
                return genetic.IsSolved;
            } else {
                return 1 / fitness;
            }
        }

        private int[] chromosomeToInt(char[] chromosome) {
            int[] chromosomeAsInt = new int[_bitLength];
            for (int chromosomeIndex = 0; chromosomeIndex < _bitLength; chromosomeIndex++) {
                char[] chrome = new char[_chromosomeLength];
                for (int i = 0; i < _chromosomeLength; i++) {
                    chrome[i] = chromosome[chromosomeIndex * _chromosomeLength + i];
                }
                chromosomeAsInt[chromosomeIndex] = Convert.ToInt32(new string(chrome), 2);
            }
            return chromosomeAsInt;
        }

        private bool IschromosomeValid(char[] chromosome) {
            int[] chromosomeAsInt = chromosomeToInt(chromosome);
            for (int index = 0; index < _bitLength; index++) {
                if (chromosomeAsInt[index] >= _bitCount) {
                    return false;
                }
            }
            return true;
        }

        private void CopyNewchromosomesToCurrentchromosomes() {
            for (int i = 0; i <= _chromosomes.GetUpperBound(0); i++) {
                char[] innerValues = _chromosomes[i];
                for (int j = 0; j <= innerValues.GetUpperBound(0); j++) {
                    _chromosomes[i][j] = _newchromosomes[i][j];
                }
            }
        }

        private void CopychromosomePairToNewchromosome(int pairIndex, int newchromosomeIndex) {
            for (int i = 0; i <= _newchromosomes[newchromosomeIndex].GetUpperBound(0); i++) {
                _newchromosomes[newchromosomeIndex][i] = _chromosomePair[pairIndex][i];
            }
        }

        private int ReferenceCheck() {
            int count = 0;
            for (int i = 0; i <= _chromosomes.GetUpperBound(0); i++) {
                char[] innerValues = _chromosomes[i];
                for (int j = 0; j <= innerValues.GetUpperBound(0); j++) {
                    if (_chromosomes[i][j] != _newchromosomes[i][j]) {
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
            private Func<int[], Genetic, float> _fitness;

            public float GetFitness(int[] chromosome, Genetic genetic) {
                return _fitness(chromosome, genetic);
            }

            public FitnessFunction(Func<int[], Genetic, float> fitness) {
                _fitness = fitness;
            }
        }

        private class ImmutableBitsVector {
            public int Index { get; set; }
            public int BitNumber { get; set; }

            public ImmutableBitsVector(int index, int bitNumber) {
                this.Index = index;
                this.BitNumber = bitNumber;
            }

            public ImmutableBitsVector() {

            }
        }

        private class ImmutableBits {
            private int _bitLength;
            private Dictionary<int, char> _immutableBits = new Dictionary<int, char>();

            public void AddImmutableBits(int index, int bitNumber) {
                string bitNumberAsBits = Convert.ToString(bitNumber, 2);
                bitNumberAsBits = AppendZeros(bitNumberAsBits);
                char[] bits = bitNumberAsBits.ToCharArray();
                for (int bitIndex = 0; bitIndex <= bits.GetUpperBound(0); bitIndex++) {
                    _immutableBits.Add(index * _bitLength + bitIndex, bits[bitIndex]);
                }
            }

            private string AppendZeros(string bitNumberAsBits) {
                while (bitNumberAsBits.Length < _bitLength) {
                    bitNumberAsBits = "0" + bitNumberAsBits;
                }
                return bitNumberAsBits;
            }

            public void TrasmuteImmutableBits(char[] bits) {
                foreach (int bitIndex in _immutableBits.Keys) {
                    bits[bitIndex] = _immutableBits[bitIndex];
                }
            }

            public ImmutableBits(int bitLength) {
                _bitLength = bitLength;
            }
        }
    }
}
