using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Blum_Blum_Shub_project.Core
{
    public class Generation
    {
        private int _n;
        private double _x0;
        private double[] _numbers = Array.Empty<double>();
        private List<int> _primeNumber = new();

        public int N { get => _n; set => _n = value; }
        public double X0 { get => _x0; set => _x0 = value; }
        public double[] Numbers => _numbers;
        public List<int> PrimeNumber { get => _primeNumber; set => _primeNumber = value; }

        private bool TestFerma(double a, int n)
        {
            var p = BigInteger.ModPow((BigInteger)a, n - 1, n);
            return p == 1;
        }

        public bool CheckNumber(int p)
        {
            Random r = new Random();

            for (int i = 0; i < 10; i++)
            {
                double a = r.Next(2, p - 1);
                if (!TestFerma(a, p))
                    return false;
            }
            return true;
        }

        // Генерация простых p, где p mod 4 = 3
        public void GenerationPrimeNumber()
        {
            int i = 2;
            _primeNumber.Clear();

            while (_primeNumber.Count < _n)
            {
                if (i % 4 == 3)
                    if (CheckNumber(i))
                        _primeNumber.Add(i);
                i++;
            }
        }

        public void GenerationNumber()
        {
            Random r = new Random();
            _numbers = new double[_n];

            _numbers[0] = _x0;

            for (int i = 1; i < _n; i++)
            {
                _numbers[i] =
                    (Math.Pow(_numbers[i - 1], 2) * _primeNumber[r.Next(0, _primeNumber.Count - 1)]) %
                    (_primeNumber[r.Next(0, _primeNumber.Count - 1)] * _primeNumber[r.Next(0, _primeNumber.Count - 1)]);
            }
        }
    }
}
