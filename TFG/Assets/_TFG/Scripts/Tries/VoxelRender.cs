using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelRender : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;

    public float scale = 1f;

    float adjScale;

    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        adjScale = scale * 0.5f;
    }
    // Start is called before the first frame update
    void Start()
    {
    }
    public void GenerateVoxelMesh(List<Vector3> vectgorPos)
    {
        VoxelData voxelData = new VoxelData(vectgorPos);
        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int i = 0; i < vectgorPos.Count; i++)
        {
            MakeCube(adjScale, vectgorPos[i] * scale, voxelData);
        }
        UpdateMesh();
    }
    void MakeCube(float cubeScale, Vector3 cubePos, VoxelData data)
    {
        for (int i = 0; i < 6; i++)
        {
            if(data.GetNeighbor(cubePos, (Direction)i) == 0)
                MakeFace((Direction)i, cubeScale, cubePos);
        }
    }
    void MakeFace(Direction dir, float faceScale, Vector3 facePos)
    {
        Vector3[] faceVertices = CubeMeshData.FaceVertices(dir, faceScale, facePos);

        int vCount = vertices.Count;

        vertices.AddRange(faceVertices);

        triangles.Add(vCount);
        triangles.Add(vCount + 1);
        triangles.Add(vCount + 2);
        triangles.Add(vCount);
        triangles.Add(vCount + 2);
        triangles.Add(vCount + 3);
    }
    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }
}
