using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grote_Opdracht
{
    public class Solution    //describes a solution for the trash collection
    {
        public int cost = 0;
        public List<List<List<Job>>> schedule = new List<List<List<Job>>>();

        public Solution()
        {
        }

        public int CalculateCost(OrderMatrix orderMatrix, DistanceMatrix distanceMatrix)  //This version of the method might be a bit costly
        {
            foreach (List<List<Job>> bigList in schedule)
            {
                foreach (List<Job> jobList in bigList)
                {
                    foreach (Job job in jobList)
                    {
                        cost += orderMatrix.GetOrderMatrix()[job.ordernr].totalEmptyingTime;   //Add the total emptying time
                        if (jobList[0] != job)
                            cost += distanceMatrix.GetDistanceMatrix()[orderMatrix.GetOrderMatrix()[jobList[jobList.IndexOf(job) - 1].ordernr].matrixId, orderMatrix.GetOrderMatrix()[job.ordernr].matrixId];           //Add the traveling time
                    }
                }
            }

            foreach (List<List<Job>> bigList in schedule)
            {
                foreach (KeyValuePair<int, Order> orderTuple in orderMatrix.GetOrderMatrix())
                {
                    bool contains = false;

                    foreach (List<Job> jobList in bigList)
                    {
                        foreach (Job job in jobList)
                        {
                            if (job.ordernr == orderTuple.Key)
                                contains = true; ;
                        }
                    }

                    if (!contains)
                        cost += orderTuple.Value.totalEmptyingTime * 3;
                }
            }

            return cost;
        }


        public Boolean IsFeasible(OrderMatrix orderMatrix, DistanceMatrix distanceMatrix)
        {
            int load = 0;

            foreach (List<List<Job>> bigList in schedule)
            {
                foreach (List<Job> jobList in bigList)
                {
                    foreach (Job job in jobList)
                    {
                        if (orderMatrix.GetOrderMatrix()[job.ordernr].matrixId == 0)
                            load = 0;
                        else
                        {
                            load += orderMatrix.GetOrderMatrix()[job.ordernr].volumeOfOneContainer * orderMatrix.GetOrderMatrix()[job.ordernr].numberOfContainers;

                            if (load > 100000)
                                return false;
                        }
                    }
                }
            }

            return false;
        }

        public void AddJob(int day, int ordernr, int vehicle)
        { 
            
        }

        public void RemoveJob()
        { }

        public void SwapJobs()
        { }

        public int Cost
        {
            get { return cost; }
        }
    }
}
