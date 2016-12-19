using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grote_Opdracht
{
    class LocalSearch
    {
        // Objects
        Week week;
        OrderMatrix orderMatrix;
        Random random = new Random();

        public LocalSearch(Week week, OrderMatrix orderMatrix)
        {
            this.week = week;
            this.orderMatrix = orderMatrix;
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

        public void AddOrder()
        {
            Order order = orderMatrix.GetOrderMatrix.Last().Value;
            orderMatrix.GetOrderMatrix.Remove(order.orderId);

            for (int x = 0; x < 5; x++)
            {
                while (true)
                {

                }
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
