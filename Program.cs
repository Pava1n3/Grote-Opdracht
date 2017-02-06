using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Grote_Opdracht
{
    class Program
    {
        static void Main(string[] args)
        {
            OrderMatrix oM = new OrderMatrix();
            DistanceMatrix dM = new DistanceMatrix();

            Week weekSchedule = new Week();
            LocalSearch LS = new LocalSearch(weekSchedule, oM, dM);
            Truck Alpha = new Truck(oM, dM, weekSchedule, 1);
            Truck Beta = new Truck(oM, dM, weekSchedule, 2);

            for (int x = 1; x <= 5; x++)
            {
                Alpha.CreateDay(x);
                Beta.CreateDay(x);
            }

            // Do Local Search
            Random random = new Random();

            double ctrlPM = 260;
            double breakPoint = 1.5;
            int badResultCounter = 0;
            int iterationBlock = 64;
            int checker = 0;

            for (int x = 1; x <= 100000; x++)
            {
                // Every 4 * #interationBlock iterations, reset the counter.
                if (x % (4 * iterationBlock) == 0)
                {
                    double percentage = badResultCounter / 4 * iterationBlock * 100;
                    badResultCounter = 0;

                    // If the amount of accepted bad results is below the breakPoint value...
                    if (percentage < breakPoint)
                        // We stop the iteration process.
                        break;
                }

                // Every #interationBlock iterations, change SOMETHING.
                if (x % iterationBlock == 0)
                {
                    ctrlPM *= 0.99f;
                }

                bool op = LS.RandomOperation(1, 0.3, ctrlPM);

                if (!op)
                    badResultCounter++;

                checker = x;
            }

            StreamWriter sw = new StreamWriter(@"..\..\Solution.txt");
            weekSchedule.PrintCosts();
            weekSchedule.PrintOutput(sw);
            sw.Flush();
            Console.WriteLine("Number of Iterations: {0}", checker);

            double costs = 0;
            double decline = 0;
            costs += weekSchedule.Costs();
            foreach (KeyValuePair<int, Order> order in oM.GetOrderMatrix)
                decline += 3 * order.Value.totalEmptyingTime;
            Console.WriteLine("Total Costs: {0}", costs + decline);
            Console.WriteLine("Total Declines: {0}", decline);
            Console.WriteLine("Total Schedule: {0}", costs);


            Console.ReadKey();
        }
    }
}
