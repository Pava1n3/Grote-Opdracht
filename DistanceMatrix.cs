using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Grote_Opdracht
{
    public class DistanceMatrix
    {
        /// <summary>
        /// Streamreader for AfstandenMatrix.txt.
        /// </summary>
        StreamReader distanceMatrixReader = new StreamReader(@"..\..\AfstandenMatrix.txt");
        /// <summary>
        /// Matrix/2D Array that holds all the traveling distances from and to all places.
        /// </summary>
        public int[,] distanceMatrix = new int[1099, 1099];
        /// <summary>
        /// Converts the inputfile to the DistanceMatrix.
        /// </summary>
        public void SetUpDistanceMatrix()
        {
            // The first read is here because the first line is "MatrixID1;MatrixID2;Afstand;Rijtijd" and we don't need that line, by doing this we skip over it.
            distanceMatrixReader.ReadLine();
            string distanceMatrixRead = distanceMatrixReader.ReadLine();    // We read a line from the text file.
            string[] distanceMatrixLine;                                    // And store the seperate data from one line in this array.

            // Goes over the whole text file and places each line in the array.
            while (distanceMatrixRead != null)
            {
                // Split a string in pieces that are delimited by a semicolon (e.g. "hello;world;!" becomes an array [hello, world, !]).
                distanceMatrixLine = distanceMatrixRead.Split(';');
                distanceMatrix[Convert.ToInt32(distanceMatrixLine[0]), Convert.ToInt32(distanceMatrixLine[1])] = Convert.ToInt16(distanceMatrixLine[3]);
                distanceMatrixRead = distanceMatrixReader.ReadLine();
            }

            // Random Sentence
            Console.WriteLine("We read the whole distance Matrix!");
        }

        /// <summary>
        /// Returns the DistanceMatrix
        /// </summary>
        /// <returns></returns>
        public int[,] GetDistanceMatrix()
        {
            return distanceMatrix;
        }
    }
}
