using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grote_Opdracht
{
    public class Truck
    {
        // Constants
        private const int MINLOAD = 240/5;
        private const int HALFFIVE = 41400;
        private const int DEPOTORDERID = 0;
        private const int DEPOTMATRIXID = 287;
        private const int MAXLOAD = 20000;
        private const int DUMPLOAD = 1800;

        // Objects
        OrderMatrix orderMatrix;
        DistanceMatrix distanceMatrix;
        Week week;

        // Variables
        /// <summary>
        /// Integer that holds the current position of the truck.
        /// </summary>
        private int currentPosition = DEPOTMATRIXID;
        /// <summary>
        /// Flags that we use to stop adding orders/routes.
        /// </summary>
        private bool routeCompleted, dayCompleted;
        /// <summary>
        /// The IDnumber of this truck.
        /// </summary>
        private int truckID;
        /// <summary>
        /// Double that holds the current time of day.
        /// </summary>
        double currentTime = 0;
        /// <summary>
        /// Integer that holds the current truckload.
        /// </summary>
        int currentLoad = 0;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="orderMatrix">Matrix that holds orders.</param>
        /// <param name="distanceMatrix">Matrix that holds the distances.</param>
        /// <param name="week">Object that represents the week.</param>
        /// <param name="truckID">The IDnumber the truck will have.</param>
        public Truck(OrderMatrix orderMatrix, DistanceMatrix distanceMatrix, Week week, int truckID)
        {
            this.orderMatrix = orderMatrix;
            this.distanceMatrix = distanceMatrix;
            this.week = week;
            this.truckID = truckID;
        }

        /// <summary>
        /// Class that creates the workingday for the truck.
        /// </summary>
        public void CreateDay(int dayNumber)
        {
            Day day = new Day(dayNumber);

            while (!dayCompleted)
            {
                CreateRoute(day);
            }

            week.AddDay(day);
            dayCompleted = false;
            currentTime = 0;
        }

        /// <summary>
        /// Class that creates a single route for the truck.
        /// </summary>
        public void CreateRoute(Day day)
        {
            // Create a new route
            Route route = new Route(distanceMatrix, truckID);

            // While the route isn't complete...
            while (!routeCompleted)
            {
                // keep adding orders.
                NextOrder(route);
            }

            if (!dayCompleted)
                day.AddRoute(route);
            // When the route is complete, add it to the workingday.
            // Reset the flag.
            routeCompleted = false;
        }

        /// <summary>
        /// Places a next order in the route.
        /// </summary>
        /// <returns></returns>
        private void NextOrder(Route route)
        {
            int orderID = CheckNextOrder();

            // If the next destination is the Depot...
            if (orderID == DEPOTORDERID)
            {
                // If the currentposition and the next destination are both the depot...
                if (currentPosition == DEPOTMATRIXID)
                {
                    // The workingday is complete and update the flags accordingly.
                    routeCompleted = true;
                    dayCompleted = true;
                    return;
                }
                else
                {
                    // Drive to the depot...
                    currentTime += distanceMatrix.CheckDistance(currentPosition, DEPOTMATRIXID);
                    // Dump the current load and update the values.
                    currentTime += DUMPLOAD;
                    currentLoad = 0;
                    currentPosition = DEPOTMATRIXID;
                    // Update the flag.
                    routeCompleted = true;
                }
            }
            // Otherwise
            else
            {
                // Get the order from the OrderMatrix.
                Order order = orderMatrix.orderMatrix[orderID];
                // Update the position, time and load.
                currentTime += distanceMatrix.CheckDistance(currentPosition, order.matrixId) + orderMatrix.TotalEmptyingTime(orderID);
                currentLoad += order.numberOfContainers * order.volumeOfOneContainer / 5;
                currentPosition = orderMatrix.GetMatrixID(orderID);
                // Remove the order from the OrderMatrix.
                orderMatrix.CompleteOrder(orderID);

                // Add the order to the route.
                route.AddDestination(order);
            }
        }

        /// <summary>
        /// Checks whether an order is viable.
        /// </summary>
        /// <param name="currentTime">The current time of day in seconds.</param>
        /// <returns></returns>
        private int CheckNextOrder()
        {
            // Needed variables
            List<int> rejects = new List<int>();
            int orderID = -1;

            while (orderID == -1)
            {
                // Find a new possibility.
                int possibility = SearchNearestOrder(rejects);

                // If there isn't a possibilty or the space available is smaller than the smallest possible volume...
                if (possibility == -1 || MAXLOAD - currentLoad < MINLOAD)
                {
                    // set the next destination to the Depot.
                    orderID = DEPOTORDERID;
                    break;
                }
                // Else check if the possibilty is viable, when it is...
                else if (CheckVolume(possibility) && CheckTime(possibility))
                    // set it as the next destination.
                    orderID = possibility;
                // Otherwise add it to the list of rejects and continue searching.
                else
                    rejects.Add(possibility);
            }

            return orderID;
        }

        /// <summary>
        /// Searches for the nearest order from the current position.
        /// </summary>
        /// <returns></returns>
        private int SearchNearestOrder(List<int> rejects)
        {
            int minDistance = int.MaxValue; // Set to maxValue to compare.
            int orderID = -1;               // If no new nearest order has been found, return -1.


            // Iterates through the order matrix...
            foreach (KeyValuePair<int, Order> order in orderMatrix.orderMatrix)
            {
                // If an orderID is present in the list of rejects, skip that order.
                if (rejects.Contains(order.Key) || order.Value.frequency > 1)
                    continue;

                // Check the distance.
                int distance = distanceMatrix.CheckDistance(currentPosition, order.Value.matrixId);

                // If the distance is smaller than the smallest distance found yet, change values accordingly.
                if (distance <= minDistance)
                {
                    minDistance = distance;
                    orderID = order.Key;
                }
            }

            return orderID;
        }

        /// <summary>
        /// Checks if an order is within the space limit of the truck.
        /// </summary>
        /// <param name="orderID">OrderID for the order that you want checked.</param>
        /// <param name="currentLoad">The current load we need to check against.</param>
        /// <returns></returns>
        private bool CheckVolume(int orderID)
        {
            // Calculate remaining space.
            int space = MAXLOAD - currentLoad;

            // Check if there is enough space.
            return orderMatrix.TotalVolume(orderID) / 5 < space;
        }

        /// <summary>
        /// Checks if an order is within reach without breaking the workingday timelimit.
        /// </summary>
        /// <param name="orderID">OrderID for the order that you want checked.</param>
        /// <param name="currentTime">The current time we need to check against.</param>
        /// <returns></returns>
        private bool CheckTime(int orderID)
        {
            // Add the Travelingtime to the Destination, Emptyingtime and Travelingtime back to Depot.
            double totalTime = distanceMatrix.CheckDistance(currentPosition, orderMatrix.GetMatrixID(orderID)) + 
                               orderMatrix.TotalEmptyingTime(orderID) + 
                               distanceMatrix.CheckDistance(orderMatrix.GetMatrixID(orderID), DEPOTMATRIXID);

            // Check if it breaks the limit.
            return currentTime + totalTime <= HALFFIVE;
        }
    }
}
