using Unity.Collections;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine;

public class MetaChunkAuthoring : MonoBehaviour
{
    public GameObject chunk;
    public GameObject block;
    public Material material;
    private class MetaChunkBaker : Baker<MetaChunkAuthoring>
    {
        public override void Bake(MetaChunkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            MetaChunkComponent component = new MetaChunkComponent
            {
                chunkEntity = GetEntity(authoring.chunk, TransformUsageFlags.Dynamic),
                blockEntity = GetEntity(authoring.block, TransformUsageFlags.Dynamic),
                material = authoring.material,
            };
            AddComponentObject(entity, component);
        }
    }
}

public class MetaChunkComponent : IComponentData
{
    public Entity chunkEntity;
    public Entity blockEntity;
    public Material material;
}

[InternalBufferCapacity(512)]
public struct ChunkElement : IBufferElementData
{
    public Entity chunkEntity;
}