using UnityEngine;

public class QuadFlock : MonoBehaviour
{
    #region Structs
    private struct Qoid
    {
        private Vector3 position;
        private Vector3 direction;
        // private Vector3 noiseOffset;

        public Qoid(Vector3 pos, Vector3 dir)
        {
            position = pos;
            direction = dir;
        }
    }
    const int SIZE_QOID = 6 * sizeof(float);

    private struct Vertex
    {
        private Vector3 position;
        private Vector2 uv;
    }
    const int SIZE_VERTEX = 5 * sizeof(float);
    #endregion

    [SerializeField] private int qoidCount = 1000;
    [SerializeField] private Material quadFlockMaterial;
    [SerializeField] private ComputeShader computeShader;
    [SerializeField, Range(0.01f, 1.0f)] private float quadSize = 0.1f;

    int numParticles;
    int kernelID;
    ComputeBuffer particleBuffer;
    ComputeBuffer vertexBuffer;


    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
