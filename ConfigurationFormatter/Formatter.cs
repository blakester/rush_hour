using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ConfigurationFormatter
{
    class Formatter
    {
        static void Main(string[] args)
        {
            foreach (string line in File.ReadLines("../../unformatted_configurations.txt"))
            {
                string[] sections = line.Split(';');

                // print the first section
                Console.Write(sections[0].Trim() + "; ");

                // format and print the vehicle encodings
                string[] vehicleEncodings = sections[1].Trim().Split(' ');
                for (int i = 0; i < vehicleEncodings.Length; i++)
                {
                    // place a space between each character of the encoding
                    for (int j = 0; j < vehicleEncodings[i].Length; j++)
                    {
                        string character = vehicleEncodings[i][j].ToString();

                        // handle last character
                        if (j == vehicleEncodings[i].Length - 1)
                            if (i == vehicleEncodings.Length - 1)
                                Console.Write(character + "; ");
                            else
                                Console.Write(character + ", ");
                        else
                            Console.Write(character + " ");
                    }
                }

                // format and print the solution moves
                string[] solutionMoves = sections[2].Trim().Split(' ');                
                for (int i = 0; i < solutionMoves.Length; i++)
                {
                    string formattedMove = "";
                    formattedMove += (solutionMoves[i][0] + " ");

                    int spaces = Int32.Parse(solutionMoves[i][2].ToString());
                    if (solutionMoves[i][1].Equals('L') || solutionMoves[i][1].Equals('U'))
                        spaces *= -1;

                    // handle last move
                    formattedMove += (i == solutionMoves.Length - 1 ? (spaces - 2).ToString() : (spaces + ", "));
                    Console.Write(formattedMove);
                }

                // newline for next config
                Console.WriteLine();
            }
            Console.Read();
        }
    }
}
