using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grote_Opdracht
{
    public class Week
    {
        private List<Day> week;

        public Week()
        {
            week = new List<Day>();
        }

        public void AddDay(Day day)
        {
            week.Add(day);
        }

        public void PrintOutput()
        {
            foreach (Day day in week)
            {
                day.PrintOutput();
            }
        }
    }
}
