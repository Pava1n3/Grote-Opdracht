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
        // Constants
        private const int HALFFIVE = 41400;
        private const int DEPOTMATRIXID = 287;
        private const int DUMPLOAD = 1800;
        private const int MAXLOAD = 20000;
        private int WORKINGDAY = 43200;
        // enum operation { Add, Swap, Delete, Null };
        Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> emptyTuple = new Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>(operation.Null, false, 0, null);

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

        public bool Delete(double ctrlPM)
        {
            int numberTries = 5;
            int possibility = 0;
            Tuple<int, int> rndRoute = SelectRandomRoute();
            Route route = week.GetWeek[rndRoute.Item1].GetRoutes[rndRoute.Item2];
            double best = double.MaxValue;
            double oTotalTime = route.TotalTime();

            if (route.GetRoute.Count == 0)
                return true;

            for (int x = 0; x < numberTries; x++)
            {
                int rnd = random.Next(route.GetRoute.Count);
                Order temp = route.GetRoute[rnd];
                // If an order has a higher frequency than 1, skip the iteration.
                if (temp.frequency > 1)
                    continue;

                // Remove the chosen order from the route.
                route.GetRoute.RemoveAt(rnd);
                // Calculate the new time + penalty.
                double nTotalTime = route.TotalTime();
                double time = nTotalTime + temp.totalEmptyingTime * 3;
                // If the deletion is less costly than the other deletions, save it.
                if (time < best)
                {
                    best = time;
                    possibility = rnd;
                }
                // Reinsert the order in the Route.
                route.GetRoute.Insert(rnd, temp);
                // When on the last iteration...
                if (x == numberTries - 1)
                {
                    // Do the best possible deletion.
                    Order order = route.GetRoute[possibility];
                    route.GetRoute.RemoveAt(possibility);
                    double pTotalTime = oTotalTime - best;
                    bool accept = false;

                    // If the deletion gives us a time increase...
                    if (pTotalTime < 0)
                        // Check if we will accept it.
                        accept = AcceptOperation(ctrlPM, pTotalTime);
                    // If we won't, reinsert the order in the route.
                    if (!accept)
                        route.GetRoute.Insert(possibility, order);
                    // If we will, add the order to the orderMatrix and Update the whole Day.
                    else
                    {
                        orderMatrix.GetOrderMatrix.Add(order.orderId, order);
                        week.GetWeek[rndRoute.Item1].UpdateRoutes();
                        return false;
                    }                       
                }
            }
            return true;
        }

        public bool Shift(double ctrlPM)
        {
            int rndStartIndex;
            Tuple<int, int> rndStart, rndTarget;
            rndStart = SelectRandomRoute();
            rndTarget = SelectRandomRoute();

            //Get a random start and target route. 
            Day tarDay = week.GetWeek[rndTarget.Item1];
            Day startDay = week.GetWeek[rndStart.Item1];
            Route startRoute = startDay.GetRoutes[rndStart.Item2];
            Route tarRoute = tarDay.GetRoutes[rndTarget.Item2];
            if (tarRoute.GetRoute.Count == 0 || startRoute.GetRoute.Count == 0)
                return true;

            //Choose a random order from the startroute
            rndStartIndex = random.Next(startRoute.GetRoute.Count);
            Order order = startRoute.GetRoute[rndStartIndex];

            if (order.frequency > 1)
            {
                if (tarDay.DayNumber != startDay.DayNumber)
                {
                    if (order.frequency == 4)
                    {
                        //An order with frequency 4 should not be swapped to another day that has the order
                        if (order.GetBit(tarDay.DayNumber))
                            return true;
                    }
                    else if (order.frequency == 2)
                    {
                        #region freq2
                        int sDay1, sDay2, sRoute1, sRoute2, index1, index2, unprocessedDay = 0, processedDay = 0;
                        Tuple<int, int, int> indices;

                        //This gets which day, of mon and tue, is processed and unprocessed (from this we can glean the other day)
                        for (int i = 1; i < 3; i++)
                        {
                            if (!order.GetBit(i))
                                unprocessedDay = i;
                            else
                                processedDay = i;
                        }

                        //startday can be mon, tue, thur, fri
                        //can be in the first or latter part of the week
                        //then, it can be bigger or smaller than the unprocessedDay
                        if (startDay.DayNumber > 2)
                        {
                            if (startDay.DayNumber - 3 > unprocessedDay) //unprocessed is mon (startday is friday: 5 - 3 = 2; 2 > 1)
                            {
                                //other call should shift tue to mon, need to find out which truck does the order
                                indices = FindOccurence(2, order); //this method finds out which day exactly contains the order
                                //In this case, we have to obtain the truck that does the order on tuesday, we know of the truck on friday
                            }
                            else  //Startday is thursday, unprocessed is tuesday
                            {
                                //other call should shift mon to tue, need to find out which truck does the order
                                indices = FindOccurence(1, order); //this method finds out which day exactly contains the order
                            }

                            //Set the parameters, start day is in the latter half of the week. That means we had to use FindOccurence to find the earlier weekday, hence the indices use
                            sDay1 = indices.Item1; sDay2 = rndStart.Item1; sRoute1 = indices.Item2; sRoute2 = rndStart.Item2; index1 = indices.Item3; index2 = rndStartIndex;
                        }
                        else //startDay is in the first half of the week
                        {
                            if (startDay.DayNumber > unprocessedDay) //Starting day is tuesday, unprocessed is monday
                            {
                                indices = FindOccurence(5, order);  //Find the day in the latter half of the week (on friday)
                            }
                            else
                            {
                                indices = FindOccurence(4, order);
                            }

                            //Set the parameters, start day is in the latter half of the week. Note how indices is now used for the different day!
                            sDay1 = rndStart.Item1; sDay2 = indices.Item1; sRoute1 = rndStart.Item2; sRoute2 = indices.Item2; index1 = rndStartIndex; index2 = indices.Item3;
                        }

                        if (indices.Item1 == -1 || indices.Item2 == -1 || indices.Item3 == -1)
                            return true;

                        return DoubleShift(ctrlPM, sDay1, sDay2, sRoute1, sRoute2, index1, index2, order);
                        #endregion
                    }
                    else
                        return true;
                }

                //All orders should be allowed to be swapped to the same day
            }

#region shiftBody
            //Original time of target route; original time of target day (to be calculated); new time of rout (tbc); value of best improvement
            double oTime = tarRoute.TotalTime(), oDayTime = 0, nTime = 0, best = double.MaxValue;
            int bestPos = -1, totalLoad = tarRoute.TotalLoad();

            //Calc total time of the target day
            foreach(Route r in tarDay.GetRoutes)
            {
                oDayTime += r.TotalTime();
            }

            //Calculate the time difference of removing the route from the starting route
            double sTime, nSTime, timeDiff;
            sTime = startRoute.TotalTime();
            startRoute.Remove(rndStartIndex);
            nSTime = startRoute.TotalTime();
            timeDiff = sTime - nSTime;

            //Go over the target route, figure out the best location to shift to
            for (int i = 0; i < tarRoute.GetRoute.Count; i++ )
            {
                tarRoute.GetRoute.Insert(i, order);
                nTime = tarRoute.TotalTime();

                //Check if it's valid (time and load wise) and whether it's better
                if(oDayTime + nTime - oTime < WORKINGDAY && totalLoad + order.numberOfContainers * order.volumeOfOneContainer < MAXLOAD && nTime < best)
                {
                    bestPos = i;
                    best = nTime;
                }

                tarRoute.Remove(i);
            }
#endregion

            bool accept = false;
            if (bestPos != -1)
            {
                //Check wether it is a net gain in time or not
                //TimeDiff is positive, because this much time is gained from the removal
                //best - oTime, the increase in route time
                //The total thing should be increase in new route measured against decrease in the other
                if (timeDiff - (best - oTime) > 0)
                {
                    //In case of this occuring, the shift was a net gain (more went out of the old route then came into the new route)
                    tarRoute.GetRoute.Insert(bestPos, order);

                    //Update Routes
                    tarDay.UpdateRoutes();
                    startDay.UpdateRoutes();

                    //Change some bits if necessary
                    if (order.frequency == 4 && startDay.DayNumber != tarDay.DayNumber)
                    {
                        order.SetBit(startDay.DayNumber, false);
                        order.SetBit(tarDay.DayNumber, true);
                    }
                    return true;
                }
                else //Order is not an improvement
                {
                    accept = AcceptOperation(ctrlPM, timeDiff - (best - oTime));

                    if (accept)
                    {
                        //We accepted a deterioration
                        tarRoute.GetRoute.Insert(bestPos, order);

                        //Update Routes
                        tarDay.UpdateRoutes();
                        startDay.UpdateRoutes();

                        //Change some bits if necessary
                        if(order.frequency == 4 && startDay.DayNumber != tarDay.DayNumber)
                        {
                            order.SetBit(startDay.DayNumber, false);
                            order.SetBit(tarDay.DayNumber, true);
                        }
                        return false;
                    }
                    else
                    {
                        //Put the order back in its original spot when we fail to shift
                        startRoute.GetRoute.Insert(rndStartIndex, order);
                        return true;
                    }
                }
            }
            else
            {
                //Put the order back in its original spot when we fail to shift
                startRoute.GetRoute.Insert(rndStartIndex, order);
                return true;
            }
        }

        public bool DoubleShift(double ctrlPM, int sDay1, int sDay2, int sRoute1, int sRoute2, int orderIndex1, int orderIndex2, Order order)
        {
            Day startDay1 = week.GetWeek[sDay1], startDay2 = week.GetWeek[sDay2];
            Route startRoute1 = startDay1.GetRoutes[sRoute1], startRoute2 = startDay2.GetRoutes[sRoute2];

            //The target Days/Routes, to shift to
            Day tarDay1, tarDay2;
            Route tarRoute1, tarRoute2;

             double totalTimeChange1 = 0, totalTimeChange2 = 0;
            
            //mo 1; tu 2; we 3; thu 4; fr 5
            //Tuesday
            if(startDay1.DayNumber == 2)
            {
                //Grab one of the two days conforming to the correct dayNumber
                tarDay1 = week.GetWeek[random.Next(0, 2)];  //To monday (0,1)
                tarDay2 = week.GetWeek[random.Next(6, 8)];  //To thursday (6,7)

                if (tarDay1.GetRoutes.Count == 0 || tarDay2.GetRoutes.Count == 0)
                    return true;

                //Grab a route randomly on the target days
                tarRoute1 = tarDay1.GetRoutes[random.Next(tarDay1.GetRoutes.Count)];
                tarRoute2 = tarDay2.GetRoutes[random.Next(tarDay2.GetRoutes.Count)];
            }
            else //startday1.DayNumber == 1 aka monday
            {
                //Grab one of the two days conforming to the correct dayNumber
                tarDay1 = week.GetWeek[random.Next(2, 4)];  //To tuesday (2,3)
                tarDay2 = week.GetWeek[random.Next(8, 10)];  //To friday (8,9)

                if (tarDay1.GetRoutes.Count == 0 || tarDay2.GetRoutes.Count == 0)
                    return true;

                //Grab a route randomly on the target days
                tarRoute1 = tarDay1.GetRoutes[random.Next(tarDay1.GetRoutes.Count)];
                tarRoute2 = tarDay2.GetRoutes[random.Next(tarDay2.GetRoutes.Count)];
            }

            int index1 = -1, index2 = -1;

            //We'll try to shift twice, once for each order. If both succeed, we will actually shift them in another part of the code
            for (int s = 0; s < 2; s++)
            {
                #region shiftstuff
                Day tarDay, startDay;
                Route tarRoute, startRoute;
                int startIndex;
                
                //We do two iterations, with different days/routes each time
                if(s == 0)
                {
                    tarDay = tarDay1;
                    startDay = startDay1;
                    tarRoute = tarRoute1;
                    startRoute = startRoute1;
                    startIndex = orderIndex1;
                }
                else
                {
                    tarDay = tarDay2;
                    startDay = startDay2;
                    tarRoute = tarRoute2;
                    startRoute = startRoute2;
                    startIndex = orderIndex2;
                }

                //Original time of target route; original time of target day (to be calculated); new time of rout (tbc); value of best improvement
                double oTime = tarRoute.TotalTime(), oDayTime = 0, nTime = 0, best = double.MaxValue;
                int bestPos = -1, totalLoad = tarRoute.TotalLoad();

                //Calc total time of the target day
                foreach (Route r in tarDay.GetRoutes)
                {
                    oDayTime += r.TotalTime();
                }

                //Calculate the time difference of removing the route from the starting route
                double sTime, nSTime, timeDiff;
                sTime = startRoute.TotalTime();
                startRoute.Remove(startIndex);
                nSTime = startRoute.TotalTime();
                timeDiff = sTime - nSTime;

                //Go over the target route, figure out the best location to shift to
                for (int i = 0; i < tarRoute.GetRoute.Count; i++)
                {
                    tarRoute.GetRoute.Insert(i, order);
                    nTime = tarRoute.TotalTime();

                    //Check if it's valid (time and load wise) and whether it's better
                    if (oDayTime + nTime - oTime < WORKINGDAY && totalLoad + order.numberOfContainers * order.volumeOfOneContainer < MAXLOAD && nTime < best)
                    {
                        bestPos = i;
                        best = nTime;
                    }

                    tarRoute.Remove(i);
                }

                bool accept = false, improvement1 = false, improvement2 = false;
                if (bestPos != -1)
                {
                    if(s == 0)
                        totalTimeChange1 = timeDiff - (best - oTime);
                    else
                        totalTimeChange2 = timeDiff - (best - oTime);

                    //Check wether it is a net gain in time or not
                    //TimeDiff is positive, because this much time is gained from the removal
                    //best - oTime, the increase in route time
                    //The total thing should be increase in new route measured against decrease in the other
                    if (timeDiff - (best - oTime) >= 0)
                    {
                        //In case of this occuring, the shift was a net gain (more went out of the old route then came into the new route)
                        if (s == 0)
                            index1 = bestPos;
                        else
                            index2 = bestPos;

                        //We re-insert here because we don't know if the other shift will succeed. If that happens, we remove again (seemed a little bit cleaner this way)
                        startRoute.GetRoute.Insert(startIndex, order);
                    }
                    else
                    {
                        //Does the operation get through the q-test?
                        accept = AcceptOperation(ctrlPM, timeDiff - (best - oTime));

                        if (accept)
                        {
                            //We accepted a loss
                            if (s == 0)
                                index1 = bestPos;
                            else
                                index2 = bestPos;

                            //We re-insert here because we don't know if the other shift will succeed. If that happens, we remove again (seemed a little bit cleaner this way)
                            startRoute.GetRoute.Insert(startIndex, order);
                        }
                        else
                        {
                            //No shift occured, re-insert the order to its original spot
                            startRoute.GetRoute.Insert(startIndex, order);
                            //startDay.UpdateRoutes();
                            return true;
                        }
                    }
                }
                else
                {
                    //No shift occured, re-insert the order to its original spot
                    startRoute.GetRoute.Insert(startIndex, order);
                    return true;
                }
                #endregion
            }


            //if both iterations succeed, we do stuff!
            if(index1 != -1 && index2 != -1)
            {
                //Don't forget to change the bits! Also for freq 4 in the normal shift!!!

                //Remove and Add (shift) the orders
                startRoute1.Remove(orderIndex1);
                startRoute2.Remove(orderIndex2);

                tarRoute1.GetRoute.Insert(index1, order);
                tarRoute2.GetRoute.Insert(index2, order);

                //Change bits so future checks get the correct info
                order.SetBit(startDay1.DayNumber, false);
                order.SetBit(startDay2.DayNumber, false);
                order.SetBit(tarDay1.DayNumber, true);
                order.SetBit(tarDay2.DayNumber, true);

                //Update the now changed routes
                startDay1.UpdateRoutes();
                startDay2.UpdateRoutes();
                tarDay1.UpdateRoutes();
                tarDay2.UpdateRoutes();

                //Make sure to return whether this is a net gain or not
                return (totalTimeChange1 + totalTimeChange2 > 0);
            }

            return true;
        }

        public bool Add(double ctrlPM)
        {
            Tuple<int, int, int> possibility;
            int[] keys = orderMatrix.GetOrderMatrix.Keys.ToArray();
            int key = keys[random.Next(keys.Length)];
            Order order = orderMatrix.GetOrderMatrix[key];

            double best = double.MaxValue;

            // Check in what place we can actually add the order, starting...
            // For every day in week...
            for (int x = 0; x < week.GetWeek.Count; x++)
            {// For every route in day...
                for (int y = 0; y < week.GetWeek[x].GetRoutes.Count; y++)
                    // For every order in route...
                    for (int z = 0; z < week.GetWeek[x].GetRoutes[y].GetRoute.Count; z++)
                    {
                        // Insert the order on the 'z' index.
                        week.GetWeek[x].GetRoutes[y].GetRoute.Insert(z, order);
                        // Update everything
                        week.GetWeek[x].UpdateRoutes();
                        // Check if the solution for the whole day is still feasible.
                        if (!(WORKINGDAY < week.GetWeek[x].Costs()))
                        {
                            // If it is, calculate the newTime for the route with the added order.
                            double newTime = week.GetWeek[x].GetRoutes[y].TotalTime();
                            // If the time is better than previous tested additions or if it is the first iteration...
                            if (newTime < best)
                            {
                                // Update the values.
                                best = newTime;
                                possibility = new Tuple<int, int, int>(x, y, z);
                            }

                        }
                        week.GetWeek[x].GetRoutes[y].GetRoute.RemoveAt(z);
                    }

            }

            return true;
        }



        /// <summary>
        /// Finds the least costly delete on a random day and a random route
        /// </summary>
        /// <returns></returns>
        public Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> Deletion()
        {
            //bool deletionDone = false;
            int routeIndex = 0, routeCount, currentBest = -1;
            double timeGain = double.MinValue, totalTime, newTime;
            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> outcome = emptyTuple;

            Tuple<int, int> randomRoute = SelectRandomRoute();
            Route route = week.GetWeek[randomRoute.Item1].GetRoutes[randomRoute.Item2];
            Order order = null, bestOrder = null;
            totalTime = route.TotalTime();
            routeCount = route.GetRoute.Count;

            for (int x = 0; x < 5; x++)
            {
                if (routeCount == 0)
                  return emptyTuple;

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

                if (totalTime - newTime > timeGain)
                {
                    timeGain = totalTime - newTime;
                    currentBest = routeIndex;
                    bestOrder = order;
                }
            }

            if (currentBest != -1)
            {
                if (timeGain > 3 * order.totalEmptyingTime * order.frequency)
                {
                    outcome = new Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>(operation.Delete, true, timeGain, new List<Tuple<int, int, int, Order>>());
                    outcome.Item4.Add(new Tuple<int, int, int, Order>(randomRoute.Item1, randomRoute.Item2, currentBest, bestOrder));

                    //deletionDone = true;
                }
                else
                {
                    outcome = new Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>(operation.Delete, false, timeGain, new List<Tuple<int, int, int, Order>>());
                    outcome.Item4.Add(new Tuple<int, int, int, Order>(randomRoute.Item1, randomRoute.Item2, currentBest, bestOrder));
                    //deletionDone = true;
                }
            }

            return outcome;
        }

        /// <summary>
        /// Add an order from the orderMatrix to the schedule *currently the last one present in the orderMatrix
        /// </summary>
        public Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> AddOrder()
        {
            if (orderMatrix.GetOrderMatrix.Count == 0)
                return emptyTuple;

            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> outcome = emptyTuple;

            int[] keys = orderMatrix.GetOrderMatrix.Keys.ToArray();
            int key = keys[random.Next(keys.Length)];

            Order order = orderMatrix.GetOrderMatrix[key];

            int weekIndex = 0, weekCounter = 0, newLoad, bestIndex = -1, bestRouteID = -1; //index keeps track of which day we are at, counter helps exiting the while if we have checked the whole week
            //Doubles for the current time of a route, the new time of a route
            double totalTime = 0, newTime = 0, timeGain = 0, currentBest = double.MinValue;

            weekIndex = random.Next(week.GetWeek.Count);

            while (weekCounter < week.GetWeek.Count)
            {
                Day day = week.GetWeek[weekIndex];

                double time = 0;
                foreach (Route route in day.GetRoutes)
                    time += route.TotalTime();

                if (day.GetRoutes.Count == 0)
                    return emptyTuple;

                //if a spot was possible at some point, accept it now
                //or make a list of all spots and choose at the end (cleaner)

                foreach(Route route in day.GetRoutes)
                {
                    totalTime = route.TotalTime();

                    for (int y = 0; y <= route.GetRoute.Count; y++)
                    {
                        if(route.GetRoute.Count == 0)
                        {
                            newTime = totalTime + distanceMatrix.GetDistanceMatrix[DEPOTMATRIXID, order.matrixId]
                                                + order.totalEmptyingTime;
                        }
                        else if (y == 0)
                        {
                            newTime = totalTime - distanceMatrix.GetDistanceMatrix[DEPOTMATRIXID, route.GetRoute[y].matrixId]
                                //- distanceMatrix.GetDistanceMatrix[route.GetRoute[y].matrixId, route.GetRoute[y + 1].matrixId]
                                                + distanceMatrix.GetDistanceMatrix[DEPOTMATRIXID, order.matrixId]
                                                + distanceMatrix.GetDistanceMatrix[order.matrixId, route.GetRoute[y].matrixId]
                                                + order.totalEmptyingTime;
                            //if(route.GetRoute.Count > 1)
                            //    newTime -= distanceMatrix.GetDistanceMatrix[route.GetRoute[y].matrixId, route.GetRoute[y + 1].matrixId];
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

                        //Positive if new is better, neg if it is worse
                        timeGain = totalTime - newTime - 3 * order.totalEmptyingTime;

                        //Check if the new route is feasible
                        if (newLoad < MAXLOAD && day.CheckNewRoutes(route.RouteID, newTime) && timeGain > currentBest)
                        {
                            currentBest = timeGain;
                            bestRouteID = route.RouteID;
                            bestIndex = y;
                        }
                    }
                }

                //So we don't go out of the week's bounds
                weekIndex++;
                weekCounter++;
                if (weekIndex >= week.GetWeek.Count)
                    weekIndex = 0;
            }


            if(bestIndex != -1)
            {
                outcome = new Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>(operation.Add, true, timeGain, new List<Tuple<int, int, int, Order>>());
                outcome.Item4.Add(new Tuple<int, int, int, Order>(weekIndex, bestRouteID - 1, bestIndex, order));
            }

            return outcome;
        }

        public Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> ShiftOrder(int startDayID = -1, int startDayRoute = -1, int startDayRouteIndex = -1, int targetDayNumber = -1, Order startOrder = null)
        {
            //Grab a random order
            //Grab a random day + route OR same route
            //check best shift
            double newTime, currentTime, targetRouteTimeDifference, targetDayTime = 0, bestShiftGain = double.MaxValue;
            int currentLoad = -1, previousOrderMatrixID, nextOrderMatrixID, bestShiftIndex = -1;
            bool targetRouteFound = false;

            //This Tuple may contain information for an additional swap
            Tuple<int, int, int, Order> secondSwapStart = null;
            Tuple<int, int, int, Order> secondSwapTarget = null;

            //Grab a random day and route twice, start and target days
            Tuple<int, int> start = SelectRandomRoute();
            Tuple<int, int> target = SelectRandomRoute();

            Day startDay = week.GetWeek[start.Item1];
            Route startRoute = startDay.GetRoutes[start.Item2];

            int counter = 0;
            while (startRoute.GetRoute.Count == 0 && counter < 20)
            {
                start = SelectRandomRoute();
                startDay = week.GetWeek[start.Item1];
                startRoute = startDay.GetRoutes[start.Item2];
                counter++;
            }

            Day targetDay = week.GetWeek[target.Item1];
            if (targetDay.GetRoutes.Count == 0) //in case we would create new routes, this needs to change
                return emptyTuple;
            Route targetRoute = targetDay.GetRoutes[target.Item2];
            currentLoad = targetRoute.TotalLoad();

            //Get a random order, this is what we'll shift
            int orderIndex = random.Next(startRoute.GetRoute.Count);
            Order order = startRoute.GetRoute[orderIndex];

            if(startDayID != -1)//This shift was called as a subcall from another shift, as part of shifting a higher frequency order
            {
                //We change the start Tuple, because it is used to generate the returned tuple
                //startDayID is the index in the week, startDayRoute is the ID of the route containing our order in that day, we also change the orderIndex because that is used by some calculations
                start = new Tuple<int, int>(startDayID, startDayRoute);
                startDay = week.GetWeek[startDayID];
                startRoute = startDay.GetRoutes[startDayRoute];
                orderIndex = startDayRouteIndex;

                order = startOrder;

                target = SelectRandomRoute(targetDayNumber);
                targetDay = week.GetWeek[target.Item1];
                if (targetDay.GetRoutes.Count == 0) //in case we would create new routes, this needs to change
                    return emptyTuple;
                targetRoute = targetDay.GetRoutes[target.Item2];
                currentLoad = targetRoute.TotalLoad();

                if (currentLoad + order.numberOfContainers * order.volumeOfOneContainer > MAXLOAD)
                    return emptyTuple;
            }

            //If order frequency is larger than 1 and this is not a shift called by a higher frequency order
            if (order.frequency > 1 && startDayID == -1)
            {
                #region frequency handling
                switch (order.frequency)
                {
                    case 2:
                        //shift on the same day (only one regular shift) or shift two things
                        if (targetDay.DayNumber != startDay.DayNumber)
                        {
                            int unprocessedDay = -1, processedDay = -1;
                            Tuple<int, int, int> indices = new Tuple<int,int,int>(-1, -1, -1);
                            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> otherShift = null;

                            for (int i = 1; i < 3; i++)
                            {
                                if (!order.GetBit(i))
                                    unprocessedDay = i;
                                else
                                    processedDay = i;
                            }

                            if (random.Next(0, 2) > 1)
                            {
                                //startday can be mon, tue, thur, fri
                                //can be in the first or latter part of the week
                                //then, it can be bigger or smaller than the unprocesedDay
                                if(startDay.DayNumber > 2)  //which means target should be in the latter part of the week as well
                                {
                                    if (startDay.DayNumber - 3 > unprocessedDay) //unprocessed is mon
                                    {
                                        //other call should shift tue to mon, need to find out which truck does the order
                                        indices = FindOccurence(2, order); //this method finds out which day exactly contains the order
                                    }
                                    else
                                    {
                                        //other call should shift mon to tue, need to find out which truck does the order
                                        indices = FindOccurence(1, order); //this method finds out which day exactly contains the order
                                    }

                                    //target should be in the latter half of the week
                                    target = SelectRandomRoute(random.Next((unprocessedDay + 3) * 2 - 2, (unprocessedDay + 3) * 2));//take the unprocessedDay
                                }
                                else //startDay is in the first half of the week
                                {
                                    if(startDay.DayNumber > unprocessedDay)
                                    {
                                        indices = FindOccurence(2, order);
                                    }
                                    else
                                    {
                                        indices = FindOccurence(1, order);
                                    }

                                    target = SelectRandomRoute(random.Next(unprocessedDay * 2 - 2, unprocessedDay * 2));
                                }

                                //Just in case indices didn't return correct values
                                if (indices.Item1 == -1 || indices.Item2 == -1 || indices.Item3 == -1)
                                    return emptyTuple;

                                //Do a call to ShiftOrder
                                otherShift = ShiftOrder(indices.Item1, indices.Item2, indices.Item3, random.Next(0, 2), order);

                                if(otherShift == null)
                                {
                                    return emptyTuple;
                                }
                            }
                            else
                                target = SelectRandomRoute(random.Next(startDay.DayNumber * 2 - 2, startDay.DayNumber * 2));//Go with the current day

                            targetDay = week.GetWeek[target.Item1];
                            if (targetDay.GetRoutes.Count == 0) //in case we would create new routes, this needs to change
                                return emptyTuple;
                            targetRoute = targetDay.GetRoutes[target.Item2];
                            currentLoad = targetRoute.TotalLoad();
                        }
                        break;
                    case 3:
                        //only shift on the same day
                        if(targetDay.DayNumber != startDay.DayNumber)
                        {
                            //This random chooses one of the two trucks (by getting an index in the week list)
                            int dayIndex = random.Next(startDay.DayNumber * 2 - 2, startDay.DayNumber * 2);
                            target = SelectRandomRoute(dayIndex);

                            targetDay = week.GetWeek[dayIndex];
                            if (targetDay.GetRoutes.Count == 0) //in case we would create new routes, this needs to change
                                return emptyTuple;
                            targetRoute = targetDay.GetRoutes[target.Item2];
                            currentLoad = targetRoute.TotalLoad();
                        }
                        break;
                    case 4:
                        //shift only on same day OR to the one free day
                        if(targetDay.DayNumber != startDay.DayNumber)
                        {
                            int unprocessedDay = -1;
                            for(int i = 1; i < 6; i++)
                            {
                                if (!order.GetBit(i))
                                    unprocessedDay = i;
                            }

                            if (random.Next(0, 2) > 1)
                                target = SelectRandomRoute(random.Next(unprocessedDay * 2 - 2, unprocessedDay * 2));//take the unprocessedDay
                            else
                                target = SelectRandomRoute(random.Next(startDay.DayNumber * 2 - 2, startDay.DayNumber * 2));//Go with the current day

                            targetDay = week.GetWeek[target.Item1];
                            if (targetDay.GetRoutes.Count == 0) //in case we would create new routes, this needs to change
                                return emptyTuple;
                            targetRoute = targetDay.GetRoutes[target.Item2];
                            currentLoad = targetRoute.TotalLoad();

                        }
                        break;
                }
                #endregion

                if (currentLoad + order.numberOfContainers * order.volumeOfOneContainer > MAXLOAD)
                    return emptyTuple;
            }
            else if(startDayID == -1) //To make sure we don't go changing target days if we are swapping a higher frequency order (that would be disastrous)
            {
                //A small check, if the current goal is impossible to plan because of waste volume, reroll the target day
                for (int i = 0; i < 9; i++)
                {
                    if (currentLoad + order.numberOfContainers * order.volumeOfOneContainer > MAXLOAD)
                    {
                        target = SelectRandomRoute();
                        targetDay = week.GetWeek[target.Item1];
                        if (targetDay.GetRoutes.Count == 0) //in case we would create new routes, this needs to change
                            return emptyTuple;
                        targetRoute = targetDay.GetRoutes[target.Item2];
                        currentLoad = targetRoute.TotalLoad();
                    }
                    else
                    {
                        targetRouteFound = true;
                        break;
                    }
                }

                //In case we have not found a potentially suitable route (garbage volume wise)
                if (!targetRouteFound)
                    return emptyTuple;
            }


            //Grab the total time of the target day
            foreach(Route route in targetDay.GetRoutes)
            {
                targetDayTime += route.TotalTime();
            }

            #region calculations
            //How much time do the current routes take
            currentTime = startRoute.TotalTime() + targetRoute.TotalTime();

            //store the first bit of the newtime, which consists of the old route minus the order we are going to shift
            if (orderIndex == 0)
            {
                newTime = startRoute.TotalTime() - distanceMatrix.GetDistanceMatrix[DEPOTMATRIXID, startRoute.GetRoute[orderIndex].matrixId]
                                                 - order.totalEmptyingTime;

                if (startRoute.GetRoute.Count > 1)
                    newTime -= distanceMatrix.GetDistanceMatrix[order.matrixId, startRoute.GetRoute[orderIndex + 1].matrixId];
            }
            else if (orderIndex == startRoute.GetRoute.Count - 1)
            {
                newTime = startRoute.TotalTime() - distanceMatrix.GetDistanceMatrix[startRoute.GetRoute[orderIndex - 1].matrixId, order.matrixId]
                                                 - distanceMatrix.GetDistanceMatrix[order.matrixId, DEPOTMATRIXID]
                                                 - order.totalEmptyingTime;
            }
            else
            {
                newTime = startRoute.TotalTime() - distanceMatrix.GetDistanceMatrix[startRoute.GetRoute[orderIndex - 1].matrixId, order.matrixId]
                                                 - distanceMatrix.GetDistanceMatrix[order.matrixId, startRoute.GetRoute[orderIndex + 1].matrixId]
                                                 - order.totalEmptyingTime;
            }

            //Go over the entire route to find the best location for our order
            for (int j = 0; j <= targetRoute.GetRoute.Count; j++)
            {
                //Get the right MatrixID's
                if (targetRoute.GetRoute.Count == 0)
                {
                    previousOrderMatrixID = DEPOTMATRIXID;
                    nextOrderMatrixID = -1;
                }
                else if (j == 0) //if this is the first order, the previous was matrixid (otherwise you query location -1)
                {
                    previousOrderMatrixID = DEPOTMATRIXID;
                    nextOrderMatrixID = targetRoute.GetRoute[j].matrixId;
                }
                else if (j == targetRoute.GetRoute.Count) //likewise with the last order
                {
                    previousOrderMatrixID = targetRoute.GetRoute[j - 1].matrixId;
                    nextOrderMatrixID = DEPOTMATRIXID;
                }
                else
                {
                    previousOrderMatrixID = targetRoute.GetRoute[j - 1].matrixId;
                    nextOrderMatrixID = targetRoute.GetRoute[j].matrixId;
                }

                if (targetRoute.GetRoute.Count == 0)
                {
                    targetRouteTimeDifference = distanceMatrix.GetDistanceMatrix[previousOrderMatrixID, order.matrixId]
                                                + order.totalEmptyingTime
                                                + distanceMatrix.GetDistanceMatrix[order.matrixId, DEPOTMATRIXID];
                }
                else
                {
                    //newTime has the previous order to new order and new order to next order added to it, and the old previous to next is substracted
                    //so if we have a, b, and new, wit the old route as (a to b) we do (a to new) + (new to b) - (a to b)
                    targetRouteTimeDifference = distanceMatrix.GetDistanceMatrix[previousOrderMatrixID, order.matrixId]
                                                + distanceMatrix.GetDistanceMatrix[order.matrixId, nextOrderMatrixID]
                                                - distanceMatrix.GetDistanceMatrix[previousOrderMatrixID, nextOrderMatrixID]
                                                + order.totalEmptyingTime;
                }

                newTime += targetRouteTimeDifference;

                if (targetDayTime + targetRouteTimeDifference > WORKINGDAY)
                    continue;
                else
                {
                    //see if this is the best swap so far
                    //curr - new. If new is better will be pos, if new is worse it will be neg. Bigger is better
                    //new - curr. If new is better will be neg, if new is worse it will be pos. Smaller is better
                    if (newTime - currentTime < bestShiftGain)
                    {
                        bestShiftGain = newTime - currentTime;
                        bestShiftIndex = j;
                    }
                }
            }
            #endregion


            //End of the checks, see if we found a possible swap
            if (bestShiftIndex != -1)
            {
                bool improvement = false;
                if (bestShiftGain <= 0)
                    improvement = true;

                Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> shiftOrder = new Tuple<operation,bool,double,List<Tuple<int,int,int,Order>>>(operation.Shift, improvement, -bestShiftGain, new List<Tuple<int,int,int,Order>>());
                shiftOrder.Item4.Add(new Tuple<int, int, int, Order>(start.Item1, start.Item2, orderIndex, order));
                shiftOrder.Item4.Add(new Tuple<int, int, int, Order>(target.Item1, target.Item2, bestShiftIndex, order));

                if(secondSwapStart != null)
                {
                    shiftOrder.Item4.Add(secondSwapStart);
                    shiftOrder.Item4.Add(secondSwapTarget);
                }
                
                return shiftOrder;
            }
            else
                return emptyTuple;
        }

        public Tuple<int, int, int> FindOccurence(int dayNumber, Order order)
        {
            int dayIndex = -1, routeIndex = -1, index = -1;

            //That means we also have to shift the other occurence (*sigh*)
            //Find the other occurence
            for(int j = dayNumber * 2 - 2; j < dayNumber * 2; j++)
            {
                foreach(Route route in week.GetWeek[j].GetRoutes)
                {
                    if(route.GetRoute.Contains(order))
                    {
                        dayIndex = j;
                        routeIndex = route.RouteID - 1;
                        index = route.GetRoute.IndexOf(order);
                    }
                }
            }

            return new Tuple<int, int, int>(dayIndex, routeIndex, index);
        }

        public Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> SwapOrder(int startDayNumber = -1, int targetDayNumber = -1, Order a = null, int routeNumber = -1, int indexInRoute = -1)
        {
            //StreamWriter sw = new StreamWriter(@"...\...\swaps.txt");
            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> outcome = emptyTuple;
            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> earlyday2 = emptyTuple;
            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> lateday1 = emptyTuple;
            Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>> lateday2 = emptyTuple;

            bool indexFound = false;
            Tuple<int, int, double> possibility = new Tuple<int, int, double>(-1, -1, double.MaxValue); //The index in the route and the routeID, and the gains
            double aTotalTime, bTotalTime, aNewTime = 0, bNewTime = 0, aGains, bGains, totalGains;
            int aLoad, bLoad, aIndex;

            //Choose a random day + route
            Tuple<int, int> initialD = SelectRandomRoute(); //Have we been in this place before? ~
            Day dayA = week.GetWeek[initialD.Item1];   
            Route routeA = dayA.GetRoutes[initialD.Item2];

            //Choose a random order on that route
            aIndex = random.Next(1, routeA.GetRoute.Count - 1);
            Order orderA = routeA.GetRoute[aIndex];

            //Pick another random day & route
            Tuple<int, int> targetDay = SelectRandomRoute();
            Day dayB = week.GetWeek[targetDay.Item1];
            Route routeB = dayB.GetRoutes[targetDay.Item2];

            //If we have been called as part of a higher-frequency swap
            if (startDayNumber != -1)
            {
                //you can decide dayA and dayB from the given data
                dayA = week.GetWeek[startDayNumber];
                routeA = dayA.GetRoutes[routeNumber - 1];
                orderA = a;
                aIndex = indexInRoute;

                targetDay = SelectRandomRoute(targetDayNumber);

                dayB = week.GetWeek[targetDayNumber];
                if (dayB.GetRoutes.Count == 0)
                    return emptyTuple;
                routeB = dayB.GetRoutes[targetDay.Item2];
            }

            #region frequencylt1
            if (orderA.frequency > 1 && startDayNumber == -1)   //the -1 check is so you don't create an infinite loop of extra calls
            {
                int targetDayIndex = -1, targetRouteIndex = -1, targetIndexInRoute = -1;

                switch (orderA.frequency)
                {
                    case 2:
                        //We try and find out which days contain our order, then attempt a swap to the other days. 
                            //Now, we have to check both tuesdays (for truck 1 and truck 2) to see which one contains the order. We will then attempt a swap from that day to a monday
                            //likewise, we call this method again with some additional parameters for the two thursdays (to swap from friday to thursday)

                            if(initialD.Item1 < 2)//dayA is mon
                            {
                                //swap to tue, later day in the week is thur -> we don't know which thursday
                                //dayB should be 2, earlyday2 should be 3 (swap a monday to tuesday)
                                dayB = week.GetWeek[2];
                                //routeB = dayB.GetRoutes[random.Next(dayB.GetRoutes.Count)];
                                earlyday2 = SwapOrder(initialD.Item1, 3, orderA, initialD.Item2 + 1, aIndex);

                                //secondly, a thursday has to be found and swapped
                                for (int t = 6; t < 8; t++)
                                {
                                    Day day = week.GetWeek[t];

                                    foreach (Route route in day.GetRoutes)
                                    {
                                        if (route.GetRoute.Contains(orderA))
                                        {
                                            targetRouteIndex = route.RouteID;
                                            targetDayIndex = t;
                                            targetIndexInRoute = route.GetRoute.IndexOf(orderA);
                                            //pass the daynumber, routenumber and index in the route
                                            indexFound = true;
                                        }

                                        if (indexFound)
                                        {
                                            t = 8;
                                            break;
                                        }
                                    }
                                }

                                if (!indexFound)
                                    return emptyTuple;

                                lateday1 = SwapOrder(targetDayIndex, 8, orderA, targetRouteIndex, targetIndexInRoute);  //try and swap to a friday
                                lateday2 = SwapOrder(targetDayIndex, 9, orderA, targetRouteIndex, targetIndexInRoute);  //try and swap to a friday
                            }
                            else
                            {
                                return emptyTuple;  //nothing else yet!
                            }
                        
                        break;
                    case 3:
                        return emptyTuple; //There's no use trying to swap a frequency 3 order with this method, they can only occur on set days, and this method does not swap on same days
                    case 4:
                        //Super flexible, it's feasible as long as you don't plan twice on the same day
                        return emptyTuple;
                }
            }
#endregion

            //Get the current times
            aTotalTime = routeA.TotalTime();

            bTotalTime = routeB.TotalTime();

            #region getswaps
            //Gather all possible (one-for-one) swaps
            for (int x = 1; x < routeB.GetRoute.Count - 1; x++)
            {
                if (routeB.GetRoute[x].frequency > 1)
                    continue;

                if (initialD.Item1 == targetDay.Item1)// && aNewTime + bNewTime > WORKINGDAY)
                    continue;

                //Get the new time with a swapped for b
                if(aIndex == 0)
                {
                    continue;   //could use some work
                }
                else if (aIndex == routeA.GetRoute.Count - 1)
                {
                    continue;  //could use some work
                }
                else
                {
                    aNewTime = aTotalTime - distanceMatrix.GetDistanceMatrix[routeA.GetRoute[aIndex - 1].matrixId, routeA.GetRoute[aIndex].matrixId] //route from a - 1 to a
                                            - distanceMatrix.GetDistanceMatrix[routeA.GetRoute[aIndex].matrixId, routeA.GetRoute[aIndex + 1].matrixId] //route from a to a + 1
                                            - orderA.totalEmptyingTime                                                                                  //a's emptying time
                                            + distanceMatrix.GetDistanceMatrix[routeA.GetRoute[aIndex - 1].matrixId, routeB.GetRoute[x].matrixId]      //route from a - 1 to b
                                            + distanceMatrix.GetDistanceMatrix[routeB.GetRoute[x].matrixId, routeA.GetRoute[aIndex + 1].matrixId]      //route from a + 1 to b
                                            + routeB.GetRoute[x].totalEmptyingTime;                                                                    //b's emptying time

                    bNewTime = GetNewTime(bTotalTime, routeB, orderA, x);
                }


                //Calculate how much time is gained
                aGains = aNewTime - aTotalTime;
                bGains = bNewTime - bTotalTime;
                totalGains = aGains + bGains;

                //If, the time and load with b for a checks out, and the time and load with a for b checks out
                aLoad = routeA.TotalLoad() - orderA.volumeOfOneContainer * orderA.numberOfContainers / 5 + routeB.GetRoute[x].volumeOfOneContainer * routeB.GetRoute[x].numberOfContainers / 5;
                bLoad = routeB.TotalLoad() - routeB.GetRoute[x].volumeOfOneContainer * routeB.GetRoute[x].numberOfContainers / 5 + orderA.numberOfContainers * orderA.volumeOfOneContainer / 5;

                if (aLoad < MAXLOAD && bLoad < MAXLOAD && dayA.CheckNewRoutes(routeA.RouteID, aNewTime) && dayB.CheckNewRoutes(routeB.RouteID, bNewTime))   //if the new solution is valid
                {
                    //PossibleIndexes.Add(new Tuple<int,int, double>(x, routeB.RouteID, totalGains));
                    if (totalGains < possibility.Item3) //if the new possibility is better than the current best, accept it
                    {
                        possibility = new Tuple<int, int, double>(x, routeB.RouteID, totalGains);
                    }
                }
            }
            #endregion

            if (possibility.Item1 != -1)    //if we found a possible swap, we return it (and in this case, it's the best swap we found)
            {
                Order orderB = week.GetWeek[targetDay.Item1].GetRoutes[targetDay.Item2].GetRoute[possibility.Item1];

                outcome = new Tuple<operation, bool, double, List<Tuple<int, int, int, Order>>>(operation.Swap, true, possibility.Item2, new List<Tuple<int, int, int, Order>>());
                outcome.Item4.Add(new Tuple<int, int, int, Order>(initialD.Item1, initialD.Item2, aIndex, orderA)); //Add order A (day, route, index in route, order)
                outcome.Item4.Add(new Tuple<int, int, int, Order>(targetDay.Item1, targetDay.Item2, possibility.Item1, orderB));
            }

            //Add the additional orders to the list in outcome, so everything will be swapped
            if(orderA.frequency > 1 && startDayNumber == -1)
            {
                switch (orderA.frequency)
                {
                    case 2:
                        if ((outcome.Item1 != operation.Null || earlyday2.Item1 != operation.Null) && (lateday1.Item1 != operation.Null || lateday2.Item1 != operation.Null))
                        {
                            if (outcome.Item1 == operation.Null)
                            {
                                outcome = earlyday2;    //the swap from the other first day succeeded, use that one
                            }

                            if (lateday1.Item1 == operation.Null)
                            {
                                outcome.Item4.Add(lateday2.Item4[0]);   //orderA
                                outcome.Item4.Add(lateday2.Item4[1]);   //orderB
                            }
                            else
                            {
                                outcome.Item4.Add(lateday1.Item4[0]);   //orderA
                                outcome.Item4.Add(lateday1.Item4[1]);   //orderB
                            }
                        }
                        else
                            return emptyTuple;
                        break;
                }
            }

            return outcome;
        }

        public bool containsOrder(int dayNumber, Order order)
        {
            bool contains = false;

            Day day = week.GetWeek[dayNumber];
            
            foreach(Route route in day.GetRoutes)
            {
                if (route.GetRoute.Contains(order))
                    contains = true;
            }

            return contains;
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
        public bool SwapLocalOrders()
        {
            bool swapComplete = false;

            // Pick a random route...   
            Tuple<int, int> tuple = SelectRandomRoute();
            Route route = week.GetWeek[tuple.Item1].GetRoutes[tuple.Item2];
            double totalTime = route.TotalTime();
            // And pick a random Order within that route.
            //int orderN = random.Next(route.GetRoute.Count());
            //if (orderN == route.GetRoute.Count()-1)
            //    return false;

            for (int orderN = 0; orderN < route.GetRoute.Count - 1; orderN++)
            {

                // Swap that order with the next order in the list.
                Order temp = route.GetRoute[orderN];
                route.GetRoute[orderN] = route.GetRoute[orderN + 1];
                route.GetRoute[orderN + 1] = temp;
                // Update the starting times of each route within the day.
                week.GetWeek[tuple.Item1].UpdateRoutes();

                // Check if there's any gain from swapping...
                if (!(route.CheckRoute() && route.TotalTime() <= totalTime) && (random.Next(101) > 95 && route.CheckRoute()))
                {
                    // If not, revert the changes.
                    route.GetRoute[orderN + 1] = route.GetRoute[orderN];
                    route.GetRoute[orderN] = temp;
                    // And update the startingtimes again.
                    week.GetWeek[tuple.Item1].UpdateRoutes();
                }
                else
                    swapComplete = true;
            }

            return swapComplete;
        }

        public void DoOperation(operation op, List<Tuple<int, int, int, Order>> orderData)
        {
            Order order, orderA, orderB;
            Day dayA, dayB;
            Route route, routeA, routeB;
            int aIndex, bIndex;

            switch(op)
            {
                case operation.Delete:
                    route = week.GetWeek[orderData[0].Item1].GetRoutes[orderData[0].Item2];
                    order = route.GetRoute[orderData[0].Item3];
                    route.Remove(orderData[0].Item3);
                    if (route.GetRoute.Count == 0)
                        week.GetWeek[orderData[0].Item1].GetRoutes.Remove(route);
                    week.GetWeek[orderData[0].Item1].UpdateRoutes();
                    orderMatrix.GetOrderMatrix.Add(order.orderId, order);
                    break;
                case operation.Add:
                    order = orderData[0].Item4;
                    week.GetWeek[orderData[0].Item1].GetRoutes[orderData[0].Item2].GetRoute.Insert(orderData[0].Item3, order);
                    week.GetWeek[orderData[0].Item1].UpdateRoutes();
                    orderMatrix.GetOrderMatrix.Remove(order.orderId);
                    break;
                case operation.Swap:
                    dayA = week.GetWeek[orderData[0].Item1];
                    dayB = week.GetWeek[orderData[1].Item1];

                    routeA = dayA.GetRoutes[orderData[0].Item2];
                    routeB = dayB.GetRoutes[orderData[1].Item2];

                    aIndex = orderData[0].Item3;
                    bIndex = orderData[1].Item3;

                    orderA = orderData[0].Item4;
                    orderB = orderData[1].Item4;

                    //remove a, insert b, clean up
                    routeA.Remove(aIndex);
                    routeA.GetRoute.Insert(aIndex, orderB);
                    dayA.UpdateRoutes();

                    //Remove b, insert a, clean up
                    routeB.Remove(bIndex);
                    routeB.GetRoute.Insert(bIndex, orderA);
                    dayB.UpdateRoutes();

                    //In case we swapped a frequency 2 order
                    if(orderData.Count == 4)
                    {
                        dayA = week.GetWeek[orderData[2].Item1];
                        dayB = week.GetWeek[orderData[3].Item1];

                        routeA = dayA.GetRoutes[orderData[2].Item2];
                        routeB = dayB.GetRoutes[orderData[3].Item2];

                        aIndex = orderData[2].Item3;
                        bIndex = orderData[3].Item3;

                        orderA = orderData[2].Item4;
                        orderB = orderData[3].Item4;

                        //remove a, insert b, clean up
                        routeA.Remove(aIndex);
                        routeA.GetRoute.Insert(aIndex, orderB);
                        dayA.UpdateRoutes();

                        //Remove b, insert a, clean up
                        routeB.Remove(bIndex);
                        routeB.GetRoute.Insert(bIndex, orderA);
                        dayB.UpdateRoutes();
                    }
                    break;
                case operation.Shift:
                    //dayA is the day we have to remove from, dayB is the day we have to Add to
                    dayA = week.GetWeek[orderData[0].Item1];
                    dayB = week.GetWeek[orderData[1].Item1];

                    routeA = dayA.GetRoutes[orderData[0].Item2];
                    routeB = dayB.GetRoutes[orderData[1].Item2];

                    aIndex = orderData[0].Item3;
                    bIndex = orderData[1].Item3;

                    orderA = orderData[0].Item4;

                    //Remove from day A
                    routeA.Remove(aIndex);
                    dayA.UpdateRoutes();

                    //Shift to day B
                    routeB.GetRoute.Insert(bIndex, orderA);
                    dayB.UpdateRoutes();

                    //Change the bit so future checks don't go boom
                    if(orderA.frequency == 4 && dayA.DayNumber != dayB.DayNumber)
                    {
                        orderA.SetBit(dayA.DayNumber, false);
                        orderA.SetBit(dayB.DayNumber, true);
                    }
                    else if(orderA.frequency == 2 && dayA.DayNumber != dayB.DayNumber)
                    {
                        orderA.SetBit(dayA.DayNumber, false);
                        orderA.SetBit(dayB.DayNumber, true);
                    }

                    if(orderData.Count > 2)
                    {
                        //dayA is the day we have to remove from, dayB is the day we have to Add to
                        dayA = week.GetWeek[orderData[2].Item1];
                        dayB = week.GetWeek[orderData[3].Item1];

                        routeA = dayA.GetRoutes[orderData[2].Item2];
                        routeB = dayB.GetRoutes[orderData[3].Item2];

                        aIndex = orderData[2].Item3;
                        bIndex = orderData[3].Item3;

                        orderA = orderData[2].Item4;

                        //Remove from day A
                        routeA.Remove(aIndex);
                        dayA.UpdateRoutes();

                        //Shift to day B
                        routeB.GetRoute.Insert(bIndex, orderA);
                        dayB.UpdateRoutes();

                        //Change the bit so future checks don't go boom
                        orderA.SetBit(dayA.DayNumber, false);
                        orderA.SetBit(dayB.DayNumber, true);
                    }

                    break;
            }
        }

        /// <summary>
        /// Returns a tuple with a random day and route.
        /// </summary>
        /// <returns></returns>
        public Tuple<int, int> SelectRandomRoute(int day = -1)
        {
            // Pick a random day and route.
            Tuple<int, int> tuple;
            int dayN = random.Next(week.GetWeek.Count());
            if (day != -1)
                dayN = day;
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

        public bool AcceptOperation(double ctrlPM, double timeIncrease)
        {
            double rnd = random.NextDouble();
            double e = Math.Exp(timeIncrease / ctrlPM);

            if (rnd <= e)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Chooses a random operation from the 3 existing operations based on the given weighted values.
        /// </summary>
        /// <param name="a">Weight for Delete</param>
        /// <param name="b">Weight for Add</param>
        /// <param name="ctrlPM">Control Parameter</param>
        public bool RandomOperation(double a, double b, double ctrlPM)
        {
            // Roll a random number.
            double rnd = random.NextDouble();
            // Set the output boolean to true (normal operation)
            bool output = true;

            // If the number rolls to a value below/equal to the a parameter...
            if (rnd <= a && orderMatrix.GetOrderMatrix.Count < 20)
                // Do a delete.
                output = Delete(ctrlPM);
            // If it rolls higher than a or equal/below b...
            else if (rnd <= a + b + orderMatrix.GetOrderMatrix.Count / 100)
                // Check if the orderMatrix if empty.
                if (orderMatrix.GetOrderMatrix.Count != 0)
                    // If it isn't, do an Add.
                    output = Add(ctrlPM);
            // If it rolls higher than b.
            else
                // Do a shift.
                output = Shift(ctrlPM);

            // Return the boolean value that represents the type of operation (normal/accepted)
            return output;
        }
    }
}
