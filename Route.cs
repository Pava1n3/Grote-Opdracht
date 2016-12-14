﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Grote_Opdracht
{
    /// <summary>
    /// Class that represents a Route from Location 0 to Location 0.
    /// </summary>
    public class Route
    {
        // Constants
        private const int HALFFIVE = 41400;
        private const int DEPOTORDERID = 0;
        private const int DEPOTMATRIXID = 287;
        private const int MAXLOAD = 20000;
        private const int DUMPLOAD = 1800;
        // Objects
        private DistanceMatrix distanceMatrix;
        private LinkedList<Order> route;
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
            route = new LinkedList<Order>();
        }

        /// <summary>
        /// Adds a destination to the route.
        /// </summary>
        public void AddDestination(Order destination)
        {
            route.AddLast(destination);
        }

        /// <summary>
        /// Checks whether a route is still feasible.
        /// </summary>
        public bool CheckRoute()
        {
            // Check if the load/time limit gets broken.
            return (TotalTime() <= HALFFIVE && TotalLoad() <= MAXLOAD);
        }

        /// <summary>
        /// Returns the total time it takes to process this Route.
        /// </summary>
        public double TotalTime()
        {
            // Set initial time to 0 and initial location to Depot.
            double totalTime = 0;
            int location = DEPOTMATRIXID;

            // Foreach order in the route, add the time it takes to process it.
            foreach (Order order in route)
            {
                totalTime += distanceMatrix.CheckDistance(location, order.matrixId) + order.totalEmptyingTime;

                location = order.matrixId;
            }

            // Finally return to the depot and empty the truckload.
            totalTime += distanceMatrix.CheckDistance(location, DEPOTMATRIXID) + DUMPLOAD;

            // Return the value.
            return totalTime;
        }

        /// <summary>
        /// Returns the total load that will be in the truck during this Route.
        /// </summary>
        public int TotalLoad()
        {
            // Set initial load to 0.
            int totalLoad = 0;

            // 
            foreach (Order order in route)
            {
                totalLoad += order.numberOfContainers * order.volumeOfOneContainer / 5;
            }

            return totalLoad;
        }

        /// <summary>
        /// Prints the route to the console in the specified format.
        /// </summary>
        public void PrintOutput(Day day, StreamWriter sw, int sequence)
        {
            foreach (Order order in route)
            {
                sw.WriteLine("{0}; {1}; {2}; {3}", truckID, day.DayNumber, sequence, order.orderId);
                Console.WriteLine("{0}; {1}; {2}; {3}", truckID, day.DayNumber, sequence, order.orderId);

                sequence++;
            }

            sw.WriteLine("{0}; {1}; {2}; {3}", truckID, day.DayNumber, sequence, DEPOTORDERID);
            Console.WriteLine("{0}; {1}; {2}; {3}", truckID, day.DayNumber, sequence, DEPOTORDERID);

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
