using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class TubeGenerator : MonoBehaviour
{
    private Mesh tubeMesh;
    [SerializeField] private int res;
    [SerializeField] private float radius;
    [SerializeField] private float height;
    [SerializeField] private bool doUpdate = true;

    private void Awake()
    {
        tubeMesh = new Mesh();
        updateTubeMesh();
        GetComponent<MeshFilter>().mesh = tubeMesh;
    }

    private void Update()
    {
        if (doUpdate)
        {
            updateTubeMesh();
        }
    }

    private void updateTubeMesh()
    {
        Vector3[] vertices = new Vector3[2 * res + 2];

        for (int i = 0; i <= res; i++)
        {
            float arg = Mathf.PI * 2f * ((float)i / res);

            float x = radius * Mathf.Cos(arg);
            float y = 0;
            float z = radius * Mathf.Sin(arg);

            vertices[2 * i] = new Vector3(x, y, z);

            y = height;

            vertices[2 * i + 1] = new Vector3(x, y, z);
        }

        Vector2[] uvs = new Vector2[vertices.Length];

        for (int i = 0; i <= res; i++)
        {
            uvs[2 * i] = new Vector2((float)i / (res - 1), 0f);
            uvs[2 * i + 1] = new Vector2((float)i / (res - 1), 1f);
        }

        int[] triangles = new int[3 * 2 * res];

        for (int i = 0; i <= res - 1; i++)
        {
            int v0 = 2 * i;
            int v1 = 2 * i + 1;
            int v2 = 2 * i + 2;
            int v3 = 2 * i + 3;

            triangles[6 * i] = v0;
            triangles[6 * i + 1] = v2;
            triangles[6 * i + 2] = v1;
            triangles[6 * i + 3] = v1;
            triangles[6 * i + 4] = v2;
            triangles[6 * i + 5] = v3;
        }

        tubeMesh.vertices = vertices;
        tubeMesh.uv = uvs;
        tubeMesh.triangles = triangles;
    }
}