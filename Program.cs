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

            double controlParameter = 260;
            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> outcome = new Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>(operation.Null, false, 0, null); //int/enum (swap,ins, del), bool improvement, double difference in time, day, route, index, order, || day2, route2, index2, order2. For high freq orders? LIST

            for (int x = 0; x < 10000; x++)
            {
                

                //Console.WriteLine("Attempt: {0} finished after {1} tries", x, attemptCounter);
                if (x % 1000 == 0)
                {
                controlParameter *= 0.99;
                }
            }

            StreamWriter sw = new StreamWriter(@"..\..\Solution.txt");
            weekSchedule.PrintCosts();
            weekSchedule.PrintOutput(sw);
            sw.Flush();

            Console.ReadKey();
        }
    }
}
