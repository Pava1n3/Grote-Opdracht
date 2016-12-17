using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grote_Opdracht
{
    public class Order
    {
        /// <summary>
        /// Integers that we read from the input.
        /// </summary>
        public int frequency, numberOfContainers, volumeOfOneContainer, orderId, xCoördinate, yCoördinate, matrixId, counter;
        /// <summary>
        /// Double that we read from the input (Double because it has decimals).
        /// </summary>
        public double totalEmptyingTime;
        /// <summary>
        /// Holds the days of week on which the order already has been processed. 0xE0 corresponds to 1110 0000.
        /// </summary>
        private byte dayByte = 0xE0;
        /// <summary>
        /// Boolean that tells if the order has been processed (used for higher frequency orders).
        /// </summary>
        public bool processed;

        public Order()
        {
        }

        /// <summary>
        /// Sets a bit in the byte to 0 or 1.
        /// </summary>
        /// <param name="dayNumber">The number of the bit you want to change (0 for Monday, 4 for Friday).</param>
        /// <param name="value">The value to which you want to change the bit (0 or 1).</param>
        public void SetBit(int dayNumber, bool value)
        {
            if (value)
                dayByte = (byte)(dayByte | (1 << dayNumber-1));
            else
                dayByte = (byte)(dayByte & ~(1 << dayNumber-1));
        }

        /// <summary>
        /// Returns if the order has been processed on the given day.
        /// </summary>
        /// <param name="dayNumber">The day that you to check.</param>
        /// <returns></returns>
        public bool GetBit(int dayNumber)
        {
            return ((dayByte & (1 << dayNumber)) != 0);
        }

        /// <summary>
        /// Returns the number of times the order has been completed before the given day.
        /// </summary>
        /// <param name="dayNumber">The day up to which you want to check.</param>
        /// <returns></returns>
        public int DaysProcessed(int dayNumber)
        {
            int days = 0;

            for (int x = 0; x < dayNumber; x++)
            {
                if (GetBit(x))
                    days++;
            }

            return days;
        }

        /// <summary>
        /// Debug method to check on which days the order has been processed.
        /// </summary>
        public void PrintDebug(Day day)
        {
            Console.WriteLine(DaysProcessed(day.DayNumber));
            Console.WriteLine("Is Pos 0 set to 1? {0}", GetBit(0));
            Console.WriteLine("Is Pos 1 set to 1? {0}", GetBit(1));
            Console.WriteLine("Is Pos 2 set to 1? {0}", GetBit(2));
            Console.WriteLine("Is Pos 3 set to 1? {0}", GetBit(3));
            Console.WriteLine("Is Pos 4 set to 1? {0}", GetBit(4));
        }

    }
}
