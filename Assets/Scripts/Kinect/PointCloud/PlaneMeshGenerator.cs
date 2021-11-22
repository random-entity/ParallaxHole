using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class PlaneMeshGenerator : MonoBehaviour
{
    public static int vertexCount;
    private void Awake()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = generateMesh();
        vertexCount = meshFilter.mesh.vertices.Length;
    }

    private Mesh generateMesh()
    {
        int width = PointCloudConfig.depthWidth;
        int height = PointCloudConfig.depthHeight;
        int downsample = PointCloudConfig.downsample;
        float pitch = PointCloudConfig.pitch;

        Mesh mesh = new Mesh();
        mesh.indexFormat = IndexFormat.UInt32;

        int limitX = width / downsample + 1;
        if (width % downsample == 0) limitX--;

        int limitY = height / downsample + 1;
        if (height % downsample == 0) limitY--;

        int vertexCount = limitX * limitY;

        Vector3[] vertices = new Vector3[vertexCount];
        Vector2[] depthUV = new Vector2[vertexCount];

        int[] triangles = new int[(limitX - 1) * (limitY - 1) * 6];

        for (int x = 0; x < limitX; x++)
        {
            for (int y = 0; y < limitY; y++)
            {
                int index = x + y * limitX;

                vertices[index] = new Vector3(
                    (x * downsample + 0.5f - width * 0.5f) * pitch,
                    (y * downsample + 0.5f - height * 0.5f) * pitch,
                    0f
                );

                depthUV[index] = new Vector2(
                    (x * downsample + 0.5f) / width,
                    (y * downsample + 0.5f) / height
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
        mesh.SetUVs(0, depthUV);
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }
}