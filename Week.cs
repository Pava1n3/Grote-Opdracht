using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

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

        public void PrintCosts()
        {
            foreach (Day day in week)
                day.PrintCosts();
        }

        public void UpdateDays()
        {
            foreach (Day day in week)
                day.UpdateRoutes();
        }

        public List<Day> GetWeek
        {
            get { return week; }
        }

        public void PrintOutput(StreamWriter sw)
        {
            foreach (Day day in week)
            {
                day.PrintOutput(sw);
            }
        }
    }
}
