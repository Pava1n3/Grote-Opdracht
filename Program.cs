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
            int randomOperationChoice = -1;
            bool operationPerformed = false;

            for (int x = 0; x < 1000; x++)
            {
                while (!operationPerformed)
                {
                    randomOperationChoice = random.Next(3);

                    switch (randomOperationChoice)
                    {
                        case 0:
                            operationPerformed = LS.AddOrder();
                            break;
                        case 1:
                            operationPerformed = LS.Deletion();
                            break;
                        case 2:
                            operationPerformed = LS.AddOrder();
                            break;
                        default:
                            break;
                    }
                }

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
