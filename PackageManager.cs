using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static System.IO.Directory;
using static Newtonsoft.Json.JsonConvert;
using System.Diagnostics;
using Console_PackageApplication;
using Console_InputApplication.InputApplicationModule.Files;

/// <summary>
/// Реализация программы управления пакетами.
/// Просто сохраняет исходники в архивную директорию
/// </summary>
public class PackageManager : DirectoryResource, IPackageManager
{
    private static string PackageStorageDefaults = @"D:\System-Config\PackageStore";

    private string PackageStorage = @"D:\System-Config\PackageStore";
    private string AppDirectory = null;


    private PackageManagerConfiguration AppConfiguration { get; set; }
    private FileController<PackageManagerConfiguration> ConfigurationController;

    public PackageManager(string appDirectory = "D:\\System-Config\\ProjectsConsole\\Console_DesctopApplication") : base(appDirectory)
    {
        this.AppDirectory = appDirectory;
        this.AppConfiguration = GetConfiguration();
        this.Info($"AppDirectory = {appDirectory}");
        this.Info($"AppConfiguration = {AppConfiguration.ToJson()}");
    }

    public PackageManagerConfiguration GetConfiguration()
    {
        try
        {
            string[] args = null;
            this.ConfigurationController = new FileController<PackageManagerConfiguration>
            (
                System.IO.Path.Combine(this.AppDirectory, $"{nameof(PackageManagerConfiguration)}.json")
            );
            if (ConfigurationController.Has() == false)
            {
                this.AppConfiguration = ConfigurationController.Model = (InputConsole.Input<PackageManagerConfiguration>
                (
                    "Конфигурация приложения",
                    (value) =>
                    {
                        var results = new List<string>();
                        value.Validate().ToList().ForEach((next) => results.AddRange(next.Value));
                        return results;
                    },
                    ref args

                ));
                ConfigurationController.Set();
            }
            this.AppConfiguration = ConfigurationController.Get();
            this.AppConfiguration.Name = this.AppDirectory.Substring(this.AppDirectory.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1);
            ConfigurationController.Set();

            System.IO.File.WriteAllText(
                System.IO.Path.Combine(AppDirectory, $"\\{GetType().Name}.json"),
                SerializeObject(AppConfiguration = ConfigurationController.Get(), Newtonsoft.Json.Formatting.Indented)
            );
            return AppConfiguration;
        }
        catch (Exception ex)
        {
            throw new Exception("Не удалось считать конфигурацию PackageManager");
        }
    }



    [Label("Настройки мэнэджера пакетов")]
    public class PackageManagerConfiguration
    {
        public string Name { get; set; } = "PackageManagerConfiguration";
        public string Version { get; set; } = "1.0.0";
        public Dictionary<string, string> Imports { get; set; } = new Dictionary<string, string>();
    }



    /// <summary>
    /// Выполняет из рабочей директории проекта
    /// Возвращает сведения о установленных пакетах, которых хранятся в в файле PackageManage.json
    /// </summary>    
    public IDictionary<string, string> GetInstalledPackages()
    {
        //return this.Commit<IDictionary<string, string>>(() =>
        //{
            if (AppDirectory.GetFiles().Any(file => file.EndsWith($"\\{GetType().Name}.json")))
            {
                AppConfiguration = DeserializeObject<PackageManagerConfiguration>(
                    System.IO.File.ReadAllText(
                        System.IO.Path.Combine(AppDirectory, $"\\{GetType().Name}.json")));
                return AppConfiguration.Imports;
            }
            else
            {
                string[] args = new string[0];
                return GetConfiguration().Imports;
            }
        //}, new Dictionary<string, object>() { });
    }



 

   

    /// <summary>
    /// Абсолютный путь к файлу архива
    /// </summary>
    public string GetFileName(string application, string version)
        => $"{PackageStorage}\\{application}\\{application}_{version}.zip";


    /// <summary>
    /// Возвращает список модулей (поддиректории хранилища)
    /// </summary>    
    public IEnumerable<string> GetApplications()
        => System.IO.Directory
            .GetDirectories(PackageStorage)
            
            .Select(path => path.Substring(path.LastIndexOf(System.IO.Path.DirectorySeparatorChar)+1))
        .Where(dir => dir.StartsWith(".") == false);

    /// <summary>
    /// Возвращает список версия (файлы в директории приложения)
    /// </summary>  
    public IEnumerable<string> GetVersions(string application)
    {
        string appDirectory = @$"{PackageStorage}\\{application}";
        if (System.IO.Directory.Exists(appDirectory) == false)
            System.IO.Directory.CreateDirectory(appDirectory);
        string pattern = $"{PackageStorage}\\{application}";
        return System.IO.Directory.GetDirectories(@$"{PackageStorage}\\{application}").Where(dir => dir.Contains(".BatchPackageManager")==false);
        /*return System.IO.Directory
            .GetFiles($"{Global}\\{application}", "*.zip")
            .Select(file => System.IO.Path.GetFileNameWithoutExtension(file))
            .Where(file => file != $",{nameof(BatchPackageManager)}");*/
    }
 

