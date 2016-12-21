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
        enum operation { Add, Swap, Delete };

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

            //Do Local Search
            Random random = new Random();
            int randomOperationChoice = -1, MaximumAttempts = 5, attemptCounter = 0;
            bool operationPerformed = false;
            List<Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>> neighbours;        //int/enum (swap,ins, del), bool improvement, double score, day, route, index, order, || day2, route2, index2, order2. For high freq orders? LIST

            for (int x = 0; x < 300; x++)
            {
                while (!operationPerformed && attemptCounter < MaximumAttempts)
                {
                    randomOperationChoice = random.Next(3);

                    
                    operationPerformed = LS.SwapOrder();

                    attemptCounter++;
                }

                attemptCounter = 0;
                operationPerformed = false;
            }

            StreamWriter sw = new StreamWriter(@"..\..\Solution.txt");
            weekSchedule.PrintCosts();
            weekSchedule.PrintOutput(sw);
            sw.Flush();

            Console.ReadKey();
        }
    }
}
