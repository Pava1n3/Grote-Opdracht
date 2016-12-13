using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Grote_Opdracht
{
    public class OrderMatrix
    {
        /// <summary>
        /// Dictionary that holds all the Data for each destination.
        /// </summary>
        public Dictionary<int, Order> orderMatrix = new Dictionary<int, Order>();
        /// <summary>
        /// Streamreader for OrderBestand.txt.
        /// </summary>
        StreamReader orderFileReader = new StreamReader(@"..\..\OrderBestand.txt");
        /// <summary>
        /// Converts the inputfile to the OrderMatrix.
        /// </summary>
        public void SetUpOrderMatrix()
        {
            // The first read is here because the first line is "MatrixID1;MatrixID2;Afstand;Rijtijd" and we don't need that line, by doing this we skip over it.
            orderFileReader.ReadLine();
            string orderMatrixRead = orderFileReader.ReadLine();    // We read a line from the text file.
            string[] orderMatrixLine;                               // And store the seperate data from one line in this array.
            int orderMatrixIndex;                                   // Variable that will hold the orderID.

            while (orderMatrixRead != null)
            {
                // Split a string in pieces that are delimited by a semicolon (e.g. "hello;world;!" become an array [hello, world, !]).
                orderMatrixLine = orderMatrixRead.Split(';');
                orderMatrixIndex = Convert.ToInt32(orderMatrixLine[0]);

                // Do note that we don't store the location name, I don't see a reason to do so.
                orderMatrix.Add(orderMatrixIndex, new Order());
                orderMatrix[orderMatrixIndex].frequency = Convert.ToInt16(orderMatrixLine[2][0].ToString());                             // frequency. This line is different because frequence is noted as 'XPWK'. We extract the first character (the number X denoting frequency) and convert it to an int. *you can treat strings as arrays in C# hence the [0] gets the first character
                orderMatrix[orderMatrixIndex].numberOfContainers = Convert.ToInt16(orderMatrixLine[3]);                                  // number of containers
                orderMatrix[orderMatrixIndex].volumeOfOneContainer = Convert.ToInt16(orderMatrixLine[4]);                                // volume of one container
                orderMatrix[orderMatrixIndex].totalEmptyingTime = (int)(Convert.ToDouble(orderMatrixLine[5].Replace('.', ',')) * 60);    // Hold on for a second, the total emptying time is in minutes. So we convert it to seconds
                orderMatrix[orderMatrixIndex].matrixId = Convert.ToInt16(orderMatrixLine[6]);                                            // MatrixId
                orderMatrix[orderMatrixIndex].xCoördinate = Convert.ToInt32(orderMatrixLine[7]);                                         // X Coördinate of the order location
                orderMatrix[orderMatrixIndex].yCoördinate = Convert.ToInt32(orderMatrixLine[8]);                                         // Y Coördinate of the order location

                // And continue reading the inputfile.
                orderMatrixRead = orderFileReader.ReadLine();
            }
        }

        /// <summary>
        /// Returns the OrderMatrix.
        /// </summary>
        /// <returns></returns>
        public Dictionary<int, Order> GetOrderMatrix()
        {
            return orderMatrix;
        }
    }
}