    /// <summary>
    /// Возвращает метку для следующей версии
    /// </summary>
    public string GetNextVersion(string application)
    => (this.GetVersions(application).Count() + 1).ToString();

    /// <summary>
    /// Возвращает метку для последней версии
    /// </summary>
    public string GetLastVersion(string application)
        => this.GetVersions(application).Count().ToString();

    internal void RemovePackage(string package)
    {
        throw new NotImplementedException();
    }

    internal string GetAppDirectory() => this.AppDirectory;


    /// <summary>
    /// СОздаёт новый ZIP-архив
    /// </summary>
    /// <param name="module">наименование модуля</param>
    /// <param name="version">версия модуля</param>
    /// <param name="directory">каталог исхолдного кода</param>
    internal void PackAndPush()
    {

        string module = AppConfiguration.Name;
        string version = this.GetLastVersion(this.AppConfiguration.Name);
     
     
        this.Info($"Pack( {module}, {version}, {this.AppDirectory} )");
        try
        {

            string appDirectory = System.IO.Path.Combine(PackageStorage, module).ToString();
            if (System.IO.Directory.Exists(appDirectory) == false)
                System.IO.Directory.CreateDirectory(appDirectory);

            this.Info("Сохранение пакета " + module + " версии " + version + " " +
                "в архив: " + appDirectory + "\\" + module + "_" + version + ".zip");

            ZipFile.CreateFromDirectory(this.AppDirectory, GetFileName(module, version));
            Console.WriteLine(new FileInfo(appDirectory + "\\" + module + "_" + version + ".zip").Length + " байт");
            this.Info($"Pack( {module}, {version}, {this.AppDirectory} ) SUCCESS");
        }
        catch (Exception ex)
        {
            this.Info($"Pack( {module}, {version}, {this.AppDirectory} ) FAILED");
            throw new Exception($"Не удалось создать пакет приложения {module} версия {version} из {this.AppDirectory}", ex);
        }
        finally
        {
            this.Info($"Pack( {module}, {version}, {this.AppDirectory} ) COMPLETE");
        }
    }








    /// <summary>
    /// СОздаёт новый ZIP-архив
    /// </summary>
    /// <param name="module">наименование модуля</param>
    /// <param name="version">версия модуля</param>
    /// <param name="directory">каталог исхолдного кода</param>
    public void Pack()
    {
        string module = this.AppDirectory.Substring(this.AppDirectory.LastIndexOf(System.IO.Path.DirectorySeparatorChar) + 1);
        string version = this.GetNextVersion(module);
        this.Pack(module,version);
    }



    public void Pack(string module, string version)
    {
        this.Info($"Pack( {module}, {version}, {this.AppDirectory} )");
        try
        {

            string appDirectory = System.IO.Path.Combine(PackageStorage, module).ToString();
            if (System.IO.Directory.Exists(appDirectory) == false)
                System.IO.Directory.CreateDirectory(appDirectory);
            this.Info("Сохранение пакета " + module + " версии " + version + " " +
                "в архив: " + appDirectory + "\\" + module + "_" + version + ".zip");
            ZipFile.CreateFromDirectory(this.AppDirectory, GetFileName(module, version));
            Console.WriteLine(new FileInfo(appDirectory + "\\" + module + "_" + version + ".zip").Length + " байт");


            this.Info($"Pack( {module}, {version}, {this.AppDirectory} ) SUCCESS");
        }
        catch (Exception ex)
        {
            this.Info($"Pack( {module}, {version}, {this.AppDirectory} ) FAILED");
            throw new Exception($"Не удалось создать пакет приложения {module} версия {version} из {this.AppDirectory}", ex);
        }
        finally
        {
            this.Info($"Pack( {module}, {version}, {this.AppDirectory} ) COMPLETE");
        }
    }














    /// <summary>
    /// Распаковка архива
    /// </summary>
    public void Unpack(string application, string version, string directory)
    {
        this.Info($"Unpack( {application}, {version}, {directory} )");
        try
        {
            string appDirectory = @$"{PackageStorage}\\{application}";
            if (System.IO.Directory.Exists(appDirectory) == false)
                System.IO.Directory.CreateDirectory(appDirectory);
            string outputdir = directory.CombinePath("modules").CombinePath($"{application}").CombinePath($"{version}");
            outputdir.EnsureDirectoryPathCreated();

            ZipFile.ExtractToDirectory(GetFileName(application, version), outputdir);
            this.Info($"Pack( {application}, {version}, {directory} ) SUCCESS");
            GetConfiguration().Imports[application] = version;
            this.ConfigurationController.Set();
        }
        catch (Exception ex)
        {
            this.Info($"Pack( {application}, {version}, {directory} ) FAILED");

            throw new Exception($"Не удалось создать разпаковать приложение {application} версия {version} в {directory}", ex);
        }
        finally
        {
            this.Info($"Pack( {application}, {version}, {directory} ) COMPLETE");
        }
    }
}

