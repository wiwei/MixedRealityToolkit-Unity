using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.SpatialAwareness;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialFloor : MonoBehaviour
{
    private IMixedRealitySpatialAwarenessSystem spatialService;

    private IMixedRealitySpatialAwarenessSystem SpatialService
    {
        get
        {
            if (spatialService == null)
            {
                MixedRealityServiceRegistry.TryGetService<IMixedRealitySpatialAwarenessSystem>(out spatialService);
            }
            return spatialService;
        }
    }

    private const int RefreshAfterSeconds = 3;
    private GameObject banana;
    private bool refreshed = false;

    private void Start()
    {
        banana = GameObject.Find("banana");
        if (banana == null)
        {
            Debug.Log("Where the Banana?!?!?!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > RefreshAfterSeconds && !refreshed)
        {
            UpdateSpatialFloor();
            refreshed = true;
        }
    }

    private void UpdateSpatialFloor()
    {
        if (SpatialService == null)
        {
            Debug.Log("Spatial system not started up, wtf mate");
            return;
        }

        var dataProviderAccess = SpatialService as IMixedRealityDataProviderAccess;
        IMixedRealitySpatialAwarenessMeshObserver observer =
            dataProviderAccess.GetDataProvider<IMixedRealitySpatialAwarenessMeshObserver>();

        List<float> values = new List<float>();

        float lowestY = 0;
        foreach (var mesh in observer.Meshes)
        {
            if (mesh.Value == null)
            {
                continue;
            }
            var meshValue = mesh.Value;
            foreach (var vertex in meshValue.Filter.sharedMesh.vertices)
            {
                lowestY = System.Math.Min(lowestY, vertex.y);
            }
        }
        
        Vector3 currentPosition = gameObject.transform.position;
        currentPosition.y = lowestY;
        gameObject.transform.position = currentPosition;

        Vector3 bananaPosition = currentPosition;
        bananaPosition.y += .25f;
        banana.transform.position = bananaPosition;
    }
}
