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

        // Objects
        Week week;
        OrderMatrix orderMatrix;
        DistanceMatrix distanceMatrix;
        Random random = new Random();

        public LocalSearch(Week week, OrderMatrix orderMatrix, DistanceMatrix distanceMatrix)
        {
            this.week = week;
            this.orderMatrix = orderMatrix;
        }

        public void DeleteOrder()
        {
            double maxGain = int.MaxValue;
            int tempOrderN = 0;
            Tuple<int, int> tempTuple = null;
            Order temp = null;

            for (int x = 0; x < 5; x++)
            {
                Tuple<int, int> tuple = SelectRandomRoute();
                Route route = week.GetWeek[tuple.Item1].GetRoutes[tuple.Item2];
                int orderN = random.Next(route.GetRoute.Count());
                double totalTime = route.TotalTime();


                Order order = route.GetRoute[orderN];
                if (order.frequency > 1)
                    continue;
                route.Remove(orderN);

                double timeGain = totalTime - route.TotalTime();

                if (timeGain <= maxGain)
                {
                    maxGain = timeGain;

                    if (temp != null)
                    {
                        route = week.GetWeek[tempTuple.Item1].GetRoutes[tempTuple.Item2];
                        route.GetRoute.Insert(tempOrderN, temp);
                    }

                    temp = order;
                    tempTuple = tuple;
                    tempOrderN = orderN;
                }
                else
                    route.GetRoute.Insert(orderN, order);                
            }

            orderMatrix.GetOrderMatrix.Add(temp.orderId, temp);
        }

        public void AddOrder()
        {
            Order order = orderMatrix.GetOrderMatrix.Last().Value;
            orderMatrix.GetOrderMatrix.Remove(order.orderId);

            bool spotFound = false;
            int weekIndex = 0, weekCounter = 0; //index keeps track of which day we are at, counter helps exiting the while if we have checked the whole week
            double totalTime, newTime = 0, dayTime;
            double chance = 80; //acceptance chance

            for (int x = 0; x < 5; x++)
            {
                weekIndex = random.Next(week.GetWeek.Count);


                while (!spotFound)
                {
                    Day day = week.GetWeek[weekIndex];
                    dayTime = day.DayTotalTime();

                    foreach(Route route in day.GetRoutes)
                    {
                        totalTime = route.TotalTime();

                        for (int y = 0; y < route.GetRoute.Count; y++)
                        {
                            if (y == route.GetRoute.Count)
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

                            if (dayTime - totalTime + newTime <= HALFFIVE)
                            {
                                //consider this spot
                            }
                        }
                    }

                    //So we don't go out of bounds
                    weekIndex++;
                    weekCounter++;
                    if (weekIndex >= week.GetWeek.Count)
                        weekIndex = 0;
                    if (weekCounter >= week.GetWeek.Count)
                        break;
                }
            }
        }


        public void SwapLocalOrders()
        {
            Tuple<int, int> tuple = SelectRandomRoute();
            Route route = week.GetWeek[tuple.Item1].GetRoutes[tuple.Item2];
            double totalTime = route.TotalTime();
            int orderN = random.Next(route.GetRoute.Count());
            if (orderN == route.GetRoute.Count()-1)
                return;

            Order temp = route.GetRoute[orderN];
            route.GetRoute[orderN] = route.GetRoute[orderN + 1];
            route.GetRoute[orderN + 1] = temp;

            if (!(route.CheckRoute() && route.TotalTime() <= totalTime))
            {
                route.GetRoute[orderN + 1] = route.GetRoute[orderN];
                route.GetRoute[orderN] = temp;
            }
        }

        public Tuple<int, int> SelectRandomRoute()
        {
            Tuple<int, int> tuple;
            int dayN = random.Next(week.GetWeek.Count());
            int routeN = random.Next(week.GetWeek[dayN].GetRoutes.Count());

            if (week.GetWeek[dayN].GetRoutes.Count() == 0)
                tuple = SelectRandomRoute();
            else
                tuple = new Tuple<int, int>(dayN, routeN);

            return tuple;
        }
    }
}
