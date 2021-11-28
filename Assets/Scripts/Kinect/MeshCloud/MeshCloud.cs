using UnityEngine;

public class MeshCloud : MonoBehaviour
{
    #region struct PointMesh
    public struct PointMesh
    {
        public Vector3 position;
        public PointMesh(Vector3 pos)
        {
            position = pos;
        }
    }
    #endregion

    #region Compute shader
    [SerializeField] private ComputeShader computeShader;
    private int kernelHandleCSMain;
    private int groupsSizeX;
    #endregion

    #region PointMesh data
    [SerializeField] private Mesh pointMeshMesh;
    [SerializeField] private Material pointMeshMaterial;
    #endregion

    #region PointMeshes collection/buffer data
    public int pointMeshesCount; // set publicly
    private int numOfPointMeshes; // actual count of point meshes drawn (cf. Start())
    private PointMesh[] pointMeshesArray;
    private ComputeBuffer pointMeshesBuffer;
    private ComputeBuffer argsBuffer;
    uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    private Bounds bounds;
    #endregion

    private void Start()
    {
        kernelHandleCSMain = computeShader.FindKernel("CSMain");

        uint x;
        computeShader.GetKernelThreadGroupSizes(kernelHandleCSMain, out x, out _, out _);
        groupsSizeX = Mathf.CeilToInt((float)pointMeshesCount / (float)x);
        numOfPointMeshes = groupsSizeX * (int)x;
        bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

        InitPointMeshes();
        InitShader();
    }

    private void InitPointMeshes()
    {
        pointMeshesArray = new PointMesh[numOfPointMeshes];

        for (int i = 0; i < numOfPointMeshes; i++)
        {
            Vector3 pos = transform.position + Random.insideUnitSphere * 2f;

            pointMeshesArray[i] = new PointMesh(pos);
        }
    }

    private void InitShader()
    {
        pointMeshesBuffer = new ComputeBuffer(numOfPointMeshes, 3 * sizeof(float));
        pointMeshesBuffer.SetData(pointMeshesArray);

        argsBuffer = new ComputeBuffer(1, 5 * sizeof(uint), ComputeBufferType.IndirectArguments);

        if (pointMeshMesh != null)
        {
            args[0] = (uint)pointMeshMesh.GetIndexCount(0);
            args[1] = (uint)numOfPointMeshes;
        }

        argsBuffer.SetData(args);

        computeShader.SetBuffer(kernelHandleCSMain, "pointMeshesBuffer", pointMeshesBuffer);

        pointMeshMaterial.SetBuffer("pointMeshesBuffer", pointMeshesBuffer);
    }

    private void Update()
    {
        computeShader.SetFloat("time", Time.time);
        computeShader.Dispatch(kernelHandleCSMain, groupsSizeX, 1, 1);
        Graphics.DrawMeshInstancedIndirect(pointMeshMesh, 0, pointMeshMaterial, bounds, argsBuffer);
    }

    private void OnDestroy()
    {
        if (pointMeshesBuffer != null)
        {
            pointMeshesBuffer.Dispose();
            pointMeshesBuffer = null;
        }
        if (argsBuffer != null)
        {
            argsBuffer.Dispose();
            argsBuffer = null;
        }
    }
}