using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Console_PackageApplication
{

    class Program 
    {

        static void Main(string[] arguments)
        {



            InputConsole.Interactive();
            var p = new PackageManagerProgram(ref arguments);
            Catch
            (
                () => p.Run(ref arguments),
                (ex) => OnError(ex)
            );
        }

        private static void NewMethod()
        {
            var test = new PackageManagerTest();
            test.DoTest().ToDocument().WriteToConsole();
        }

        static void OnError(Exception ex)
        {
            Console.Clear();
            var p = ex;
            do
            {
                string app = p.Source;
                

                MethodBase action = p.TargetSite;
                string type = action.DeclaringType.Name;
                Console.WriteLine($"[{app}] => {type}.{action}:");
                if (p.StackTrace.IndexOf(":line")>0)
                {
                    int l =p.StackTrace.Length; 
                    int i = p.StackTrace.IndexOf(":line");
                    
                    string line = p.StackTrace.Substring(i + 5);
                    i = 0;
                    while ("0123456789".Contains(line[i]))i++;
                    line = line.Substring(0, i);
                    Console.WriteLine(
                        $"{type}:{line}" +
                        $"\n\t{action}" +
                        $"\n\t => {p.GetTypeName()} \n\t\t{p.Message}");
                    Console.WriteLine(p.HelpLink);
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine(
                        $"{type}" +
                        $"\n\t{action}" +
                        $"\n\t => {p.GetTypeName()} \n\t\t{p.Message}");
                    Console.WriteLine(p.HelpLink);
                    Console.WriteLine();
                }           
                p = p.InnerException;
            } while (p!=null);
        }

        static void Catch(Action action, Action<Exception> catcher)
        {
            try
            {
                Console.Clear();
                action();
            }
            catch(Exception ex)
            {
                catcher(ex);
            }
        }
    }
}
