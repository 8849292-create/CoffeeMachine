using System;
using System.Linq;
using System.Text;
using System.Threading;

namespace CoffeeMachine
{
    internal class Program
    {
        const int Employes = 12;
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("\t\tКОФЕМАШИНА В ОФИСЕ");
            Console.WriteLine(new string('=', 60));

            int[] timeouts = {0,200,400,1000 };
            foreach(var t in timeouts)
            {
                RunExperiment(t);
            }
            Console.WriteLine("\npress any key...");
            Console.ReadKey();
        }
        static void RunExperiment(int timeout)
        {
            Console.WriteLine();
            Console.WriteLine($"========== ТАЙМАУТ {timeout} мс ==========");

            CoffeeMachinOffice machine = new CoffeeMachinOffice(Employes);
            Thread[] employees = new Thread[Employes];

            for (int i = 0; i < Employes; i++)
            {
                Employee emp = new Employee(i, machine, timeout);
                employees[i] = new Thread(emp.Work);

                employees[i].Start();
            }

            foreach (Thread thread in employees)
            {
                thread.Join();
            }

            Console.WriteLine();
            Console.WriteLine("--------------- СТАТИСТИКА ---------------");
            Console.WriteLine($"Выпито чашек: {machine.TotalCups}");
            Console.WriteLine($"Отказы:{machine.TotalRefusals}");
            Console.WriteLine($"Общее время ожидания: {machine.TotalWaitTime} мс\n");            
                        
            var sortedCups = machine.Statistics
                    .OrderByDescending(x => x.Cups)
                    .ToArray();

            Console.WriteLine($"{"Работник",-12} {"Чашки",-10} {"Отказы",-10} {"Ожидание",-15}");
            Console.WriteLine(new string('-', 50));

            foreach (var stat in sortedCups)
            {
                Console.WriteLine(
                    $"{stat.Id,-15}" +
                    $"{stat.Cups,-13}" +
                    $"{stat.Refusals,-10}" +
                    $"{stat.WaitTime,-15}");
            }
            Console.WriteLine(new string('-', 50));
        }

    }
}
