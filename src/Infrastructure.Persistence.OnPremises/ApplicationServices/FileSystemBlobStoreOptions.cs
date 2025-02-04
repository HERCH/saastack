using Common.Configuration;

namespace Infrastructure.Persistence.OnPremises.ApplicationServices;

public class FileSystemBlobStoreOptions
{
    public const string RootPathSettingName = "ApplicationServices:Persistence:FileSystem:RootPath";

    public FileSystemBlobStoreOptions(string rootPath)
    {
        RootPath = rootPath;
        if (!Path.IsPathRooted(rootPath))
        {
            throw new ArgumentException("La ruta debe ser absoluta", nameof(rootPath));
        }
    }

    public string RootPath { get; }

    public static FileSystemBlobStoreOptions FromConfiguration(IConfigurationSettings settings)
    {
        var rootPath = settings.GetString(RootPathSettingName);
        return new FileSystemBlobStoreOptions(rootPath);
    }
}