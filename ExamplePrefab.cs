using UnityEngine;
using UWE;
using Nautilus.Assets;
using System.Collections;
using System.Collections.Generic;

using Nautilus.Extensions;

namespace ExportRebuilder;

public class RebuiltPrefab : MonoBehaviour
{
    private static PrefabInfo Info { get; } = PrefabInfo.WithTechType("RebuiltPrefab", "RebuiltPrefab", "??")
        .WithIcon(SpriteManager.Get(TechType.Seamoth));
    
    private static IEnumerator LoadGameObjectAsync(IOut<GameObject> output)
    {
        var prefabIds = new List<string>
        {
            "04ad6244-1766-4622-bb8a-7fa29845bc68", // Pillar
            "b1f54987-4652-4f62-a983-4bf3029f8c5b", // Control Terminal
            "9a5ff289-4b82-4b9c-b9f6-d60f46dd2d7a",  // Teleporter
            "3d625dbb-d15a-4351-bca0-0a0526f01e6e", //"WorldEntities/Environment/Precursor/Gun/Precursor_Gun_ControlRoom_CentralColumn.prefab",
            "3c5abaf7-b18e-4835-8282-874763343d57" //"WorldEntities/Doodads/Precursor/Prison/Relics/Alien_relic_06.prefab",
        };

        Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        
        foreach (var id in prefabIds)
        {
            var prefabRequest = PrefabDatabase.GetPrefabAsync(id);
            yield return prefabRequest;

            if (!prefabRequest.TryGetPrefab(out var prefabObject))
            {
                yield break; // Exit the coroutine if any prefab fails
            }
            prefabs.Add(id, prefabObject);
        }
        
        GameObject obj = PrefabRebuilder.Build(prefabs,PrefabExport.Data);
        output.Set(obj);
    }

    public static void Register()
    {
        CustomPrefab prefab = new CustomPrefab(Info);

        // Pass the instance method as a delegate
        prefab.SetGameObject(LoadGameObjectAsync);
        prefab.Register();
    }
}
