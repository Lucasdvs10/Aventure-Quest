using System;
using System.Collections;
using System.Collections.Generic;

namespace DungeonQuest.Trails
{
    /// <summary>
    /// Talks to the backend at /api/trilhas using the same envelope/coroutine
    /// pattern as the rest of the API.
    ///
    /// NOT ACTIVE BY DEFAULT — this endpoint does not exist on the API yet.
    /// See README ("Ativando a API remota") for the suggested FastAPI contract.
    /// To enable: set TrailConfigController.useRemoteApi = true once the backend
    /// implements the routes below.
    ///
    /// Expected contract (envelope { "response": ... } on every response):
    ///   GET    /api/trilhas?limit=&offset=     -> [ TrailDto, ... ]
    ///   POST   /api/trilhas    body: TrailDto   -> TrailDto (201)
    ///   PATCH  /api/trilhas/{id} body: TrailDto -> TrailDto
    /// </summary>
    public class RemoteTrailRepository : ITrailRepository
    {
        public IEnumerator SaveTrail(TrailDto trail, Action<TrailDto> onSuccess, Action<string> onError)
        {
            bool isUpdate = !string.IsNullOrEmpty(trail.id);
            string url = TrailApiService.BaseUrl + "/api/trilhas" + (isUpdate ? "/" + trail.id : "");
            string method = isUpdate ? "PATCH" : "POST";
            return TrailApiService.Send<TrailDto>(method, url, trail, onSuccess, onError);
        }

        public IEnumerator LoadTrails(Action<List<TrailDto>> onSuccess, Action<string> onError)
        {
            return TrailApiService.Send<List<TrailDto>>(
                "GET", TrailApiService.BaseUrl + "/api/trilhas?limit=100", null, onSuccess, onError);
        }
    }
}
