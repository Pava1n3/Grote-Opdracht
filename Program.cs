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
            int randomOperationChoice = -1, MaximumAttempts = 8, attemptCounter = 0;
            bool operationPerformed = false;
            //List<Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>> neighbours;        //int/enum (swap,ins, del), bool improvement, double score, day, route, index, order, || day2, route2, index2, order2. For high freq orders? LIST

            for (int x = 0; x < 30000; x++)
            {
                //Console.WriteLine("============ Attempt: {0} started ===========", x);

                while (!operationPerformed && attemptCounter < MaximumAttempts)
                {
                    randomOperationChoice = random.Next(102);

                    //ifs to determine chances
                    if(randomOperationChoice >= 70 + oM.GetOrderMatrix.Count / 2 && oM.GetOrderMatrix.Count < 50)
                    {
                        operationPerformed = LS.Deletion();
                    }
                    else if (randomOperationChoice < 40 + oM.GetOrderMatrix.Count && oM.GetOrderMatrix.Count > 0)
                    {
                        for (int y = 0; y < 8; y++)
                            if (!operationPerformed)
                                operationPerformed = LS.AddOrder();
                    }
                    else if(randomOperationChoice > 50)
                    {
                        operationPerformed = LS.SwapOrder();
                    }
                    else
                    {
                        for (int y = 0; y < 2; y++)
                            if (!operationPerformed)
                                operationPerformed = LS.SwapLocalOrders();
                    }

                    attemptCounter++;
                }

                //Console.WriteLine("Attempt: {0} finished after {1} tries", x, attemptCounter);

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
