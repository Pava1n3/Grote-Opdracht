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
        private const int MINLOAD = 140/5;
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
            // Start a new day...
            Day day = new Day(dayNumber);

            // While the day hasn't ended, keep creating routes.
            while (!dayCompleted)
                CreateRoute(day);

            // When the day is over, add the day to the week.
            week.AddDay(day);
            // Reset the flag and time.
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
                // keep adding orders.
                NextOrder(route, day);

            if (!dayCompleted)
                day.AddRoute(route);
            // When the route is complete, add it to the workingday.
            // Reset the flag.
            routeCompleted = false;
        }


        /// <summary>
        /// Places a next order in the route.
        /// </summary>
        /// <param name="route">The route where the order has to be added.</param>
        private void NextOrder(Route route, Day day)
        {
            int orderID = CheckNextOrder(day);

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
                    //Drive to the depot...
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
                Order order = orderMatrix.GetOrderMatrix[orderID];
                // Update the position, time and load.
                currentTime += distanceMatrix.CheckDistance(currentPosition, order.matrixId) + orderMatrix.TotalEmptyingTime(orderID);
                currentLoad += order.numberOfContainers * order.volumeOfOneContainer / 5;
                currentPosition = orderMatrix.GetMatrixID(orderID);
                // Complete the order in the OrderMatrix.
                orderMatrix.GetOrderMatrix[orderID].SetBit(day.DayNumber, true);
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
        private int CheckNextOrder(Day day)
        {
            // Needed variables
            List<int> rejects = new List<int>();
            int orderID = -1;

            while (orderID == -1)
            {
                // Find a new possibility.
                int possibility = SearchNearestOrder(rejects, day);
                // If there isn't a possibilty or the space available is smaller than the smallest possible volume...
                if (possibility == -1 || MAXLOAD - currentLoad < MINLOAD)
                {
                    // set the next destination to the Depot.
                    orderID = DEPOTORDERID;
                    break;
                }
                // Else check if the possibilty is viable, when it is...
                else if (CheckVolume(possibility) && CheckTime(possibility) && CheckFrequency(possibility, day))
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
        private int SearchNearestOrder(List<int> rejects, Day day)
        {
            int minDistance = int.MaxValue; // Set to maxValue to compare.
            int priorityDistance = int.MaxValue;
            int orderID = -1;               // If no new nearest order has been found, return -1.
            bool priority = false;          // Orders with a frequency number > 1 that already have been processed before have priority.

            // Iterates through the order matrix...
            foreach (KeyValuePair<int, Order> order in orderMatrix.GetOrderMatrix)
            {
                // If an orderID is present in the list of rejects, skip that order.
                if (rejects.Contains(order.Key))
                    continue;

                // Check the distance.
                int distance = distanceMatrix.CheckDistance(currentPosition, order.Value.matrixId);

                // If there is a priority order...
                if (order.Value.processed)
                {
                    // set flag...
                    priority = true;
                    // and if the distance is smaller than the smallest priorityDistance found yet, change values accordingly.
                    if (distance <= priorityDistance)
                    {
                        priorityDistance = distance;
                        orderID = order.Key;
                    }

                }
                // If the distance is smaller than the smallest distance found yet, change values accordingly.
                else if (distance <= minDistance && !priority)
                {
                    minDistance = distance;
                    orderID = order.Key;
                }
            }

            return orderID;
        }

        /// <summary>
        /// Checks if an order is allowed to be planned according to its frequency and previously planned occurances
        /// </summary>
        /// <returns></returns>
        private bool CheckFrequency(int orderID, Day day)
        {
            // If the order has been processed already on the given day, return false 
            if (orderMatrix.GetOrderMatrix[orderID].GetBit(day.DayNumber - 1))
                return false;

            // Based on the frequency of the given orderID, switch...
            switch(orderMatrix.GetFrequency(orderID))
            {
                // if the frequency is 1, return true...
                case 1:
                    return true;
                // if the frequency is 2...
                case 2:
                    // Monday, return true.
                    if (day.DayNumber == 1)
                        return true;
                    // Tuesday, check if it has been processed before.
                    else if (day.DayNumber == 2)
                        if (orderMatrix.GetOrderMatrix[orderID].GetBit(0))
                            return false;
                        else
                            return true;
                    // Wednesday, return false.
                    else if (day.DayNumber == 3)
                        return false;
                    // Thursday, check if it has been processed on the Monday.
                    else if (day.DayNumber == 4)
                        if (orderMatrix.GetOrderMatrix[orderID].GetBit(0))
                            return true;
                        else
                            return false;
                    // Friday, check if it has been processed on the Tuesday.
                    else if (day.DayNumber == 5)
                        if (orderMatrix.GetOrderMatrix[orderID].GetBit(1))
                            return true;
                        else
                            return false;
                    break;
                // if the frequency is 3...
                case 3:
                    // Monday, return true.
                    if (day.DayNumber == 1)
                        return true;
                    // Tuesday, return false.
                    else if (day.DayNumber == 2)
                        return false;
                    // Wednesday, check if it has been processed on Monday.
                    else if (day.DayNumber == 3)
                        if (orderMatrix.GetOrderMatrix[orderID].GetBit(0))
                            return true;
                        else
                            return false;
                    // Thursday, return false.
                    else if (day.DayNumber == 4)
                        return false;
                    // Friday, check if it has been processed on Monday and Wednesday.
                    else if (day.DayNumber == 5)
                        if (orderMatrix.GetOrderMatrix[orderID].GetBit(0) && orderMatrix.GetOrderMatrix[orderID].GetBit(2))
                            return true;
                        else
                            return false;
                        break;
                // if the frequency is 4,
                case 4:
                    // Monday, return true.
                    if (day.DayNumber == 1)
                        return true;
                    // Tuesday, return true.
                    else if (day.DayNumber == 2)
                        return true;
                    // Wednesday, check if it has been processed atleast once.
                    else if (day.DayNumber == 3)
                        if (orderMatrix.GetOrderMatrix[orderID].DaysProcessed(3) > 0)
                            return true;
                        else
                            return false;
                    // Thursday, check if it has been processed atleast twice.
                    else if (day.DayNumber == 4)
                        if (orderMatrix.GetOrderMatrix[orderID].DaysProcessed(3) > 1)
                            return true;
                        else
                            return false;
                    // Friday, check if it has been processed three times.
                    else if (day.DayNumber == 5)
                        if (orderMatrix.GetOrderMatrix[orderID].DaysProcessed(3) == 3)
                            return true;
                        else
                            return false;
                        break;
                // if the frequency is 5, return true...
                case 5:
                    if (day.DayNumber == 1)
                        return true;
                    // Tuesday, check if it has been processed once.
                    else if (day.DayNumber == 2)
                        if (orderMatrix.GetOrderMatrix[orderID].DaysProcessed(2) == 1)
                            return true;
                        else
                            return false;
                    // Wednesday, check if it has been processed atleast twice.
                    else if (day.DayNumber == 3)
                        if (orderMatrix.GetOrderMatrix[orderID].DaysProcessed(3) == 2)
                            return true;
                        else
                            return false;
                    // Thursday, check if it has been processed three times.
                    else if (day.DayNumber == 4)
                        if (orderMatrix.GetOrderMatrix[orderID].DaysProcessed(4) == 3)
                            return true;
                        else
                            return false;
                    // Friday, check if it has been processed four times.
                    else if (day.DayNumber == 5)
                        if (orderMatrix.GetOrderMatrix[orderID].DaysProcessed(5) == 4)
                            return true;
                        else
                            return false;
                    break;
            }
            // If the frequency isn't a value within the range 1-5, return false;
            return false;
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
