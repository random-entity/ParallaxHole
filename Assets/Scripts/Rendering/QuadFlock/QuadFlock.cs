using UnityEngine;

public class QuadFlock : MonoBehaviour
{
    #region Kinect Frame
    [SerializeField] private Vector2Int frameGridSize;
    private int pointCount => frameGridSize.x * frameGridSize.y;
    #endregion

    #region Structs, Arrays, Buffers
    #region Quoid
    private struct Quoid
    {
        private Vector3 position;
        private Vector3 direction;
        private float noise;

        public Quoid(Vector3 pos, Vector3 dir, float noi)
        {
            position = pos;
            direction = dir;
            noise = noi;
        }
    }
    const int SIZE_QUOID = 7 * sizeof(float);
    private Quoid[] quoidArray;
    private ComputeBuffer quoidBuffer;
    #endregion
    #region KinectPoint
    private struct KinectPoint {
        public Vector3 cameraSpacePosition;
        public Vector2 uv_color;
    }
    const int SIZE_KINECTPOINT = 5 * sizeof(float);
    private KinectPoint[] kinectPointArray;
    private ComputeBuffer kinectPointBuffer;
    #endregion
    #endregion

    #region Compute shader
    [SerializeField] private ComputeShader computeShader;
    private int kernelHandleCSMain;
    private int groupsX, groupsY;
    #endregion

    #region Rendering
    [SerializeField] private Material quoidFlockMaterial;
    [SerializeField, Range(0.01f, 1.0f)] private float quadSize = 0.1f;
    private float quadHalfSize => quadSize * 0.5f;
    #endregion

    #region Flocking
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float speedVariation = 1f;
    [SerializeField] private float neighbourDistance = 1f;
    [SerializeField] private Transform target;
    #endregion

    #region Unity runners
    private void Start()
    {
        kernelHandleCSMain = computeShader.FindKernel("CSMain");

        uint threadsX, threadsY;
        computeShader.GetKernelThreadGroupSizes(kernelHandleCSMain, out threadsX, out threadsY, out _);

        groupsX = Mathf.CeilToInt((float)frameGridSize.x / (float)threadsX);
        groupsY = Mathf.CeilToInt((float)frameGridSize.y / (float)threadsY);

        InitQuoids();
        InitShader();
    }
    private void Update()
    {
        SetComputeShaderDynamicProperties();

        computeShader.Dispatch(kernelHandleCSMain, groupsX, groupsY, 1);
    }
    private void OnRenderObject()
    {
        quoidFlockMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, pointCount);
    }
    private void OnDestroy()
    {
        if (quoidBuffer != null)
        {
            quoidBuffer.Release();
            quoidBuffer = null;
        }
    }
    #endregion

    private void InitQuoids()
    {
        quoidArray = new Quoid[pointCount]; // private int quoidCount => frameGridSize.x * frameGridSize.y;
        kinectPointArray = new KinectPoint[pointCount];

        int quoidIndex;
        for (int x = 0; x < frameGridSize.x; x++)
        {
            for (int y = 0; y < frameGridSize.y; y++)
            {
                quoidIndex = x + y * frameGridSize.x;

                Vector3 pos = new Vector3(x * 1f, 0f, y * 1f);

                kinectPointArray[quoidIndex].cameraSpacePosition = pos; // should be set to kinect tracked body mesh points tomorrow
                kinectPointArray[quoidIndex].uv_color = Vector2.zero;

                Quaternion rot = Random.rotation;
                float noise = Random.value * 1000f;

                quoidArray[quoidIndex] = new Quoid(pos, rot.eulerAngles, noise);
            }
        }
    }

    private void InitShader()
    {
        quoidBuffer = new ComputeBuffer(pointCount, SIZE_QUOID);
        quoidBuffer.SetData(quoidArray);

        kinectPointBuffer = new ComputeBuffer(pointCount, 3 * sizeof(float));
        kinectPointBuffer.SetData(kinectPointArray);

        computeShader.SetBuffer(kernelHandleCSMain, "quoidBuffer", quoidBuffer);



        computeShader.SetBuffer(kernelHandleCSMain, "originalPositionBuffer", kinectPointBuffer);

        quoidFlockMaterial.SetBuffer("quoidBuffer", quoidBuffer);

        SetComputeShaderStaticProperties();
    }

    private void SetComputeShaderStaticProperties()
    {
        computeShader.SetInt("frameGridSizeX", frameGridSize.x);
        computeShader.SetInt("frameGridSizeY", frameGridSize.y);
        computeShader.SetInt("quoidCount", pointCount);
        computeShader.SetFloat("quadHalfSize", quadHalfSize);

        computeShader.SetFloat("rotationSpeed", rotationSpeed);
        computeShader.SetFloat("speed", speed);
        computeShader.SetFloat("speedVariation", speedVariation);
        computeShader.SetVector("flockPosition", target.position);
        computeShader.SetFloat("neighbourDistance", neighbourDistance);
    }

    private void SetComputeShaderDynamicProperties()
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("deltaTime", Time.deltaTime);
    }
}