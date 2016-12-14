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


        public Day()
        {
            dayNumber = 1;
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

        public void PrintOutput()
        {
            int sequence = 1;

            foreach (Route route in day)
            {
                route.PrintOutput(this, sequence);

                sequence += route.Length();
                sequence++;
            }
        }
    }
}
