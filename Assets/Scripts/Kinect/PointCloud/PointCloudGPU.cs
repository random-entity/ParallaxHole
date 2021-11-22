using UnityEngine;
using Windows.Kinect;

public class PointCloudGPU : MonoBehaviour
{
    #region Kinect
    private KinectSensor sensor;
    private CoordinateMapper mapper;
    [SerializeField] private MultiSourceManager multiSourceManager;
    #endregion

    #region Mesh
    private Mesh mesh;
    private Material material;
    private Vector2[] colorUV;
    #endregion

    private void Awake()
    {
        sensor = KinectSensor.GetDefault();
        if (sensor != null)
        {
            mapper = sensor.CoordinateMapper;

            var frameDesc = sensor.DepthFrameSource.FrameDescription;

            if (!sensor.IsOpen) sensor.Open();
        }
    
        mesh = GetComponent<MeshFilter>().mesh;
        material = GetComponent<MeshRenderer>().material;
    }

    private void FixedUpdate()
    {
        if (sensor == null) return;

        mesh.SetUVs(1, colorUV);
        material.SetTexture("_ColorTexture", multiSourceManager.GetColorTexture());
        

        // GetComponent<Renderer>().material.mainTexture = multiSourceManager.GetColorTexture();
        // updateMesh(multiSourceManager.GetDepthData(), multiSourceManager.ColorWidth, multiSourceManager.ColorHeight);
    }

    private void updateMesh(ushort[] depthData, int colorWidth, int colorHeight)
    {
        // var frameDesc = sensor.DepthFrameSource.FrameDescription;
        // int width = frameDesc.Width;
        // int height = frameDesc.Height;

        // ColorSpacePoint[] colorSpacePoints = new ColorSpacePoint[depthData.Length];
        // mapper.MapDepthFrameToColorSpace(depthData, colorSpacePoints);

        // for (int x = 0; x < width; x += downsample)
        // {
        //     for (int y = 0; y < height; y += downsample)
        //     {
        //         int downsampledX = x / downsample;
        //         int downsampledY = y / downsample;
        //         int downsampledIndex = (downsampledY * (frameDesc.Width / downsample)) + downsampledX;

        //         int depthDataIndex = y * width + x;

        //         var depth = depthData[depthDataIndex];
        //         if (depth <= 0)
        //         {
        //             depth = 4500;
        //         }
        //         vertices[downsampledIndex].z = depth * depthScale;

        //         ColorSpacePoint colorSpacePoint = colorSpacePoints[depthDataIndex];

        //         uv[downsampledIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
        //     }
        // }

        // mesh.vertices = vertices;
        // mesh.uv = uv;

        // mesh.RecalculateNormals();
        // mesh.RecalculateBounds();
    }






    void OnApplicationQuit()
    {
        if (mapper != null) mapper = null;

        // if (reader != null)
        // {
        //     reader.Dispose();
        //     reader = null;
        // }

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