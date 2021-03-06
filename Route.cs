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
        private const int WORKINGDAY = 43200;
        private const int DEPOTORDERID = 0;
        private const int DEPOTMATRIXID = 287;
        private const int MAXLOAD = 20000;
        private const int DUMPLOAD = 1800;
        // Objects
        private DistanceMatrix distanceMatrix;
        private List<Order> route;
        // Variables
        private int routeID;
        private double startTime;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="distanceMatrix">Matrix that holds the distances.</param>
        /// <param name="truckID">IDnumber of the truck that will process this route.</param>
        public Route(DistanceMatrix distanceMatrix, int routeID, double startTime)
        {
            this.distanceMatrix = distanceMatrix;
            this.routeID = routeID;
            this.startTime = startTime;
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
        public bool CheckRoute(double totalTime = 0, int totalLoad = 0)
        {
            if (totalTime == 0)
                totalTime = TotalTime();
            if (totalLoad == 0)
                totalLoad = TotalLoad();

            // Check if the load/time limit gets broken.
            return (startTime + totalTime <= WORKINGDAY && totalLoad <= MAXLOAD);
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
            if(route.Count > 0)
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

            // Add the load of each order to the total load for this route.
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
            // For each order...
            foreach (Order order in route)
            {
                // Print a line in the format: trucknumber; daynumber; sequence number; orderID
                sw.WriteLine("{0}; {1}; {2}; {3}", day.TruckID, day.DayNumber, sequence, order.orderId);
                // Increment the sequence after each print.
                sequence++;
            }
            // And end the route with a trip back to the depot.
            if(route.Count > 0)
                sw.WriteLine("{0}; {1}; {2}; {3}", day.TruckID, day.DayNumber, sequence, DEPOTORDERID);

        }

        /// <summary>
        /// Removes an order on the given index.
        /// </summary>
        /// <param name="index"></param>
        public void Remove(int index)
        {
            route.RemoveAt(index);
        }

        /// <summary>
        /// Returns the list of Orders.
        /// </summary>
        public List<Order> GetRoute
        {
            get { return route; }
        }

        /// <summary>
        /// Returns the routeID for this route.
        /// </summary>
        public int RouteID
        {
            get { return routeID; }
        }

        /// <summary>
        /// Returns the startingtime for this route.
        /// </summary>
        public double StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        /// <summary>
        /// Returns the number of elements that the route contains plus one.
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            // Returns the length, the plus one is because of the trip back to the depot that isn't specified in the routeList.
            return route.Count() + 1;
        }
    }
}
