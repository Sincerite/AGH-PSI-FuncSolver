using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
    
namespace FuncSolver
{
    public partial class MainForm : Form
    {
        // Zegar do aktualizacji decyzji na wykresie
        Timer timer = new Timer() { Interval = 100 };
        // Szablon funkcji do znalezienia
        Func<double, double[], double> Function;

        PointF[] points = new PointF[5]
        {
                new PointF(-0.8634f,-2.5847f),
                new PointF(1.4037f,4.1770f),
                new PointF(0.3185f, 2.8579f),
                new PointF(2.5410f,5.7101f),
                new PointF(-1.4804f,4.9645f)
        };
        /*
        PointF[] points5 = new PointF[5]
        {
                new PointF(0,0),
                new PointF(1,1),
                new PointF(-1, 1),
                new PointF(2,4),
                new PointF(-2,4)
        };

        PointF[] pointsSin = new PointF[5]
        {
                new PointF(0,0),
                new PointF((float)Math.PI,0),
                new PointF((float)Math.PI/6, 2),
                new PointF((float)Math.PI/2, 4),
                new PointF(-2*(float)Math.PI,0)
        };

        PointF[] points1 = new PointF[]
        {
                new PointF(-3, 5.77728f),
                new PointF(-1, 0.306336f),
                new PointF(1, 7.69366f),
                new PointF(0, 3),
                new PointF(3, 18.2227f),
        };

        PointF[] points3 = new PointF[]
        {
                new PointF(-4, 51.3927f),
                new PointF(-1.234f, 9.20274f),
                new PointF(-1, 6.99781f),
                new PointF(-0.95f, 2.55526f),
                new PointF(0.5f, 2.78597f),
                new PointF(-5.5f, 107.527f),
                new PointF(2.56f, 36.3313f),
                new PointF(-0.3f, 7.84793f)
        };

        PointF[] points2 = new PointF[]
        {
                new PointF(-5, -13.316f),
                new PointF(-4.5f, -11.3193f),
                new PointF(-4, -9.76596f),
                new PointF(0, -1.25f),
                new PointF(1.23f, 2.48818f),
                new PointF(-2, -8.3852f),
                new PointF(0.5f, -0.699706f)
        };
        */

        // Populacja, z którą pracuje formularz
        Population population;

        public MainForm()
        {
            InitializeComponent();
            timer.Tick += Timer_Tick;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Tworzenie populacji
            NewPopulation(0);

            //  Dodaj punkty do tabeli.
            dataGridViewPoints.RowCount = points.Length + 1;
            for (int i = 0; i < points.Length; i++)
            {
                dataGridViewPoints[0, i].Value = points[i].X;
                dataGridViewPoints[1, i].Value = points[i].Y;
            }

            // Inicjalizacja wykresu punktowego
            chart.Series.Clear();
            chart.Series.Add("Points").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Point;

            // Inicjowanie wykresów rozwiązań
            chart.Series.Add("Best").ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;
            for (int i = 1; i < population.Lenght; i++)
                chart.Series.Add("#" + (i+1)).ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.Line;

            // Zaplanuj aktualizacje
            RefreshPointsOnChart();
            RefreshChrOnChart();
        }

        /// <summary>
        /// Jeden krok naszym GA
        /// </summary>
        private void NextStep()
        {
            // Uzyskaj aktualną wartość funkcji fitness
            var prevfit = population.Fitness(population.First().Gens);

            if (checkBox1.Checked)
            {
                // Próbuję znaleźć rozwiązanie na 1000 pokoleń
                for (int i = 0; i < 1000; i++)
                {
                    population.NextGeneration();
                    if (prevfit != population.Fitness(population.First().Gens))
                        break;
                }
            }
            else
                population.NextGeneration();

            // Otrzymujemy dla niego najlepszy chromosom i wartość funkcji Fitness
            var chr = population.First();
            var fit = population.Fitness(chr.Gens);

            // Aktualizacja danych wyjściowych
            label2.Text = population.Age.ToString();
            label6.Text = Math.Round(100 * fit, 3).ToString() + "%";
            label4.Text = chr.ToString();

            // Aktualizacja punktów na wykresie
            RefreshChrOnChart();
        }

