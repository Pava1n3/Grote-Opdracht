using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grote_Opdracht
{
    /// <summary>
    /// Class that represents a Route from Location 0 to Location 0.
    /// </summary>
    public class Route
    {
        // Constants
        private const int HALFFIVE = 41400;
        private const int DEPOT = 0;
        private const int DEPOTMATRIXID = 287;
        private const int MAXLOAD = 20000;
        private const int DUMPLOAD = 1800;
        // Objects
        private DistanceMatrix distanceMatrix;
        private List<Order> route;
        // Variables
        /// <summary>
        /// Holds the IDnumber of the truck that will process this route.
        /// </summary>
        private int truckID;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="distanceMatrix">Matrix that holds the distances.</param>
        /// <param name="truckID">IDnumber of the truck that will process this route.</param>
        public Route(DistanceMatrix distanceMatrix, int truckID)
        {
            this.distanceMatrix = distanceMatrix;
            this.truckID = truckID;
            route = new List<Order>();
        }

        /// <summary>
        /// Adds a destination to the route.
        /// </summary>
        public void AddDestination(Order destination)
        {
            route.Add(destination);
        }

        /// <summary>
        /// Checks whether a route is still feasible.
        /// </summary>
        public bool CheckRoute()
        {
            return (TotalTime() <= HALFFIVE && TotalLoad() <= MAXLOAD);
        }

        /// <summary>
        /// Returns the total time it takes to process this Route.
        /// </summary>
        public int TotalTime()
        {
            int totalTime = 0;
            int location = DEPOTMATRIXID;

            foreach (Order order in route)
            {
                totalTime += distanceMatrix.CheckDistance(location, order.matrixId) + order.totalEmptyingTime;

                location = order.matrixId;
            }

            totalTime += distanceMatrix.CheckDistance(location, DEPOTMATRIXID) + DUMPLOAD;

            return totalTime;
        }

        /// <summary>
        /// Returns the total load that will be in the truck during this Route.
        /// </summary>
        public int TotalLoad()
        {
            int totalLoad = 0;

            foreach (Order order in route)
            {
                totalLoad += order.numberOfContainers * order.volumeOfOneContainer / 5;
            }

            return totalLoad;
        }

        /// <summary>
        /// Prints the route to the console in the specified format.
        /// </summary>
        public void PrintOutput(Day day, int sequence)
        {
            foreach (Order order in route)
            {
                Console.WriteLine("{0}; {1}; {2}; {3}", truckID, day.DayNumber, sequence, order.orderId);
                sequence++;
            }

            Console.WriteLine("{0}; {1}; {2}; {3}", truckID, day.DayNumber, sequence, DEPOT);
        }

        /// <summary>
        /// Returns the number of elements that the route contains plus one.
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            return route.Count() + 1;
        }
    }
}
