using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.VR.WSA.WebCam;
using System.IO;

public class takePicture : MonoBehaviour {
    bool go = false;
    PhotoCapture photoCaptureObject = null;
    public sendData sendModule = null;
    CameraParameters c = new CameraParameters();

    // Use this for initialization
    void Start()
    {
        Debug.Log("Reached Start()");
        //PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
    }

    void OnPhotoCaptureCreated(PhotoCapture captureObject)
    {
        Debug.Log("Reached OnPhotoCaptureCreated()");
        photoCaptureObject = captureObject;
        Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();

        c.hologramOpacity = 0.0f;
        c.cameraResolutionWidth = cameraResolution.width;
        c.cameraResolutionHeight = cameraResolution.height;
        c.pixelFormat = CapturePixelFormat.BGRA32;

        captureObject.StartPhotoModeAsync(c, OnPhotoModeStarted);
    }

    void OnStoppedPhotoMode(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("Reached OnStoppedPhotoMode()");
        photoCaptureObject.Dispose();
        photoCaptureObject = null;
    }

    /*
    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("Reached OnPhotoModeStarted()");
        if (result.success)
        {
            string filename = string.Format(@"CapturedImage{0}_n.jpg", Time.time);
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, filename);
            Debug.Log(filePath);
            
            photoCaptureObject.TakePhotoAsync(filePath, PhotoCaptureFileOutputFormat.PNG, OnCapturedPhotoToDisk);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }
    
    void OnCapturedPhotoToDisk(PhotoCapture.PhotoCaptureResult result)
    {
        Debug.Log("Reached OnCapturedPhotoToDisk()");
        if (result.success)
        {
            Debug.Log("Saved Photo to disk!");
            photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
        }
        else
        {
            Debug.Log("Failed to save Photo to disk");
        }
    }
     */

    private void OnPhotoModeStarted(PhotoCapture.PhotoCaptureResult result)
    {
        if (result.success)
        {
            photoCaptureObject.TakePhotoAsync(OnCapturedPhotoToMemory);
        }
        else
        {
            Debug.LogError("Unable to start photo mode!");
        }
    }

    int viewNumber = 0;

    void OnCapturedPhotoToMemory(PhotoCapture.PhotoCaptureResult result, PhotoCaptureFrame photoCaptureFrame)
    {
        if (result.success)
        {
            // Create our Texture2D for use and set the correct resolution
            Resolution cameraResolution = PhotoCapture.SupportedResolutions.OrderByDescending((res) => res.width * res.height).First();
            Texture2D targetTexture = new Texture2D(cameraResolution.width, cameraResolution.height);
            // Copy the raw image data into our target texture
            photoCaptureFrame.UploadImageDataToTexture(targetTexture);

            byte[] PNGfile = targetTexture.EncodeToPNG();
            string filePath = System.IO.Path.Combine(Application.persistentDataPath, "CapturedImage" + Time.time + ".png");
            Debug.Log("!!!!!!!!!!!!!!!" + filePath);
            File.WriteAllBytes(filePath, PNGfile);//todo: enumerate

            Debug.Log("saved png");


            Matrix4x4 worldTrans;
            Matrix4x4 viewTrans;
            if (photoCaptureFrame.TryGetCameraToWorldMatrix(out worldTrans) && photoCaptureFrame.TryGetProjectionMatrix(out viewTrans))
            {
                filePath = System.IO.Path.Combine(Application.persistentDataPath, "CapturedImage" + Time.time + ".png.matr");
                File.WriteAllText(filePath, worldTrans + "\n\n" + viewTrans);
                sendModule.addView(worldTrans, viewTrans, filePath);
            }
            else
            {
                Debug.LogError("failed to save matrices");
            }


        }
        // Clean up
        photoCaptureObject.StopPhotoModeAsync(OnStoppedPhotoMode);
    }


    bool taken = false;
    double target = 10;
    // Update is called once per frame
    void Update()
    {
        //Debug.Log("time is");
        //Debug.Log(Time.time.ToString());

        if (Time.time > target)
        {
            viewNumber = 0;
            target += 10;
            PhotoCapture.CreateAsync(false, OnPhotoCaptureCreated);
            Debug.Log("10 seconds elapsed");


        }

    }
}
