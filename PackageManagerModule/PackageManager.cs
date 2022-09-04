using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


/// <summary>
/// Реализация программы управления пакетами.
/// Просто сохраняет исходники в архивную диекторию
/// </summary>
public class PackageManager: IPackageManager
{

    /// <summary>
    /// Хранилище файлов
    /// </summary>
    
    private string PackageStoreDirectory = @"D:\System-Config\PackageStore";

    public PackageManager(string PackageStoreDirectory)
    {
        this.PackageStoreDirectory = PackageStoreDirectory;

    }

    /// <summary>
    /// СОздаёт новый ZIP-архив
    /// </summary>
    /// <param name="application">наименование модуля</param>
    /// <param name="version">версия модуля</param>
    /// <param name="directory">каталог исхолдного кода</param>
    public void Pack(string application, string version, string directory)
    {

        try
        {
            string appDirectory = Path.Combine(PackageStoreDirectory,application).ToString();
            if (System.IO.Directory.Exists(appDirectory) == false)
                System.IO.Directory.CreateDirectory(appDirectory);
            Console.WriteLine("Сохранение пакета "+application+" версии "+version+" в архив: "+appDirectory+"\\"+application+"_"+version+".zip");

            ZipFile.CreateFromDirectory(directory, GetFileName(application, version));
            Console.WriteLine(new FileInfo(appDirectory + "\\" + application + "_" + version + ".zip").Length + " байт");


        }
        catch (Exception ex)
        {
            throw new Exception($"Не удалось создать пакет приложения {application} версия {version} из {directory}", ex);
        }
    }

    /// <summary>
    /// Распаковка архива
    /// </summary>
    public void Unpack(string application, string version, string directory)
    {
        try
        {
            string appDirectory = @$"{PackageStoreDirectory}\\{application}";
            if (System.IO.Directory.Exists(appDirectory) == false)
                System.IO.Directory.CreateDirectory(appDirectory);

            ZipFile.CreateFromDirectory(directory, GetFileName(application, version));
        }
        catch (Exception ex)
        {
            throw new Exception($"Не удалось создать разпаковать приложение {application} версия {version} в {directory}", ex);
        }
    }

    /// <summary>
    /// Абсолютный путь к файлу архива
    /// </summary>
    public string GetFileName(string application, string version)
        => $"{PackageStoreDirectory}\\{application}\\{application}_{version}.zip";


    /// <summary>
    /// Возвращает список модулей (поддиректории хранилища)
    /// </summary>    
    public IEnumerable<string> GetApplications()    
        => System.IO.Directory
            .GetDirectories(PackageStoreDirectory);

    /// <summary>
    /// Возвращает список версия (файлы в директории приложения)
    /// </summary>  
    public IEnumerable<string> GetVersions(string application)
    {
        string appDirectory = @$"{PackageStoreDirectory}\\{application}";
        if (System.IO.Directory.Exists(appDirectory) == false)
            System.IO.Directory.CreateDirectory(appDirectory);
        return System.IO.Directory
            .GetFiles($"{PackageStoreDirectory}\\{application}", "*.zip")
            .Select(file => Path.GetFileNameWithoutExtension(file));
    }

    /// <summary>
    /// Печать всех приложений (всех версий)
    /// </summary>
    public void Log()
    {
        foreach (var application in this.GetApplications())
        {
            foreach (var version in this.GetVersions(application))
            {
                Console.WriteLine(application + " " + version);
            }
        }
    }

    /// <summary>
    /// Возвращает метку для следующей версии
    /// </summary>
    public string GetNextVersion(string application)
    {
        return (this.GetVersions(application).Count() + 1).ToString();
    }

    /// <summary>
    /// Возвращает метку для последней версии
    /// </summary>
    public string GetLastVersion(string application)
    {
        return this.GetVersions(application).Count().ToString();
    }
}
