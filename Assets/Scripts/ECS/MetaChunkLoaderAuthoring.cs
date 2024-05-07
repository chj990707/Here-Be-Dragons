using Unity.Entities;
using Unity.Entities.Serialization;
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
            EntitySceneReference sceneReference = new EntitySceneReference(authoring.scene);
            AddComponent(entity, new MetaChunkLoaderComponent
            {
                scene = sceneReference,
            });
        }
    }
}
#endif

public struct MetaChunkLoaderComponent : IComponentData
{
    public EntitySceneReference scene;
}

public struct MetaChunkOffset : IComponentData
{
    public float3 offset;
}