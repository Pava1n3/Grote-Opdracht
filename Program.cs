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

            double ctrlPM = 360;
            double breakPoint = 0.0;
            double badResultCounter = 0;
            double iterationBlock = 128;
            double totalBCounter = 0;
            int checker = 0;

            for (int x = 1; x <= 5000000; x++)
            {
                // Every 4 * #interationBlock iterations, reset the counter.
                if (x % (4 * iterationBlock) == 0)
                {
                    double percentage = (badResultCounter) / (4.0 * iterationBlock) * 100;
                    badResultCounter = 0;

                    //Console.WriteLine("Percentage: {0}", percentage);

                    // If the amount of accepted bad results is below the breakPoint value...
                    if (percentage < breakPoint)
                    {
                        // We stop the iteration process.
                        break;
                    }
                }

                // Every #interationBlock iterations, change SOMETHING.
                if (x % iterationBlock == 0)
                {
                    ctrlPM *= 0.99f;
                }

                bool op = LS.RandomOperation(0.2, 0.2, 0.2, ctrlPM);
                if (!op)
                { badResultCounter++; totalBCounter++; }

                checker = x;

                if (x % 1000 == 0)
                    Console.WriteLine("Number of Iterations: {0}", checker);
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
            Console.WriteLine("Total Costs:    {0} | {1}", costs + decline, (costs + decline) / 60);
            Console.WriteLine("Total Declines: {0} | {1}", decline, decline / 60);
            Console.WriteLine("Total Schedule: {0} | {1}", costs, costs / 60);
            Console.WriteLine("Total Adds:     {0} | {1}", LS.adds, LS.bAdds);
            Console.WriteLine("Total Deletes:  {0} | {1}", LS.deletes, LS.bDeletes);
            Console.WriteLine("Total Shifts:   {0} | {1}", LS.shifts, LS.bShifts);
            Console.WriteLine("Total Swaps:    {0} | {1}", LS.swaps, LS.bSwaps);
            Console.WriteLine("Total Accepted: {0}", totalBCounter);


            Console.ReadKey();
        }
    }
}
