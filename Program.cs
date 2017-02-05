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

            //int randomOperationChoice = -1, MaximumAttempts = 8, attemptCounter = 0;
            //bool operationPerformed = false;
            //double controlParameter = 260;

            //int/enum (swap,ins, del), bool improvement, double difference in time, day, route, index, order, || day2, route2, index2, order2. For high freq orders? LIST
            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> outcome = new Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>(operation.Null, false, 0, null);

            for (int x = 1; x <= 1000000; x++)
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

                outcome = LS.RandomOperation(0.1, 0.3, ctrlPM);
                if (!outcome.Item2)
                    badResultCounter++;

                LS.DoOperation(outcome.Item1, outcome.Item4);
                checker = x;
            }

            StreamWriter sw = new StreamWriter(@"..\..\Solution.txt");
            weekSchedule.PrintCosts();
            weekSchedule.PrintOutput(sw);
            sw.Flush();

            Console.WriteLine("Number of Iterations: {0}", checker);

            Console.ReadKey();
        }
    }
}
