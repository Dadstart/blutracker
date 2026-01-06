using System.Net;
using Dadstart.Labs.Blutracker.Functions.Storage;
using Dadstart.Labs.Blutracker.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Dadstart.Labs.Blutracker.Functions.Functions;

public sealed class MoviesFunctions
{
    readonly CosmosMediaStore _store;

    public MoviesFunctions(CosmosMediaStore store) => _store = store;

    [Function("ListMovies")]
    public async Task<HttpResponseData> ListMoviesAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "movies")] HttpRequestData request,
        CancellationToken cancellationToken)
    {
        var movies = await _store.ListMoviesAsync(cancellationToken).ConfigureAwait(false);
        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(movies, cancellationToken).ConfigureAwait(false);
        return response;
    }

    [Function("GetMovie")]
    public async Task<HttpResponseData> GetMovieAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "movies/{id:guid}")] HttpRequestData request,
        Guid id,
        CancellationToken cancellationToken)
    {
        var movie = await _store.GetMovieAsync(id, cancellationToken).ConfigureAwait(false);
        if (movie is null)
            return request.CreateResponse(HttpStatusCode.NotFound);

        var response = request.CreateResponse(HttpStatusCode.OK);
        await response.WriteAsJsonAsync(movie, cancellationToken).ConfigureAwait(false);
        return response;
    }

    [Function("UpsertMovie")]
    public async Task<HttpResponseData> UpsertMovieAsync(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "movies/{id:guid}")] HttpRequestData request,
        Guid id,
        CancellationToken cancellationToken)
    {
        var movie = await request.ReadFromJsonAsync<Movie>(cancellationToken).ConfigureAwait(false);
        if (movie is null || movie.Id != id)
            return request.CreateResponse(HttpStatusCode.BadRequest);

        await _store.UpsertMovieAsync(movie, cancellationToken).ConfigureAwait(false);
        return request.CreateResponse(HttpStatusCode.NoContent);
    }

    [Function("DeleteMovie")]
    public async Task<HttpResponseData> DeleteMovieAsync(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "movies/{id:guid}")] HttpRequestData request,
        Guid id,
        CancellationToken cancellationToken)
    {
        await _store.DeleteMovieAsync(id, cancellationToken).ConfigureAwait(false);
        return request.CreateResponse(HttpStatusCode.NoContent);
    }
}

