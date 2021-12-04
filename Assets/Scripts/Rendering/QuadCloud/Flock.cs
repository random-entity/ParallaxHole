using UnityEngine;

public class Flock : MonoBehaviour
{
    private readonly Vector2Int frameGridSize = new Vector2Int(512, 424) / 8;
    private int pointCount => frameGridSize.x * frameGridSize.y;

    #region Structs, Arrays, Buffers
    #region Quoid
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
    #endregion
    #endregion

    #region Compute shader
    [SerializeField] private ComputeShader computeShader;
    private int kernelHandleCSMain;
    private int groupsX, groupsY;
    #endregion

    #region Flocking
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float speed = 1f;
    [SerializeField] private float speedVariation = 1f;
    [SerializeField] private float neighbourDistance = 1f;
    [SerializeField] private Transform target;
    #endregion

    #region Unity runners
    private void Awake()
    {
        kernelHandleCSMain = computeShader.FindKernel("CSMain");

        uint threadsX, threadsY;
        computeShader.GetKernelThreadGroupSizes(kernelHandleCSMain, out threadsX, out threadsY, out _);

        groupsX = Mathf.CeilToInt(512f / (float)threadsX);
        groupsY = Mathf.CeilToInt(424f / (float)threadsY);

        InitBoids();
        InitShader();
    }
    private void FixedUpdate()
    {
        SetComputeShaderDynamicProperties();
        computeShader.Dispatch(kernelHandleCSMain, groupsX, groupsY, 1);
    }
    private void OnDestroy()
    {
        if (boidBuffer != null)
        {
            boidBuffer.Release();
            boidBuffer = null;
        }
    }
    #endregion

    private void InitBoids()
    {
        boidArray = new Boid[pointCount]; // private int pointCount => frameGridSize.x * frameGridSize.y;

        int boidIndex;
        for (int x = 0; x < frameGridSize.x; x++)
        {
            for (int y = 0; y < frameGridSize.y; y++)
            {
                boidIndex = x + y * frameGridSize.x;

                Vector3 pos = Random.insideUnitSphere * 0.01f;
                Quaternion rot = Random.rotation;
                float noise = Random.value * 1000f;

                boidArray[boidIndex] = new Boid(pos, rot.eulerAngles, noise);
            }
        }
    }

    private void InitShader()
    {
        boidBuffer = new ComputeBuffer(pointCount, SIZE_BOID);
        boidBuffer.SetData(boidArray);

        computeShader.SetBuffer(kernelHandleCSMain, "boidBuffer", boidBuffer);

        SetComputeShaderStaticProperties();
    }

    private void SetComputeShaderStaticProperties()
    {
        computeShader.SetInt("frameGridSizeX", frameGridSize.x);
        computeShader.SetInt("frameGridSizeY", frameGridSize.y);
        computeShader.SetInt("boidCount", pointCount);

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
}