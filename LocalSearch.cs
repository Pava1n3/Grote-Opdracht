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

            for (int x = 0; x < 5; x++)
            {
                while (true)
                {

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
