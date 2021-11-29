using UnityEngine;

public class QuadCloud : MonoBehaviour
{
    #region structs
    private struct Point
    {
        public Vector3 position;
        public Vector4 color;
    }

    private struct Vertex
    {
        public Vector3 position;
        public Vector2 uv;
    }

    const int SIZE_POINT = 7 * sizeof(float);
    const int SIZE_VERTEX = 5 * sizeof(float);
    #endregion

    #region Compute shader
    [SerializeField] private ComputeShader computeShader;
    private ComputeBuffer pointsBuffer;
    private ComputeBuffer verticesBuffer;
    private int kernelIdCSMain;
    private int groupSizeX;
    #endregion

    #region Quad
    [SerializeField] private Material quadMaterial;
    [SerializeField] private float quadSize;
    #endregion

    #region Quad cloud
    [SerializeField] private int pointsCount;
    private int pointsActualCount;
    #endregion

    private void Start()
    {
        InitShader();
    }

    private void InitShader()
    {
        kernelIdCSMain = computeShader.FindKernel("CSMain");

        uint threadsX;
        computeShader.GetKernelThreadGroupSizes(kernelIdCSMain, out threadsX, out _, out _);
        groupSizeX = Mathf.CeilToInt((float)pointsCount / (float)threadsX);
        pointsActualCount = groupSizeX * (int)threadsX;

        Point[] pointsArray = new Point[pointsActualCount];

        int verticesActualCount = pointsActualCount * 6;
        Vertex[] vertexArray = new Vertex[verticesActualCount];

        Vector3 pos = new Vector3();

        int vertexIndex;

        for (int i = 0; i < pointsActualCount; i++)
        {
            pos.Set(Random.value * 2 - 1.0f, Random.value * 2 - 1.0f, Random.value * 2 - 1.0f);
            pos.Normalize();
            pos *= Random.value;
            pos *= 0.5f;

            pointsArray[i].position.Set(pos.x, pos.y, pos.z);
            pointsArray[i].color.Set(0f, 1f, 0f, 0.5f);

            vertexIndex = i * 6;
            //Triangle 1 - bottom-left, top-left, top-right
            vertexArray[vertexIndex].uv.Set(0, 0);
            vertexArray[vertexIndex + 1].uv.Set(0, 1);
            vertexArray[vertexIndex + 2].uv.Set(1, 1);
            //Triangle 2 - bottom-left, top-right, bottom-right
            vertexArray[vertexIndex + 3].uv.Set(0, 0);
            vertexArray[vertexIndex + 4].uv.Set(1, 1);
            vertexArray[vertexIndex + 5].uv.Set(1, 0);
        }

        // create compute buffers
        pointsBuffer = new ComputeBuffer(pointsActualCount, SIZE_POINT);
        pointsBuffer.SetData(pointsArray);
        verticesBuffer = new ComputeBuffer(verticesActualCount, SIZE_VERTEX);
        verticesBuffer.SetData(vertexArray);

        // bind the compute buffers to the computeShader and the compute computeShader
        computeShader.SetBuffer(kernelIdCSMain, "pointsBuffer", pointsBuffer);
        computeShader.SetBuffer(kernelIdCSMain, "verticesBuffer", verticesBuffer);
        computeShader.SetFloat("halfSize", quadSize * 0.5f);

        quadMaterial.SetBuffer("pointsBuffer", pointsBuffer);
        quadMaterial.SetBuffer("verticesBuffer", verticesBuffer);
    }

    private void OnRenderObject()
    {
        quadMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, pointsActualCount);
    }

    private void OnDestroy()
    {
        if (pointsBuffer != null)
            pointsBuffer.Release();
        if (verticesBuffer != null)
            verticesBuffer.Release();
    }

    private void Update()
    {
        computeShader.Dispatch(kernelIdCSMain, groupSizeX, 1, 1);
    }
}
