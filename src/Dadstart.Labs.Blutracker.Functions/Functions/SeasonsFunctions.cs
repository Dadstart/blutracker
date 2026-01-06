using System.Net;
using Dadstart.Labs.Blutracker.Functions.Storage;
using Dadstart.Labs.Blutracker.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dadstart.Labs.Blutracker.Functions.Functions;

public sealed class SeasonsFunctions
{
    readonly CosmosMediaStore _store;

    public SeasonsFunctions(CosmosMediaStore store) => _store = store;

    [Function("GetSeason")]
    public async Task<HttpResponseData> GetSeasonAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "seasons/{id:guid}")] HttpRequestData request,
        Guid id,
        CancellationToken cancellationToken)
    {
        var season = await _store.GetSeasonAsync(id, cancellationToken).ConfigureAwait(false);
        if (season is null)
            return request.CreateResponse(HttpStatusCode.NotFound);

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(season, cancellationToken).ConfigureAwait(false);
        return response;
    }

    [Function("DeleteSeason")]
    public async Task<HttpResponseData> DeleteSeasonAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "seasons/{id:guid}")] HttpRequestData request,
        Guid id,
        CancellationToken cancellationToken)
    {
        await _store.DeleteSeasonAsync(id, cancellationToken).ConfigureAwait(false);
        return request.CreateResponse(HttpStatusCode.NoContent);
    }
}

