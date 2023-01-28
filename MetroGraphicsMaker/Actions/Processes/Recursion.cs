using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WpfApplication1.Actions.Processes
{
    class Recursion
    {
            static List<String[]> _stringList;

            private static void MakeSentence(int i, string res)
            {
                String[] current;
                string Prevres;
                current = _stringList[i];
                for (int y = 0; y < current.Length; y++)
                {
                    Prevres = res;
                    res = res + " " + current[y];
                    if (i < _stringList.Count - 1)
                    {
                        MakeSentence(i + 1, res);
                    }
                    else
                    {
                        Console.WriteLine(res);
                    }
                    res = Prevres;
                }
            }

            private static void Main()
            {

                _stringList = new List<String[]>();

                String[] name = { 
                                "Anthon",
                                "Peter", 
                                "John" };

                String[] verb = { 
                                "goes",
                                "runs" };

                String[] mission = {
                                   "to School",
                                   "to Park" };

                _stringList.Add(name);
                _stringList.Add(verb);
                _stringList.Add(mission);
                string res = "";
                MakeSentence(0, res);

                Console.ReadKey();
            }
        }
}
