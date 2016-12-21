using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grote_Opdracht
{
    class LocalSearch
    {
        //TECHNICALLY, DO WE NEED TO CHECK IF WE CAN CREATE A NEW ROUTE OR DELETE AN EMPTY ONE? IT'S RATHER UNLIKELY A ROUTE WILL BECOME COMPLETELY EMPTY AND IT WILL BE FILLED UP EVENTUALLY
        //ALSO, RUNNING MORE THAN TWO ROUTES IS RISKY AT BEST. THE TRIP FROM THE LASTPOINT OF ONE ROUTE TO THE FIRST OF A NEW ONE NEEDS TO EXCEED 30MINS PLUS THE DRIVE TIMES TO AND FROM THE DEPOT

        /* Add a random order, at a fairly random position (AddOrder)
         * Delete a random order (DeleteOrder)
         * Swap two random orders
         * Put an order in a different time of the route/day (SwapLocalOrders)
         */

        //Constants
        private const int HALFFIVE = 41400;
        private const int DEPOTMATRIXID = 287;
        private const int DUMPLOAD = 1800;
        private const int MAXLOAD = 20000;
        private int WORKINGDAY = 43200;
        enum operation { Add, Swap, Delete };

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
        public bool DeleteOrder()
        {
            // Needed objects/variables.
            double maxGain = int.MaxValue;
            int tempOrderN = 0, orderN = -1;
            Tuple<int, int> tempTuple = null;
            Order temp = null;
            double timeGain = 0;
            Order order = null;
            Route route = null;
            bool deletionDone = false;

            // Check for possible deletions.
            for (int x = 0; x < 5; x++)
            {
                // Pick a random order from a random route.
                Tuple<int, int> tuple = SelectRandomRoute();
                route = week.GetWeek[tuple.Item1].GetRoutes[tuple.Item2];
                orderN = random.Next(route.GetRoute.Count());
                // Check the totalTime if takes to complete the route with that order.
                double totalTime = route.TotalTime();

                // If the order has a frequency higher than 1, continue;
                if (route.GetRoute[orderN].frequency > 1)
                    continue;
                order = route.GetRoute[orderN];
                // Remove the order...
                route.Remove(orderN);
                // and check the time you gain with this deletion.
                timeGain = totalTime - route.TotalTime();

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

            //Run a test to see if the new change will pass
            if (order != null)
            {
                if (timeGain > 3 * order.totalEmptyingTime * order.frequency) //unlikely, but if the route was really bad it might be an improvement not to do it
                {
                    deletionDone = true;
                    // Update the new routedata and add the removed order to the orderMatrix.
                    week.GetWeek[tempTuple.Item1].UpdateRoutes();
                    orderMatrix.GetOrderMatrix.Add(temp.orderId, temp);
                }
                else if(75 + timeGain / 100 > random.Next(101))//only accept it with a certain chance
                {
                    deletionDone = true;
                    // Update the new routedata and add the removed order to the orderMatrix.
                    week.GetWeek[tempTuple.Item1].UpdateRoutes();
                    orderMatrix.GetOrderMatrix.Add(temp.orderId, temp);
                }
                else //revert the last delete, we have not succeeded
                {
                    orderN = Math.Min(route.GetRoute.Count, orderN);    //while this might seem weird, I had a crash where orderN was 100 while the route had 92 entries (how)

                    route.GetRoute.Insert(orderN, order);
                }
            }

            return deletionDone;
        }

        /// <summary>
        /// Difference here is that we stay in the same route
        /// </summary>
        /// <returns></returns>
        public bool Deletion()
        {
            bool deletionDone = false;
            int routeIndex = 0, routeCount, currentBest = -1;
            double timeGain = double.MaxValue, totalTime, newTime;

            Tuple<int, int> randomRoute = SelectRandomRoute();
            Route route = week.GetWeek[randomRoute.Item1].GetRoutes[randomRoute.Item2];
            Order order = null, bestOrder = null;
            totalTime = route.TotalTime();
            routeCount = route.GetRoute.Count;

            for (int x = 0; x < 5; x++)
            {
                if (routeCount == 0)
                  return false;

                routeIndex = random.Next(routeCount);
                order = route.GetRoute[routeIndex];
                if (order.frequency > 1)
                    continue;

                if(routeCount == 1)
                {
                    newTime = totalTime;
                }
                else if(routeIndex == 0)
                {
                    newTime = totalTime - distanceMatrix.GetDistanceMatrix[DEPOTMATRIXID, order.matrixId]
                                        - order.totalEmptyingTime
                                        + distanceMatrix.GetDistanceMatrix[DEPOTMATRIXID, route.GetRoute[routeIndex + 1].matrixId];
                }
                else if(routeIndex == routeCount - 1)
                {
                    newTime = totalTime - distanceMatrix.GetDistanceMatrix[route.GetRoute[routeIndex - 1].matrixId, order.matrixId]
                                        - distanceMatrix.GetDistanceMatrix[order.matrixId, DEPOTMATRIXID]
                                        - order.totalEmptyingTime
                                        + distanceMatrix.GetDistanceMatrix[route.GetRoute[routeIndex - 1].matrixId, DEPOTMATRIXID];
                }
                else
                {
                    newTime = totalTime - distanceMatrix.GetDistanceMatrix[route.GetRoute[routeIndex - 1].matrixId, order.matrixId]
                                        - distanceMatrix.GetDistanceMatrix[order.matrixId, route.GetRoute[routeIndex + 1].matrixId]
                                        - order.totalEmptyingTime
                                        + distanceMatrix.GetDistanceMatrix[route.GetRoute[routeIndex - 1].matrixId, route.GetRoute[routeIndex + 1].matrixId];
                }

                if (totalTime - newTime < timeGain)
                {
                    timeGain = totalTime - newTime;
                    currentBest = routeIndex;
                    bestOrder = order;
                }
            }

            if (currentBest != -1)
            {
                if (timeGain >= 3 * order.totalEmptyingTime * order.frequency)
                {
                    route.Remove(currentBest);
                    week.GetWeek[randomRoute.Item1].UpdateRoutes();
                    orderMatrix.GetOrderMatrix.Add(bestOrder.orderId, bestOrder);
                    deletionDone = true;
                }
                else if (101 > random.Next(101))
                {
                    route.Remove(currentBest);
                    week.GetWeek[randomRoute.Item1].UpdateRoutes();
                    orderMatrix.GetOrderMatrix.Add(bestOrder.orderId, bestOrder);
                    deletionDone = true;
                }
            }

            return deletionDone;
        }

        /// <summary>
        /// Add an order from the orderMatrix to the schedule *currently the last one present in the orderMatrix
        /// </summary>
        public bool AddOrder()
        {
            if (orderMatrix.GetOrderMatrix.Count == 0)
                return false;
            Order order = orderMatrix.GetOrderMatrix.Last().Value;
            
            List<Tuple<int, int, int, double>> possibleSpots = new List<Tuple<int, int, int, double>>();    //weekIndex, routeID, index in the route, the extra time this route takes

            bool spotFound = false;
            int weekIndex = 0, weekCounter = 0, newLoad, spotCounter = 0, maximumAttempts; //index keeps track of which day we are at, counter helps exiting the while if we have checked the whole week
            //Doubles for the current time of a route, the new time of a route
            double totalTime = 0, newTime = 0;
            double chance = 30; //acceptance chance

            weekIndex = random.Next(week.GetWeek.Count);

            while (weekCounter < week.GetWeek.Count)
            {
                Day day = week.GetWeek[weekIndex];

                //if a spot was possible at some point, accept it now
                //or make a list of all spots and choose at the end (cleaner)

                foreach(Route route in day.GetRoutes)
                {
                    totalTime = route.TotalTime();

                    for (int y = 0; y <= route.GetRoute.Count; y++)
                    {
                        if (y == 0)
                        {
                            newTime = totalTime - distanceMatrix.GetDistanceMatrix[DEPOTMATRIXID, route.GetRoute[y].matrixId]
                                //- distanceMatrix.GetDistanceMatrix[route.GetRoute[y].matrixId, route.GetRoute[y + 1].matrixId]
                                                + distanceMatrix.GetDistanceMatrix[DEPOTMATRIXID, order.matrixId]
                                                + distanceMatrix.GetDistanceMatrix[order.matrixId, route.GetRoute[y].matrixId]
                                                + order.totalEmptyingTime;
                        }
                        else if (y == route.GetRoute.Count) //If we're at the last entry, the next stop is the depot and we need to do things in a slightly different way
                        {
                            //Calculate the new travel time for this route
                            newTime = totalTime - distanceMatrix.GetDistanceMatrix[route.GetRoute[y - 1].matrixId, DEPOTMATRIXID] //distance from current route to depot
                                + distanceMatrix.GetDistanceMatrix[route.GetRoute[y - 1].matrixId, order.matrixId]  //distance form curr to new order
                                + distanceMatrix.GetDistanceMatrix[order.matrixId, DEPOTMATRIXID]   //new order to depot
                                + order.totalEmptyingTime;  //order emptying time
                            //- 1800
                                //depot emptying time is already in totalTime
                        }
                        else
                        {
                            //Calculate the new travel time for this route
                            newTime = totalTime - distanceMatrix.GetDistanceMatrix[route.GetRoute[y - 1].matrixId, route.GetRoute[y].matrixId]
                                + distanceMatrix.GetDistanceMatrix[route.GetRoute[y - 1].matrixId, order.matrixId]
                                + distanceMatrix.GetDistanceMatrix[order.matrixId, route.GetRoute[y].matrixId]
                                + order.totalEmptyingTime;
                        }

                        //The new load, now includes the new order
                        newLoad = route.TotalLoad() + order.volumeOfOneContainer * order.numberOfContainers / 5;

                        //insert into route
                        //updateroutes
                        //check
                        //remove from route

                        //add difference in time to relevant starttimes

                        //Check if the new route is feasible
                        if (newLoad < MAXLOAD && day.CheckNewRoutes(route.RouteID, newTime))
                        {
                            possibleSpots.Add(new Tuple<int, int, int, double>(weekIndex, route.RouteID, y, newTime - totalTime));
                        }
                    }
                }

                //So we don't go out of the week's bounds
                weekIndex++;
                weekCounter++;
                if (weekIndex >= week.GetWeek.Count)
                    weekIndex = 0;
            }

            if (possibleSpots.Count == 0)
                return false;

            maximumAttempts = Math.Max(1, possibleSpots.Count / 5);

            while(!spotFound && spotCounter < maximumAttempts)
            {
                //Pick a random thingy from the possible spots
                int index = random.Next(possibleSpots.Count);

                Tuple<int, int, int, double> spot = possibleSpots[index];

                //If it's an improvement
                if(spot.Item4 <= 3 * orderMatrix.GetOrderMatrix[order.orderId].totalEmptyingTime * orderMatrix.GetOrderMatrix[order.orderId].frequency)
                {
                    week.GetWeek[spot.Item1].GetRoutes[spot.Item2 - 1].GetRoute.Insert(spot.Item3, order);
                    week.GetWeek[spot.Item1].UpdateRoutes();
                    orderMatrix.GetOrderMatrix.Remove(order.orderId);
                    spotFound = true;
                }
                else if (chance + spot.Item4 / 100 > random.Next(101))//Spot can still be accepted because we are annealing simulations
                {
                    week.GetWeek[spot.Item1].GetRoutes[spot.Item2 - 1].GetRoute.Insert(spot.Item3, order);
                    week.GetWeek[spot.Item1].UpdateRoutes();
                    orderMatrix.GetOrderMatrix.Remove(order.orderId);
                    spotFound = true;
                }

                spotCounter++;
            }

            return spotFound;
        }

        public bool SwapOrder()
        {
            //StreamWriter sw = new StreamWriter(@"...\...\swaps.txt");

            bool swapComplete = false;
            List<Tuple<int, double>> PossibleIndexes = new List<Tuple<int, double>>();
            double aTotalTime, bTotalTime, aNewTime = 0, bNewTime = 0, aGains, bGains, totalGains;
            int aLoad, bLoad, aIndex;

            //Choose a random day + route
            Tuple<int, int> initialD = SelectRandomRoute(); //Have we been in this place before?
            Day dayA = week.GetWeek[initialD.Item1];
            Route routeA = dayA.GetRoutes[initialD.Item2];

            //Choose a random order on that route
            aIndex = random.Next(1, routeA.GetRoute.Count - 1);
            Order orderA = routeA.GetRoute[aIndex];
            if (orderA.frequency > 1)
                return false;

            //Pick another random day & route
            Tuple<int, int> targetDay = SelectRandomRoute();
            Day dayB = week.GetWeek[targetDay.Item1];
            Route routeB = dayB.GetRoutes[targetDay.Item2];

            //Get the current times
            aTotalTime = routeA.TotalTime();
            bTotalTime = routeB.TotalTime();

            //Gather all possible (one-for-one) swaps
            for (int x = 1; x < routeB.GetRoute.Count - 1; x++ )
            {
                if (routeB.GetRoute[x].frequency > 1)
                    continue;

                //Get the new time with a swapped for b
                aNewTime = aTotalTime - distanceMatrix.GetDistanceMatrix[routeA.GetRoute[aIndex - 1].matrixId, routeA.GetRoute[aIndex].matrixId] //route from a - 1 to a
                                      - distanceMatrix.GetDistanceMatrix[routeA.GetRoute[aIndex].matrixId, routeA.GetRoute[aIndex + 1].matrixId] //route from a to a + 1
                                      - orderA.totalEmptyingTime                                                                                  //a's emptying time
                                      + distanceMatrix.GetDistanceMatrix[routeA.GetRoute[aIndex - 1].matrixId, routeB.GetRoute[x].matrixId]      //route from a - 1 to b
                                      + distanceMatrix.GetDistanceMatrix[routeB.GetRoute[x].matrixId, routeA.GetRoute[aIndex + 1].matrixId]      //route from a + 1 to b
                                      + routeB.GetRoute[x].totalEmptyingTime;                                                                    //b's emptying time

                bNewTime = GetNewTime(bTotalTime, routeB, orderA, x);

                if (initialD.Item1 == targetDay.Item1)// && aNewTime + bNewTime > WORKINGDAY)
                    continue;

                //Calculate how much time is gained
                aGains = aNewTime - aTotalTime;
                bGains = bNewTime - bTotalTime;
                totalGains = aGains + bGains;

                //If, the time and load with b for a checks out, and the time and load with a for b checks out
                aLoad = routeA.TotalLoad() - orderA.volumeOfOneContainer * orderA.numberOfContainers / 5 + routeB.GetRoute[x].volumeOfOneContainer * routeB.GetRoute[x].numberOfContainers / 5;
                bLoad = routeB.TotalLoad() - routeB.GetRoute[x].volumeOfOneContainer * routeB.GetRoute[x].numberOfContainers / 5 + orderA.numberOfContainers * orderA.volumeOfOneContainer / 5;

                if (aLoad < MAXLOAD && bLoad < MAXLOAD && dayA.CheckNewRoutes(routeA.RouteID, aNewTime) && dayB.CheckNewRoutes(routeB.RouteID, bNewTime))
                {
                    PossibleIndexes.Add(new Tuple<int, double>(x, totalGains));
                }
            }

            //Akin to addorder, go over them
            int swapCounter = 0, MaximumAttempts = Math.Max(1, PossibleIndexes.Count / 5);
            int randomIndex;

            while(!swapComplete && swapCounter < MaximumAttempts)
            {
                if (PossibleIndexes.Count == 0)
                    break;

                randomIndex = random.Next(PossibleIndexes.Count);
                Tuple<int, double> possibility = PossibleIndexes[randomIndex];
                Order orderB = routeB.GetRoute[possibility.Item1];

                if(possibility.Item2 <= 0)
                {
                    //remove a, insert b, clean up
                    routeA.Remove(aIndex);
                    routeA.GetRoute.Insert(aIndex, orderB);
                    dayA.UpdateRoutes();

                    //Remove b, insert a, clean up
                    routeB.Remove(possibility.Item1);
                    routeB.GetRoute.Insert(possibility.Item1, orderA);
                    dayB.UpdateRoutes();

                    swapComplete = true;

                    if(!(dayA.CheckDay() && dayB.CheckDay()))
                        swapComplete = true;

                    //Console.WriteLine("SWAPPED: =======================");
                    //Console.WriteLine("RouteID of a {0} ; Daynumber {1}; StartTime {2}; Total time {3}, Check {4} ", routeA.RouteID, initialD.Item1, routeA.StartTime, routeA.TotalTime(), dayA.CheckDay());
                    //Console.WriteLine("RouteID of b {0} ; Daynumber {1}; StartTime {2}; Total time {3}, Check {4} ", routeB.RouteID, targetDay.Item1, routeB.StartTime, routeB.TotalTime(), dayB.CheckDay());
                    //Console.WriteLine("================================");
                }
                //else if(30 > random.Next(101))
                //{
                //    //remove a, insert b, clean up
                //    routeA.Remove(aIndex);
                //    routeA.GetRoute.Insert(aIndex, orderB);
                //    dayA.UpdateRoutes();

                //    //Remove b, insert a, clean up
                //    routeB.Remove(possibility.Item1);
                //    routeB.GetRoute.Insert(possibility.Item1, orderA);
                //    dayB.UpdateRoutes();

                //    swapComplete = true;

                //    Console.WriteLine("SWAPPED: =======================");
                //    Console.WriteLine("RouteID of a {0} ; Newtime {1}, StartTime {2}, Day time {3}, Check {4} {5}", routeA.RouteID, aNewTime, routeA.StartTime, routeA.TotalTime(), dayA.CheckDay(), dayA.CheckNewRoutes(routeA.RouteID, routeA.TotalTime()));
                //    Console.WriteLine("RouteID of b {0} ; Newtime {1}, StartTime {2}, Day time {3}, Check {4} {5}", routeB.RouteID, bNewTime, routeB.StartTime, routeB.TotalTime(), dayB.CheckDay(), dayB.CheckNewRoutes(routeB.RouteID, routeB.TotalTime()));
                //    Console.WriteLine("================================");
                //}

                swapCounter++;
            }



            return swapComplete;
        }

        /// <summary>
        /// Recalculates a route's time given would-be-new parameters
        /// </summary>
        /// <param name="currentTime">Current TotalTime of Route a</param>
        /// <param name="a">Route where Order b will be inserted</param>
        /// <param name="b">Order that will be inserted into a</param>
        /// <param name="aIndex">Position at which b will be inserted</param>
        /// <returns>The new time should b be inserted</returns>
        public double GetNewTime(double currentTime, Route a, Order b, int aIndex)
        {
            double newTime = 0;

            newTime = currentTime - distanceMatrix.GetDistanceMatrix[a.GetRoute[aIndex - 1].matrixId, a.GetRoute[aIndex].matrixId] //route from a - 1 to a
                                      - distanceMatrix.GetDistanceMatrix[a.GetRoute[aIndex].matrixId, a.GetRoute[aIndex + 1].matrixId] //route from a to a + 1
                                      - a.GetRoute[aIndex].totalEmptyingTime                                                                                  //a's emptying time
                                      + distanceMatrix.GetDistanceMatrix[a.GetRoute[aIndex - 1].matrixId, b.matrixId]      //route from a - 1 to b
                                      + distanceMatrix.GetDistanceMatrix[b.matrixId, a.GetRoute[aIndex + 1].matrixId]      //route from a + 1 to b
                                      + b.totalEmptyingTime;                       

            return newTime;
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
