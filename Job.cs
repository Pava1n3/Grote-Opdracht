using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Grote_Opdracht
{
    public class Job             //An order (or job as I've decided to call it) as given in the Orderbestand.txt
    {
        public int day, ordernr, distanceMatrixId, vehicle;
        public Job previousJob = new Job(0, 33226, 0, 0);   //standard job is the waste disposal location

        public Job(int Day, int OrderNumber, int MatrixID, int VehicleNumber, Job PreviousJob = null)
        {
            day = Day;
            ordernr = OrderNumber;
            distanceMatrixId = MatrixID;
            vehicle = VehicleNumber;
            if (PreviousJob != null)
                previousJob = PreviousJob;
        }
    }
}
