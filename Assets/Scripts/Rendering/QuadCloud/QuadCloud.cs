using UnityEngine;
using Windows.Kinect;

public class QuadCloud : MonoBehaviour
{
    #region Kinect
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
    private KinectSensor sensor;
    private CoordinateMapper mapper;
    private MultiSourceFrameReader reader;
    private ushort[] depthData;
    private Texture2D colorTexture;
    private byte[] colorData;
    [SerializeField] private Transform lookTarget;
    [SerializeField] private Flock flock;

    private void InitKinect()
    {
        sensor = KinectSensor.GetDefault();

        if (sensor != null)
        {
            mapper = sensor.CoordinateMapper;
            reader = sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth);

            var colorFrameDesc = sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
            ColorWidth = colorFrameDesc.Width;
            ColorHeight = colorFrameDesc.Height;

            colorTexture = new Texture2D(colorFrameDesc.Width, colorFrameDesc.Height, TextureFormat.RGBA32, false);
            colorData = new byte[colorFrameDesc.BytesPerPixel * colorFrameDesc.LengthInPixels];

            var depthFrameDesc = sensor.DepthFrameSource.FrameDescription;
            depthData = new ushort[depthFrameDesc.LengthInPixels];

            cameraSpacePointArray = new CameraSpacePoint[depthFrameDesc.LengthInPixels];
            colorSpacePointArray = new ColorSpacePoint[depthFrameDesc.LengthInPixels];

            if (!sensor.IsOpen)
            {
                sensor.Open();
            }
        }
    }
    private void UpdateKinectData()
    {
        if (reader != null)
        {
            var frame = reader.AcquireLatestFrame();
            if (frame != null)
            {
                var colorFrame = frame.ColorFrameReference.AcquireFrame();
                if (colorFrame != null)
                {
                    var depthFrame = frame.DepthFrameReference.AcquireFrame();
                    if (depthFrame != null)
                    {
                        colorFrame.CopyConvertedFrameDataToArray(colorData, ColorImageFormat.Rgba);
                        colorTexture.LoadRawTextureData(colorData);
                        colorTexture.Apply();

                        depthFrame.CopyFrameDataToArray(depthData);

                        depthFrame.Dispose();
                        depthFrame = null;
                    }

                    colorFrame.Dispose();
                    colorFrame = null;
                }

                frame = null;
            }
        }
    }
    #endregion

    #region Arrays and Buffers
    private CameraSpacePoint[] cameraSpacePointArray;
    private ColorSpacePoint[] colorSpacePointArray;
    private ComputeBuffer cameraSpacePointBuffer;
    private ComputeBuffer colorSpacePointBuffer;
    #endregion

    #region Flocking

    #endregion

    #region Rendering
    [SerializeField] private Material quadCloudMaterial;
    #endregion

    private void InitShader()
    {
        cameraSpacePointBuffer = new ComputeBuffer(cameraSpacePointArray.Length, 3 * sizeof(float));
        colorSpacePointBuffer = new ComputeBuffer(colorSpacePointArray.Length, 2 * sizeof(float));

        quadCloudMaterial.SetBuffer("cameraSpacePointBuffer", cameraSpacePointBuffer);
        quadCloudMaterial.SetBuffer("colorSpacePointBuffer", colorSpacePointBuffer);
        quadCloudMaterial.SetBuffer("boidBuffer", flock.boidBuffer);

        quadCloudMaterial.SetTexture("_ColorTexture", colorTexture);
    }

    private void UpdateArraysAndBuffers()
    {
        mapper.MapDepthFrameToCameraSpace(depthData, cameraSpacePointArray);
        mapper.MapDepthFrameToColorSpace(depthData, colorSpacePointArray);

        cameraSpacePointBuffer.SetData(cameraSpacePointArray);
        colorSpacePointBuffer.SetData(colorSpacePointArray);
    }

    private void SetShaderDynamicProperties()
    {
        quadCloudMaterial.SetVector("_LookTarget", lookTarget.position);
    }

    #region Unity Runners
    private void Start()
    {
        InitKinect();
        InitShader();
    }
    void FixedUpdate()
    {
        UpdateKinectData();
        SetShaderDynamicProperties();
        UpdateArraysAndBuffers();
    }
    private void OnRenderObject()
    {
        quadCloudMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, 512 * 424);
    }
    private void OnDestroy()
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
        if (cameraSpacePointBuffer != null)
        {
            cameraSpacePointBuffer.Dispose();
            cameraSpacePointBuffer = null;
        }
        if (colorSpacePointBuffer != null)
        {
            colorSpacePointBuffer.Dispose();
            colorSpacePointBuffer = null;
        }
    }
    #endregion
}