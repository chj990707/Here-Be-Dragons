using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class ChunkAuthoring : MonoBehaviour
{
    private class ChunkBaker : Baker<MetaChunkAuthoring>
    {
        public override void Bake(MetaChunkAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
        }
    }
}
