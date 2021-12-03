using UnityEngine;

public class QuadFlock : MonoBehaviour
{
    #region Structs, Arrays, Buffers
    [SerializeField] private Vector2Int frameGridSize;
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
    private int quoidCount => frameGridSize.x * frameGridSize.y;
    private Quoid[] quoidArray;
    private ComputeBuffer quoidBuffer;
    private Vector3[] originalPositionArray;
    private ComputeBuffer originalPositionBuffer;
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
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, quoidCount);
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
        quoidArray = new Quoid[quoidCount]; // private int quoidCount => frameGridSize.x * frameGridSize.y;
        originalPositionArray = new Vector3[quoidCount];

        int quoidIndex;
        for (int x = 0; x < frameGridSize.x; x++)
        {
            for (int y = 0; y < frameGridSize.y; y++)
            {
                quoidIndex = x + y * frameGridSize.x;

                Vector3 pos = target.position + new Vector3(x * 1f, 0f, y * 1f);
                originalPositionArray[quoidIndex] = pos;

                Quaternion rot = Quaternion.Slerp(target.rotation, Random.rotation, 0.3f);
                float noise = Random.value * 1000f;

                quoidArray[quoidIndex] = new Quoid(pos, rot.eulerAngles, noise);
            }
        }
    }

    private void InitShader()
    {
        quoidBuffer = new ComputeBuffer(quoidCount, SIZE_QUOID);
        quoidBuffer.SetData(quoidArray);

        originalPositionBuffer = new ComputeBuffer(quoidCount, 3 * sizeof(float));
        originalPositionBuffer.SetData(originalPositionArray);

        computeShader.SetBuffer(kernelHandleCSMain, "quoidBuffer", quoidBuffer);
        computeShader.SetBuffer(kernelHandleCSMain, "originalPositionBuffer", originalPositionBuffer);

        quoidFlockMaterial.SetBuffer("quoidBuffer", quoidBuffer);

        SetComputeShaderStaticProperties();
    }

    private void SetComputeShaderStaticProperties()
    {
        computeShader.SetInt("frameGridSizeX", frameGridSize.x);
        computeShader.SetInt("frameGridSizeY", frameGridSize.y);
        computeShader.SetInt("quoidCount", quoidCount);
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