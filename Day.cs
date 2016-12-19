using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Grote_Opdracht
{
    /// <summary>
    /// Class the represents an entire working Day for one truck.
    /// </summary>
    public class Day
    {
        // Constants
        private int WORKINGDAY = 43200;
        private int TOTALLOAD = 20000;
        // Objects
        private List<Route> routes;
        // Variables
        /// <summary>
        /// Integer that corresponds to the day of the week.
        /// </summary>
        private int dayNumber;
        /// <summary>
        /// Holds the IDnumber of the truck that will process this route.
        /// </summary>
        private int truckID;


        public Day(int dayNumber, int truckID)
        {
            this.dayNumber = dayNumber;
            this.truckID = truckID;
            routes = new List<Route>();
        }

        /// <summary>
        /// Adds an extra route to the workingday.
        /// </summary>
        public void AddRoute(Route route)
        {
            routes.Add(route);
        }

        public int DayNumber
        {
            get { return dayNumber; }
        }

        /// <summary>
        /// Checks if the solution for a day is feasible.
        /// </summary>
        /// <returns></returns>
        public bool CheckDay()
        {
            bool feasible = true;

            foreach (Route route in routes)
            {
                if (!route.CheckRoute())
                    return false;
            }

            return feasible;
        }

        public double DayTotalTime()
        {
            double time = 0;

            foreach(Route route in routes)
            {
                time += route.TotalTime();
            }

            return time;
        }
        
        /// <summary>
        /// Returns the truckID of the truck that will process this day.
        /// </summary>
        public int TruckID
        {
            get { return truckID; }
        }

        /// <summary>
        /// Prints the statistics for every route to the console.
        /// </summary>
        public void PrintCosts()
        {
            double costs = 0;

            Console.WriteLine("Day: {0}, TruckID: {1}", dayNumber, truckID);
            Console.WriteLine("");

            foreach (Route route in routes)
            {
                double cost = route.TotalTime();
                int load = route.TotalLoad();

                Console.WriteLine("ROUTE {0} -> StartTime: {1}, ProcessTime: {2}, Load: {3}", route.RouteID, route.StartTime, cost, load);
                costs += cost;
            }
            
            Console.WriteLine("Total Costs: {0}", costs);
            Console.WriteLine("Feasible: {0}", CheckDay());
            Console.WriteLine("---------------------------------------------------");
            Console.WriteLine("---------------------------------------------------");
        }

        /// <summary>
        /// Returns the list of Routes.
        /// </summary>
        public List<Route> GetRoutes
        {
            get { return routes; }
        }

        public void PrintOutput(StreamWriter sw)
        {
            int sequence = 1;

            foreach (Route route in routes)
            {
                route.PrintOutput(this, sw, sequence);

                sequence += route.Length();
            }
        }
    }
}
