using System;
using System.Text;
using System.Numerics;
using System.Linq;
using System.Threading;
using System.Diagnostics;

namespace ConsoleApp
{
    class Program
    {

        static float[] X;
        static float[] Y;
        static int n;// число точек для интерполирования
        static int s;// степень интерполирования
        static float xx = 0;// координата для нахождения
        static float ResultPoint;// точка результата
        static float[,] Matrix;
        static float[] MatrixResult;

        //Timers
        static float Time_Program;
        static float Time_Thread_1;
        static float Time_Thread_2;

        static void Main()
        {

            Console.OutputEncoding = Encoding.UTF8;

            Input.PointCount();//Метод ввода количества точек

            X = new float[n];//Массив с точками по X
            Y = new float[n];//Массив с точками по Y

            Input.POWCount();//Метод ввода степени

            Input.Points();//Ввод точек

            Stopwatch timer3 = new Stopwatch();
            timer3.Start();

            Work.SORT();//Сортировка точек в массиве 

            Work.FILLMATRIX();// Построение матрицы из исходных точек


            Thread Thread_1 = new Thread(new ThreadStart(Work.SOLVEMATRIX));// Поток Интерполяции методом наименьших квадратов
            Thread_1.Start();


            Work.STP(s);//Работа со степенью

            timer3.Stop();
            Input.EndPointX();//Конечный х
            timer3.Start();

            Thread Thread_2 = new Thread(new ThreadStart(Work.IPL));// Поток Интерполяции методом Лагранжа
            Thread_2.Start();


            Work.OUTMATRES();

            Work.CheckInfinity(ResultPoint);

            timer3.Stop();
            Time_Program = (float)timer3.Elapsed.TotalMilliseconds;

            Console.WriteLine("-----------------------");
            Console.WriteLine($"Время интерполяции методом наименьших квадратов [ {Time_Thread_1} ms ]");
            Console.WriteLine($"Время интерполяции методом Лагранжа [ {Time_Thread_2} ms ]");
            Console.WriteLine($"Время выполнения программы [ {Time_Program} ms ]");
            Console.WriteLine("-----------------------");
            Process currentProc = Process.GetCurrentProcess();
            Console.WriteLine($"Объем закрытой системной памяти в байтах [ {currentProc.PrivateMemorySize64} ] ");
            Console.WriteLine($"Объем физической памяти в байтах [ {currentProc.WorkingSet64} ]");

            Console.Read();
        }

        public class Input
        {
            public static void PointCount()
            {
                Console.WriteLine("Введите количество точек:");

                string StringInput = Console.ReadLine();
                int IntInput;

                if (!int.TryParse(StringInput, out IntInput))
                {
                    Console.WriteLine("Некорректный ввод!");
                    PointCount();
                    return;
                }

                if (IntInput < 2 || IntInput > 1000)
                {
                    Console.WriteLine("Точек не может быть меньше двух или более одной тысячи !");
                    PointCount();
                    return;

                }

                n = IntInput;
            }

            public static void POWCount()
            {
                Console.WriteLine("Введите степень интерполирования");

                string StringInput = Console.ReadLine();
                int IntInput;

                if (!int.TryParse(StringInput, out IntInput))
                {
                    Console.WriteLine("Некорректный ввод!");
                    POWCount();
                    return;
                }

                if (IntInput < 1 || IntInput > n)
                {
                    Console.WriteLine($"Степень интерполирования должна быть от 1 до {n} !");
                    POWCount();
                    return;

                }


                s = IntInput;
            }

            public static void Points()
            {
                for (int i = 0; i < n; i++)
                {
                wkt:

                    Console.WriteLine("Введите координаты точки " + (i + 1));
                    string StringPoint = Console.ReadLine();

                    if (!StringPoint.Contains(" ") || !float.TryParse(StringPoint.Split(' ')[0], out X[i]) || !float.TryParse(StringPoint.Split(' ')[1], out Y[i]))
                    {
                        Console.WriteLine("Некорректный ввод координаты, введите точку в виде [ x y ] !");
                        goto wkt;
                    }
                    for (int j = 0; j < i; j++)
                    {
                        if (X[j] == X[i]) //сравнение текущего со всеми предыдущими 
                        {
                            Console.WriteLine("Ошибка. Введите координату X точки, отличную от предыдущей");//Ошибка
                            goto wkt;
                        }
                    }
                }
            }

            public static void EndPointX()
            {
                Console.WriteLine("Введите координату X точки:");
                string p = Console.ReadLine();

                if (!float.TryParse(p, out xx))//считывание string координаты для нахождения и конвертирование в float
                {
                    Console.WriteLine("Некорректный ввод!");
                    EndPointX();
                }
                if (p.Contains(" "))
                {
                    Console.WriteLine("Некорректный ввод, введите координату X точки без пробела!");
                    EndPointX();
                }
                if (xx < X[0] || xx > X[X.Length - 1])
                {
                    Console.WriteLine($"Введите координату X от {X[0]} до {X[X.Length - 1]}");
                    EndPointX();
                }
            }
        }

