using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grote_Opdracht
{
    class LocalSearch
    {
        //TECHNICALLY, DO WE NEED TO CHECK IF WE CAN CREATE A NEW ROUTE OR DELETE AN EMPTY ONE? IT'S RATHER UNLIKELY A ROUTE WILL BECOME COMPLETELY EMPTY THO AND IT WILL BE FILLED UP EVENTUALLY
        //ALSO, RUNNING MORE THAN TWO ROUTES IS RISKY AT BEST. THE TRIP FROM THE LASTPOINT OF ONE ROUTE TO THE FIRST OF A NEW ONE NEEDS TO EXCEED 30MINS PLUS THE DRIVE TIMES TO AND FROM THE DEPOT

        //Constants
        private const int HALFFIVE = 41400;
        private const int DEPOTMATRIXID = 287;
        private const int DUMPLOAD = 1800;
        private const int MAXLOAD = 20000;

        // Objects
        Week week;
        OrderMatrix orderMatrix;
        DistanceMatrix distanceMatrix;
        Random random = new Random();

        public LocalSearch(Week week, OrderMatrix orderMatrix, DistanceMatrix distanceMatrix)
        {
            this.week = week;
            this.orderMatrix = orderMatrix;
            this.distanceMatrix = distanceMatrix;
        }

        /// <summary>
        ///  Delete an order from the list and add it to the orderMatrix.
        /// </summary>
        public void DeleteOrder()
        {
            // Needed objects/variables.
            double maxGain = int.MaxValue;
            int tempOrderN = 0;
            Tuple<int, int> tempTuple = null;
            Order temp = null;

            // Check for possible deletions.
            for (int x = 0; x < 5; x++)
            {
                // Pick a random order from a random route.
                Tuple<int, int> tuple = SelectRandomRoute();
                Route route = week.GetWeek[tuple.Item1].GetRoutes[tuple.Item2];
                int orderN = random.Next(route.GetRoute.Count());
                // Check the totalTime if takes to complete the route with that order.
                double totalTime = route.TotalTime();

                // If the order has a frequency higher than 1, continue;
                Order order = route.GetRoute[orderN];
                if (order.frequency > 1)
                    continue;
                // Remove the order...
                route.Remove(orderN);
                // and check the time you gain with this deletion.
                double timeGain = totalTime - route.TotalTime();

                // If the gain is bigger than previous gains...
                if (timeGain <= maxGain)
                {
                    // Update the new maximum gain.
                    maxGain = timeGain;
                    
                    // If this isn't the first iteration, revert the changes from a previous maximum.
                    if (temp != null)
                    {
                        route = week.GetWeek[tempTuple.Item1].GetRoutes[tempTuple.Item2];
                        route.GetRoute.Insert(tempOrderN, temp);
                    }

                    // Save the data from the current maximum.
                    temp = order;
                    tempTuple = tuple;
                    tempOrderN = orderN;
                }
                // If the gain isn't bigger than previous iterations...
                else
                    // Revert the changes you made.
                    route.GetRoute.Insert(orderN, order);                
            }
            // Update the new routedata and add the removed order to the orderMatrix.
            week.GetWeek[tempTuple.Item1].UpdateRoutes();
            orderMatrix.GetOrderMatrix.Add(temp.orderId, temp);
        }

        /// <summary>
        /// Add an order from the orderMatrix to the schedule *currently the last one present in the orderMatrix
        /// </summary>
        public void AddOrder()
        {
            Order order = orderMatrix.GetOrderMatrix.Last().Value;
            
            List<Tuple<int, int, int, double>> possibleSpots = new List<Tuple<int, int, int, double>>();    //weekIndex, routeID, index in the route, gains

            bool spotFound = false;
            int weekIndex = 0, weekCounter = 0, newLoad, spotCounter = 0; //index keeps track of which day we are at, counter helps exiting the while if we have checked the whole week
            double totalTime, newTime = 0, dayTime;
            double chance = 70; //acceptance chance

            weekIndex = random.Next(week.GetWeek.Count);

            while (weekCounter < week.GetWeek.Count)
            {
                Day day = week.GetWeek[weekIndex];
                dayTime = day.DayTotalTime();

                //if a spot was possible at some point, accept it now
                //or make a list of all spots and choose at the end (cleaner)

                foreach(Route route in day.GetRoutes)
                {
                    totalTime = route.TotalTime();

                    for (int y = 0; y < route.GetRoute.Count; y++)
                    {
                        //If we're at the last entry, the next stop is the depot and we need to do things in a slightly different way
                        if (y == route.GetRoute.Count - 1)
                        {
                            //Calculate the new travel time for this route
                            newTime = totalTime - distanceMatrix.GetDistanceMatrix[route.GetRoute[y].matrixId, DEPOTMATRIXID] //distance from current route to depot
                                + distanceMatrix.GetDistanceMatrix[route.GetRoute[y].matrixId, order.matrixId]  //distance form curr to new order
                                + distanceMatrix.GetDistanceMatrix[order.matrixId, DEPOTMATRIXID]   //new order to depot
                                + order.totalEmptyingTime;  //order emptying time
                                //depot emptying time is already in totalTime
                        }
                        else
                        {
                            //Calculate the new travel time for this route
                            newTime = totalTime - distanceMatrix.GetDistanceMatrix[route.GetRoute[y].matrixId, route.GetRoute[y + 1].matrixId]
                                + distanceMatrix.GetDistanceMatrix[route.GetRoute[y].matrixId, order.matrixId]
                                + distanceMatrix.GetDistanceMatrix[order.matrixId, route.GetRoute[y + 1].matrixId]
                                + order.totalEmptyingTime;
                        }

                        //The new load, now includes the new order
                        newLoad = route.TotalLoad() + order.volumeOfOneContainer * order.numberOfContainers / 5;

                        //Check if the new route is feasible
                        if (route.CheckRoute(newTime, newLoad))
                        {
                            possibleSpots.Add(new Tuple<int, int, int, double>(weekIndex, route.RouteID, y, totalTime - newTime));
                        }
                    }
                }

                //So we don't go out of the week's bounds
                weekIndex++;
                weekCounter++;
                if (weekIndex >= week.GetWeek.Count)
                    weekIndex = 0;
            }

            //go over the possible spots and try adding the order with a chance based on the time change
            while (!spotFound && spotCounter < 3)
            {
                foreach (Tuple<int, int, int, double> spot in possibleSpots)
                {
                    if (chance + spot.Item4 / 100 > random.Next(101))
                    {
                        week.GetWeek[spot.Item1].GetRoutes[spot.Item2 - 1].GetRoute.Insert(spot.Item3, order);
                        orderMatrix.GetOrderMatrix.Remove(order.orderId);
                        spotFound = true;
                        break;
                    }
                }

                spotCounter++;
            }
        }

        /// <summary>
        /// Swap orders within a route.
        /// </summary>
        public void SwapLocalOrders()
        {
            // Pick a random route...   
            Tuple<int, int> tuple = SelectRandomRoute();
            Route route = week.GetWeek[tuple.Item1].GetRoutes[tuple.Item2];
            double totalTime = route.TotalTime();
            // And pick a random Order within that route.
            int orderN = random.Next(route.GetRoute.Count());
            if (orderN == route.GetRoute.Count()-1)
                return;

            // Swap that order with the next order in the list.
            Order temp = route.GetRoute[orderN];
            route.GetRoute[orderN] = route.GetRoute[orderN + 1];
            route.GetRoute[orderN + 1] = temp;
            // Update the starting times of each route within the day.
            week.GetWeek[tuple.Item1].UpdateRoutes();

            // Check if there's any gain from swapping...
            if (!(route.CheckRoute() && route.TotalTime() <= totalTime))
            {
                // If not, revert the changes.
                route.GetRoute[orderN + 1] = route.GetRoute[orderN];
                route.GetRoute[orderN] = temp;
                // And update the startingtimes again.
                week.GetWeek[tuple.Item1].UpdateRoutes();
            }
        }

        /// <summary>
        /// Returns a tuple with a random day and route.
        /// </summary>
        /// <returns></returns>
        public Tuple<int, int> SelectRandomRoute()
        {
            // Pick a random day and route.
            Tuple<int, int> tuple;
            int dayN = random.Next(week.GetWeek.Count());
            int routeN = random.Next(week.GetWeek[dayN].GetRoutes.Count());

            // If you've picked a day without any routes.
            if (week.GetWeek[dayN].GetRoutes.Count() == 0)
                // Look for another option.
                tuple = SelectRandomRoute();
            else
                // If you've a valid choice
                tuple = new Tuple<int, int>(dayN, routeN);

            // Return the tuple.
            return tuple;
        }
    }
}
