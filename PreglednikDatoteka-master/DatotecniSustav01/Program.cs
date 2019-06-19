using System;
using System.IO;
using System.Linq;
using System.Timers;

namespace DatotecniSustav01
{
    class Program
    {
        static Timer blink = null;
        static bool diskOtvoren = false;
        const int blinkInterval = 600;
        const int cursorX = 1;
        const int offsetY = 3;
        static int maxY = 0;
        static bool cursorBlinkingState = false;
        static bool run = true;
        static DriveInfo[] diskovi;
        static void Main(string[] args)
        {
            // Iscrtaj meni
            Console.CursorVisible = false;
            DiskMeni();
            blink = Izbornik();

            // Loop
            while (run)
            {
                if (Console.KeyAvailable)
                {
                    StisakTipke();
                }
            }
        } //Main

        // POMOCNE FUNKCIJE //

        private static void DiskMeni()
        {
            Console.WriteLine("+------+-------------+------------+-------------------------+");
            Console.WriteLine("| Disk | Veličina GB | Zauzeto GB | Slobodno GB | Zauzeto % |");
            Console.WriteLine("+------+-------------+------------+-------------+-----------+");
            diskovi = DriveInfo.GetDrives().Where(x => x.IsReady == true).ToArray(); // izbaci non-ready diskove (faulty, Linux patricije i sl)
            maxY = diskovi.Length + offsetY - 1; // Max broj redaka za ograničavanje pokazivača
            // Ispisi disk info
            IspisiDiskInfo();
            Console.SetCursorPosition(1, diskovi.Length / 2 + offsetY); // postavi pokazivač na pola
        }

        private static void OtvoriDisk ()
        {
            diskOtvoren = true;
            string direktorij = diskovi[Console.CursorTop - offsetY].Name;
            Console.Clear();
            DirectoryInfo dirInfo = new DirectoryInfo(direktorij);

            var datoteke = dirInfo.GetFiles();
            long velicina = 0;

            // ugasi blinker dok pišeš (za out of bounds error)
            blink.Enabled = false;
            maxY = offsetY + datoteke.Length - 1;
            Console.WriteLine("+------------------+-------------+---------+------------------------------------------+");
            Console.WriteLine("| Veličina       B |          KB |      MB | Nazivi datoteka                          |");
            Console.WriteLine("+------------------+-------------+---------+------------------------------------------+");
            foreach (FileInfo d in datoteke)
            {
                velicina += d.Length;
                Console.WriteLine("|{0, 15} B | {1, 8} KB | {2, 4} MB | {3,40} |",
                    d.Length,
                    d.Length / 1024,
                    d.Length / (1024 * 1024),
                    d.FullName);
            }
            Console.WriteLine("+------------------+-------------+---------+------------------------------------------+");
            Console.WriteLine("|{0, 15} B | {1, 8} KB | {2, 4} MB |                                          |",
                velicina,
                velicina / 1024,
                velicina / (1024 * 1024));
            Console.WriteLine("+------------------+-------------+---------+------------------------------------------+");

            Console.SetCursorPosition(1, datoteke.Length/2 + offsetY);
            blink.Enabled = true;
        }

        private static void StisakTipke()
        {
            var tipka = Console.ReadKey(true);
            switch (tipka.Key)
            {
                case ConsoleKey.DownArrow:
                    if (Console.CursorTop == maxY)
                        return;
                    Console.Write(" ");
                    Console.SetCursorPosition(cursorX, ++Console.CursorTop);
                    Console.Write(">");
                    Console.SetCursorPosition(cursorX, Console.CursorTop);
                    break;
                case ConsoleKey.UpArrow:
                    if (Console.CursorTop - offsetY == 0)
                        return;
                    Console.Write(" ");
                    Console.SetCursorPosition(cursorX, --Console.CursorTop);
                    Console.Write(">");
                    Console.SetCursorPosition(cursorX, Console.CursorTop);
                    break;
                case ConsoleKey.Enter:
                    if (!diskOtvoren)
                        OtvoriDisk();
                    break;
                case ConsoleKey.Backspace:
                    if (diskOtvoren)
                    {
                        diskOtvoren = false;
                        Console.Clear();
                        DiskMeni();
                    }
                    break;
                case ConsoleKey.Escape:
                    run = false;
                    break;
            }
        }

        private static Timer Izbornik()
        {
            Console.SetCursorPosition(1, 4);
            var t = new Timer
            {
                AutoReset = true,
                Interval = blinkInterval,
                Enabled = true,
            };
            t.Elapsed += Blink; // Zovi Blink u intervalima od Interval
            return t;
        }

        private static void Blink(object sender, ElapsedEventArgs e) // Ova funkcija se zove automatski svako "blinkInterval" milisekundi
        {
            var s = sender as Timer;
            if (!s.Enabled) // Spriječava da se pozove funkcija ako je Enabled stavljen na false nakon što je ciklus već u hodu
                return;
            if (cursorBlinkingState)
            {
                Console.Write(" ");
            }
            else {
                Console.Write(">");
            }
            Console.CursorLeft--;
            cursorBlinkingState = !cursorBlinkingState;
        }

        private static void IspisiDiskInfo()
        {
            foreach (var item in diskovi)
            {
                try { // Za slučaj lošeg diska
                    Console.WriteLine("| {0, 4} | {1, 8} GB | {2, 7} GB | {3, 8} GB | {4, 7} % |",
                    item.Name,
                    ByteToGb(item.TotalSize),
                    ByteToGb(item.TotalSize - item.TotalFreeSpace),
                    ByteToGb(item.TotalFreeSpace),
                    Math.Round(100 * (double)(item.TotalSize - item.TotalFreeSpace) / item.TotalSize), 2);
                } catch (Exception e)
                {
                   
                }
            }
        }

        private static double ByteToGb(long b) => Math.Round(b / Math.Pow(1024, 3), 2);
    }
}
