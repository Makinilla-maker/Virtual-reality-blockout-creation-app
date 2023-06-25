using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class VoxelRender : MonoBehaviour
{
    Mesh mesh;
    List<Vector3> vertices;
    List<int> triangles;
    List<Vector3> normals;
    List<Vector2> uvs;

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
        normals = new List<Vector3>();
        uvs = new List<Vector2>();

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

        // Calcular y agregar las normales para la cara actual
        Vector3 faceNormal = GetDirectionNormal(dir);
        for (int i = 0; i < 4; i++)
        {
            normals.Add(faceNormal);
        }

        // Calcular las coordenadas de textura para la cara actual
        Vector2[] faceUVs = CalculateFaceUVs(dir);
        uvs.AddRange(faceUVs);

        triangles.Add(vCount);
        triangles.Add(vCount + 1);
        triangles.Add(vCount + 2);
        triangles.Add(vCount);
        triangles.Add(vCount + 2);
        triangles.Add(vCount + 3);
    }
    Vector3 GetDirectionNormal(Direction dir)
    {
        switch (dir)
        {
            case Direction.NORTH:
                return Vector3.back;
            case Direction.EAST:
                return Vector3.right;
            case Direction.SOUTH:
                return Vector3.forward;
            case Direction.WEST:
                return Vector3.left;
            case Direction.UP:
                return Vector3.up;
            case Direction.DOWN:
                return Vector3.down;
            default:
                return Vector3.zero;
        }
    }
    Vector2[] CalculateFaceUVs(Direction dir)
    {
        Vector2[] faceUVs = new Vector2[4];
        float uCoord = 0.0f;
        float vCoord = 0.0f;

        switch (dir)
        {
            case Direction.UP:
                uCoord = 0.0f;
                vCoord = 1.0f;
                break;
            case Direction.DOWN:
                uCoord = 0.0f;
                vCoord = 0.0f;
                break;
            case Direction.NORTH:
            case Direction.SOUTH:
            case Direction.WEST:
            case Direction.EAST:
                uCoord = 1.0f;
                vCoord = 1.0f;
                break;
        }

        faceUVs[0] = new Vector2(0.0f, 0.0f);
        faceUVs[1] = new Vector2(uCoord, 0.0f);
        faceUVs[2] = new Vector2(uCoord, vCoord);
        faceUVs[3] = new Vector2(0.0f, vCoord);

        return faceUVs;
    }
    public void UpdateMesh()
    {
        mesh.Clear();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray(); // Asignar las coordenadas de textura al mesh
        mesh.RecalculateNormals();
    }
}
