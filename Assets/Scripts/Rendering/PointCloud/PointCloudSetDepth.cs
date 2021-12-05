using System;
using UnityEngine;
using Windows.Kinect;

[RequireComponent(typeof(MeshRenderer))]
public class PointCloudSetDepth : MonoBehaviour
{
    private KinectSensor sensor;
    private DepthFrameReader depthReader;
    private ushort[] depthData;
    private byte[] rawDepthData;
    private Texture2D depthTexture;
    private Material pointCloudMaterial;

    private void Start()
    {
        sensor = KinectSensor.GetDefault();
        if (sensor != null)
        {
            depthReader = sensor.DepthFrameSource.OpenReader();
            FrameDescription depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            depthData = new ushort[depthFrameDesc.LengthInPixels];
            rawDepthData = new byte[depthFrameDesc.LengthInPixels * 2];
            depthTexture = new Texture2D(depthFrameDesc.Width, depthFrameDesc.Height, TextureFormat.R16, false);

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }
        }

        pointCloudMaterial = GetComponent<MeshRenderer>().material;
        if (pointCloudMaterial.shader.name != "Random Entity/Point Cloud/PointCloud") Debug.LogWarning("MeshRenderer's material not set to PointCloud material");

        pointCloudMaterial.SetTexture("_DepthTexture", depthTexture);
    }

    private void FixedUpdate()
    {
        DepthFrame depthFrame = depthReader.AcquireLatestFrame();
        FrameDescription depthFrameDesc = sensor.DepthFrameSource.FrameDescription;

        if (depthFrame != null)
        {
            depthFrame.CopyFrameDataToArray(depthData);
            Buffer.BlockCopy(depthData, 0, rawDepthData, 0, depthData.Length * 2);
            depthTexture.LoadRawTextureData(rawDepthData);
            depthTexture.Apply();

            depthFrame.Dispose();
            depthFrame = null;
        }
    }

    private void OnApplicationQuit()
    {
        if (depthReader != null)
        {
            depthReader.Dispose();
            depthReader = null;
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