        /// <summary>
        /// Aktualizacja wizualizacji chromosomów na wykresie
        /// </summary>
        private void RefreshChrOnChart()
        {
            for (int i = 0; i < population.Lenght; i++)
            {
                chart.Series[i + 1].Points.Clear();
                var cur = population[i];
                for (double j = chart.ChartAreas[0].AxisX.Minimum; j <= -1; j += 0.2)
                {
                    j = Math.Round(j, 2);
                    chart.Series[i + 1].Points.AddXY(j, Function(j, cur.Gens));
                }
                // W zakresie od -1 do 1, robimy punkty w przyrostach co 0,01, ponieważ niektóre funkcje zachowują się bardzo ostro w sąsiedztwie zera
                for (double j = -1; j <= 1; j += 0.01)
                {
                    j = Math.Round(j, 3);
                    chart.Series[i + 1].Points.AddXY(j, Function(j, cur.Gens));
                }
                for (double j = 1; j <= chart.ChartAreas[0].AxisX.Maximum; j += 0.2)
                {
                    j = Math.Round(j, 2);
                    chart.Series[i + 1].Points.AddXY(j, Function(j, cur.Gens));
                }
            }
        }

        /// <summary>
        /// Zaktualizuj punkty na mapie
        /// </summary>
        private void RefreshPointsOnChart()
        {
            chart.Series[0].Points.Clear();

            for (int i = 0; i < points.Length; i++)
                chart.Series[0].Points.AddXY(points[i].X, points[i].Y);

            double w = points.Max(p => p.X) - points.Min(p => p.X);
            double h = points.Max(p => p.Y) - points.Min(p => p.Y);
            chart.ChartAreas[0].AxisX.Minimum = Math.Round(points.Min(p => p.X) - w / 5);
            chart.ChartAreas[0].AxisX.Maximum = Math.Round(points.Max(p => p.X) + w / 5);
            chart.ChartAreas[0].AxisY.Minimum = Math.Round(points.Min(p => p.Y) - h / 5);
            chart.ChartAreas[0].AxisY.Maximum = Math.Round(points.Max(p => p.Y) + h / 5);
        }

        /// <summary>
        /// Przycisk następnego kroku
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            NextStep();
        }

