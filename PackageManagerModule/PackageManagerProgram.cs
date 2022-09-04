using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
    
public class PackageManagerProgram
{
    private const string DefaultsPackageStoreDirectory = @"D:\System-Config\PackageStore";

    public static void Test() 
        => Run(new string[] { 
            @"d:\Commands", 
            @"d:\Commands.zip" 
        });
        

    /// <summary>
    /// Выполнение с параметрами
    /// </summary>    
    public static void Run(params string[] args)
    {        
        if (args.Length == 0)
        {
            var manager = new PackageManager(DefaultsPackageStoreDirectory);

            
            string directory = System.IO.Directory.GetCurrentDirectory();
            Console.WriteLine("Создание пакета: "+directory);

            var path = new List<string>(directory.Split("\\"));
            string application = path.Last();
            string version = manager.GetNextVersion(application); 
            manager.Pack(application, version, directory);
        }
        else
        {
            var manager = new PackageManager(DefaultsPackageStoreDirectory);
            string directory = System.IO.Directory.GetCurrentDirectory();
            foreach (string application in args)
            {                          
                string version = manager.GetLastVersion(application);
                manager.Pack(application, version, directory);
            }
        }
        
    }

    
}
 
