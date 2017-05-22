using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VR.WSA;
using System.IO;
using System;

public class captureMesh : MonoBehaviour {

    SurfaceObserver surfaceObserver;
    public bool start = false;
    public SurfaceId surfaceID;
    public int resolution;
    public string objPath;

    void Start()
    {
        surfaceObserver = new SurfaceObserver();
    }

    // Update is called once per frame
    void Update () {
        if (start)
        {
            start = false;
            GameObject newSurface = new GameObject("Surface-"+surfaceID.handle);

            SurfaceData sd;
            sd.id = surfaceID;

            sd.outputMesh = newSurface.AddComponent<MeshFilter>();
            sd.outputAnchor = newSurface.AddComponent<WorldAnchor>();
            sd.outputCollider = newSurface.AddComponent<MeshCollider>();
            sd.trianglesPerCubicMeter = resolution;
            sd.bakeCollider = false;

            if (surfaceObserver.RequestMeshAsync(sd, SurfaceBaked))
            {

            }
            else
            {
                System.Console.WriteLine("error while requesting mesh");

            }


        }


	}

    void SurfaceBaked(SurfaceData sd, bool outputWritten, float elapsedBakeTimeSeconds)
    {
        if (outputWritten)
        {
            File.WriteAllText(objPath, ObjExporterScript.MeshToString(sd.outputMesh, sd.outputMesh.transform));
            Console.WriteLine("written out all lines");
        }

    }
}
