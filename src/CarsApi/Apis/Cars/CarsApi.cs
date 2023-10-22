using Application.Interfaces;
using Application.Interfaces.Resources;
using CarsApplication;
using Infrastructure.Web.Api.Common;
using Infrastructure.Web.Api.Interfaces;
using Infrastructure.Web.Api.Interfaces.Operations.Cars;

namespace CarsApi.Apis.Cars;

public class CarsApi : IWebApiService
{
    private readonly ICarsApplication _carsApplication;
    private readonly ICallerContext _context;

    public CarsApi(ICallerContext context, ICarsApplication carsApplication)
    {
        _context = context;
        _carsApplication = carsApplication;
    }

    public async Task<ApiDeleteResult> Delete(DeleteCarRequest request, CancellationToken cancellationToken)
    {
        var car = await _carsApplication.DeleteCarAsync(_context, request.Id, cancellationToken);
        return () => car.HandleApplicationResult();
    }

    public async Task<ApiGetResult<Car, GetCarResponse>> Get(GetCarRequest request, CancellationToken cancellationToken)
    {
        var car = await _carsApplication.GetCarAsync(_context, request.Id, cancellationToken);

        return () => car.HandleApplicationResult(c => new GetCarResponse { Car = c });
    }

    public async Task<ApiPostResult<Car, GetCarResponse>> Register(RegisterCarRequest request,
        CancellationToken cancellationToken)
    {
        var car = await _carsApplication.RegisterCarAsync(_context, request.Make, request.Model, request.Year,
            cancellationToken);

        return () => car.HandleApplicationResult<GetCarResponse, Car>(c =>
            new PostResult<GetCarResponse>(new GetCarResponse { Car = c }, $"/cars/{c.Id}"));
    }

    public async Task<ApiSearchResult<Car, SearchAllCarsResponse>> SearchAll(SearchAllCarsRequest request,
        CancellationToken cancellationToken)
    {
        var cars = await _carsApplication.SearchAllCarsAsync(_context, request.ToSearchOptions(),
            request.ToGetOptions(), cancellationToken);

        return () =>
            cars.HandleApplicationResult(c => new SearchAllCarsResponse { Cars = c.Results, Metadata = c.Metadata });
    }

    public async Task<ApiPutPatchResult<Car, GetCarResponse>> TakeOffline(TakeOfflineCarRequest request,
        CancellationToken cancellationToken)
    {
        var car = await _carsApplication.TakeOfflineCarAsync(_context, request.Id!, request.Reason, request.StartAtUtc,
            request.EndAtUtc, cancellationToken);
        return () => car.HandleApplicationResult(c => new GetCarResponse { Car = c });
    }
}