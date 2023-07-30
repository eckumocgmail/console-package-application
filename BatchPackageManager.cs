using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Console_PackageApplication
{
    using static System.IO.Directory;
    public class BatchPackageManager : IPackageManager
    {
        private static string PackagesStoreDirectory = @"D:\System-Config\PackageStore";



        private static string BatchPackagesDirectory = @"D:\System-Config\PackageBatches";
        /*    public static string FormatKebabStyle(string title, IEnumerable<string> options) => (string)
                PackageManagerProgram.InvokeFromFile<string>(
                    "D:\\System-Config\\MyApplications\\Console_InputApplication\\Console_InputApplication.dll", "InputApplicationProgram", "SingleSelect", new object[] { title, options });*/

        private PackageManager _packageManager;
        private string _packagesDir;
        private string _batcheesDir;



        private string GetBatchFormattedName(string name) => $"{name.ToLower()}";


        public BatchPackageManager() : this(PackagesStoreDirectory, BatchPackagesDirectory)
        {
        }


        public BatchPackageManager(string PackagesDirectory) : this(PackagesDirectory, Path.Combine(PackagesDirectory, $".{nameof(BatchPackageManager)}"))
        {

        }

        public BatchPackageManager(string packagesStoreDirectory, string batchPackagesDirectory)
        {
            _packageManager = new PackageManager(PackagesStoreDirectory = packagesStoreDirectory);
            if (Exists(BatchPackagesDirectory = batchPackagesDirectory) == false)
                CreateDirectory(BatchPackagesDirectory);
            if (PackagesStoreDirectory == batchPackagesDirectory)
                throw new ArgumentException("packagesStoreDirectory", "batchPackagesDirectory");

        }

        public bool HasName(string name)
            => ListBatchPackageNames()
                .Contains(GetBatchFormattedName(name));




        [Description("Создать новый модуль пакетной установки")]
        public IEnumerable<string> ListBatchPackageNames()
            => GetFiles(_packagesDir, "*.pack")
                .Select(path => path.Substring(path.LastIndexOf("\\") + 1));


        [Description("Создать новый модуль пакетной установки")]
        public void ExecuteBatchPackage(string name)
        {
            if (!HasName(name))
            {
                throw new Exception();
            }
            else
            {
                foreach (var next in File.ReadAllLines($"{name}.pack"))
                {
                    if (string.IsNullOrWhiteSpace(next))
                        continue;

                    string[] words;
                    if ((words = next.Split(":")).Length != 2)
                        throw new FormatException("Текст операции не соответвует формату");
                    string directory = GetCurrentDirectory();
                    string application = words[0];
                    string version = words[1];
                    _packageManager.Unpack(application, version, directory);
                }
            }
        }

        [Description("Создать новый модуль пакетной установки")]
        public void AddBatchPackage(string name, IEnumerable<KeyValuePair<string, string>> moduleVersions)
        {
            File.WriteAllLines($"{name}.pack", moduleVersions.Select(kv => $"{kv.Key}:{kv.Value}"));
        }


        [Description("Удалить данные модуля пакетной установки")]
        public void RemoveBatchPackage(string name, IEnumerable<KeyValuePair<string, string>> moduleVersions)
        {
            File.Delete($"{name}.pack");
        }

        public IEnumerable<string> GetApplications()
        {
            return ((IPackageManager)_packageManager).GetApplications();
        }

        public IEnumerable<string> GetVersions(string application)
        {
            return ((IPackageManager)_packageManager).GetVersions(application);
        }

        public string GetFileName(string application, string version)
        {
            return ((IPackageManager)_packageManager).GetFileName(application, version);
        }

        public void Pack(string application, string version )
        {
            ((IPackageManager)_packageManager).Pack(application, version );
        }

        public void Unpack(string application, string version, string directory)
        {
            ((IPackageManager)_packageManager).Unpack(application, version, directory);
        }

        public string GetNextVersion(string application)
        {
            return ((IPackageManager)_packageManager).GetNextVersion(application);
        }

        public string GetLastVersion(string application)
        {
            return ((IPackageManager)_packageManager).GetLastVersion(application);
        }
    }
}
