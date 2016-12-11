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
            //C:\Users\Marijn\Documents\INFOOPT\Grote Opdracht\bin\Debug\AfstandenMatrix.txt
            StreamReader distanceMatrixReader = new StreamReader(@"..\..\AfstandenMatrix.txt"), orderFileReader = new StreamReader(@"..\..\OrderBestand.txt");   //Streamreaders can read from a text file

            #region ReadDistanceMatrix
            int[,] distanceMatrix = new int[1207801,4];     //This is the array (matrix) we will store the AfstandenMatrix in. The 1207801 is how many lines there are in the AfstandenMatrix
            int distanceMatrixIndex = 0;

            distanceMatrixReader.ReadLine();    //The first read is here because the first line is "MatrixID1;MatrixID2;Afstand;Rijtijd" and we don't need that line, by doing this we skip over it
            string distanceMatrixRead = distanceMatrixReader.ReadLine();    //We read a line from the text file
            string[] distanceMatrixLine;

            while(distanceMatrixRead != null)   //Goes over the whole text file and places each line in the array
            {
                distanceMatrixLine = distanceMatrixRead.Split(';');    //Split a string in pieces that are delimited by a semicolon (e.g. "hello;world;!" become an array [hello, world, !])

                distanceMatrix[distanceMatrixIndex,0] = Convert.ToInt16(distanceMatrixLine[0]);    //MatrixId1
                distanceMatrix[distanceMatrixIndex,1] = Convert.ToInt16(distanceMatrixLine[1]);    //MatrixId2
                distanceMatrix[distanceMatrixIndex,2] = Convert.ToInt32(distanceMatrixLine[2]);    //Distance in meters
                distanceMatrix[distanceMatrixIndex,3] = Convert.ToInt16(distanceMatrixLine[3]);    //Travel time in seconds

                distanceMatrixRead = distanceMatrixReader.ReadLine();
            }

            Console.WriteLine("We read the whole distance Matrix!");
            #endregion 

            #region ReadOrderFile
            int[,] orderMatrix = new int[1177, 8];
            int orderMatrixIndex = 0;

            orderFileReader.ReadLine();     //The first read is here because the first line is "MatrixID1;MatrixID2;Afstand;Rijtijd" and we don't need that line, by doing this we skip over it
            string orderMatrixRead = orderFileReader.ReadLine();    //We read a line from the text file
            string[] orderMatrixLine;

            while(orderMatrixRead != null)
            {
                orderMatrixLine = orderMatrixRead.Split(';');    //Split a string in pieces that are delimited by a semicolon (e.g. "hello;world;!" become an array [hello, world, !])

                orderMatrix[orderMatrixIndex, 0] = Convert.ToInt32(orderMatrixLine[0]);          //order Id
                    //Do note that we don't store the location name, I don't see a reason to do so
                orderMatrix[orderMatrixIndex, 1] = Convert.ToInt16(orderMatrixLine[2][0]);       //frequency. This line is different because frequence is noted as 'XPWK'. We extract the first character (the number X denoting frequency) and convert it to an int. *you can treat strings as arrays in C# hence the [0] gets the first character
                orderMatrix[orderMatrixIndex, 2] = Convert.ToInt16(orderMatrixLine[3]);          //number of containers
                orderMatrix[orderMatrixIndex, 3] = Convert.ToInt16(orderMatrixLine[4]);          //volume of one container
                orderMatrix[orderMatrixIndex, 4] = (int)(Convert.ToDouble(orderMatrixLine[5].Replace('.', ',')) * 60);          //Hold on for a second, the emptying time is in minutes. So we convert it to seconds
                orderMatrix[orderMatrixIndex, 5] = Convert.ToInt16(orderMatrixLine[6]);          //MatrixId
                orderMatrix[orderMatrixIndex, 6] = Convert.ToInt32(orderMatrixLine[7]);          //X Coördinate of the order location
                orderMatrix[orderMatrixIndex, 7] = Convert.ToInt32(orderMatrixLine[8]);          //Y Coördinate of the order location

                orderMatrixRead = orderFileReader.ReadLine();
            }
            #endregion



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
        int id; 

        public Order()
        {

        }
    }
}
