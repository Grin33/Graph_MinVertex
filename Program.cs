using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Matvey
{
    class Point
    {
        public int Number { get; set; }
        public List<int> Connections { get; set; }
        public Point(int number, List<int> connections)
        {
            Connections = connections;
            Number = number;
        }
        private string ConsStr(List<int> cons)
        {
            string str = "";
            foreach (int i in cons) { str += i; str += ","; }
            return str;
        }
        public override string ToString()
        {
            return ($"Number: {Number} Connected With {ConsStr(Connections)}");
        }
        //public Point Clone() => new Point { Number, Connections };
    }
    class Program
    {
        static object locker = new object();
        static List<int> Ans = new List<int>();

        static List<Point> Init2()
        {
            var toFill = new List<Point>
            {
                new Point (1,new List<int>{2,4 })
                ,new Point (2,new List<int>{1,4})
                ,new Point (3,new List<int>{4 })
                ,new Point (4,new List<int>{1,2,3,5 })
                ,new Point (5,new List<int>{4 })
                ,new Point (6,new List<int>{ })
            };
            return toFill;
        }
        static List<Point> Init1()
        {
            var toFill = new List<Point>
            {
                new Point (1,new List<int>{5,2 })
                ,new Point (2,new List<int>{1,3,5})
                ,new Point (3,new List<int>{2,4 })
                ,new Point (4,new List<int>{3,5,6 })
                ,new Point (5,new List<int>{1,2,4 })
                ,new Point (6,new List<int>{4 })
            };
            return toFill;
        }
        static List<Point> DeleteVertex(List<Point> tem, int i)
        {
            var ToRet = new List<Point>(tem.Count);
            foreach (var item in tem)
            {
                var newP = new Point(item.Number, item.Connections);
                ToRet.Add(newP);
            }
            for (int k = 0; k < ToRet[i].Connections.Count; k++) // проход по коннектам работающей точки графа
            {
                for(int j = 0; j < ToRet.Count; j++)
                {
                    if (ToRet[j].Number == ToRet[i].Connections[k]) // че не нравится блять
                    {
                        for(int c = 0; c < ToRet[j].Connections.Count; c++)
                        {
                            if (ToRet[j].Connections[c] == ToRet[i].Number)
                            {
                                var tet = new List<int>(ToRet[j].Connections);
                                tet.RemoveAt(c);
                                ToRet[j].Connections = new List<int>(tet);
                                //ToRet[j].Connections.RemoveAt(c);
                                //ToRet[i].Connections.Remove(ToRet[j].Number);
                                break;
                            }
                        }
                    }
                }
            }
            ToRet[i].Connections = new List<int>();

            return ToRet;
        }
        static void Check(List<Point> tempoints,List<int> tempans)
        {
            bool ch = true;
            foreach(var p in tempoints)
            {
                if (p.Connections.Count != 0)
                {
                    ch = false;
                }
            }
            if (ch)
            {
                if (Ans.Count > tempans.Count)
                {
                    Ans = new List<int>(tempans);
                }
                else if (Ans.Count == 0)
                {
                    Ans = new List<int>(tempans);
                }
            }
        }
        static void ShuffleNested(List<Point> tempoints, int v, List<int> tempans)
        {
            Check(tempoints, tempans);
            int n = v + 1;
            for(int i = n; i < tempoints.Count; i++)
            {
                var newtemp = new List<Point>(tempoints.Count);
                var newans = new List<int> (tempans);
                newans.Add(tempoints[i].Number);
                foreach(var item in tempoints)
                {
                    var newP = new Point((int)item.Number, (List<int>)item.Connections);
                    newtemp.Add(newP);
                }
                newtemp = DeleteVertex(newtemp, i);
                //Console.WriteLine("Workin on " + tempoints[i].Number);
                //foreach (var r in newtemp)
                //{
                //    Console.WriteLine(r);
                //}
                //Console.WriteLine();
                ShuffleNested(newtemp, i, newans);
                
            }
        }
        static void ShuffleStart(List<Point> points)
        {
            for(int i = 0; i < points.Count; i++)
            {
                var tempPoints = new List<Point>(points.Count);
                foreach (var item in points)
                {
                    var newP = new Point((int)item.Number, (List<int>)item.Connections);
                    tempPoints.Add(newP);
                }
                tempPoints = DeleteVertex(tempPoints, i);
                List<int> tempans = new List<int>() { points[i].Number };
                ShuffleNested(tempPoints, i, tempans);
            }
        }

        static void ParalCheck(List<Point> tempoints, List<int> tempans,ref List<int> LocalThreadAns)
        {
            bool ch = true;
            foreach(var p in tempoints)
            {
                if (p.Connections.Count != 0)
                {
                    ch = false;
                }
            }
            if (ch)
            {
                if (LocalThreadAns.Count > tempans.Count)
                {
                    LocalThreadAns = new List<int>(tempans);
                }
                else if (LocalThreadAns.Count == 0)
                {
                    LocalThreadAns = new List<int>(tempans);
                }
            }
        }
        static void Parallel_Nested(List<Point> tempoints, int v, List<int> iterans,ref List<int> localThreadAns)
        {
            ParalCheck(tempoints, iterans,ref localThreadAns);
            int n = v + 1;
            for(int i = n; i < tempoints.Count; i++)
            {
                var newtemp = new List<Point>(tempoints.Count);
                var newans = new List<int>(iterans);
                newans.Add(tempoints[i].Number);
                foreach (var item in tempoints)
                {
                    var newP = new Point((int)item.Number, (List<int>)item.Connections);
                    newtemp.Add(newP);
                }
                newtemp = DeleteVertex(newtemp, i);
                Parallel_Nested(newtemp, i, newans,ref localThreadAns);
            }
        }

        static void Parallel_Shuffle(List<Point> points)
        {
            Parallel.For(0, points.Count, () => new List<int>(), (i, loop, localans) =>
            {
                var tempoints = new List<Point>(points.Count);
                foreach (var item in points)
                {
                    var newP = new Point((int)item.Number, (List<int>)item.Connections);
                    tempoints.Add(newP);
                }
                tempoints = DeleteVertex(tempoints, i);
                var iterans = new List<int>() { points[i].Number };
                Parallel_Nested(tempoints, i, iterans, ref localans);
                return localans;
            },
            (x)=>
            {
                lock(locker)
                {
                    if ((Ans.Count > x.Count) && (x.Count != 0))
                    {
                        Ans = new List<int>(x);
                    }
                    else if((Ans.Count == 0) && (x.Count != 0))
                    {
                        Ans = new List<int>(x);
                    }
                    Console.WriteLine();
                }
            }
            );
        }
        static void Main()
        {
            var points = new List<Point>();
            points = Init1();
            var points1 = new List<Point>();
            points1 = Init1();
            foreach (var item in points)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("\n Straight Ans: ");
            var sw = new Stopwatch();
            sw.Start();
            ShuffleStart(points);
            foreach(int i in Ans)
            {
                Console.WriteLine(i);
            }
            sw.Stop();
            Console.WriteLine("Straight Time: "+sw.Elapsed.ToString());
            
            
            Ans = new List<int>();
            var sw1 = new Stopwatch();
            sw1.Start();
            Parallel_Shuffle(points);
            Console.WriteLine("\n Parallel Ans:");
            foreach (int i in Ans)
            {
                Console.WriteLine(i);
            }
            sw1.Stop();
            Console.WriteLine("Parallel Time: " + sw1.Elapsed.ToString());
            Console.WriteLine("Straight Time: " + sw.Elapsed.ToString());

        }
    }
}
