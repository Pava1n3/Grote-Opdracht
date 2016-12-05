using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SolverFoundation;
using Microsoft.SolverFoundation.Common;
using Microsoft.SolverFoundation.Services;
using System.IO;

namespace Grote_Opdracht
{
    class Program
    {
        static void Main(string[] args)
        {
            //TO DO
            //Het model begrijpen
            //Afstanden Matrix begrijpen/inlezen
            //Orderbestand begrijpen/inlezen

            /*AFSTANDEN MATRIX
             * from ; to ; distance in meters ; time needed for travel in seconds
             * e.g. 870 ; 880 ; 65 ; 40
             * Travel from location 870 to location 880 is 65 meters and costs you 40 seconds
             */

            //Suggestion: we gaan eerst rondrijden totdat we vol zijn

            //SolverContext context = SolverContext.GetContext();
            //Model model = context.CreateModel();

            //Decision  = new Decision(Domain.RealNonnegative, "");

            //model.AddDecisions();

            //model.AddConstraint();
            //model.AddConstraints();

            //model.AddGoal();

            /*OUTPUT FORMAT
             * Vehicle ; Day ; Sequence Number ; Order
             * 
             * Seq. nr. would just be : go to 1 first, 2 second, that's the route to take 
             */

            List<int> test = new List<int>();

            StreamWriter sw = new StreamWriter("Solution.txt");

            //Solution solution = context.Solve(new SimplexDirective());
            //Console.Write(solution.GetReport());
            //Console.ReadLine();

        }
    }

    public class ProblemSolution    //describes a solution for the trash collection
    {
        public int cost;
        //public List<Job> schedule = new List<Job>();

        public ProblemSolution()
        {
        }
    }

    public class Job                //describes one line in a solution, so a vehicle, day, sequence number and order number
    {
        int v, d, snr, onr;

        public Job(int vehicleId, int day, int sequenceNumber, int orderNumber)
        {
            v = vehicleId;
            d = day;
            snr = sequenceNumber;
            onr = orderNumber;
        }
    }

    public class Order             //An order as given in the Orderbestand.txt
    {
        //Order;Plaats;Frequentie;AantContainers;VolumePerContainer;LedigingsDuurMinuten;MatrixID;XCoordinaat;YCoordinaat

        public Order()
        {

        }
    }
}
