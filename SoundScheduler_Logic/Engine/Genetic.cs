﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SoundScheduler_Logic.Engine {
    public class Genetic {
        private enum ThreadJob {
            RunFitnessScores = 0,
            CopyNewChromomesToCurrent,
            CreateNewChromosomes,
            Wait
        }

        public float ScoreSolveValue { get; set; }
        public float IsSolved {
            get { return -1; }
        }

        private int _generationCount;
        public int GenerationCount {
            get { return _generationCount; }
        }

        private int _chromosomeCount = 1000;
        public int ChromosomeCount {
            get { return _chromosomeCount; }
            set { _chromosomeCount = value; }
        }

        private int _mutationRate = 250;
        public int MutationRate {
            get { return _mutationRate; }
            set { _mutationRate = value; }
        }

        private int _threadCount = Environment.ProcessorCount;
        public int ThreadCount {
            get { return _threadCount; }
            set { _threadCount = value; }
        }

        private string _startingSolution;
        public string StartingSolution {
            get { return _startingSolution; }
            set { _startingSolution = value; }
        }

        private int _mutationRateDiff;
        private int _bitLength;
        private int _bitCount;
        private FitnessFunction _fitness;
        private int[][] _chromosomes;
        private int[][] _newchromosomes;
        private float[] _ranks;
        private float[] _roulette;
        private Dictionary<char, char> _mutateRef;
        private bool _stop;
        private int _solutionIndex;
        private int[] _bestSoFarChar;
        private float _bestSoFarScore;
        private string[] _chromosomesAsString;
        private int _seed;
        private float[] _elitistScore;
        private int[] _elitistIndex;
        private ImmutableBits _immutableBits;
        private List<ImmutableBitsVector> _immutableBitsVectors = new List<ImmutableBitsVector>();
        private int[] _solution;
        private ResultsFunction _results;
        private FinishAction _finish;
        private System.Timers.Timer _timer;
        private GeneticResults _lastResults;

        private ThreadData _threadData;
        private int _threadMain;
        static readonly object _object = new object();

        public void AddImmutableBits(int index, int bitNumber) {
            _immutableBitsVectors.Add(new ImmutableBitsVector(index, bitNumber));
        }

        public void BeginAsync(int bitLength, int bitCount, Func<int[], Genetic, float> fitness, Func<GeneticResults, bool> results, Action<int[]> finish) {
            _results = new ResultsFunction(results);
            _finish = new FinishAction(finish);
            _bitLength = bitLength;
            _bitCount = bitCount;
            _fitness = new FitnessFunction(fitness);
            _seed = -1;
            Thread thread = new Thread(new ThreadStart(DoAsync));
            thread.Start();
        }

        private void DoAsync() {
            Core();
            _solution = _chromosomes[_solutionIndex];
            _timer.Stop();
            _finish.Finish(_solution);
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
            return _chromosomes[_solutionIndex];
        }

        private void Core() {
            Instantiate();
            InstantiateThreads();
            GenerateRandomchromosomes();
            SetStartingSolution();
            BeginGenetics();
        }

        private void Instantiate() {
            GenerateImmutableBits();
            _ranks = new float[_chromosomeCount];
            _roulette = new float[_chromosomeCount];
            _newchromosomes = new int[_chromosomeCount][];
            _chromosomesAsString = new string[_chromosomeCount];
            for (int i = 0; i < _chromosomeCount; i++) {
                _newchromosomes[i] = new int[_bitLength];
            }
            _mutateRef = new Dictionary<char, char>();
            _mutateRef.Add('0', '1');
            _mutateRef.Add('1', '0');
            _elitistScore = new float[(int)(_chromosomeCount * 0.05)];
            _elitistIndex = new int[(int)(_chromosomeCount * 0.05)];
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(TimerElapsed);
            _timer.Enabled = true;
            _mutationRateDiff = _mutationRate + 1 - _bitLength;
        }

        private void InstantiateThreads() {
            _threadData = new ThreadData.Builder()
                .SetThreadCount(_threadCount)
                .SetThreadDoWork(ThreadDoWork)
                .SetChromosomeLength(_bitLength)
                .SetRandomSeed(_seed)
                .Build();
            InstantiateThreadRanges();
            _threadData.StartThreads();
        }

        private void InstantiateThreadRanges() {
            int threadRangeCount = _chromosomeCount / (_threadData.ThreadCount);
            for (int i = 0; i < _threadData.ThreadCount; i++) {
                int threadStart = i * threadRangeCount;
                int threadStartElitist = threadStart;
                int threadEnd = ((i + 1) * threadRangeCount) - 1;
                if (i == _threadData.ThreadCount - 1) {
                    threadEnd = _chromosomeCount - 1;
                }
                if (i == 0) {
                    threadStartElitist = (int)(_chromosomeCount * 0.05) + 1;
                }
                _threadData.AddThreadRange(_threadData.Threads.ElementAt(i).ManagedThreadId, new ThreadRange(threadStart, threadStartElitist, threadEnd));
            }
        }

        private void ThreadDoWork() {
            ThreadDoWork(false);
        }

        private void ThreadDoWork(bool runOnce) {
            do {
                switch (_threadData.ThreadJob) {
                    case ThreadJob.RunFitnessScores:
                        RankChromosomesInRoulette_Thread();
                        _threadData.ThreadJob = ThreadJob.Wait;
                        break;
                    case ThreadJob.CopyNewChromomesToCurrent:
                        CopyNewChromosomesToCurrentChromomes_Thread();
                        _threadData.ThreadJob = ThreadJob.Wait;
                        break;
                    case ThreadJob.CreateNewChromosomes:
                        GenerateNewChromosome_Threaded();
                        _threadData.ThreadJob = ThreadJob.Wait;
                        break;
                }
            } while (!_threadData.ThreadsEnd && !runOnce);
        }

        private void RunThreads(ThreadJob job) {
            foreach (Thread thread in _threadData.Threads) {
                _threadData.SetThreadJob(thread.ManagedThreadId, job);
            }
            _threadMain = Thread.CurrentThread.ManagedThreadId;
            ThreadDoWork(true);
            bool isFinished = false;
            do {
                isFinished = true;
                foreach (Thread thread in _threadData.Threads) {
                    if (_threadData.GetThreadJob(thread.ManagedThreadId) != ThreadJob.Wait) {
                        isFinished = false;
                    }
                }
            } while (isFinished == false);
        }

        private void EndThreads() {
            _threadData.ThreadsEnd = true;
        }

        private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e) {
            int generationsPerSecond = _generationCount;
            if (_lastResults != null) {
                generationsPerSecond = _generationCount - _lastResults.GenerationCount;
            }
            _lastResults = new GeneticResults.Builder()
                .SetBestSolutionSoFarScore((_bestSoFarScore / this.ScoreSolveValue) * 100)
                .SetBestSolutionSoFarSolution(_bestSoFarChar)
                .SetGenerationCount(_generationCount)
                .SetGenerationsPerSecond(generationsPerSecond)
                .SetCPUCoreCount(_threadCount)
                .Build();
            if (!_results.Results(_lastResults)) {
                _stop = true;
            }
        }

        private void GenerateImmutableBits() {
            _immutableBits = new ImmutableBits(_bitLength);
            foreach (ImmutableBitsVector vector in _immutableBitsVectors) {
                _immutableBits.AddImmutableBits(vector.Index, vector.BitNumber);
            }
        }

        private void GenerateRandomchromosomes() {
            _chromosomes = new int[_chromosomeCount][];
            for (int chromosomeIndex = 0; chromosomeIndex < _chromosomeCount; chromosomeIndex++) {
                _chromosomes[chromosomeIndex] = new int[_bitLength];
                for (int bitIndex = 0; bitIndex < _bitLength; bitIndex++) {
                    _chromosomes[chromosomeIndex][bitIndex] = _threadData.GetRandom.Next(0, _bitCount);
                }
                _immutableBits.TrasmuteImmutableBits(_chromosomes[chromosomeIndex]);
                _chromosomesAsString[chromosomeIndex] = OutputToOneLine(_chromosomes[chromosomeIndex]);
            }
        }

        private void SetStartingSolution() {
            if (!string.IsNullOrEmpty(_startingSolution)) {
                //_chromosomes[0] = _startingSolution.Split(',').Cast<int>().ToArray();
                _chromosomes[0] = _startingSolution.Split(',').Select(x => Convert.ToInt32(x)).ToArray();
            }
        }

        private void BeginGenetics() {
            _generationCount = 0;
            do {
                RunThreads(ThreadJob.RunFitnessScores);
                RankchromosomesInRoulette();
                if (_stop) {
                    break;
                } else {
                    _threadData.ClearUniqueChromosomes();
                    CopyElitistToNewChromosomes();
                    RunThreads(ThreadJob.CreateNewChromosomes);
                    RunThreads(ThreadJob.CopyNewChromomesToCurrent);
                }
                ++_generationCount;
            } while (true);
            EndThreads();
        }

        private void RankChromosomesInRoulette_Thread() {
            ThreadRange range = _threadData.Range;
            float sum = 0;
            for (int index = range.Start; index <= range.Stop; index++) {
                if (_threadData.HasPreviousScore(_chromosomesAsString[index])) {
                    _ranks[index] = _threadData.GetPreviousScore(_chromosomesAsString[index]);
                } else {
                    _ranks[index] = _fitness.GetFitness(_chromosomes[index], this);
                    _threadData.AddPreviousScore(_chromosomesAsString[index], _ranks[index]);
                }
                sum += _ranks[index];
            }
            _threadData.ScoreSum = sum;
        }

        private void RankchromosomesInRoulette() {
            float totalFitnessSum = _threadData.ScoreSumAllThreads;
            for (int index = 0; index < _chromosomeCount; index++) {
                if (_ranks[index] == this.IsSolved) {
                    _stop = true;
                    _solutionIndex = index;
                }
                if (_ranks[index] > _bestSoFarScore) {
                    _bestSoFarScore = _ranks[index];
                    _bestSoFarChar = (int[])_chromosomes[index].Clone();
                }
                AddToElitist(index);
                _roulette[index] = _ranks[index] / totalFitnessSum;
            }
        }

        private void AddToElitist(int index) {
            if (_elitistScore[0] < _ranks[index]) {
                _elitistScore[0] = _ranks[index];
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

        private void CopyElitistToNewChromosomes() {
            for (int elitistIndex = 0; elitistIndex <= _elitistIndex.GetUpperBound(0); elitistIndex++) {
                for (int bitIndex = 0; bitIndex <= _newchromosomes[elitistIndex].GetUpperBound(0); bitIndex++) {
                    _newchromosomes[elitistIndex][bitIndex] = _chromosomes[_elitistIndex[elitistIndex]][bitIndex];
                }
                _threadData.AddUniqueChromosomeToAllThreads(OutputToOneLine(_newchromosomes[elitistIndex]));
                _elitistIndex[elitistIndex] = elitistIndex;
            }
        }

        private void GenerateNewChromosome_Threaded() {
            ThreadRange threadRange = _threadData.Range;
            int count = threadRange.StartAfterElitist;
            do {
                int[] newChromosome = _threadData.CurrentChromosome;
                PerformCopyAndCrossover(newChromosome);
                MutatechromosomePair(newChromosome);
                _immutableBits.TrasmuteImmutableBits(newChromosome);
                string newChromsomeAsString = OutputToOneLine(newChromosome);
                if (!_threadData.HasUniqueChromosome(newChromsomeAsString)) {
                    CopychromosomePairToNewchromosome(count);
                    _chromosomesAsString[count] = newChromsomeAsString;
                    count++;
                    _threadData.AddUniqueChromosome(newChromsomeAsString);
                }

            } while (count <= threadRange.Stop);
        }
        
        private void PerformCopyAndCrossover(int[] currentChromosome) {
            int oneOrTwo = _threadData.GetRandom.Next(0, 2);
            if (oneOrTwo == 1) {
                PerformCopyAndCrossoverOneSwap(currentChromosome);
            } else {
                PerformCopyAndCrossoverTwoSwap(currentChromosome);
            }
        }

        private void PerformCopyAndCrossoverOneSwap(int[] currentChromosome) {
            int swapIndex = int.MaxValue;
            int chromosome1 = GetRandomFromRoulette();
            int chromosome2 = GetRandomFromRoulette();
            swapIndex = _threadData.GetRandom.Next(0, _bitLength);
            for (int index = 0; index < _bitLength; index++) {
                if (index > swapIndex) {
                    currentChromosome[index] = _chromosomes[chromosome2][index];
                } else {
                    currentChromosome[index] = _chromosomes[chromosome1][index];
                }
            }
        }

        private void PerformCopyAndCrossoverTwoSwap(int[] currentChromosome) {
            int swapIndex1 = int.MaxValue;
            int swapIndex2 = int.MaxValue;
            int chromosome1 = GetRandomFromRoulette();
            int chromosome2 = GetRandomFromRoulette();
            swapIndex1 = _threadData.GetRandom.Next(0, _bitLength - 1);
            swapIndex2 = _threadData.GetRandom.Next(swapIndex1 + 1, _bitLength);
            for (int index = 0; index < _bitLength; index++) {
                if (index >= swapIndex1 & index <= swapIndex2) {
                    currentChromosome[index] = _chromosomes[chromosome2][index];
                } else {
                    currentChromosome[index] = _chromosomes[chromosome1][index];
                }
            }
        }

        private void MutatechromosomePair(int[] currentChromosome) {
            for (int chromosomeIndex = 0; chromosomeIndex < _bitLength; chromosomeIndex++) {
                int canMutate = _threadData.GetRandom.Next(0, _mutationRate + 1);
                if (canMutate == _mutationRate) {
                    int mutateCount = _threadData.GetRandom.Next(1, 3);
                    while (mutateCount > 0) {
                        int mutateTo = _threadData.GetRandom.Next(0, _bitCount);
                        currentChromosome[chromosomeIndex] = mutateTo;
                        mutateCount--;
                    }
                }
            }
        }

        private List<int> Order(HashSet<int> data) {
            List<int> list = data.ToList();
            list.Sort();
            return list;
        }

        private int GetRandomFromRoulette() {
            double threshold = _threadData.GetRandom.NextDouble();
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

        private void ReplaceInArray(ref int[] source, int[] target, int index) {
            for (int i = 0; i < target.Length; i++) {
                source[index + i] = target[i];
            }
        }

        private float RandomFitness(int[] chromosome, Genetic genetic) {
            float fitness = _threadData.GetRandom.Next(1, 101);
            if (fitness == 101) {
                return genetic.IsSolved;
            } else {
                return 1 / fitness;
            }
        }

        private void CopyNewchromosomesToCurrentchromosomes() {
            for (int i = 0; i <= _chromosomes.GetUpperBound(0); i++) {
                int[] innerValues = _chromosomes[i];
                for (int j = 0; j <= innerValues.GetUpperBound(0); j++) {
                    _chromosomes[i][j] = _newchromosomes[i][j];
                }
            }
        }

        private void CopyNewChromosomesToCurrentChromomes_Thread() {
            ThreadRange range = _threadData.Range;
            for (int index = range.Start; index <= range.Stop; index++) {
                int[] innerValues = _chromosomes[index];
                for (int j = 0; j <= innerValues.GetUpperBound(0); j++) {
                    _chromosomes[index][j] = _newchromosomes[index][j];
                }
            }
        }

        private void CopychromosomePairToNewchromosome(int newchromosomeIndex) {
            int[] currentChromosome = _threadData.CurrentChromosome;
            for (int i = 0; i <= _newchromosomes[newchromosomeIndex].GetUpperBound(0); i++) {
                _newchromosomes[newchromosomeIndex][i] = currentChromosome[i];
            }
        }

        private int ReferenceCheck() {
            int count = 0;
            for (int i = 0; i <= _chromosomes.GetUpperBound(0); i++) {
                int[] innerValues = _chromosomes[i];
                for (int j = 0; j <= innerValues.GetUpperBound(0); j++) {
                    if (_chromosomes[i][j] != _newchromosomes[i][j]) {
                        count++;
                    }
                }
            }
            return count;
        }

        private string OutputToOneLine(int[] someInt) {
            return string.Join("", someInt);
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
        
        public class ResultsFunction {
            private Func<GeneticResults, bool> _results;

            public bool Results(GeneticResults results) {
                return _results(results);
            }

            private bool Default(GeneticResults results) {
                return true;
            }

            public ResultsFunction(Func<GeneticResults, bool> results) {
                if (results == null) {
                    _results = Default;
                } else {
                    _results = results;
                }
            }
        }

        public class FinishAction {
            private Action<int[]> _finish;

            public void Finish(int[] solution) {
                _finish(solution);
            }

            public FinishAction(Action<int[]> finish) {
                _finish = finish;
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
            private Dictionary<int, int> _immutableBits = new Dictionary<int, int>();

            public void AddImmutableBits(int index, int bitNumber) {
                _immutableBits.Add(index, bitNumber);
            }

            public void TrasmuteImmutableBits(int[] bits) {
                foreach (int bitIndex in _immutableBits.Keys) {
                    bits[bitIndex] = _immutableBits[bitIndex];
                }
            }

            public ImmutableBits(int bitLength) {
                _bitLength = bitLength;
            }
        }

        public class GeneticResults {
            private int[] _bestSolutionSoFarSolution;
            public int[] BestSolutionSoFarSolution {
                get { return _bestSolutionSoFarSolution; }
            }

            private float _bestSolutionSoFarScore;
            public float BestSolutionSoFarScore {
                get { return _bestSolutionSoFarScore; }
            }

            private int _generationCount;
            public int GenerationCount {
                get { return _generationCount; }
            }

            private int _generationsPerSecond;
            public int GenerationsPerSecond {
                get { return _generationsPerSecond; }
            }

            private int _cpuCoreCount;
            public int CPUCoreCount {
                get { return _cpuCoreCount; }
            }

            public GeneticResults(Builder builder) {
                _bestSolutionSoFarSolution = builder.BestSolutionSoFarSolution;
                _bestSolutionSoFarScore = builder.BestSolutionSoFarScore;
                _generationCount = builder.GenerationCount;
                _generationsPerSecond = builder.GenerationsPerSecond;
                _cpuCoreCount = builder.CPUCoreCount;
            }

            public class Builder {
                public int[] BestSolutionSoFarSolution;
                public float BestSolutionSoFarScore;
                public int GenerationCount;
                public int GenerationsPerSecond;
                public int CPUCoreCount;

                public Builder SetBestSolutionSoFarSolution(int[] bestSolutionSoFarSolution) {
                    this.BestSolutionSoFarSolution = bestSolutionSoFarSolution;
                    return this;
                }

                public Builder SetBestSolutionSoFarScore(float bestSolutionSoFarScore) {
                    this.BestSolutionSoFarScore = bestSolutionSoFarScore;
                    return this;
                }

                public Builder SetGenerationCount(int generationCount) {
                    this.GenerationCount = generationCount;
                    return this;
                }

                public Builder SetGenerationsPerSecond(int generationsPerSecond) {
                    this.GenerationsPerSecond = generationsPerSecond;
                    return this;
                }

                public Builder SetCPUCoreCount(int cpuCoreCount) {
                    this.CPUCoreCount = cpuCoreCount;
                    return this;
                }

                public GeneticResults Build() {
                    return new GeneticResults(this);
                }
            }
        }

        private class ThreadRange {
            private int _start;
            public int Start {
                get { return _start; }
            }

            private int _stop;
            public int Stop {
                get { return _stop; }
            }

            private int _startAfterElitist;
            public int StartAfterElitist {
                get { return _startAfterElitist; }
            }

            public ThreadRange(int start, int startAfterElitist, int stop) {
                _start = start;
                _stop = stop;
                _startAfterElitist = startAfterElitist;
            }
        }

        private class ThreadData {
            private int _previousScoreCount = 500000;
            private int _previousScoreCountPerThread;
            private int _chromosomeLength;
            private int _randomSeed;
            private List<Thread> _threads;
            private Action _threadDoWork;
            private Dictionary<int, Dictionary<string, float>> _previousScores = new Dictionary<int,Dictionary<string,float>>();
            private Dictionary<int, HashSet<string>> _uniqueChromosomes = new Dictionary<int, HashSet<string>>();

            public bool ThreadsEnd { get; set; }

            public IEnumerable<Thread> Threads {
                get { return _threads; }
            }

            private Dictionary<int, ThreadJob> _threadJobs = new Dictionary<int, ThreadJob>();
            public ThreadJob ThreadJob {
                get { return _threadJobs[Thread.CurrentThread.ManagedThreadId]; }
                set { _threadJobs[Thread.CurrentThread.ManagedThreadId] = value; }
            }

            private Dictionary<int, ThreadRange> _threadRanges = new Dictionary<int, ThreadRange>();
            public ThreadRange Range {
                get { return _threadRanges[Thread.CurrentThread.ManagedThreadId]; }
            }

            private Dictionary<int, int[]> _currentChromosome = new Dictionary<int, int[]>();
            public int[] CurrentChromosome {
                get { return _currentChromosome[Thread.CurrentThread.ManagedThreadId]; }
            }

            private int _threadCount;
            public int ThreadCount {
                get { return _threadCount; }
            }

            private Dictionary<int, Random> _random = new Dictionary<int, Random>();
            public Random GetRandom {
                get { return _random[Thread.CurrentThread.ManagedThreadId]; }
            }

            private Dictionary<int, float> _scoreSum = new Dictionary<int, float>();
            public float ScoreSum {
                get { return _scoreSum[Thread.CurrentThread.ManagedThreadId]; }
                set { _scoreSum[Thread.CurrentThread.ManagedThreadId] = value; }
            }

            public float ScoreSumAllThreads {
                get {
                    float sum = 0;
                    foreach (Thread thread in this.Threads) {
                        sum += _scoreSum[thread.ManagedThreadId];
                    }
                    return sum;
                }
            }

            public void AddThreadRange(int threadId, ThreadRange threadRange) {
                _threadRanges.Add(threadId, threadRange);
            }

            public void SetThreadJob(int threadId, ThreadJob job) {
                _threadJobs[threadId] = job;
            }

            public void AddPreviousScore(string key, float score) {
                Dictionary<string, float> previousScore = _previousScores[Thread.CurrentThread.ManagedThreadId];
                if (previousScore.Count == _previousScoreCountPerThread) {
                    previousScore.Remove(previousScore.Keys.ElementAt(0));
                }
                previousScore.Add(key, score);
            }

            public bool HasPreviousScore(string key) {
                return _previousScores[Thread.CurrentThread.ManagedThreadId].ContainsKey(key);
            }

            public float GetPreviousScore(string key) {
                return _previousScores[Thread.CurrentThread.ManagedThreadId][key];
            }

            public void ClearUniqueChromosomes() {
                foreach (Thread thread in this.Threads) {
                    _uniqueChromosomes[thread.ManagedThreadId].Clear();
                }
            }

            public bool HasUniqueChromosome(string chromosome) {
                return _uniqueChromosomes[Thread.CurrentThread.ManagedThreadId].Contains(chromosome);
            }

            public void AddUniqueChromosome(string chromosome) {
                _uniqueChromosomes[Thread.CurrentThread.ManagedThreadId].Add(chromosome);
            }

            public void AddUniqueChromosomeToAllThreads(string chromosome) {
                foreach (Thread thread in this.Threads) {
                    _uniqueChromosomes[thread.ManagedThreadId].Add(chromosome);
                }
            }

            public ThreadJob GetThreadJob(int threadId) {
                return _threadJobs[threadId];
            }

            public void StartThreads() {
                for (int i = 1; i < _threadCount; i++) {
                    _threads[i].Start();
                }
            }

            public ThreadData(Builder builder) {
                _threadCount = builder.ThreadCount;
                _threadDoWork = builder.ThreadDoWork;
                _chromosomeLength = builder.ChromosomeLength;
                _randomSeed = builder.RandomSeed;
                InstantiateThreads();
                InstantiateThreadData();
            }

            private void InstantiateThreads() {
                _threads = new List<Thread>();
                for (int i = 0; i < _threadCount; i++) {
                    if (i == 0) {
                        _threads.Add(Thread.CurrentThread);
                        _threadJobs.Add(_threads[i].ManagedThreadId, ThreadJob.Wait);
                    } else {
                        _threads.Add(new Thread(new ThreadStart(_threadDoWork)));
                        _threadJobs.Add(_threads[i].ManagedThreadId, ThreadJob.Wait);
                    }
                }
            }

            private void InstantiateThreadData() {
                _previousScoreCountPerThread = _previousScoreCount / _threadCount;
                foreach (Thread thread in this.Threads) {
                    _previousScores.Add(thread.ManagedThreadId, new Dictionary<string,float>());
                    _currentChromosome.Add(thread.ManagedThreadId, new int[_chromosomeLength]);
                    _uniqueChromosomes.Add(thread.ManagedThreadId, new HashSet<string>());
                    if (_randomSeed != -1) {
                        _random.Add(thread.ManagedThreadId, new Random(_randomSeed));
                    } else {
                        _random.Add(thread.ManagedThreadId, new Random());
                    }
                    _scoreSum.Add(thread.ManagedThreadId, 0);
                }
            }

            public class Builder {
                public int ThreadCount;
                public Action ThreadDoWork;
                public int ChromosomeLength;
                public int RandomSeed;

                public Builder SetThreadCount(int threadCount) {
                    this.ThreadCount = threadCount;
                    return this;
                }

                public Builder SetThreadDoWork(Action threadDoWork) {
                    this.ThreadDoWork = threadDoWork;
                    return this;
                }

                public Builder SetChromosomeLength(int chromosomeLength) {
                    this.ChromosomeLength = chromosomeLength;
                    return this;
                }

                public Builder SetRandomSeed(int randomSeed) {
                    this.RandomSeed = randomSeed;
                    return this;
                }

                public ThreadData Build() {
                    return new ThreadData(this);
                }
            }
        }
    }
}
