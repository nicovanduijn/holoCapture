using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;
using System.IO;
using System;

public class captureMesh : MonoBehaviour {

    SurfaceObserver surfaceObserver;
    public bool save = false;
    public int resolution;
    public string objPath;
    public bool recording;
    public Vector3 origin;
    public float radius;
    public sendData sendModule = null;

    private HashSet<SurfaceId> relevantIDs = new HashSet<SurfaceId>();

    void Start()
    {
        surfaceObserver = new SurfaceObserver();
        //surfaceObserver.update();
    }

    // Update is called once per frame
    void Update () {


        if (recording)
        {
            surfaceObserver.SetVolumeAsSphere(origin, radius);
            surfaceObserver.Update(SurfaceChangedHandler);
        }

        if (save)
        {
            save = false;

            foreach(SurfaceId surfaceID in relevantIDs) {
                GameObject newSurface = new GameObject("Surface-" + surfaceID.handle);

                SurfaceData sd;
                sd.id = surfaceID;

                sd.outputMesh = newSurface.AddComponent<MeshFilter>();
                sd.outputAnchor = newSurface.AddComponent<WorldAnchor>();
                sd.outputCollider = newSurface.AddComponent<MeshCollider>();
                sd.trianglesPerCubicMeter = resolution;
                sd.bakeCollider = false;

                if (!surfaceObserver.RequestMeshAsync(sd, SurfaceBaked))
                {
                    System.Console.WriteLine("error while requesting mesh");
                }
            }
            AudioSource[] clickSound = GetComponents<AudioSource>();
            clickSound[1].Play();
            if (sendModule != null) sendModule.objectPath = objPath;
        }
	}

    void SurfaceBaked(SurfaceData sd, bool outputWritten, float elapsedBakeTimeSeconds)
    {
        if (outputWritten)
        {
            File.WriteAllText(objPath+sd.id.handle, ObjExporterScript.MeshToString(sd.outputMesh, sd.outputMesh.transform));
            Console.WriteLine("written obj file");
        }

    }


    void SurfaceChangedHandler(SurfaceId id, SurfaceChange changeType, Bounds bounds, DateTime updateTime)
    { 
        switch (changeType)
        {
            case SurfaceChange.Added:
                relevantIDs.Add(id);
                break;
            case SurfaceChange.Updated:
                relevantIDs.Add(id);
                break;
            case SurfaceChange.Removed:
                relevantIDs.Remove(id);
                break;
        }
    }

}
