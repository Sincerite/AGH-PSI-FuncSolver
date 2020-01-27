using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FuncSolver
{
    /// <summary>
    /// Chromosom - jest przykładem rozwiązania
    /// </summary>
    public class Chromosome
    {
        /// <summary>
        /// Geny chromosomowe
        /// </summary>
        public double[] Gens { get; set; }
        /// <summary>
        /// Wiek chromosomów
        /// </summary>
        public int Age { get; set; }

        /// <summary>
        /// Tworzenie chromosomu o danym rozmiarze tablicy genów
        /// </summary>
        /// <param name="size">Długość szeregu genów</param>
        public Chromosome(int size)
        {
            Gens = new double[size];
        }

        /// <summary>
        /// Generowanie Zromosomu na podstawie Podanych Wartości Genów
        /// </summary>
        /// <param name="gens">Гены</param>
        public Chromosome(params double[] gens)
        {
            Gens = gens.ToArray();
        }

        public override string ToString()
        {
            return '{' + string.Join(";", Gens.Select(g => string.Format("{0,0:N1}", g))) + '}';
        }
    }
}
