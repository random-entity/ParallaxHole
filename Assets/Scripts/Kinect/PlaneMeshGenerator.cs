using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class PlaneMeshGenerator : MonoBehaviour
{
    private static readonly int width = 512;
    private static readonly int height = 424;
    [SerializeField] private int resolution = 1;
    [SerializeField] private float pitch = 0.01f;

    private void Awake()
    {
        GetComponent<MeshFilter>().mesh = generateMesh();
    }

    private Mesh generateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;

        int limitX = width / resolution + 1;
        if (width % resolution == 0) limitX--;

        int limitY = height / resolution;
        if (height % resolution == 0) limitY--;

        int vertexCount = limitX * limitY;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] uv = new Vector2[vertexCount];

        int[] triangles = new int[(limitX - 1) * (limitY - 1) * 6];

        for (int x = 0; x < limitX; x++)
        {
            for (int y = 0; y < limitY; y++)
            {
                int index = x + y * limitX;

                vertices[index] = new Vector3(
                    (x * resolution + 0.5f - width / 2) * pitch,
                    (y * resolution + 0.5f - height / 2) * pitch,
                    0f
                );

                uv[index] = new Vector2(
                    (x * resolution + 0.5f) / width,
                    (y * resolution + 0.5f) / height
                );
            }
        }

        for (int x = 0; x < limitX - 1; x++)
        {
            for (int y = 0; y < limitY - 1; y++)
            {
                int triangleIndex = x + y * (limitX - 1);
                int vertexIndex = x + y * limitX;

                triangles[triangleIndex * 6] = vertexIndex;
                triangles[triangleIndex * 6 + 1] = vertexIndex + limitX + 1;
                triangles[triangleIndex * 6 + 2] = vertexIndex + 1;
                triangles[triangleIndex * 6 + 3] = vertexIndex;
                triangles[triangleIndex * 6 + 4] = vertexIndex + limitX;
                triangles[triangleIndex * 6 + 5] = vertexIndex + limitX + 1;
            }
        }

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        return mesh;
    }
}