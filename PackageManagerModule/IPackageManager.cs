using System.Collections.Generic;
/// <summary>
/// Управление пакетами
/// </summary>
public interface IPackageManager
{
    public IEnumerable<string> GetApplications();
    public IEnumerable<string> GetVersions(string application);
    public string GetFileName(string application, string version);


    public void Pack(string application, string version, string directory);
    public void Unpack(string application, string version, string directory);
    

    public string GetNextVersion(string application);
    public string GetLastVersion(string application);
}
