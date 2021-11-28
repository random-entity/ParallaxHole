using UnityEngine;

public class QuadCloud : MonoBehaviour
{
    #region structs
    private struct Point
    {
        public Vector3 position;
        // public Vector4 color;
    }

    private struct Vertex
    {
        public Vector3 position;
        public Vector2 uv;
    }

    const int SIZE_POINT = 3 * sizeof(float);
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
        Init();
    }

    private void Init()
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

        int index;

        for (int i = 0; i < pointsActualCount; i++)
        {
            pos.Set(Random.value * 2 - 1.0f, Random.value * 2 - 1.0f, Random.value * 2 - 1.0f);
            pos.Normalize();
            pos *= Random.value;
            pos *= 0.5f;

            pointsArray[i].position.Set(pos.x, pos.y, pos.z + 3);

            index = i * 6;
            //Triangle 1 - bottom-left, top-left, top-right
            vertexArray[index].uv.Set(0, 0);
            vertexArray[index + 1].uv.Set(0, 1);
            vertexArray[index + 2].uv.Set(1, 1);
            //Triangle 2 - bottom-left, top-right, bottom-right  // // 
            vertexArray[index + 3].uv.Set(0, 0);
            vertexArray[index + 4].uv.Set(1, 1);
            vertexArray[index + 5].uv.Set(1, 0);
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
        // Send datas to the compute computeShader
        computeShader.SetFloat("deltaTime", Time.deltaTime);
        // computeShader.SetFloats("mousePosition", mousePosition2D);

        // Update the Particles
        computeShader.Dispatch(kernelIdCSMain, groupSizeX, 1, 1);
    }
}
