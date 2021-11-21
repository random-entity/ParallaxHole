using System;
using UnityEngine;
using Windows.Kinect;

[RequireComponent(typeof(PlaneMeshGenerator))]
public class PointCloudSetDepth : MonoBehaviour
{
    private KinectSensor sensor;
    private DepthFrameReader reader;
    private ushort[] data;
    private byte[] rawData;
    private Texture2D depthTexture;
    private Material material;

    private void Start()
    {
        sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            reader = sensor.DepthFrameSource.OpenReader();
            FrameDescription frameDesc = sensor.DepthFrameSource.FrameDescription;
            data = new ushort[frameDesc.LengthInPixels];
            rawData = new byte[frameDesc.LengthInPixels * 2];
            depthTexture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.R16, false);
        }

        if (!sensor.IsOpen)
        {
            sensor.Open();
        }

        material = GetComponent<MeshRenderer>().material;
        material.SetTexture("_DepthTexture", depthTexture);
    }

    private void Update()
    {
        DepthFrame frame = reader.AcquireLatestFrame();
        FrameDescription frameDesc = sensor.DepthFrameSource.FrameDescription;

        if (frame != null)
        {
            frame.CopyFrameDataToArray(data);
            Buffer.BlockCopy(data, 0, rawData, 0, data.Length * 2);
            depthTexture.LoadRawTextureData(rawData);
            depthTexture.Apply();

            frame.Dispose();
            frame = null;
        }
    }

    private void OnApplicationQuit()
    {
        if (reader != null)
        {
            reader.Dispose();
            reader = null;
        }

        if (sensor != null)
        {
            if (sensor.IsOpen)
            {
                sensor.Close();
            }

            sensor = null;
        }
    }
}
