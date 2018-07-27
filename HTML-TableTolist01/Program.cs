using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HTML_TableTolist01
{
    class Program
    {
        public const int htmlTableWidth = 15;

        static void Main(string[] args)
        {

            List<string> linesFromFile = GetLinesFromFile(@"X:\Feature Line Report2.html", out int numberOfFetchedLines);

            List<string> listOfStations = GetNumberOfStations(linesFromFile);

            string[,] matrixFromSource = new string[numberOfFetchedLines, htmlTableWidth];

            matrixFromSource = GetCleanMatrix(linesFromFile, numberOfFetchedLines, htmlTableWidth);

            List<DataPoint> points = GetPointsFromMatrix(matrixFromSource, numberOfFetchedLines, htmlTableWidth, listOfStations);
            
            SaveNXYH(points, "test.txt");

            SaveFull(points, "testFull.txt");



            Console.WriteLine("End of operation");

            Console.ReadLine();
        }












        public string[,] TransposeStringMatrix(string[,] matrix)
        {
            int w = matrix.GetLength(0);
            int h = matrix.GetLength(1);

            string[,] result = new string[h, w];

            for (int i = 0; i < w; i++)
            {
                for (int j = 0; j < h; j++)
                {
                    result[j, i] = matrix[i, j];
                }
            }

            return result;
        }


        public static List<string> GetLinesFromFile(string filename, out int noOflines)
        {
            List<string> results = new List<string>();

            // Read data from file.
            using (StreamReader sr = File.OpenText(filename))
            {
                string input = null;
                int i = 0;
                while ((input = sr.ReadLine()) != null)
                {
                    results.Add(input);
                    i++;
                }
                noOflines = i;
            }
            return results;
        }

        public static void SaveNXYH(List<DataPoint> pointList, string filename)
        {
            using (StreamWriter writer = File.CreateText(filename))
            {
                foreach (DataPoint p in pointList)
                {
                    writer.Write(p.Number + "\t");
                    writer.Write(p.Easting + "\t");
                    writer.Write(p.Northing + "\t");
                    writer.Write(p.Elevation);
                    writer.Write(writer.NewLine);
                }
            }
        }

        public static void SaveFull(List<DataPoint> pointList, string filename)
        {
            using (StreamWriter writer = File.CreateText(filename))
            {
                foreach (DataPoint p in pointList)
                {
                    writer.Write(p.Number + "\t");
                    writer.Write(p.Easting + "\t");
                    writer.Write(p.Northing + "\t");
                    writer.Write(p.Elevation + "\t");
                    writer.Write(p.Station + "\t");
                    writer.Write(p.Offset + "\t");
                    writer.Write(p.Slope + "\t");
                    writer.Write(p.Code);
                    writer.Write(writer.NewLine);
                }
            }
        }

        public static List<string> GetNumberOfStations (List<string> input)
        {
            List<string> result = new List<string>();

            foreach (string item in input)
            {
                string newItem = null;

                if (item != null)
                {
                    newItem = Regex.Replace(item, "\t", "");
                    newItem = Regex.Replace(newItem, ",", "");
                    newItem = Regex.Replace(newItem, "\\+", "");
                    newItem = Regex.Replace(newItem, "m", "");

                    Regex r1 = new Regex("Station", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    MatchCollection matches1 = r1.Matches(newItem);

                    foreach (Match match in matches1)
                    {
                        result.Add(newItem);
                        //Console.Write(match.Groups[1].ToString());
                        //Console.WriteLine();
                    }                 
                }
            }

            // clean list of stations
            result.RemoveAt(0);
            for (int j = 0; j < result.Count; j++)
            {
                result[j] = result[j].Replace("Station: ", "");
            }

            return result;
        }

        public static string [,] GetCleanMatrix (List<string> input, int linesNumber, int tableWidth)
        {
            string[,] cleanStrings = new string[linesNumber, tableWidth];
            string[,] cleanStringsCurated = new string[linesNumber, tableWidth];

            int i = 0;
            foreach (string item in input)
            {
                string newItem = null;

                if (item != null)
                {
                    newItem = Regex.Replace(item, "\t", "");
                    newItem = Regex.Replace(newItem, ",", "");
                    newItem = Regex.Replace(newItem, "\\+", "");
                    newItem = Regex.Replace(newItem, "m", "");

                    Regex r2 = new Regex("<td.*?>(.*?)</td>", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    MatchCollection matches2 = r2.Matches(newItem);

                    int j = 0;
                    foreach (Match match in matches2)
                    {
                        cleanStrings[i, j] = match.Groups[1].ToString();
                        //Console.Write(cleanStrings[i, j] + " ");
                        j++;
                    }

                    i++;
                    //Console.WriteLine();
                }
            }            

            for (int x = 0; x < linesNumber; x++)
            {
                for (int y = 0; y < tableWidth; y++)
                {
                    if (cleanStrings[x, 0] == "Code" ||
                        cleanStrings[x, 0] == "Offset" ||
                        cleanStrings[x, 0] == "Elevation" ||
                        cleanStrings[x, 0] == "Slope" ||
                        cleanStrings[x, 0] == "Easting" ||
                        cleanStrings[x, 0] == "Northing")
                        if (cleanStrings[x, y] != "&nbsp;")
                            cleanStringsCurated[x, y] = cleanStrings[x, y];
                }
            }

            return cleanStringsCurated;
        }

        public static List<DataPoint> GetPointsFromMatrix (string[,] input, int linesNumber, int tableWidth, List<string> stations)
        {
            int pointCounter = 0;
            int stationCounter = 0;

            List<DataPoint> newPoints = new List<DataPoint>();

            for (int x = 0; x < linesNumber; x++)
            {

                if (input[x, 0] == "Offset")
                {
                    for (int y = 0; y < tableWidth; y++)
                    {
                        if (double.TryParse(input[x, y], out _))
                        {
                            pointCounter++;
                            DataPoint p = new DataPoint();
                            p.Number = pointCounter;
                            p.Station = stations[stationCounter];
                            p.Code = input[(x - 2), y] ?? input[(x - 2), (y + 1)];
                            p.Offset = input[x, y];
                            p.Elevation = input[(x + 2), y];
                            p.Slope = input[(x + 4), y];
                            p.Easting = input[(x + 6), y];
                            p.Northing = input[(x + 8), y];
                            newPoints.Add(p);
                        }
                    }

                    stationCounter++;
                }
            }

            return newPoints;
        }
    }
}
