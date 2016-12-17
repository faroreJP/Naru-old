using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Naru;
using Naru.Analyzer;
using Naru.Value;

namespace NaruTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(NaruEngine.Information);

			if (args.Length > 0) {
				NaruEngine.ExecutesScript ( System.IO.File.ReadAllText(args[0]) );
			}
			else {
				Script();
			}
        }

        static void Script() {
            while (true) {
                StringBuilder sb = new StringBuilder();


                Console.WriteLine("---script---");
                while (true) {
                    var f = Console.ReadLine();
                    if (f == "end") {
                        break;
                    }

                    sb.AppendLine(f);
                }
                Console.WriteLine("---script end---\n");

                Console.WriteLine("---output---");
                NaruEngine.ExecutesScript(sb.ToString());
                Console.WriteLine("\n---output end---");
            }
        }
        static void ScriptTill(int t) {
            while (true) {
                StringBuilder sb = new StringBuilder();


                Console.WriteLine("---script---");
                while (true) {
                    var f = Console.ReadLine();
                    if (f == "end") {
                        break;
                    }

                    sb.AppendLine(f);
                }
                Console.WriteLine("---script end---\n");

                Console.WriteLine("---output---");
                NaruEngine.ExecutesScript(sb.ToString(), Console.Out, t);
                Console.WriteLine("\n---output end---");
            }
        }
        static void Once() {
            var d = new MathDictionary();
            var naru = new NaruEngine(d);

            while (true) {
                Console.Write(">");
                var f = Console.ReadLine();
                try {
                    if (f == "printDir") {
                        Console.WriteLine(d.ToString());
                    }
                    else {
                        Console.WriteLine("=" + naru.Evaluates(f).ToString());
                    }
                }
                catch (Exception e) {
                    Console.WriteLine(e);
                    //Console.WriteLine(e.Message);
                }
            }
        }
        static void Release(string[] args) {
            Console.WriteLine(NaruEngine.Information);

            if (args.Contains("-s")) {
                Script();
            }
            else {
                Once();
            }
        }
    }
}
