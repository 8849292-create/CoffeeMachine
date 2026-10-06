using System;
using System.Diagnostics;
using System.Threading;

namespace CoffeeMachine
{
    public class EmployeeStats
    {
        public int Id;
        public int Cups;
        public int Refusals;
        public long WaitTime;

        public EmployeeStats(int id)
        {
            Id = id;
        }
    }
    //-------------------------
    public class CoffeeMachinOffice
    {
        private int CupsCount;
        private const int MaxCups = 15;

        public int TotalCups;
        public int TotalRefusals;
        public long TotalWaitTime;

        public EmployeeStats[] Statistics;

        public CoffeeMachinOffice(int employeCount)
        {
            Statistics = new EmployeeStats[employeCount];
            for (int i = 0; i < employeCount; i++)
            {
                Statistics[i] = new EmployeeStats(i + 1);
            }
        }
        public void MakeCoffee(Random rdn)
        {
            int coffeeTime = rdn.Next(300, 701);
            Console.WriteLine($"  КОФЕМАШИНА: готовлю кофе ({coffeeTime} мс)");
            Thread.Sleep(coffeeTime);

            Interlocked.Increment(ref TotalCups);
            Console.WriteLine("  Кофе готов");
        }
        public void Addbeans()
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" Зерна закончились, ждемc...");
            Console.ResetColor();
            if (CupsCount == MaxCups)
            {
                Thread.Sleep(1000);
                CupsCount = 0;

                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine(" Зерна пополнены");
                Console.ResetColor();
            }
        }
        public bool TryGetCoffee(int employeeId, int timeout,Random rdn)
        {
            bool lockTaken = false;
            Stopwatch waitTimer = Stopwatch.StartNew();

            try
            {
                Monitor.TryEnter(this,timeout, ref lockTaken);
                long waitTime = waitTimer.ElapsedMilliseconds;

                if(!lockTaken)
                {
                    Interlocked.Increment(ref TotalRefusals);
                    Interlocked.Increment(ref Statistics[employeeId].Refusals);
                    Interlocked.Add(ref Statistics[employeeId].WaitTime, waitTime);
                    Interlocked.Add(ref TotalWaitTime, waitTime);
                    return false;
                }

                Interlocked.Add(ref TotalWaitTime, waitTime);
                Interlocked.Add(ref Statistics[employeeId].WaitTime, waitTime);

                int coffeTime = rdn.Next(300,701);
                Thread.Sleep(coffeTime);
                Interlocked.Increment(ref TotalCups);
                Interlocked.Increment(ref Statistics[employeeId].Cups);
                
                int cups = Interlocked.Increment(ref CupsCount);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Сотрудник {employeeId + 1}, взял кофе. Чашка № {cups}\n");
                Console.ResetColor();

                if (cups == MaxCups)
                {
                    Addbeans();
                }
                return true;
            }
            finally
            {
                if (lockTaken) Monitor.Exit(this);
            }
        }
    }  
    //-----------------------
    public class Employee
    {
        private readonly int Id;
        private readonly CoffeeMachinOffice Machine;
        private readonly int Timeout;

        public Employee(int id, CoffeeMachinOffice machine, int timeout)
        {
            Id = id;
            Machine = machine;
            Timeout = timeout;
        }

        public void Work()
        {
            Random rdn = new Random(Environment.TickCount + Id * 1000);
            DateTime endTime = DateTime.Now.AddSeconds(10);

            while (DateTime.Now < endTime)
            {
                Thread.Sleep(rdn.Next(500, 1501));
                if (DateTime.Now >= endTime)
                    break;

                bool gotCoffee = Machine.TryGetCoffee(Id,Timeout,rdn);
                if (!gotCoffee)
                {
                    Console.WriteLine($"   Сотрудник {Id + 1}: очередь, прииду потом...");
                    Thread.Sleep(rdn.Next(1000, 2001));
                }                
            }
        }
    }
}
