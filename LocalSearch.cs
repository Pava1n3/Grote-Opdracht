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

        // Checkers
        public int adds, deletes, shifts, bAdds, bDeletes, bShifts, swaps, bSwaps;

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
                        bDeletes++;
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
                        return true;
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
                        bShifts++;

                        //Change some bits if necessary
                        if (order.frequency == 4 && startDay.DayNumber != tarDay.DayNumber)
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
            Tuple<int, int, int> possibility = new Tuple<int, int, int>(0, 0, 0);
            int[] keys = orderMatrix.GetOrderMatrix.Keys.ToArray();
            int key = keys[random.Next(keys.Length)];
            Order order = orderMatrix.GetOrderMatrix[key];

            double oTime = 0;
            double best = double.MinValue;

            // Check in what place we can actually add the order, starting...
            // For every day in week...
            for (int x = 0; x < week.GetWeek.Count; x++)
            {
                // For every route in day...
                for (int y = 0; y < week.GetWeek[x].GetRoutes.Count; y++)
                    // For every order in route...
                    for (int z = 0; z < week.GetWeek[x].GetRoutes[y].GetRoute.Count; z++)
                    {
                        // Calculate the original time of the day with the added penalty.
                        oTime = week.GetWeek[x].Costs() + order.totalEmptyingTime * 3;
                        // Insert the order on the 'z' index.
                        week.GetWeek[x].GetRoutes[y].GetRoute.Insert(z, order);
                        // Update everything
                        week.GetWeek[x].UpdateRoutes();
                        // Check if the solution for the whole day is still feasible.

                        if (!(WORKINGDAY < week.GetWeek[x].Costs() || week.GetWeek[x].GetRoutes[y].TotalLoad() > MAXLOAD))
                        {
                            // If it is, calculate the newTime for the day with the added order.
                            double newTime = oTime - week.GetWeek[x].Costs();
                            // If the time is better than previous tested additions or if it is the first iteration...
                            if (best < newTime)
                            {
                                // Update the values.
                                best = newTime;
                                possibility = new Tuple<int, int, int>(x, y, z);
                            }
                        }

                        // Reinsert the order.
                        week.GetWeek[x].GetRoutes[y].GetRoute.RemoveAt(z);
                        // And reset the values.
                        week.GetWeek[x].UpdateRoutes();
                    }

                // If this is the last iteration...
                if (x == week.GetWeek.Count - 1)
                {
                    // If the addition is a TimeIncrease, run the acceptation Method.
                    if (best < 0)
                    {
                        // Make an acceptationFlag.
                        bool accept = AcceptOperation(ctrlPM, best);
                        // If the addition is accepted (always when it isn't a TimeIncrease)...
                        if (accept)
                        {
                            // Add the order on the optimal spot.
                            orderMatrix.GetOrderMatrix.Remove(key);
                            week.GetWeek[possibility.Item1].GetRoutes[possibility.Item2].GetRoute.Insert(possibility.Item3, order);
                            week.GetWeek[possibility.Item1].UpdateRoutes();
                            bAdds++;
                            return false;
                        }
                        else
                            return true;
                    }
                    // If the addition is an optimization...
                    else
                    {
                        // Add the order on the optimal spot.
                        orderMatrix.GetOrderMatrix.Remove(key);
                        week.GetWeek[possibility.Item1].GetRoutes[possibility.Item2].GetRoute.Insert(possibility.Item3, order);
                        week.GetWeek[possibility.Item1].UpdateRoutes();
                    }
                }
            }
            return true;
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

            for (int orderN = 0; orderN < route.GetRoute.Count - 1; orderN++)
            {
                // Swap that order with the next order in the list.
                Order temp = route.GetRoute[orderN];
                route.GetRoute[orderN] = route.GetRoute[orderN + 1];
                route.GetRoute[orderN + 1] = temp;
                // Update the starting times of each route within the day.
                week.GetWeek[tuple.Item1].UpdateRoutes();

                // Check if there's any gain from swapping...
                if (week.GetWeek[tuple.Item1].Costs() > WORKINGDAY || route.TotalTime() > totalTime)
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

            if (!swapComplete)
                bSwaps++;
            swaps++;

            return swapComplete;
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
        public bool RandomOperation(double a, double b, double c, double ctrlPM)
        {
            // Roll a random number.
            double rnd = random.NextDouble();
            // Set the output boolean to true (normal operation)
            bool output = true;

            // If the number rolls to a value below/equal to the a parameter...
            if (rnd <= a)
            {
                // Do a delete.
                output = Delete(ctrlPM);
                deletes++;
            }
            // If it rolls higher than a or equal/below b...
            else if (rnd <= a + b)
            {   
                // Check if the orderMatrix if empty.
                if (orderMatrix.GetOrderMatrix.Count != 0)
                {
                    // If it isn't, do an Add.
                    output = Add(ctrlPM);
                    adds++;
                }
            }
            else if (rnd <= a + b + c)
            {
                output = SwapLocalOrders();

            }
            // If it rolls higher than b.
            else
            {
                // Do a shift.
                output = Shift(ctrlPM);
                shifts++;
            }

            // Return the boolean value that represents the type of operation (normal/accepted)
            return output;
        }
    }
}