        public class Work
        {
            public static void STP(int s)
            {

                switch (s)
                {
                    case 1:
                        // Отбор точек не нужен
                        break;
                    case 2:
                        X[1] = X[X.Length - 1];//записываем координату X последней точки на второе место
                        Y[1] = Y[Y.Length - 1];//записываем координату Y последней точки на второе место
                        Array.Resize(ref X, 2);
                        Array.Resize(ref Y, 2);
                        break;
                    case 3:
                        X[1] = X[(int)(X.Length / 2)];//записываем координату X средней точки на второе место
                        Y[1] = Y[(int)(Y.Length / 2)];//записываем координату Y средней точки на второе место
                        X[2] = X[X.Length - 1];//записываем координату X последней точки на третье место
                        Y[2] = Y[Y.Length - 1];//записываем координату Y последней точки на третье место
                        Array.Resize(ref X, 3);
                        Array.Resize(ref Y, 3);
                        break;
                    case 4:
                        X[1] = X[(int)(X.Length / 4)];
                        Y[1] = Y[(int)(Y.Length / 4)];
                        X[2] = X[(int)(2 * (X.Length / 4))];
                        Y[2] = Y[(int)(2 * (Y.Length / 4))];
                        X[3] = X[X.Length - 1];//записываем координату X последней точки на четвертое место
                        Y[3] = Y[Y.Length - 1];//записываем координату Y последней точки на четвертое место
                        Array.Resize(ref X, 4);
                        Array.Resize(ref Y, 4);
                        break;
                }

            }
            public static void IPL()
            {
                Stopwatch timer1 = new Stopwatch();
                timer1.Start();

                float result = 0;
                int n = X.Length;

                //------------Формула интерполирования методом Лагранжа---------------
                for (int i = 0; i < n; i++)
                {
                    float tmp = 1;
                    for (int j = 0; j < n; j++)
                    {
                        if (j != i)
                            tmp *= (xx - X[j]) / (X[i] - X[j]);
                    }
                    result += Y[i] * tmp;
                }
                //--------------------------------------------------------------------
                ResultPoint = result;

                timer1.Stop();
                Time_Thread_2 = (float)timer1.Elapsed.TotalMilliseconds;

            }
            public static void CheckInfinity(float result)

            {
                Console.WriteLine("-----------------------");
                if (float.IsInfinity(result))
                    Console.WriteLine("Ошибка. На данном графике не существует такой точки");//Ошибка
                else
                    Console.WriteLine($"Y={result}"); //Вывод результата запросом функции IPL

            }
            public static void OUTMATRES()
            {
                Console.WriteLine();
                for (int i = 0; i < MatrixResult.Length; i++)
                    Console.WriteLine($"a[{i}]={MatrixResult[i]}");
            }
            public static void SORT()
            {


                Vector2[] Points = new Vector2[X.Length];

                for (int i = 0; i < X.Length; i++)
                {
                    Points[i].X = X[i];
                    Points[i].Y = Y[i];
                }

                Vector2[] SortPoints = Points.OrderBy(v => v.X).ToArray<Vector2>();

                for (int i = 0; i < X.Length; i++)
                {
                    X[i] = SortPoints[i].X;
                    Y[i] = SortPoints[i].Y;
                }
            }
            public static void FILLMATRIX()
            {
                Matrix = new float[s + 1, s + 2];

                for (int i = 0; i < s + 1; i++)
                {
                    for (int j = 0; j < s + 1; j++)
                    {
                        for (int k = 0; k < n; k++)
                            Matrix[i, j] += (float)Math.Pow(X[k], i + j);
                    }
                    for (int k = 0; k < n; k++)
                        Matrix[i, s + 1] += Y[k] * (float)Math.Pow(X[k], i);
                }

            }
            public static void SOLVEMATRIX()
            {

                Stopwatch timer2 = new Stopwatch();
                timer2.Start();

                MatrixResult = new float[s + 1];
                float kf = 0;
                float tempSum = 0;
                for (int i = 0; i < s; i++)
                {
                    for (int k = i + 1; k < s + 1; k++)
                    {
                        if (Matrix[i, i] == 0)
                        {

                            for (int q = n + 1; q < s; q++)
                            {
                                if (Matrix[q, i] != 0)
                                {
                                    float[] tmp = new float[s + 1];

                                    for (int p = 0; p < s + 1; p++)
                                        tmp[p] = Matrix[q, p];

                                    for (int p = 0; p < s + 1; p++)
                                        Matrix[q, p] = Matrix[i, p];

                                    for (int p = 0; p < s + 1; p++)
                                        Matrix[i, p] = tmp[p];
                                }
                            }


                        }

                        kf = Matrix[k, i] / Matrix[i, i];
                        for (int j = 0; j < s + 2; j++)
                        {
                            Matrix[k, j] -= kf * Matrix[i, j];
                        }
                    }
                }

                if (Matrix[s, s] == 0)
                    Console.WriteLine("Error! Bad matrix! \n");

                for (int i = s; i >= 0; i--)
                {
                    tempSum = Matrix[i, s + 1];
                    for (int j = i + 1; j < s + 1; j++)
                    {
                        tempSum -= MatrixResult[s - j] * Matrix[i, j];
                    }
                    MatrixResult[s - i] = tempSum / Matrix[i, i];
                }

                timer2.Stop();
                Time_Thread_1 = (float)timer2.Elapsed.TotalMilliseconds;

            }

        }
    }
}