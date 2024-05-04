using UnityEngine;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Scenes;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System;
using Unity.Jobs;
using Unity.Rendering;

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ProcessAfterLoad)]
[UpdateInGroup(typeof(ProcessAfterLoadGroup))]
public partial class ChunkOffsetSystem : SystemBase
{
    public Entity chunkTemplate;
    private EntityQuery queryOffset;

    [BurstCompile]
    protected override void OnCreate()
    {
        RequireForUpdate<MetaChunkOffset>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        queryOffset = GetEntityQuery(ComponentType.ReadWrite<MetaChunkOffset>());
        NativeArray<MetaChunkOffset> offsets = queryOffset.ToComponentDataArray<MetaChunkOffset>(Allocator.TempJob);
        foreach (MetaChunkOffset offset in offsets)
        {
            foreach (var transform in SystemAPI.Query<RefRW<LocalTransform>>())
            {
                transform.ValueRW.Position += offset.offset;
            }
        }
        offsets.Dispose();
        EntityManager.DestroyEntity(queryOffset);

        EntityQuery query = GetEntityQuery(typeof(MetaChunkComponent));
        Entity metachunkEntity = query.ToEntityArray(Allocator.Temp)[0];
        float3 metaChunkOffset = EntityManager.GetComponentData<LocalTransform>(metachunkEntity).Position;
        MetaChunkComponent metaChunkComponent = EntityManager.GetComponentObject<MetaChunkComponent>(metachunkEntity);
        NativeArray<Entity> chunkEntities = EntityManager.Instantiate(metaChunkComponent.chunkEntity, 512, Allocator.Temp);
        EntityManager.AddBuffer<ChunkElement>(metachunkEntity);
        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
            {
                for (int k = 0; k < 8; k++)
                {
                    ChunkElement chunkComponent = new ChunkElement { chunkEntity = chunkEntities[toIndex(i, j, k)] };
                    LocalTransform transform = new LocalTransform { Position = new float3(metaChunkOffset.x + i * 8, metaChunkOffset.y + j * 8, metaChunkOffset.z + k * 8), Rotation = Quaternion.identity, Scale = 1f};
                    EntityManager.GetBuffer<ChunkElement>(metachunkEntity).Add(chunkComponent);
                    EntityManager.SetComponentData(chunkEntities[toIndex(i, j, k)], transform);
                    EntityManager.AddBuffer<BlockElement>(chunkEntities[toIndex(i, j, k)]);
                    for (int i2 = 0; i2 < 8; i2++) 
                    {
                        for(int j2 = 0; j2 < 8; j2++)
                        {
                            for(int k2 = 0; k2 < 8; k2++)
                            {
                                EntityManager.GetBuffer<BlockElement>(chunkEntities[toIndex(i, j, k)]).Add(new BlockElement { value = (int)(Mathf.PerlinNoise((metaChunkOffset.x + i * 8 + i2) / 100f, (metaChunkOffset.z + k * 8 + k2) / 100f) * 100 - metaChunkOffset.y - j * 8 - j2 + 200) });
                            }
                        }
                    }
                }
            }
        }
        EntityManager.AddComponentData(metachunkEntity, new RenderNeededComponent { });
    }

    private int toIndex(int x, int y, int z)
    {
        return x * 64 + y * 8 + z;
    }
}