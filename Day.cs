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
        // Objects
        private List<Route> day;
        // Variables
        /// <summary>
        /// Integer that corresponds to the day of the week.
        /// </summary>
        private int dayNumber;


        public Day(int dayNumber)
        {
            this.dayNumber = dayNumber;
            day = new List<Route>();
        }

        /// <summary>
        /// Adds an extra route to the workingday.
        /// </summary>
        public void AddRoute(Route route)
        {
            day.Add(route);
        }

        public int DayNumber
        {
            get { return dayNumber; }
        }

        public void PrintOutput(StreamWriter sw)
        {
            int sequence = 1;

            foreach (Route route in day)
            {
                route.PrintOutput(this, sw, sequence);

                sequence += route.Length();
            }
        }
    }
}
