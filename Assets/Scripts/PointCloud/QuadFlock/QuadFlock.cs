using UnityEngine;

public class QuadFlock : MonoBehaviour
{
    #region Structs, Arrays, Buffers
    [SerializeField] private Vector2Int frameGridSize;
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
    private int quoidCount => frameGridSize.x * frameGridSize.y;
    private Quoid[] quoidArray;
    private ComputeBuffer quoidBuffer;
    #endregion

    #region Vertex
    private struct Vertex
    {
        private Vector3 position;
        public Vector2 uv;
    }
    const int SIZE_VERTEX = 5 * sizeof(float);
    private int vertexCount => 6 * frameGridSize.x * frameGridSize.y; // 2 * 2 vertices, 6 triangles
    private Vertex[] vertexArray;
    private ComputeBuffer vertexBuffer;
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

        InitQuoidsAndVertices();
        InitShader();
    }
    private void Update()
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.SetFloat("deltaTime", Time.deltaTime);

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
        if (vertexBuffer != null)
        {
            vertexBuffer.Release();
            vertexBuffer = null;
        }
    }
    #endregion

    private void InitQuoidsAndVertices()
    {
        quoidArray = new Quoid[quoidCount]; // private int quoidCount => frameGridSize.x * frameGridSize.y;
        vertexArray = new Vertex[vertexCount];

        int quoidIndex;
        int vertexIndex;
        for (int x = 0; x < frameGridSize.x; x++)
        {
            for (int y = 0; y < frameGridSize.y; y++)
            {
                #region Init quoidArray
                quoidIndex = x + y * frameGridSize.x;

                Vector3 pos = transform.position + new Vector3(x * 1f, 0f, y * 1f);
                Quaternion rot = Quaternion.Slerp(transform.rotation, Random.rotation, 0.3f);
                float noise = Random.value * 1000f;

                quoidArray[quoidIndex] = new Quoid(pos, rot.eulerAngles, noise);
                #endregion

                #region Init vertexArray
                // Set only uvs here (Set Vertex.position in compute shader)

                vertexIndex = 6 * quoidIndex;

                // Triangle 1 : bottom-left, top-left, top-right
                vertexArray[vertexIndex].uv.Set(0, 0);
                vertexArray[vertexIndex + 1].uv.Set(0, 1);
                vertexArray[vertexIndex + 2].uv.Set(1, 1);

                // Triangle 2 : bottom-left, top-right, bottom-right
                vertexArray[vertexIndex + 3].uv.Set(0, 0);
                vertexArray[vertexIndex + 4].uv.Set(1, 1);
                vertexArray[vertexIndex + 5].uv.Set(1, 0);
                #endregion
            }
        }
    }

    private void InitShader()
    {
        quoidBuffer = new ComputeBuffer(quoidCount, SIZE_QUOID);
        quoidBuffer.SetData(quoidArray);

        vertexBuffer = new ComputeBuffer(vertexCount, SIZE_VERTEX);
        vertexBuffer.SetData(vertexArray);

        computeShader.SetBuffer(kernelHandleCSMain, "quoidBuffer", quoidBuffer);
        computeShader.SetBuffer(kernelHandleCSMain, "vertexBuffer", vertexBuffer);
        quoidFlockMaterial.SetBuffer("vertexBuffer", vertexBuffer);

        SetComputeShaderProperties();
    }

    private void SetComputeShaderProperties()
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
}