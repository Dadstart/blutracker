using System.Net;
using Dadstart.Labs.Blutracker.Functions.Storage;
using Dadstart.Labs.Blutracker.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dadstart.Labs.Blutracker.Functions.Functions;

public sealed class TvShowsFunctions
{
    readonly CosmosMediaStore _store;

    public TvShowsFunctions(CosmosMediaStore store) => _store = store;

    [Function("ListTvShows")]
    public async Task<HttpResponseData> ListTvShowsAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tvshows")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var shows = await _store.ListTvShowsAsync(cancellationToken).ConfigureAwait(false);
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(shows, cancellationToken).ConfigureAwait(false);
        return response;
    }

    [Function("GetTvShow")]
    public async Task<HttpResponseData> GetTvShowAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tvshows/{id:guid}")] HttpRequestData request,
        Guid id,
        CancellationToken cancellationToken)
    {
        var show = await _store.GetTvShowAsync(id, cancellationToken).ConfigureAwait(false);
        if (show is null)
            return request.CreateResponse(HttpStatusCode.NotFound);

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(show, cancellationToken).ConfigureAwait(false);
        return response;
    }

    [Function("UpsertTvShow")]
    public async Task<HttpResponseData> UpsertTvShowAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "tvshows/{id:guid}")] HttpRequestData request,
        Guid id,
        CancellationToken cancellationToken)
    {
        var show = await request.ReadFromJsonAsync<TvShow>(cancellationToken).ConfigureAwait(false);
        if (show is null || show.Id != id)
            return request.CreateResponse(HttpStatusCode.BadRequest);

        await _store.UpsertTvShowAsync(show, cancellationToken).ConfigureAwait(false);
        return request.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("DeleteTvShow")]
    public async Task<HttpResponseData> DeleteTvShowAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "tvshows/{id:guid}")] HttpRequestData request,
        Guid id,
        CancellationToken cancellationToken)
    {
        await _store.DeleteTvShowAsync(id, cancellationToken).ConfigureAwait(false);
        return request.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("ListSeasons")]
    public async Task<HttpResponseData> ListSeasonsAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "tvshows/{tvShowId:guid}/seasons")] HttpRequestData request,
        Guid tvShowId,
        CancellationToken cancellationToken)
    {
        var seasons = await _store.ListSeasonsAsync(tvShowId, cancellationToken).ConfigureAwait(false);
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(seasons, cancellationToken).ConfigureAwait(false);
        return response;
    }

    [Function("UpsertSeason")]
    public async Task<HttpResponseData> UpsertSeasonAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "tvshows/{tvShowId:guid}/seasons/{id:guid}")] HttpRequestData request,
        Guid tvShowId,
        Guid id,
        CancellationToken cancellationToken)
    {
        var season = await request.ReadFromJsonAsync<Season>(cancellationToken).ConfigureAwait(false);
        if (season is null || season.Id != id || season.TvShowId != tvShowId)
            return request.CreateResponse(HttpStatusCode.BadRequest);

        await _store.UpsertSeasonAsync(season, cancellationToken).ConfigureAwait(false);
        return request.CreateResponse(HttpStatusCode.NoContent);
    }
}

