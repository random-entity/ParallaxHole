using UnityEngine;
using Windows.Kinect;

public class QuadCloud : MonoBehaviour
{
    #region Kinect
    [SerializeField] private Transform unityRealKinect;
    public int ColorWidth { get; private set; }
    public int ColorHeight { get; private set; }
    private KinectSensor sensor;
    private CoordinateMapper mapper;
    private MultiSourceFrameReader reader;
    private ushort[] depthData;
    private Texture2D colorTexture;
    private byte[] colorData;
    private readonly Vector2Int frameGridSize = new Vector2Int(512, 424);
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
    private struct Boid
    {
        private Vector3 position;
        private Vector3 direction;
        private float noise;

        public Boid(Vector3 pos, Vector3 dir, float noi)
        {
            position = pos;
            direction = dir;
            noise = noi;
        }
    }
    const int SIZE_BOID = 7 * sizeof(float);
    private Boid[] boidArray;
    public ComputeBuffer boidBuffer;
    [SerializeField] private int flockDownSample = 16;
    private Vector2Int flockGridSize => frameGridSize / flockDownSample;
    private int boidCount => flockGridSize.x * flockGridSize.y;

    #region Compute shader
    [SerializeField] private ComputeShader computeShader;
    private int kernelHandleCSMain;
    private int groupsX, groupsY;
    #endregion

    #region Flock Properties
    [SerializeField] private float rotationSpeed = 8f;
    [SerializeField] private float speed = 6f;
    [SerializeField] private float speedVariation = 0.8f;
    [SerializeField] private float neighbourDistance = 1f;
    [SerializeField] private Transform target;
    #endregion
    #endregion

    #region Rendering
    [SerializeField] private Material quadCloudMaterial;
    [SerializeField] private Transform lookTarget;
    #endregion

    #region Flocking Methods
    private void InitBoidsAndComputeShader()
    {
        kernelHandleCSMain = computeShader.FindKernel("CSMain");

        uint threadsX, threadsY;
        computeShader.GetKernelThreadGroupSizes(kernelHandleCSMain, out threadsX, out threadsY, out _);

        groupsX = Mathf.CeilToInt((float)flockGridSize.x / (float)threadsX);
        groupsY = Mathf.CeilToInt((float)flockGridSize.y / (float)threadsY);

        boidArray = new Boid[boidCount];

        int boidIndex;
        for (int x = 0; x < flockGridSize.x; x++)
        {
            for (int y = 0; y < flockGridSize.y; y++)
            {
                boidIndex = x + y * flockGridSize.x;

                Vector3 pos = Random.insideUnitSphere;
                Quaternion rot = Random.rotation;
                float noise = Random.value * 1000f;

                boidArray[boidIndex] = new Boid(pos, rot.eulerAngles, noise);
            }
        }

        boidBuffer = new ComputeBuffer(boidCount, SIZE_BOID);
        boidBuffer.SetData(boidArray);

        computeShader.SetBuffer(kernelHandleCSMain, "boidBuffer", boidBuffer);

        SetComputeShaderStaticProperties();
    }

    private void SetComputeShaderStaticProperties()
    {
        computeShader.SetInt("flockGridSizeX", flockGridSize.x);
        computeShader.SetInt("flockGridSizeY", flockGridSize.y);
        computeShader.SetInt("boidCount", boidCount);

        computeShader.SetFloat("rotationSpeed", rotationSpeed);
        computeShader.SetFloat("speed", speed);
        computeShader.SetFloat("speedVariation", speedVariation);
        computeShader.SetVector("flockPosition", target.position);
        computeShader.SetFloat("neighbourDistance", neighbourDistance);
    }

    private void SetComputeShaderDynamicProperties()
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("deltaTime", Time.fixedDeltaTime);
    }
    #endregion

    private void InitMaterialShader()
    {
        cameraSpacePointBuffer = new ComputeBuffer(cameraSpacePointArray.Length, 3 * sizeof(float));
        colorSpacePointBuffer = new ComputeBuffer(colorSpacePointArray.Length, 2 * sizeof(float));
        // boidBuffer = new ComputeBuffer()

        quadCloudMaterial.SetVector("_KinectPosition", unityRealKinect.position);
        quadCloudMaterial.SetBuffer("cameraSpacePointBuffer", cameraSpacePointBuffer);
        quadCloudMaterial.SetBuffer("colorSpacePointBuffer", colorSpacePointBuffer);
        quadCloudMaterial.SetBuffer("boidBuffer", boidBuffer);
        quadCloudMaterial.SetInt("_FlockDownSample", flockDownSample);

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
        InitBoidsAndComputeShader();
        InitMaterialShader();
    }
    void FixedUpdate()
    {
        UpdateKinectData();

        SetComputeShaderDynamicProperties();
        computeShader.Dispatch(kernelHandleCSMain, groupsX, groupsY, 1);

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
        if (boidBuffer != null)
        {
            boidBuffer.Dispose();
            boidBuffer = null;
        }
    }
    #endregion
}