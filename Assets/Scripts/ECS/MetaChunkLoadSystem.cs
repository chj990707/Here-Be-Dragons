using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Scenes;
using System.Collections.Generic;
using Unity.Entities.Serialization;

[BurstCompile]
public partial class MetaChunkLoadSystem : SystemBase
{
    private EntityQuery newRequests;
    private SceneSystem sceneSystem;
    MetaChunkLoaderComponent chunkLoaderComponent;
    private List<Entity> subSceneEntity = new List<Entity>();
    public const int renderRadius = 5;
    private int loadedChunks = 0;
    private bool created = false;

    [BurstCompile]
    protected override void OnCreate()
    {
    }
    [BurstCompile]
    protected override void OnStartRunning()
    {
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        if (!created)
        {
            if (SystemAPI.TryGetSingleton(out chunkLoaderComponent))
            {
                for (int i = 0; i < renderRadius * renderRadius * renderRadius * 8; i++)
                {
                    subSceneEntity.Add(LoadMetaChunk(chunkLoaderComponent.scene, new float3(64 * (i / (renderRadius * 2) / (renderRadius * 2) - renderRadius), 64 * (i / (renderRadius * 2) % (renderRadius * 2) - renderRadius), 64 * (i % (renderRadius * 2) - renderRadius))));
                }
                created = true;
            }
        }
        for(int i = 0; i < subSceneEntity.Count; i++)
        {
            if (SceneSystem.IsSceneLoaded(World.Unmanaged, subSceneEntity[i]))
            {
                subSceneEntity.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
    }
    [BurstCompile]
    private Entity LoadMetaChunk(EntitySceneReference sceneReference, float3 offset)
    {
        SceneSystem.LoadParameters loadParameters = new SceneSystem.LoadParameters() { 
            Flags = SceneLoadFlags.NewInstance
        };
        Entity entityScene = SceneSystem.LoadSceneAsync(World.Unmanaged, sceneReference, loadParameters);
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

    public int getChunksLoaded()
    {
        if (!created) return 0;
        else return renderRadius * renderRadius * renderRadius * 8 - subSceneEntity.Count;
    }
}
