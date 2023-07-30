using System;

public class PackageManagerTest : TestingElement<PackageManager>
{
    
    public override void OnTest()
    {
        try
        {
            string[] args = new string[0];
             
            var pm = new PackageManager();
            pm.PackAndPush();
            var module = InputConsole.SingleSelect("Выберите пакет",    pm.GetApplications(), ref args);
            var version = InputConsole.SingleSelect("Выберите версию",  pm.GetVersions(module), ref args);
            pm.Unpack(module,version, pm.GetAppDirectory());
            
            this.Messages.Add($"Функция установка модулей мэнэджнром пакетов выполнена успешно");
           
        }
        catch (Exception ex) 
        {
            this.Messages.Add("Регистрация хранилища пакетов/упаковка-распаковка не в порядке");
            this.Messages.Add(ex.Message);
        }        
    }

   
}