        /// <summary>
        /// Przycisk „Wyszukaj rozwiązanie” i „Zatrzymaj wyszukiwanie”
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            if (button2.Text == "Zatrzymaj wyszukiwanie")
            {
                button2.Text = "Wyszukaj rozwiązanie";
                timer.Stop();
                button1.Enabled = button4.Enabled = button3.Enabled = true;
            }
            else
            {
                button2.Text = "Zatrzymaj wyszukiwanie";
                timer.Start();
                button1.Enabled = button4.Enabled = button3.Enabled = false;
            }
        }

        /// <summary>
        /// Populacja timerów nowej generacji
        /// </summary>
        private void Timer_Tick(object sender, EventArgs e)
        {
            NextStep();
        }

        /// <summary>
        /// Nowy przycisk Nowa populacja
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            NewPopulation(comboBoxTemplate.SelectedIndex);
            RefreshChrOnChart();
        }

        /// <summary>
        /// Utwórz nową populację
        /// </summary>
        /// <param name="type">numer szablonu funkcji do przeszukania</param>
        private void NewPopulation(int type)
        {
            /*  
             *  Ax² + Bx + Csin(Dx) + E
                Ax³ + Bx² + Cx + D√x + E
                Asin(Bx) + Csin(Dx) + Ex + F
                Ax³ + Bx² + Cx + D
                Ax² + Bx + C√x + D
                Ax² + Bx + C
                Ax^B + C
                Ax + B|x| + C
                Ax + B
            */

            int gensCount;
            double mutrange = 0.3;
            double min = 0, max = 1;

            switch (type)
            {
                case 9:
                    gensCount = 2;
                    min = -4;
                    max = 4;
                    mutrange = 0.3;
                    Function = (x, g) => g[0] + Math.Pow(g[1], x);
                    break;
                case 8:
                    gensCount = 2;
                    min = -5;
                    max = 5;
                    mutrange = 0.5;
                    Function = (x, g) => g[0] * x + g[1];
                    break;
                case 7:
                    gensCount = 3;
                    min = -5;
                    max = 5;
                    mutrange = 0.5;
                    Function = (x, g) => g[0] * x + g[1] * Math.Abs(x) + g[2];
                    break;
                case 6:
                    gensCount = 3;
                    min = -3;
                    max = 3;
                    mutrange = 0.25;
                    Function = (x, g) => g[0] * Math.Sign(x) * Math.Pow(Math.Abs(x),g[1]) + g[2];
                    break;
                case 5:
                    gensCount = 3;
                    min = -8;
                    max = 8;
                    mutrange = 0.5;
                    Function = (x, g) => g[0] * x * x + g[1] * x + g[2];
                    break;
                case 4:
                    gensCount = 4;
                    min = -10;
                    max = 10;
                    mutrange = 0.4;
                    Function = (x, g) => g[0] * x * x + g[1] * x + g[2] * Math.Sign(x) * Math.Sqrt(Math.Abs(x)) + g[3];
                    break;
                case 3:
                    gensCount = 4;
                    min = -10;
                    max = 10;
                    mutrange = 0.4;
                    Function = (x, g) => g[0] * x * x * x + g[1] * x * x + g[2] * x + g[3];
                    break;
                case 2: // Asin(Bx) + Csin(Dx) + Ex + F
                    gensCount = 5;
                    min = -3;
                    max = 3;
                    mutrange = 0.4;
                    Function = (x, g) => g[0] * Math.Sin(g[1] * x) + g[2] * Math.Cos(g[3] * x) + g[4];
                    break;
                case 1: // Ax³ + Bx² + Cx + D√x + E
                    gensCount = 5;
                    min = -5;
                    max = 5;
                    mutrange = 0.4;
                    Function = (x, g) => g[0] * x * x * x + g[1] * x * x + g[2] * x + g[3] * Math.Sign(x) * Math.Sqrt(Math.Abs(x)) + g[4];
                    break;
                case 0: // Ax² + Bx + Csin(Dx) + E
                    gensCount = 5;
                    min = -3;
                    max = 3;
                    mutrange = 0.4;
                    Function = (x, g) => g[0] * x * x + g[1] * x + g[2] * Math.Sin(g[3] * x) + g[4];
                    break;
                default:
                    gensCount = 1;
                    MessageBox.Show("Nie udało się utworzyć populacji. Wybierz szablon funkcji");
                    return;
            }

            //Func<double[], double> fitnessFunc = (g) => -points.Select(p => Math.Abs(Function(p.X, g) - p.Y)).Sum();
            Func<double[], double> fitnessFunc = (g) => 1.0 / (1 + Math.Abs(points.Select(p => Math.Pow(Function(p.X, g) - p.Y, 2)).Sum()));
            population = new Population(gensCount, fitnessFunc, 3, mutrange, null, new Tuple<double, double>(min, max));
        }

        /// <summary>
        /// Przycisk do aktualizacji punktów na wykresie
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            points = new PointF[dataGridViewPoints.RowCount - 1];

            for (int i = 0; i < dataGridViewPoints.RowCount - 1; i++)
            {
                float x, y;
                bool parsing = true;
                parsing = float.TryParse(dataGridViewPoints[0, i].Value.ToString(), out x);
                parsing = float.TryParse(dataGridViewPoints[1, i].Value.ToString(), out y) && parsing;
                if (!parsing)
                {
                    MessageBox.Show("Błąd podczas tworzenia punktu na wykresie");
                    return;
                }
                points[i] = new PointF(x, y);
            }

            RefreshPointsOnChart();
        }

        /// <summary>
        /// Zmiana prędkości
        /// </summary>
        private void trackBarTimerInterval_Scroll(object sender, EventArgs e)
        {
            int interval = 1000;
            switch(trackBarTimerInterval.Value)
            {
                case 1: interval = 1500; break;
                case 2: interval = 1000; break;
                case 3: interval = 500; break;
                case 4: interval = 250; break;
                case 5: interval = 200; break;
                case 6: interval = 100; break;
                case 7: interval = 50; break;
                case 8: interval = 25; break;
                case 9: interval = 10; break;
                case 10: interval = 2; break;
                default: break;
            }
            timer.Interval = interval;
        }

        /// <summary>
        /// Usuwanie punktów ze stołu
        /// </summary>
        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridViewPoints.SelectedCells.Count > 0 && dataGridViewPoints.SelectedCells[0].RowIndex != dataGridViewPoints.RowCount - 1)
                dataGridViewPoints.Rows.Remove(dataGridViewPoints.SelectedCells[0].OwningRow);
        }

        /// <summary>
        /// Aktualizacja populacji przy zmianie szablonu funkcji
        /// </summary>
        private void comboBoxTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            NewPopulation(comboBoxTemplate.SelectedIndex);
            RefreshChrOnChart();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
