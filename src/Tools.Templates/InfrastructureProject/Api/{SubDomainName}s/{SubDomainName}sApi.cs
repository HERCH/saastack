namespace ProjectName.Api.{SubDomainName}s;

public class {SubDomainName}sApi : IwebApiService
{
    private readonly I{SubDomainName}sApplication _{SubDomainNameLower}sApplication;
    private readonly ICallerContextFactory _contextFactory;

    public CarsApi(ICallerContextFactory contextFactory, I{SubDomainName}sApplication {SubDomainNameLower}sApplication)
    {
        _contextFactory = contextFactory;
        _{SubDomainNameLower}sApplication = {SubDomainNameLower}sApplication;
    }
    
    //TODO: Add your service operation methods here
}