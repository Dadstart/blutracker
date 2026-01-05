using System.Net;
using Dadstart.Labs.Blutracker.Functions.Storage;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dadstart.Labs.Blutracker.Functions.Functions;

public sealed class SeasonsFunctions
{
    readonly CosmosMediaStore _store;

    public SeasonsFunctions(CosmosMediaStore store) => _store = store;

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

