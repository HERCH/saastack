using Common.Configuration;

namespace Infrastructure.Persistence.OnPremises.ApplicationServices;

public class RabbitMqStoreOptions
{
    internal const string HostNameSettingName = "ApplicationServices:Persistence:RabbitMQ:HostName";
    internal const string UserNameSettingName = "ApplicationServices:Persistence:RabbitMQ:UserName";
    internal const string PasswordSettingName = "ApplicationServices:Persistence:RabbitMQ:Password";
    internal const string VirtualHostSettingName = "ApplicationServices:Persistence:RabbitMQ:VirtualHost";

    public RabbitMqStoreOptions(
        string hostName,
        string? userName = null,
        string? password = null,
        string? virtualHost = null,
        bool useAsyncDispatcher = true)
    {
        HostName = hostName;
        UserName = userName;
        Password = password;
        VirtualHost = virtualHost;
        UseAsyncDispatcher = useAsyncDispatcher;
    }

    public string HostName { get; }
    public string? UserName { get; }
    public string? Password { get; }
    public string? VirtualHost { get; }
    public bool UseAsyncDispatcher { get; }

    public static RabbitMqStoreOptions FromConfiguration(IConfigurationSettings settings)
    {
        return new RabbitMqStoreOptions(
            hostName: settings.GetString(HostNameSettingName),
            userName: settings.GetString(UserNameSettingName),
            password: settings.GetString(PasswordSettingName),
            virtualHost: settings.GetString(VirtualHostSettingName));
    }
}