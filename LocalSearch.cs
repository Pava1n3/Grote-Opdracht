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
        Random random;

        public LocalSearch(Week week)
        {
            this.week = week;          
        }

        public void SwapLocalOrders(Route route)
        {
            


        }

        public Route SelectRandomRoute()
        {
            int dayN = random.Next(week.GetWeek.Count());
            int routeN = random.Next(week.GetWeek[dayN].GetRoutes.Count());
            Route route = week.GetWeek[dayN].GetRoutes[routeN];

            if (route.GetRoute.Count() == 0)
                route = SelectRandomRoute();

            return route;
        }

        public Tuple<int, int> SelectRandomOrder(Route route)
        {

            int firstID = random.Next(route.GetRoute.Count());
            int secondID = random.Next(route.GetRoute.Count());
            Tuple<int, int> tuple = new Tuple<int, int>(firstID, secondID);

            if (firstID == secondID)
                tuple = SelectRandomOrder(route);

            return tuple;
        }



    }
}
