using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class MetaChunkLoaderAuthoring : MonoBehaviour
{
    public SceneAsset scene;

    public class MetaChunkLoaderBaker : Baker<MetaChunkLoaderAuthoring>
    {
        public override void Bake(MetaChunkLoaderAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            string path = AssetDatabase.GetAssetPath(authoring.scene);
            Unity.Entities.Hash128 guid = AssetDatabase.GUIDFromAssetPath(path);
            AddComponent(entity, new MetaChunkLoaderComponent
            {
                GUID = guid
            });
        }
    }
}
#endif

public struct MetaChunkLoaderComponent : IComponentData
{
    public Unity.Entities.Hash128 GUID;
}

public struct MetaChunkOffset : IComponentData
{
    public float3 offset;
}