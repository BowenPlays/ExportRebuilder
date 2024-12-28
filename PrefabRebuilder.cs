using System;
using System.Collections.Generic;
using UnityEngine;

namespace ExportRebuilder;

public static class PrefabRebuilder
{
    [Serializable]
    public class BoxColliderData
    {
        public Vector3 center;
        public Vector3 size;
    }

    [Serializable]
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
    
    public static GameObject FindAndInstantiateByName(string prefabId, GameObject prefabObject, string meshName)
    {
        Transform objectTransform = FindObjectInHierarchy(prefabObject.transform, meshName);

        if (objectTransform != null)
        {
            GameObject instantiatedObject = GameObject.Instantiate(objectTransform.gameObject, prefabObject.transform.position, prefabObject.transform.rotation);
            instantiatedObject.transform.localScale = objectTransform.localScale; 
            return instantiatedObject;
        }
        else
        {
            Debug.LogError($"mesh with name '{meshName}' not found in the hierarchy.");
            return null;
        }
    }
    
    private static Transform FindObjectInHierarchy(Transform parentTransform, string meshName)
    {
        MeshFilter meshFilter = parentTransform.GetComponent<MeshFilter>();
        SkinnedMeshRenderer skinnedMeshRenderer = parentTransform.GetComponent<SkinnedMeshRenderer>();

        if ((meshFilter != null && meshFilter.sharedMesh != null && meshFilter.sharedMesh.name == meshName) ||
            (skinnedMeshRenderer != null && skinnedMeshRenderer.sharedMesh != null && skinnedMeshRenderer.sharedMesh.name == meshName))
        {
            return parentTransform;
        }
        foreach (Transform child in parentTransform)
        {
            Transform foundObject = FindObjectInHierarchy(child, meshName);
            if (foundObject != null)
            {
                return foundObject;
            }
        }

        return null; 
    }

    public static GameObject Build( Dictionary<string, GameObject> prefabs,ObjectData data)
    {
        Debug.Log("Rebuilding GameObject hierarchy...");    
        return RebuildHierarchy(data, null,prefabs);
    }

    private static GameObject RebuildHierarchy(ObjectData data, Transform parent, Dictionary<string, GameObject> prefabs)
    {
        GameObject obj = new GameObject(data.name);
        if (!string.IsNullOrEmpty(data.mesh))
        {
            foreach (var prefab in prefabs)
            {
                if (prefab.Value == null)
                {
                    return null;
                }
                obj = FindAndInstantiateByName(prefab.Key,prefab.Value, data.mesh);
                if (obj != null)
                {
                    obj.name = data.name;
                    break;
                }
            }
        }

        if (obj == null)
        {
            obj = new GameObject(data.name);
        }
        
        if (parent != null)
        {
            obj.transform.SetParent(parent);
        }
        
        obj.transform.localPosition = data.position;
        obj.transform.localRotation = data.rotation;
        obj.transform.localScale = data.scale;
        
        if (data.boxCollider != null)
        {
            BoxCollider boxCollider = obj.AddComponent<BoxCollider>();
            boxCollider.center = data.boxCollider.center;
            boxCollider.size = data.boxCollider.size;
        }
        
        foreach (ObjectData childData in data.children)
        {
            RebuildHierarchy(childData, obj.transform,prefabs);
        }

        return obj;
    }
}
