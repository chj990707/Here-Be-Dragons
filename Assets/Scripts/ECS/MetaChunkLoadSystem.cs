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
    private int renderDiameter = 10;

    [BurstCompile]
    protected override void OnCreate()
    {
    }
    [BurstCompile]
    protected override void OnStartRunning()
    {
        chunkLoaderComponent = SystemAPI.GetSingleton<MetaChunkLoaderComponent>();
        for(int i = 0; i < subSceneEntity.Length; i++)
        {
            subSceneEntity[i] = LoadMetaChunk(chunkLoaderComponent.GUID, new float3(64 * (i / renderDiameter / renderDiameter), 64 * (i / renderDiameter % renderDiameter), 64 * (i % renderDiameter)));
        }
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        /*
        if (delay < 1000)
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
        for(int i = 0; i < refreshChunk.Length && i < 10; i++)
        {
            EntityManager.AddComponent<RenderNeededComponent>(refreshChunk[i]);
        }
        */
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
    [BurstCompile]
    private void UnloadMetaChunk(SubScene subScene)
    {
        SceneSystem.UnloadScene(World.Unmanaged, subScene.SceneGUID);
    }
}
