using System;
using System.Collections;
using System.Collections.Generic;

namespace DungeonQuest.Trails
{
    /// <summary>
    /// Persistence boundary for trails. The controller depends only on this,
    /// so swapping local storage for the future /api/trilhas endpoint is a
    /// one-line change (see TrailConfigController.useRemoteApi).
    ///
    /// All methods are coroutines (run via StartCoroutine) to match the
    /// project's coroutine-based networking style.
    /// </summary>
    public interface ITrailRepository
    {
        IEnumerator SaveTrail(TrailDto trail, Action<TrailDto> onSuccess, Action<string> onError);
        IEnumerator LoadTrails(Action<List<TrailDto>> onSuccess, Action<string> onError);
    }
}
