using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class BlockAuthoring : MonoBehaviour
{
    public int value;

    private class BlockBaker : Baker<BlockAuthoring>
    {
        public override void Bake(BlockAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);  
            AddComponent(entity, new BlockComponent
            {
                value = authoring.value,
            });
        }
    }
}
public struct BlockComponent : IComponentData
{
    public int value;
}

public struct BlockElement : IBufferElementData
{
    public int value;
}