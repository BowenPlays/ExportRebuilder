using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class PrefabExporter1
{
    //[Serializable]
    public static class Config
    {
        public static string prefix = "" +
                                     "using System.Collections.Generic;\n" +
                                     "using UnityEngine;\n" +
                                     "namespace ExportRebuilder;\n" +
                                     "public static class PrefabExport\n{";
        public static string postfix = "\n}";
    }
            
    //[Serializable]
    public class BoxColliderData
    {
        public Vector3 center;
        public Vector3 size;
    }

    //[Serializable]
    public class ObjectData
    {
        public string mesh;
        public string name;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public BoxColliderData boxCollider;
        public List<ObjectData> children = new List<ObjectData>();
    }
    public static ObjectData CaptureGameObjectData(Transform transform)
    {
        ObjectData data = new ObjectData
        {
            name = transform.name,
            position = transform.localPosition,
            rotation = transform.localRotation,
            scale = transform.localScale,
            mesh = null
        };
        
        MeshFilter meshFilter = transform.GetComponent<MeshFilter>();
        SkinnedMeshRenderer skinnedMeshRenderer = transform.GetComponent<SkinnedMeshRenderer>();
        
        if (meshFilter != null && meshFilter.sharedMesh != null)
        {
            data.mesh = meshFilter.sharedMesh.name;
        }
        else if (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null)
        {
            data.mesh = skinnedMeshRenderer.sharedMesh.name;
        }
        else
        {
            data.mesh = null;
        }
        
        BoxCollider boxCollider = transform.GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            data.boxCollider = new BoxColliderData
            {
                center = boxCollider.center,
                size = boxCollider.size
            };
        }

        foreach (Transform child in transform)
        {
            data.children.Add(CaptureGameObjectData(child));
        }

        return data;
    }

    [MenuItem("Tools/Export Prefab Data")]
    public static void ExportHierarchyToCSharp()
    {
        if (Selection.activeGameObject == null)
        {
            Debug.LogError("No GameObject selected! Please select a root GameObject.");
            return;
        }

        GameObject root = Selection.activeGameObject;
        ObjectData rootData = CaptureGameObjectData(root.transform);

        string codeString = GenerateCodeString(rootData);
        codeString = Config.prefix + codeString + Config.postfix;

        string path = EditorUtility.SaveFilePanel("Save Prefab Data", "", $"{root.name}PrefabExport.cs", "cs");

        if (!string.IsNullOrEmpty(path))
        {
            File.WriteAllText(path, codeString);
            Debug.Log($"Hierarchy exported to: {path}");
        }
        else
        {
            Debug.LogWarning("Export cancelled or invalid path.");
        }
    }

    private static string GenerateCodeString(ObjectData rootData)
    {
        string codeString = "public static PrefabRebuilder.ObjectData Data = new PrefabRebuilder.ObjectData\n{\n";
        codeString += $"    name = \"{rootData.name}\",\n";
        codeString += $"    position = new Vector3({rootData.position.x}f, {rootData.position.y}f, {rootData.position.z}f),\n";
        codeString += $"    rotation = new Quaternion({rootData.rotation.x}f, {rootData.rotation.y}f, {rootData.rotation.z}f, {rootData.rotation.w}f),\n";
        codeString += $"    scale = new Vector3({rootData.scale.x}f, {rootData.scale.y}f, {rootData.scale.z}f),\n";
        codeString += $"    mesh = \"{rootData.mesh}\",\n";

        if (rootData.boxCollider != null)
        {
            codeString += "    boxCollider = new PrefabRebuilder.BoxColliderData\n    {\n";
            codeString += $"        center = new Vector3({rootData.boxCollider.center.x}f, {rootData.boxCollider.center.y}f, {rootData.boxCollider.center.z}f),\n";
            codeString += $"        size = new Vector3({rootData.boxCollider.size.x}f, {rootData.boxCollider.size.y}f, {rootData.boxCollider.size.z}f)\n";
            codeString += "    },\n";
        }

        codeString += AddChildrenData(rootData.children);
        codeString += "};\n";

        return codeString;
    }

    private static string AddChildrenData(List<ObjectData> children)
    {
        string codeString = string.Empty;

        if (children.Count > 0)
        {
            codeString += "    children = new List<PrefabRebuilder.ObjectData>\n    {\n";
            foreach (var child in children)
            {
                codeString += "        new PrefabRebuilder.ObjectData\n        {\n";
                codeString += $"            name = \"{child.name}\",\n";
                codeString += $"            position = new Vector3({child.position.x}f, {child.position.y}f, {child.position.z}f),\n";
                codeString += $"            rotation = new Quaternion({child.rotation.x}f, {child.rotation.y}f, {child.rotation.z}f, {child.rotation.w}f),\n";
                codeString += $"            scale = new Vector3({child.scale.x}f, {child.scale.y}f, {child.scale.z}f),\n";
                codeString += $"            mesh = \"{child.mesh}\",\n";

                if (child.boxCollider != null)
                {
                    codeString += "            boxCollider = new PrefabRebuilder.BoxColliderData\n            {\n";
                    codeString += $"                center = new Vector3({child.boxCollider.center.x}f, {child.boxCollider.center.y}f, {child.boxCollider.center.z}f),\n";
                    codeString += $"                size = new Vector3({child.boxCollider.size.x}f, {child.boxCollider.size.y}f, {child.boxCollider.size.z}f)\n";
                    codeString += "            },\n";
                }

                codeString += AddChildrenData(child.children);
                codeString += "        },\n";
            }
            codeString += "    }\n";
        }
        else
        {
            codeString += "    children = new List<PrefabRebuilder.ObjectData>()\n";
        }
        return codeString;
    }
}
