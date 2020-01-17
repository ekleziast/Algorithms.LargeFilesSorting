using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcatConsoleApp
{
    class Program
    {
        static readonly string FirstFilePath  = Path.Combine(AppContext.BaseDirectory, @"file1.txt");   // путь к первому файлу
        static readonly string SecondFilePath = Path.Combine(AppContext.BaseDirectory, @"file2.txt");   // путь ко второму файлу
        static readonly string ResultFilePath = Path.Combine(AppContext.BaseDirectory, @"result.txt");  // путь к выходному файлу
        const int LENGTH_OF_FILE = 200000; // Максимальное количество строк во временном файле
        const bool USE_TEST_DATA = true;   // Использование тестового набора данных (Int32)
        static void Main(string[] args)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            if (USE_TEST_DATA)
            {
                Fill(FirstFilePath, count: 10000000);
                Fill(SecondFilePath, count: 10000000);
                Console.WriteLine($"Тестовые данные созданы. Время на создание: {stopwatch.Elapsed}");
            }

            RemoveFile(ResultFilePath);

            System.Diagnostics.Stopwatch stopwatchWritingBlocks = new System.Diagnostics.Stopwatch();
            stopwatchWritingBlocks.Start();
            WriteFileBlocks(FirstFilePath);
            WriteFileBlocks(SecondFilePath);
            stopwatchWritingBlocks.Stop();
            Console.WriteLine($"Файлы разбиты на части. Время на разбитие: {stopwatchWritingBlocks.Elapsed}");

            System.Diagnostics.Stopwatch stopwatchConcat = new System.Diagnostics.Stopwatch();
            stopwatchConcat.Start();
            ConcatFiles(ResultFilePath);
            stopwatchConcat.Stop();
            Console.WriteLine($"Слияние файлов выполнено. Время на слияние: {stopwatchConcat.Elapsed}");

            stopwatch.Stop();

            Console.WriteLine($"Общее затраченное время: {stopwatch.Elapsed}");
            Console.WriteLine($"Файл доступен по пути: {ResultFilePath}");
            Console.WriteLine("Press any key...");
            Console.ReadKey();
        }

        /// <summary>
        /// Удаляет временные файлы
        /// </summary>
        public static void CleanDatFiles()
        {
            foreach (var path in Directory.GetFiles($"{AppContext.BaseDirectory}", "*_*.dat"))
            {
                File.Delete(path);
            }
        }

        /// <summary>
        /// Выполняет слияние временных файлов в один выходной файл
        /// </summary>
        /// <param name="ResultFilePath">Путь к выходному файлу</param>
        public static void ConcatFiles(string ResultFilePath)
        {
            List<StreamReader> readers = new List<StreamReader>();
            foreach(var path in Directory.GetFiles($"{AppContext.BaseDirectory}", "*_*.dat"))
            {
                readers.Add(new StreamReader(path));
            }
            using (StreamWriter sw = new StreamWriter(ResultFilePath, append: true))
            {
                List<long> ints = new List<long>();
                for(int i = 0; i < readers.Count; i++)
                {
                    string line = readers[i].ReadLine();
                    ints.Add(Int64.Parse(line));
                }
                while(readers.Count > 0)
                {
                    int pos = ints.IndexOf(ints.Min());
                    sw.WriteLine(ints[pos]);
                    ints.RemoveAt(pos);

                    string line = readers[pos].ReadLine();
                    if(line != null)
                    {
                        ints.Insert(pos, Int64.Parse(line));
                    }
                    else
                    {
                        readers[pos].Dispose();
                        readers.RemoveAt(pos);
                    }
                }
            }
            CleanDatFiles();
        }

        /// <summary>
        /// Заполняет файл случайными числами типа Int32
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        /// <param name="count">Количество случайных чисел</param>
        public static void Fill(string path, int count = 1000000)
        {
            RemoveFile(path);
            using(StreamWriter sw = new StreamWriter(path, append: true))
            {
                Random rnd = new Random();
                int value;
                for (int i = 0; i < count; i++)
                {
                    value = rnd.Next(Int32.MinValue, Int32.MaxValue);
                    sw.WriteLine(value);
                }
            }
        }

        /// <summary>
        /// Удаляет файл, если он существует
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public static void RemoveFile(string path)
        {
            if (File.Exists(path)) { File.Delete(path); }
        }

        /// <summary>
        /// Разбивает файл на части, в каждом выходном файле по <see cref="LENGTH_OF_FILE"/> строк.
        /// Автоматически сортирует строки файла по возрастанию.
        /// </summary>
        /// <param name="inPath">Путь к файлу</param>
        public static void WriteFileBlocks(string inPath)
        {
            if (!File.Exists(inPath)) { throw new FileNotFoundException(); }
            using(StreamReader sr = new StreamReader(inPath))
            {
                List<long> longs = new List<long>();
                string line;

                int fileCounter = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    long longLine;
                    if (!Int64.TryParse(line, out longLine)) // На всякий случай проверяем строку на Int64
                    {
                        continue;
                    }
                    if (longs.Count != LENGTH_OF_FILE)
                    {
                        longs.Add(longLine);
                    }
                    else
                    {
                        longs.Sort();
                        using(StreamWriter sw = new StreamWriter($"{inPath.Replace(".txt", String.Empty)}_{fileCounter}.dat"))
                        {
                            longs.ForEach(o => sw.WriteLine(o));
                        }
                        fileCounter++;
                        longs.Clear();
                        longs.Add(longLine);
                    }
                }
                // Записываем оставшиеся в StringBuilder данные
                if(longs.Count > 0)
                {
                    longs.Sort();
                    using (StreamWriter sw = new StreamWriter($"{inPath.Replace(".txt", String.Empty)}_{fileCounter++}.dat"))
                    {
                        longs.ForEach(o => sw.WriteLine(o));
                    }
                    longs.Clear();
                }
            }
        }
    }
}
