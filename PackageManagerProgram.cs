 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

using static Newtonsoft.Json.JsonConvert;


public class PackageManagerProgram: IUserControl
{

    // туда будут устанавливаться пакеты
    private string ApplicationDirectory;
    private PackageManager _packageManager = new PackageManager(@"D:\");
    private IUserControl _userControl;

    private string GetHelp()
    => $"Используйте следующие аргументы выполнения: \n" +
        "pack.exe interactive - для выполнения импорта ресурсов в рабочую директорию\n" +
        "pack.exe interactive [path] - для выполнения импорта ресурсов во внешнюю папку\n";


    public PackageManagerProgram(ref string[] args)
    {

        this._userControl = new ConsoleControl();
        this._packageManager = new PackageManager(
            ApplicationDirectory = InputConsole.SelectDirectory(ref args)
        );
    }
    
    public void Run(ref string[] args)
    {
        
      
        InputConsole.Clear();
        this.Info(GetInfo(_packageManager));

        switch (SingleSelect(
            "Выберите де", new string[] {
                "Установленные пакеты","Установить пакет","Зарегистрировать источник",
                "Создать пакет","Удалить источник","Выход"
            }, ref args))
        {
            case "Установленные пакеты":
                InputConsole.Clear();
                var packages = _packageManager.GetInstalledPackages();
                _packageManager.Info($"\n\nУстановленные пакеты ({packages.Count()})");

                foreach (var packageInfo in packages)
                    Console.WriteLine($"  {packageInfo.Key} -v {packageInfo.Value}");
                ConfirmContinue("Для работы с пакетами нажмите любую клавише ..");
                InputConsole.Clear();
                Run(ref args);
                break;
            case "Установить пакет":
                InputConsole.Clear();
                var selected = CheckList($"Установить пакет(ы)\n\n{GetInfo(_packageManager)}", _packageManager.GetApplications(), ref args);
                foreach (var packageInfo in selected)
                {                                     
                    _packageManager.Unpack(packageInfo, _packageManager.GetLastVersion(packageInfo), _packageManager.GetAppDirectory());
                }
                Run(ref args);
                break;
           
            case "Выход":
                OnExitProgram(ref args); break;
            default: break;
        }
    }

    private string GetInfo(PackageManager _packageManager)
     => $"\n => \n{ApplicationDirectory}\n\n{_packageManager.GetConfiguration().ToJsonOnScreen()}\n\n";
    

    private void OnCreatePackageSelected(ref string[] args)
    {
        Console.WriteLine("Создание пакета: " + ApplicationDirectory);
        var path = new List<string>(ApplicationDirectory.Split("\\")); 
        string application = path.Last();
        string version = new PackageManager( ).GetNextVersion(application);
        _packageManager.Pack( application, version  );
    }




    /*public void RunInteractiveStoreEditor( ref string[] args)
    {
        switch (SingleSelect("Редактор хранилища файловых модулей",
            new string[] {
                "Просмотреть источники",
                "Зарегистрировать источник",
                "Удалить источник"
            }))
        { 
            case "Просмотреть пакетные источники": 
                OnPackageSelected(SingleSelect(
                    "Пакетные источники",
                    _packageManager.GetApplications(),
                    ref args
                ),ref args);
                break;
            case "Зарегистрировать источник":
                OnPackageCreate(InputString("Наименование пакета", value =>
                {
                    var errors = new List<string>();
                    //TODO: import files
                    return errors;
                    
                }, ref args),ref args); 
                break;
            case "Удалить источник":
                OnPackageDelete(SingleSelect(
                    "Пакетные источники",
                    _packageManager.GetApplications(),
                    ref args
                ), ref args);                
                break;
            case "Выход":
                OnExitProgram(ref args); break;
            default: break;
        }
    }*/

    private void OnPackageDelete(string package, ref string[] args)
    {
        if(ConfirmContinue($"Вы действительно хотите удалить пакеты {package}?")){
            _packageManager.RemovePackage(package);
        };
    }

    private void OnPackageSelected(string package, ref string[] args)
    {
        switch (SingleSelect("Редактор хранилища файловых модулей",
                    new string[] {
                "Просмотреть источники",
                "Зарегистрировать источник",
                "Удалить источник"
                    },ref args))
        {
            case "Просмотреть пакетные источники":
                OnPackageSelected(SingleSelect(
                    "Пакетные источники",
                    _packageManager.GetApplications(),
                    ref args
                ), ref args);
               break;              
            case "Выход":
                OnExitProgram(ref args); break;
            default: break;
        }
    }

   

     
    private void OnExitProgram(ref string[] args)
    {
        
    }

    public void RunInteractiveManager(ref string[] args)
    {
        
        var modules = CheckList(
            $"Ваши ресурсы" +
            $"Выберите устаночные модули", 
            _packageManager.GetApplications(), ref args);
        foreach (string application in modules)
        {
            Console.WriteLine(application);

            if (ConfirmContinue($"Требуется установка {application}"))
            {
                _packageManager.Unpack(
                    application,

                    this.SingleSelect(
                        $"Выберите версию пакета {application}",
                        _packageManager.GetVersions(application), ref args
                    ), 
                    ApplicationDirectory
                );
                Console.WriteLine($"Установлен {application}");
            }

            
        }
    }

    public bool ConfirmContinue(string title)
    {
        return _userControl.ConfirmContinue(title);
    }

    public IEnumerable<string> CheckList(string title, IEnumerable<string> options, ref string[] args)
    {
        return _userControl.CheckList(title, options, ref args);
    }

    public IEnumerable<string> MultiSelect(string title, IEnumerable<string> options, ref string[] args)
    {
        return _userControl.MultiSelect(title, options, ref args);
    }

    public string SingleSelect(string title, IEnumerable<string> options, ref string[] args)
    {
        return _userControl.SingleSelect(title, options, ref args);
    }

    public string InputCreditCard(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputCreditCard(title, validate, ref args);
    }

    public string InputCurrency(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputCurrency(title, validate, ref args);
    }

    public string InputColor(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputColor(title, validate, ref args);
    }

    public string InputDirectory(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputDirectory(title, validate, ref args);
    }

 

    public List<Dictionary<string, object>> InputPrimitiveCollection(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputPrimitiveCollection(title, validate, ref args);
    }

    public List<Dictionary<string, object>> InputStructureCollection(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputStructureCollection(title, validate, ref args);
    }

    public bool InputBool(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputBool(title, validate, ref args);
    }

    public string InputFile(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputFile(title, validate, ref args);
    }

    public string InputFilePath(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputFilePath(title, validate, ref args);
    }

    public string InputImage(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputImage(title, validate, ref args);
    }

    public string InputIcon(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputIcon(title, validate, ref args);
    }

    public string InputRusWord(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputRusWord(title, validate, ref args);
    }

    public string InputPassword(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputPassword(title, validate, ref args);
    }

    public string InputPhone(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputPhone(title, validate, ref args);
    }

    public string InputXml(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputXml(title, validate, ref args);
    }

    public string InputDate(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputDate(title, validate, ref args);
    }

    public string InputWeek(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputWeek(title, validate, ref args);
    }

    public string InputText(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputText(title, validate, ref args);
    }

    public string InputYear(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputYear(title, validate, ref args);
    }

    public string InputMonth(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputMonth(title, validate, ref args);
    }

    public string InputTime(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputTime(title, validate, ref args);
    }

    public string InputEmail(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputEmail(title, validate, ref args);
    }

    public string InputUrl(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputUrl(title, validate, ref args);
    }

    public string InputName(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputName(title, validate, ref args);
    }

    public int InputNumber(string title, Func<int, List<string>> validate, ref string[] args)
    {
        return _userControl.InputNumber(title, validate, ref args);
    }

    public string InputDecimal(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputDecimal(title, validate, ref args);
    }

    public int InputPercent(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputPercent(title, validate, ref args);
    }

    public int InputInt(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputInt(title, validate, ref args);
    }

    public int InputPositivNumber(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputPositiveNumber(title, validate, ref args);
    }

    public string InputString(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.InputString(title, validate, ref args);
    }

    
    public int InputPositiveNumber(string title, Func<object, List<string>> validate, ref string[] args)
    {
        throw new NotImplementedException();
    }

    public TModel Input<TModel>(string title, Func<object, List<string>> validate, ref string[] args)
    {
        return _userControl.Input<TModel>(title, validate, ref args);
    }

    List<Dictionary<string, object>> IUserControl.InputPrimitiveCollection(string title, Func<object, List<string>> validate, ref string[] args)
    {
        throw new NotImplementedException();
    }

    List<Dictionary<string, object>> IUserControl.InputStructureCollection(string title, Func<object, List<string>> validate, ref string[] args)
    {
        throw new NotImplementedException();
    }
}
 
