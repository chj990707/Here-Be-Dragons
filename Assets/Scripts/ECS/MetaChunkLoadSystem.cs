using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using UnityEngine.LowLevel;
using UnityEngine;

[BurstCompile]
public partial class MetaChunkLoadSystem : SystemBase
{
    private EntityQuery newRequests;
    private SceneSystem sceneSystem;
    MetaChunkLoaderComponent chunkLoaderComponent;
    private Entity[] subSceneEntity = new Entity[1000];
    private int delay = 0;

    [BurstCompile]
    protected override void OnCreate()
    {
    }
    [BurstCompile]
    protected override void OnStartRunning()
    {
        chunkLoaderComponent = SystemAPI.GetSingleton<MetaChunkLoaderComponent>();
        for(int i = 0; i < 1000; i++)
        {
            subSceneEntity[i] = LoadMetaChunk(chunkLoaderComponent.GUID, new float3(64 * (i / 100), 64 * (i / 10 % 10), 64 * (i % 10)));
        }
    }

    protected override void OnUpdate()
    {
        if (delay < 400)
        {
            delay++;
            return;
        }
        delay = 0;
        EntityQuery refreshquery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(MetaChunkComponent) },
            None = new ComponentType[] { typeof(RenderNeededComponent) }
        });
        NativeArray<Entity> refreshChunk = refreshquery.ToEntityArray(Allocator.Temp);
        if (refreshChunk.Length > 0)
        {
            EntityManager.AddComponent<RenderNeededComponent>(refreshChunk[0]);
        }
    }
    [BurstCompile]
    private Entity LoadMetaChunk(Unity.Entities.Hash128 hash, float3 offset)
    {
        SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { 
            Flags = SceneLoadFlags.NewInstance
        };
        Entity entityScene = SceneSystem.LoadSceneAsync(World.Unmanaged, hash, loadParameters);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.Persistent, PlaybackPolicy.MultiPlayback);
        MetaChunkOffset metaChunkOffset = new MetaChunkOffset() { offset = offset };
        Entity entity = ecb.CreateEntity();
        ecb.AddComponent(entity, metaChunkOffset);
        PostLoadCommandBuffer postLoadCommandBuffer = new PostLoadCommandBuffer()
        {
            CommandBuffer = ecb
        };
        EntityManager.AddComponentData(entityScene, postLoadCommandBuffer);
        return entityScene;
    }
    private void UnloadMetaChunk(SubScene subScene)
    {
        SceneSystem.UnloadScene(World.Unmanaged, subScene.SceneGUID);
    }
}
