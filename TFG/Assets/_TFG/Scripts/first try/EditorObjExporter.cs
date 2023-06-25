using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System;
using Unity.Collections;
using AsciiFBXExporter;
using UnityEditor.Rendering.Universal;

public class EditorObjExporter : MonoBehaviour
{
    struct ObjMaterial
    {
        public string name;
        public string textureName;
    }

    public int vertexOffset = 0;
    public int normalOffset = 0;
    public int uvOffset = 0;
    private string targetFolder;
    public string name = "ExporterObject";

    private void Start()
    {
        targetFolder = Application.dataPath;
        Debug.Log(targetFolder);
    }
    // Update is called once per frame
    public void ExportGameObjects()
    {
        if (!CreateFolder())
            return;
        StartExport(name);       
    }
    bool CreateFolder()
    {
        try
        {
            Debug.Log("Folder created");
            System.IO.Directory.CreateDirectory(targetFolder);
        }
        catch
        {
            Debug.Log(targetFolder);
            //EditorUtility.DisplayDialog("Error!", "Failed to create target folder!", "");
            return false;
        }

        return true;
    }
    public void StartExport(string fileName)
    { 
        MeshFilter[] filters = GetMeshFilter();

        if (filters.Length == 0) return;

        MeshesToFile(filters, targetFolder, fileName + ".obj");
    }
    MeshFilter[] GetMeshFilter()
    {
        GameObject[] allGo = GameObject.FindGameObjectsWithTag("ObjectToExport");
        int i = 0;
        MeshFilter[] meshFilters = new MeshFilter[allGo.Length];
        foreach (GameObject go in allGo)
        {
            meshFilters[i] = go.GetComponent<MeshFilter>();
            i++;
        }
        return meshFilters;
    }
    void MeshesToFile(MeshFilter[] mf, string folder, string filename)
    {
        Clear();

        Dictionary<string, ObjMaterial> materialList = new Dictionary<string, ObjMaterial>();

        Debug.Log("Created at: " + folder + "/" + filename);

        using (StreamWriter sw = new StreamWriter(folder + "/" + filename))
        {
            sw.Write("mtllib ./" + filename + ".mtl\n");

            for (int i = 0; i < mf.Length; i++)
            {
                sw.Write(MeshToString(mf[i], materialList));
            }
        }
        ExportMaterialFile(folder, filename, materialList);
    }
    void ExportMaterialFile(string folder, string filename, Dictionary<string, ObjMaterial> materialList)
    {
        using (StreamWriter sw = new StreamWriter(folder + "/" + name + ".mtl"))
        {
            foreach (KeyValuePair<string, ObjMaterial> kvp in materialList)
            {
                sw.Write("\nnewmtl " + kvp.Key + "\n");
                sw.Write("Ka 0.6 0.6 0.6\n"); // Default ambient color
                sw.Write("Kd 1 1 1\n"); // Default diffuse color
                sw.Write("Ks 0.9 0.9 0.9\n"); // Default specular color
                sw.Write("d 1.0\n"); // Default opacity
                sw.Write("illum 2\n"); // Default illumination model

                if (kvp.Value.textureName != null)
                {
                    string textureFileName = Path.GetFileName(kvp.Value.textureName);
                    sw.Write("map_Kd " + textureFileName + "\n"); // Diffuse texture
                }
            }
        }
    }
    void Clear()
    {
        vertexOffset = 0;
        normalOffset = 0;
        uvOffset = 0;
    }
    string MeshToString(MeshFilter mf, Dictionary<string, ObjMaterial> materialList)
    {
        Mesh m = mf.sharedMesh;
        Material[] mats = mf.GetComponent<Renderer>().sharedMaterials;

        StringBuilder sb = new StringBuilder();

        sb.Append("g ").Append(mf.name).Append("\n");
        foreach (Vector3 lv in m.vertices)
        {
            Vector3 wv = mf.transform.TransformPoint(lv);

            //This is sort of ugly - inverting x-component since we're in
            //a different coordinate system than "everyone" is "used to".
            sb.Append(string.Format("v {0} {1} {2}\n", -wv.x, wv.y, wv.z).Replace(",", "."));
        }
        sb.Append("\n");

        foreach (Vector3 lv in m.normals)
        {
            Vector3 wv = mf.transform.TransformDirection(lv);

            sb.Append(string.Format("vn {0} {1} {2}\n", -wv.x, wv.y, wv.z).Replace(",", "."));
        }
        sb.Append("\n");

        foreach (Vector3 v in m.uv)
        {
            sb.Append(string.Format("vt {0} {1}\n", v.x, v.y).Replace(",", "."));
        }

        for (int material = 0; material < m.subMeshCount; material++)
        {
            sb.Append("\n");
            sb.Append("usemtl ").Append(mats[material].name).Append("\n");
            sb.Append("usemap ").Append(mats[material].name).Append("\n");

            //See if this material is already in the materiallist.
            try
            {
                ObjMaterial objMaterial = new ObjMaterial();

                objMaterial.name = mats[material].name;

                if (mats[material].mainTexture)
                {
                    //objMaterial.textureName = AssetDatabase.GetAssetPath(mats[material].mainTexture);
                }
                else
                    objMaterial.textureName = null;

                materialList.Add(objMaterial.name, objMaterial);
            }
            catch (ArgumentException)
            {
                //Already in the dictionary
            }


            int[] triangles = m.GetTriangles(material);
            for (int i = 0; i < triangles.Length; i += 3)
            {
                //Because we inverted the x-component, we also needed to alter the triangle winding.
                sb.Append(string.Format("f {1}/{1}/{1} {0}/{0}/{0} {2}/{2}/{2}\n",
                                       triangles[i] + 1 + vertexOffset, triangles[i + 1] + 1 + normalOffset, triangles[i + 2] + 1 + uvOffset));
            }
        }

        vertexOffset += m.vertices.Length;
        normalOffset += m.normals.Length;
        uvOffset += m.uv.Length;

        return sb.ToString();
    }
}


