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
        public static Dictionary<string, Tuple<int, int>> distanceMatrix = new Dictionary<string, Tuple<int, int>>(); //This is the dictionairy we will store the AfstandenMatrix in. 

        static void Main(string[] args)
        {
            StreamReader distanceMatrixReader = new StreamReader(@"..\..\AfstandenMatrix.txt");   //Streamreaders can read from a text file

            #region ReadDistanceMatrix
            string distanceMatrixIndex;

            distanceMatrixReader.ReadLine();    //The first read is here because the first line is "MatrixID1;MatrixID2;Afstand;Rijtijd" and we don't need that line, by doing this we skip over it
            string distanceMatrixRead = distanceMatrixReader.ReadLine();    //We read a line from the text file
            string[] distanceMatrixLine;

            while(distanceMatrixRead != null)   //Goes over the whole text file and places each line in the array
            {
                distanceMatrixLine = distanceMatrixRead.Split(';');    //Split a string in pieces that are delimited by a semicolon (e.g. "hello;world;!" become an array [hello, world, !])
                distanceMatrixIndex = distanceMatrixLine[0] + ';' +  distanceMatrixLine[1];     //This is a dictionary that uses a string as a key, the key being a concatenation of MatrixID1 and MatrixID2

                distanceMatrix[distanceMatrixIndex] = new Tuple<int,int>(Convert.ToInt32(distanceMatrixLine[2]), Convert.ToInt16(distanceMatrixLine[3]));    //Distance in meters and travel time in seconds

                distanceMatrixRead = distanceMatrixReader.ReadLine();
            }

            Console.WriteLine("We read the whole distance Matrix!");
            #endregion 



            //beginoplossing maken

            //kostenfunctie



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


    public class Solution    //describes a solution for the trash collection
    {
        public int cost = 0;
        public List<Job> schedule = new List<Job>();

        public Solution()
        {
        }

        public int CalculateCost()  //This version of the method might be a bit costly
        {
            string distanceMatrixKey;

            foreach(Job job in schedule)
            {
                cost += OrderMatrix.orderMatrix[job.ordernr].totalEmptyingTime;   //Add the total emptying time
                distanceMatrixKey = OrderMatrix.orderMatrix[job.previousJob.ordernr].matrixId.ToString() + ";" + OrderMatrix.orderMatrix[job.ordernr].matrixId.ToString();
                cost += Program.distanceMatrix[distanceMatrixKey].Item1;           //Add the traveling time
            }

            return cost;
        }

        public int Cost
        {
            get { if (cost == 0) return CalculateCost(); else return cost; }
        }
    }

    public class Job             //An order (or job as I've decided to call it) as given in the Orderbestand.txt
    {
        public int day, ordernr, distanceMatrixId, vehicle;
        public Job previousJob = new Job(0, 33226, 0, 0);   //standard job is the waste disposal location

        public Job(int Day, int OrderNumber, int MatrixID, int VehicleNumber, Job PreviousJob = null)
        {
            day = Day;
            ordernr = OrderNumber;
            distanceMatrixId = MatrixID;
            vehicle = VehicleNumber;
            if(PreviousJob != null)
                previousJob = PreviousJob;
        }
    }

    public class Order
    {
        public int frequency, numberOfContainers, volumeOfOneContainer, totalEmptyingTime, matrixId, xCoördinate, yCoördinate;

        public Order()//int Frequency, int NumberOfContainers, int VolumeOfOneContainer, int TotalEmptyingTime, int MatrixId, int XCoördinate, int YCoördinate)
        {
            /*
            frequency = Frequency;
            numberOfContainers = NumberOfContainers;
            volumeOfOneContainer = VolumeOfOneContainer;
            totalEmptyingTime = TotalEmptyingTime;
            matrixId = MatrixId;
            xCoördinate = XCoördinate;
            yCoördinate = YCoördinate;
            */
        }

    }

    public static class OrderMatrix
    {
        public static Dictionary<int, Order> orderMatrix = new Dictionary<int, Order>();
        StreamReader orderFileReader = new StreamReader(@"..\..\OrderBestand.txt");

        public void ConstructOrderMatrix()
        {
            int orderMatrixIndex = 0;

            orderFileReader.ReadLine();     //The first read is here because the first line is "MatrixID1;MatrixID2;Afstand;Rijtijd" and we don't need that line, by doing this we skip over it
            string orderMatrixRead = orderFileReader.ReadLine();    //We read a line from the text file
            string[] orderMatrixLine;

            while (orderMatrixRead != null)
            {
                orderMatrixLine = orderMatrixRead.Split(';');    //Split a string in pieces that are delimited by a semicolon (e.g. "hello;world;!" become an array [hello, world, !])
                orderMatrixIndex = Convert.ToInt32(orderMatrixLine[0]);          //order Id


                //Do note that we don't store the location name, I don't see a reason to do so
                orderMatrix.Add(orderMatrixIndex, new Order());
                orderMatrix[orderMatrixIndex].frequency = Convert.ToInt16(orderMatrixLine[2][0]);                                        //frequency. This line is different because frequence is noted as 'XPWK'. We extract the first character (the number X denoting frequency) and convert it to an int. *you can treat strings as arrays in C# hence the [0] gets the first character
                orderMatrix[orderMatrixIndex].numberOfContainers = Convert.ToInt16(orderMatrixLine[3]);                                  //number of containers
                orderMatrix[orderMatrixIndex].volumeOfOneContainer = Convert.ToInt16(orderMatrixLine[4]);                                //volume of one container
                orderMatrix[orderMatrixIndex].totalEmptyingTime = (int)(Convert.ToDouble(orderMatrixLine[5].Replace('.', ',')) * 60);    //Hold on for a second, the total emptying time is in minutes. So we convert it to seconds
                orderMatrix[orderMatrixIndex].matrixId = Convert.ToInt16(orderMatrixLine[6]);                                            //MatrixId
                orderMatrix[orderMatrixIndex].xCoördinate = Convert.ToInt32(orderMatrixLine[7]);                                         //X Coördinate of the order location
                orderMatrix[orderMatrixIndex].yCoördinate = Convert.ToInt32(orderMatrixLine[8]);                                         //Y Coördinate of the order location

                orderMatrixRead = orderFileReader.ReadLine();
            }
        }
    }
}
