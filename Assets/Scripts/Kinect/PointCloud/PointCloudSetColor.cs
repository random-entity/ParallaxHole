using UnityEngine;
using Windows.Kinect;

public class PointCloudSetColor : MonoBehaviour
{
    private KinectSensor sensor;
    private CoordinateMapper mapper;
    [SerializeField] private MultiSourceManager multiSourceManager;
    private Mesh mesh;
    private Material material;
    private Texture2D colorTexture;
    private Vector2[] colorUV;
    private int downsample;
    private int vertexCount;

    private void Start()
    {
        sensor = KinectSensor.GetDefault();
        if (sensor != null)
        {
            mapper = sensor.CoordinateMapper;

            var frameDesc = sensor.ColorFrameSource.FrameDescription;
            colorTexture = new Texture2D(frameDesc.Width, frameDesc.Height);

            if (!sensor.IsOpen) sensor.Open();
        }

        mesh = GetComponent<MeshFilter>().mesh;
        material = GetComponent<MeshRenderer>().material;

        downsample = PointCloudConfig.downsample;
        vertexCount = PlaneMeshGenerator.vertexCount;

        colorUV = new Vector2[vertexCount];
    }

    private void FixedUpdate()
    {
        if (sensor == null) return;

        material.SetTexture("_ColorTexture", multiSourceManager.GetColorTexture());

        setColorUV(multiSourceManager.GetDepthData(), multiSourceManager.ColorWidth, multiSourceManager.ColorHeight);
        mesh.SetUVs(1, colorUV);
    }

    private void setColorUV(ushort[] depthData, int colorWidth, int colorHeight)
    {
        var frameDesc = sensor.DepthFrameSource.FrameDescription;
        int width = frameDesc.Width;
        int height = frameDesc.Height;

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
}