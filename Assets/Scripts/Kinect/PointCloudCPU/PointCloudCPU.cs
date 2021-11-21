using UnityEngine;
using UnityEngine.Rendering;
using Windows.Kinect;

public class PointCloudCPU : MonoBehaviour
{
    [SerializeField] private MultiSourceManager multiSourceManager;
    private KinectSensor sensor;
    private CoordinateMapper mapper;
    private const int downsample = 4;
    private const float pitch = 0.001f;
    [SerializeField] private float depthScale = 0.001f;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector2[] uv;
    private int[] triangles;

    private void Awake()
    {
        sensor = KinectSensor.GetDefault();
        if (sensor != null)
        {
            mapper = sensor.CoordinateMapper;
            
            var frameDesc = sensor.DepthFrameSource.FrameDescription;

            createMesh(frameDesc.Width / downsample, frameDesc.Height / downsample);

            if (!sensor.IsOpen) sensor.Open();
        }
    }

    private void FixedUpdate()
    {
        if (sensor == null) return;

        GetComponent<Renderer>().material.mainTexture = multiSourceManager.GetColorTexture();
        updateMesh(multiSourceManager.GetDepthData(), multiSourceManager.ColorWidth, multiSourceManager.ColorHeight);
    }

    void OnApplicationQuit()
    {
        if (mapper != null) mapper = null;

        if (sensor != null)
        {
            if (sensor.IsOpen) sensor.Close();
            sensor = null;
        }
    }

    private void createMesh(int width, int height)
    {
        mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;
        GetComponent<MeshFilter>().mesh = mesh;

        vertices = new Vector3[width * height];
        uv = new Vector2[width * height];
        triangles = new int[6 * ((width - 1) * (height - 1))];

        int triangleIndex = 0;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = (y * width) + x;

                vertices[index] = new Vector3(x, -y, 0);
                uv[index] = new Vector2(((float)x / (float)width), ((float)y / (float)height));

                // Skip the last row/col
                if (x != (width - 1) && y != (height - 1))
                {
                    int topLeft = index;
                    int topRight = topLeft + 1;
                    int bottomLeft = topLeft + width;
                    int bottomRight = bottomLeft + 1;

                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        mesh.triangles = triangles;
        mesh.RecalculateNormals();
    }

    private void updateMesh(ushort[] depthData, int colorWidth, int colorHeight)
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

                var depth = depthData[depthDataIndex];
                if (depth <= 0)
                {
                    depth = 4500;
                }
                vertices[downsampledIndex].z = depth * depthScale;

                ColorSpacePoint colorSpacePoint = colorSpacePoints[depthDataIndex];

                uv[downsampledIndex] = new Vector2(colorSpacePoint.X / colorWidth, colorSpacePoint.Y / colorHeight);
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}