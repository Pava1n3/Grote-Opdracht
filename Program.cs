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

        public void GetNeighbours(Solution solution)
        {
            /*Given a solution, it could iterate over the schedule contained within and generate as many neighbours as possible
             * For each job, create a solution without that job
             * For each job, try to add another route before this job (insert a new job without removing the current one)
             * For each job, switch it with each other job
             * Holy fuck this is going to be a lot of neighbours
             * 
             * How do we change when we list a ride to the depot?
             * 
             * We could keep a small list of ten jobs, five with the highest score and five random ones and choose from that list (thats not truly random I geuss)
             */


        }

        public void SimulatedAnnealing()
        {
            //we could feed this thing the neighbours we found and choose the next thing here
        }

        public void FindBaseSolution()
        {
            Solution baseSolution = new Solution();


        }
    }


    public class Solution    //describes a solution for the trash collection
    {
        public int cost = 0;
        public List<List<Job>> schedule = new List<List<Job>>();    //Lists of Jobs, which is from depot to depot 

        public Solution()
        {
        }

        public int CalculateCost(OrderMatrix orderMatrix, DistanceMatrix distanceMatrix)  //This version of the method might be a bit costly
        {
            foreach (List<Job> jobList in schedule)
            {
                foreach (Job job in jobList)
                {
                    cost += orderMatrix.GetOrderMatrix()[job.ordernr].totalEmptyingTime;   //Add the total emptying time
                    cost += distanceMatrix.GetDistanceMatrix()[orderMatrix.GetOrderMatrix()[job.previousJob.ordernr].matrixId, orderMatrix.GetOrderMatrix()[job.ordernr].matrixId];           //Add the traveling time

                }
            }

            foreach (KeyValuePair<int, Order> orderTuple in orderMatrix.GetOrderMatrix())
            {
                bool contains = false;

                foreach (List<Job> jobList in schedule)
                {
                   foreach (Job job in jobList)
                   {
                       if (job.ordernr == orderTuple.Key)
                           contains = true; ;
                   }
                }

                if (!contains)
                    cost += orderTuple.Value.totalEmptyingTime * 3;
            }

            return cost;
        }

        public void AddJob()
        {

        }

        public void RemoveJob()
        { }

        public void SwapJobs()
        { }

        public int Cost
        {
            get{ return cost; }
        }

        public List<List<Job>> GetSchedule()
        {
            return schedule;
        }

        //IsFeasible method?
        public Boolean IsFeasible(OrderMatrix orderMatrix, DistanceMatrix distanceMatrix)
        {
            int load = 0;

            foreach (List<Job> jobList in schedule)
            {
                foreach (Job job in jobList)
                {
                    if (orderMatrix.GetOrderMatrix()[job.ordernr].matrixId == 0)
                        load = 0;
                    else
                    {
                        load += orderMatrix.GetOrderMatrix()[job.ordernr].volumeOfOneContainer * orderMatrix.GetOrderMatrix()[job.ordernr].numberOfContainers;

                        if (load > 100000)
                            return false;
                    }
                }
            }

            return false;
        }

        //Truckload checker?
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

    public class DistanceMatrix
    {
        //public static Dictionary<string, Tuple<int, int>> distanceMatrix = new Dictionary<string, Tuple<int, int>>(); //This is the dictionairy we will store the AfstandenMatrix in. 
        public int[,] distanceMatrix = new int[1099, 1099];

        StreamReader distanceMatrixReader = new StreamReader(@"..\..\AfstandenMatrix.txt");   //Streamreaders can read from a text file
        //string distanceMatrixIndex;

        public void SetUpDistanceMatrix()
        {
            distanceMatrixReader.ReadLine();    //The first read is here because the first line is "MatrixID1;MatrixID2;Afstand;Rijtijd" and we don't need that line, by doing this we skip over it
            string distanceMatrixRead = distanceMatrixReader.ReadLine();    //We read a line from the text file
            string[] distanceMatrixLine;

            while (distanceMatrixRead != null)   //Goes over the whole text file and places each line in the array
            {
                distanceMatrixLine = distanceMatrixRead.Split(';');    //Split a string in pieces that are delimited by a semicolon (e.g. "hello;world;!" become an array [hello, world, !])
                //distanceMatrixIndex = distanceMatrixLine[0] + ';' + distanceMatrixLine[1];     //This is a dictionary that uses a string as a key, the key being a concatenation of MatrixID1 and MatrixID2

                //distanceMatrix[distanceMatrixIndex] = new Tuple<int, int>(Convert.ToInt32(distanceMatrixLine[2]), Convert.ToInt16(distanceMatrixLine[3]));    //Distance in meters and travel time in seconds
                distanceMatrix[Convert.ToInt32(distanceMatrixLine[0]), Convert.ToInt32(distanceMatrixLine[1])] = Convert.ToInt16(distanceMatrixLine[3]);

                distanceMatrixRead = distanceMatrixReader.ReadLine();
            }

            Console.WriteLine("We read the whole distance Matrix!");
        }

        public int[,] GetDistanceMatrix()
        {
            return distanceMatrix;
        }
    }

    public class OrderMatrix
    {
        public Dictionary<int, Order> orderMatrix = new Dictionary<int, Order>();
        StreamReader orderFileReader = new StreamReader(@"..\..\OrderBestand.txt");

        public void SetUpOrderMatrix()
        {
            int orderMatrixIndex;

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

        public Dictionary<int, Order> GetOrderMatrix()
        {
            return orderMatrix;
        }
    }
}
