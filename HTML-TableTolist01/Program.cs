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
        static void Main(string[] args)
        {
            //const string msgFormat = "table[{0}], tr[{1}], td[{2}], a: {3}, b: {4}";
            //const string table_pattern = "<table.*?>(.*?)</table>";
            //const string tr_pattern = "<tr.*?>(.*?)</tr>";
            //const string td_pattern = "<td.*?>(.*?)</td>";
            //const string a_pattern = "<a href=\"(.*?)\"></a>";
            //const string b_pattern = "<b>(.*?)</b>";

            int numberOfFetchedLines;

            List<string> listOfStations = new List<string>();

            List<string> linesOfFile = FetchFileByLine(@"X:\Feature Line Report2.html", out numberOfFetchedLines);

            string[,] cleanStrings = new string[numberOfFetchedLines, 15];
            string[,] cleanStringsCurated = new string[numberOfFetchedLines, 15];

            List<DataPoint> points = new List<DataPoint>();

            int i = 0;
            foreach (string item in linesOfFile)
            {
                string newItem = null;

                if (item != null)
                {
                    newItem = Regex.Replace(item, "\t","");
                    newItem = Regex.Replace(newItem, ",", "");
                    newItem = Regex.Replace(newItem, "\\+", "");
                    newItem = Regex.Replace(newItem, "m", "");

                    //newItem = item;
                    
                    Regex r1 = new Regex("Station", RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline | RegexOptions.IgnoreCase);
                    MatchCollection matches1 = r1.Matches(newItem);

                    foreach (Match match in matches1)
                    {
                        listOfStations.Add(newItem);
                        Console.Write(match.Groups[1].ToString());
                        Console.WriteLine();                        
                    }

                    Regex r2 = new Regex("<td.*?>(.*?)</td>",RegexOptions.IgnorePatternWhitespace|RegexOptions.Singleline|RegexOptions.IgnoreCase);
                    MatchCollection matches2 = r2.Matches(newItem);


                    int j = 0;
                    foreach (Match match in matches2)
                    {
                        cleanStrings[i, j] = match.Groups[1].ToString();
                        Console.Write(cleanStrings[i, j] + " ");
                        j++;
                    }

                    i++;
                    Console.WriteLine();
                }
            }

            // fix list of stations
            listOfStations.RemoveAt(0);
            for (int j = 0; j < listOfStations.Count; j++)
            {
                listOfStations[j] = listOfStations[j].Replace("Station: ", "");
            }


            for (int x =0; x < numberOfFetchedLines; x++)
            {
                for (int y=0;y<15;y++)
                {
                    if (cleanStrings[x, 0] == "Code"||
                        cleanStrings[x, 0] == "Offset" ||
                        cleanStrings[x, 0] == "Elevation" ||
                        cleanStrings[x, 0] == "Slope" ||
                        cleanStrings[x, 0] == "Easting" ||
                        cleanStrings[x, 0] == "Northing")
                        if(cleanStrings[x, y] != "&nbsp;")
                            cleanStringsCurated[x, y] = cleanStrings[x, y];
                }
                
            }


            i = 0;

            int pointCounter = 0;
            int stationCounter = 0;

            for (int x = 0; x < numberOfFetchedLines; x++)
            {

                if (cleanStringsCurated[x, 0] == "Offset")
                {
                    for (int y = 0; y < 15; y++)
                    {
                        if (double.TryParse(cleanStringsCurated[x, y], out _))
                        {
                            pointCounter++;
                            DataPoint p = new DataPoint();
                            p.Number = pointCounter;
                            p.Station = listOfStations[stationCounter];
                            p.Code = cleanStringsCurated[(x - 2), y] ?? cleanStringsCurated[(x - 2), (y + 1)];
                            p.Offset = cleanStringsCurated[x, y];
                            p.Elevation= cleanStringsCurated[(x + 2), y];
                            p.Slope = cleanStringsCurated[(x + 4), y];
                            p.Easting = cleanStringsCurated[(x + 6), y];
                            p.Northing = cleanStringsCurated[(x + 8), y];
                            points.Add(p);
                        }                        
                    }

                    stationCounter++;
                }
            }

            SaveNXYH(points, "test.txt");

            SaveFull(points, "testFull.txt");



            Console.WriteLine("End of operation");

            Console.ReadLine();
        }

        public string [,] TransposeStringMatrix(string[,] matrix)
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


        public static List<string> FetchFileByLine(string filename, out int noOflines)
        {
            List<string> results = new List<string>();

            // Now read data from file.
            Console.WriteLine("Reading File:\n");
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

        public static void SaveNXYH (List<DataPoint> pointList, string filename)
        {
            using (StreamWriter writer = File.CreateText(filename))
            {
                foreach (DataPoint p in pointList)
                {
                    writer.Write(p.Number+"\t");
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


    }
}
