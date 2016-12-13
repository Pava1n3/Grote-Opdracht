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

            oM.SetUpOrderMatrix();
            dM.SetUpDistanceMatrix();

            //beginoplossing maken


            /*AFSTANDEN MATRIX
             * from ; to ; distance in meters ; time needed for travel in seconds
             * e.g. 870 ; 880 ; 65 ; 40
             * Travel from location 870 to location 880 is 65 meters and costs you 40 seconds
             */

            //Suggestion: we gaan eerst rondrijden totdat we vol zijn


            /*OUTPUT FORMAT
             * Vehicle ; Day ; Sequence Number ; Order
             * 
             * Seq. nr. would just be : go to 1 first, 2 second, that's the route to take 
             */


            StreamWriter sw = new StreamWriter("Solution.txt");
        }
    }
}
