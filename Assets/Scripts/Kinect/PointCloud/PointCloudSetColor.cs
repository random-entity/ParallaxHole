using UnityEngine;
using Windows.Kinect;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class PointCloudSetColor : MonoBehaviour
{
    private KinectSensor sensor;
    private CoordinateMapper mapper;
    [SerializeField] private MultiSourceManager multiSourceManager;
    private Vector2[] colorUV;
    private Mesh pointCloudMesh;
    private Material pointCloudMaterial;
    private void Start()
    {
        sensor = KinectSensor.GetDefault();
        if (sensor != null)
        {
            mapper = sensor.CoordinateMapper;
            if (!sensor.IsOpen) sensor.Open();
        }

        colorUV = new Vector2[PlaneMeshGenerator.vertexCount];

        pointCloudMesh = GetComponent<MeshFilter>().mesh;
        pointCloudMaterial = GetComponent<MeshRenderer>().material;
    }

    private void FixedUpdate()
    {
        if (sensor == null) return;

        pointCloudMaterial.SetTexture("_ColorTexture", multiSourceManager.GetColorTexture());

        setColorUV(multiSourceManager.GetDepthData(), multiSourceManager.ColorWidth, multiSourceManager.ColorHeight);
        pointCloudMesh.SetUVs(1, colorUV);
    }

    private void setColorUV(ushort[] depthData, int colorWidth, int colorHeight)
    {
        var frameDesc = sensor.DepthFrameSource.FrameDescription;
        int width = frameDesc.Width;
        int height = frameDesc.Height;
        int downsample = PointCloudConfig.downsample;

        ColorSpacePoint[] colorSpacePoints = new ColorSpacePoint[depthData.Length];
        mapper.MapDepthFrameToColorSpace(depthData, colorSpacePoints);

        for (int x = 0; x < width; x += downsample)
        {
            for (int y = 0; y < height; y += downsample)
            {
                int downsampledX = x / downsample;
                int downsampledY = y / downsample;
                int downsampledIndex = (downsampledY * (frameDesc.Width / downsample)) + downsampledX;

                int depthDataIndex = y * width + x;

                ColorSpacePoint colorSpacePoint = colorSpacePoints[depthDataIndex];

                colorUV[downsampledIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }
    }

    private void OnApplicationQuit()
    {
        if (mapper != null) mapper = null;

        if (sensor != null)
        {
            if (sensor.IsOpen) sensor.Close();
            sensor = null;
        }
    }
}