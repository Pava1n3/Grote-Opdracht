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

            //Do Local Search
            Random random = new Random();
            int randomOperationChoice = -1, MaximumAttempts = 8, attemptCounter = 0;
            bool operationPerformed = false;
            double controlParameter = 6;
            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> outcome = new Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>(operation.Null, false, 0, null); //int/enum (swap,ins, del), bool improvement, double difference in time, day, route, index, order, || day2, route2, index2, order2. For high freq orders? LIST

            for (int x = 0; x < 10000; x++)
            {
                //Console.WriteLine("============ Attempt: {0} started ===========", x);

                while (!operationPerformed && attemptCounter < MaximumAttempts)
                {
                    randomOperationChoice = random.Next(100);

                    LS.ShiftOrder();
                    operationPerformed = true;

                    //if(randomOperationChoice > 50)
                    //{
                    //    outcome = LS.Deletion();
                    //    if (outcome.Item1 != operation.Null)
                    //    {
                    //        operationPerformed = true;
                    //    }
                    //}
                    //else
                    //{
                    //    int y = 0;
                    //    while (!operationPerformed && y < 8)
                    //    {
                    //        outcome = LS.AddOrder();
                    //        if (outcome.Item1 != operation.Null)
                    //            operationPerformed = true;
                    //        y++;
                    //    }
                    //}

                    //ifs to determine chances
                    //if(randomOperationChoice > 90)
                    //{
                    //    for (int y = 0; y < 2; y++)
                    //        if (!operationPerformed)
                    //            operationPerformed = LS.SwapLocalOrders();
                    //}
                    //if(randomOperationChoice >= 80 + oM.GetOrderMatrix.Count / 2 && oM.GetOrderMatrix.Count < 30)
                    //{
                    //    outcome = LS.Deletion();
                    //    if (outcome.Item1 != operation.Null)
                    //    {
                    //        //neighbours.Add(outcome);
                    //        operationPerformed = true;
                    //    }
                    //}
                    //else if (randomOperationChoice < 50 + oM.GetOrderMatrix.Count && oM.GetOrderMatrix.Count > 0)
                    //{
                    //    int y = 0;
                    //    while (!operationPerformed && y < 8)
                    //    {
                    //        outcome = LS.AddOrder();
                    //        if (outcome.Item1 != operation.Null)
                    //            operationPerformed = true;
                    //        y++;
                    //    }
                    //}
                    //else if(randomOperationChoice > 50)
                    //{
                    //    outcome = LS.SwapOrder();
                    //    if(outcome.Item1 != operation.Null)
                    //        operationPerformed = true;
                    //}                    

                    attemptCounter++;
                }

                if (operationPerformed)
                    LS.DoOperation(outcome.Item1, outcome.Item4);

                //Console.WriteLine("Attempt: {0} finished after {1} tries", x, attemptCounter);

                controlParameter *= 0.999;

                attemptCounter = 0;
                operationPerformed = false;
            }

            StreamWriter sw = new StreamWriter(@"..\..\Solution.txt");
            weekSchedule.PrintCosts();
            weekSchedule.PrintOutput(sw);
            sw.Flush();

            Console.ReadKey();
        }

        //public operation operation
        //{
        //    get { return Grote_Opdracht.operation.Add; }
        //}
    }
}
