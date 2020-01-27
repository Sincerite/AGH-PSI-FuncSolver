using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncSolver
{
    interface IPopulation
    {
        Chromosome Mutate(Chromosome chromosome);
        Chromosome Crossover(Chromosome a, Chromosome b);
        void NextGeneration();
    }

    /// <summary>
    /// Populacja
    /// </summary>
    public class Population : List<Chromosome>
    {
        // Generator liczb losowych
        private Random Rnd;
        // Wielkość populacji (ograniczenie wyboru)
        public int Lenght { get; private set; }
        // Długość kodu genetycznego (tablica genów)
        private int GensCount;
        // Znormalizowana funkcja sprawnościowa jednostek
        public Func<double[], double> Fitness { get; private set; }
        // Promień mutacji (+ - MumateRange dla osoby)
        private double MutateRange = 0.2;
        // Zakres wartości genów
        private Tuple<double,double> Range;
        // Wiek populacji
        public long Age { get; private set; }
        // Maksymalny wiek osoby, w której stosowane jest rozwiązanie bankowe
        private int? MaxAgeToBank;
        // Bank decyzji
        public List<Chromosome> Bank = new List<Chromosome>();

        /// <summary>
        /// Tworzenie populacji
        /// </summary>
        /// <param name="gensCount">Liczba genów</param>
        /// <param name="fitnessFunction">Funkcja fitness</param>
        /// <param name="count">Wielkość osób pozostawionych w populacji po selekcji</param>
        /// <param name="mutateRange">Promień mutacji(+ - do aktualnej wartości genu)</param>
        /// <param name="maxAgeToBank">Maksymalny wiek osoby, w której stosowane jest rozwiązanie bankowe. Null - nie korzystaj z bankowości</param>
        /// <param name="range">Zakres wartości genów</param>
        public Population(int gensCount, Func<double[],double> fitnessFunction, int count = 5, double mutateRange = 0.2, int? maxAgeToBank = 30000, Tuple<double, double> range = null)
        {
            Rnd = new Random();
            GensCount = gensCount;
            Fitness = fitnessFunction;
            Lenght = count;
            MutateRange = mutateRange;
            MaxAgeToBank = maxAgeToBank;
            Range = range ?? new Tuple<double, double>(0,1);
            CreateRandomChromosomes(Lenght);
        }

        /// <summary>
        /// Tworzenie N osobników w populacji
        /// </summary>
        /// <param name="count">Liczba osób w populacji</param>
        private void CreateRandomChromosomes(int count)
        {
            for (int i = 0; i < count; i++)
            {
                double[] gens = new double[GensCount];
                for (int j = 0; j < gens.Length; j++)
                    gens[j] = Rnd.NextDouble() * (Range.Item2 - Range.Item1) + Range.Item1;
                Add(new Chromosome(gens));
            }
        }

        /// <summary>
        /// Sortowanie populacji według malejącej funkcji sprawności
        /// </summary>
        public new void Sort()
        {
            var sorted = this.OrderByDescending(c => Fitness(c.Gens)).ToArray();
            this.Clear();
            this.AddRange(sorted);
        }

        /// <summary>
        /// Tworzenie mutacji chromosomowej
        /// </summary>
        /// <param name="a">Chromosom macierzysty</param>
        /// <returns>Zmutowany chromosom</returns>
        private Chromosome Mutate(Chromosome a)
        {
            var n = new Chromosome(GensCount);
            for (int i = 0; i < GensCount; i++)
                n.Gens[i] = a.Gens[i] + Rnd.NextDouble() * MutateRange * 2 - MutateRange;
            return n;
        }

        /// <summary>
        /// Przejście chromosomowe
        /// </summary>
        /// <param name="a">Pierwszy rodzic</param>
        /// <param name="b">Drugi rodzic</param>
        /// <returns>Potomek</returns>
        private Chromosome CrossOver(Chromosome a, Chromosome b)
        {
            var percent = new double[GensCount];
            for (int i = 0; i < GensCount; i++)
                percent[i] = Rnd.NextDouble();

            var n = new Chromosome(GensCount);
            for (int i = 0; i < GensCount; i++)
                n.Gens[i] = percent[i] * a.Gens[i] + (1 - percent[i]) * b.Gens[i];
            return n;
        }

        /// <summary>
        /// Przejście do następnej generacji
        /// </summary>
        public void NextGeneration()
        {
            // Zapisz bieżącą populację w tablicy
            var prevGen = this.ToArray();

            // Dla każdego genu używamy mutacji i dodajemy ją do populacji
            foreach (var chr in prevGen)
                Add(Mutate(chr));

            // Przekraczanie wszystkich osób
            for (int i = 0; i < prevGen.Length; i++)
            {
                for (int j = 0; j < prevGen.Length; j++)
                {
                    if (i == j)
                        continue;

                    var child = CrossOver(prevGen[i], prevGen[j]);
                    if (child != null)
                        Add(child);
                }
            }

            // Zwiększ wiek każdego chromosomu
            this.ForEach(c => c.Age++);

            // Dodaj więcej losowych chromosomów
            CreateRandomChromosomes(Lenght / 2);

            // Bankowość
            if (MaxAgeToBank.HasValue && this.First().Age > MaxAgeToBank.Value)
            {
                Bank.Add(this.First());
                this.RemoveAll(c => true);
                CreateRandomChromosomes(Lenght);
            }

            // Sortuj malejące funkcje fitness
            Sort();

            // Wybór
            this.RemoveRange(Lenght, this.Count - Lenght);

            // Zwiększamy wiek populacj
            Age++;
        }
    }
}
