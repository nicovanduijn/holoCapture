using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System;

public class sendData : MonoBehaviour {
    public bool dataIsReady = false;
    public string server = "localhost";
    private bool refined = false;
    private LinkedList<lView> views = new LinkedList<lView>();
    public string objectPath = "";
    public string refinedObjectPath = "";

    public bool dataIsRefined
    {
        get
        {
            return refined;
        }
    }

    class lView
    {
        public lView(Matrix4x4 viewTrans, Matrix4x4 projTrans, string path)
        {
            this.viewTrans = viewTrans;
            this.projTrans = projTrans;
            this.path = path;
        }
        public string path;
        public Matrix4x4 viewTrans;
        public Matrix4x4 projTrans;
    }

    // Use this for initialization
    void Start () {
        Console.WriteLine("start");
    }
	
    public void addView(Matrix4x4 viewTrans, Matrix4x4 projTrans, string picPath)
    {
        views.AddLast(new lView(viewTrans, projTrans, picPath));
    }

    // Update is called once per frame
    void Update () {
        if (dataIsReady)
        {
            dataIsReady = false;
            //make Thread;
            Thread t = new Thread(Run);
            t.Start();
        }
	}

    private void Run()
    {
        try
        {
            BinaryReader reader;

            int port = 11000;

            TcpClient client = new TcpClient(server, port);

            NetworkStream stream = client.GetStream();
            //send data about number of pics
            stream.Write(BitConverter.GetBytes(views.Count), 0, 4);
           
            foreach(lView view in views)
            {
                //send matrices
                for(int i = 0; i<16; i++)
                    stream.Write(BitConverter.GetBytes(view.projTrans[i]), 0, 4);

                for (int i = 0; i < 16; i++)
                    stream.Write(BitConverter.GetBytes(view.viewTrans[i]), 0, 4);

                //pic
                reader = new BinaryReader(File.Open(view.path, FileMode.Open));
                Byte[] picData = reader.ReadBytes(int.MaxValue);
                //size
                stream.Write(BitConverter.GetBytes(picData.Length), 0, 4);
                //pic itself
                stream.Write(picData, 0, picData.Length);
            }
            //save obj
            //reader = new BinaryReader(File.Open(objectPath, FileMode.Open));
            Byte[] objData = File.ReadAllBytes(objectPath);
            //size
            stream.Write(BitConverter.GetBytes(objData.Length), 0, 4);
            //obj itself
            stream.Write(objData, 0, objData.Length);

            //wait for incoming data
            //read data
            byte[] bl = new byte[4];
            stream.Read(bl, 0, 4);
            bl = new byte[BitConverter.ToInt32(bl,0)];
            stream.Read(bl, 0, bl.Length);

            //the obj file is now in at the specified path
            BinaryWriter writer = new BinaryWriter(File.Open(refinedObjectPath, FileMode.Create));
            writer.Write(bl);
            writer.Close();

            client.Close();

        }
        catch(Exception e)
        {

            Console.Out.WriteLine("Error: " + e.ToString());
        }
    }
}
