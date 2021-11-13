using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class ToPointCloud : MonoBehaviour
{
    private void Start()
    {
        // Make sure to do this after the mesh is generated
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh.SetIndices(meshFilter.mesh.GetIndices(0), MeshTopology.Points, 0);
    }
}