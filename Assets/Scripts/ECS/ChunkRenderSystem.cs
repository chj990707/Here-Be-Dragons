using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine.Rendering;
using Unity.Burst;

[BurstCompile]
public partial class ChunkMeshGenerateSystem : SystemBase
{
    private EntityQuery queryNeeded;
    private JobHandle innerMeshJobHandle;
    private JobHandle combineJobHandle;
    private Entity metaChunkEntity;
    private Material material;
    public NativeList<BlockElement> blocks;
    private NativeList<float3> verts;
    private NativeList<float2> uvs;
    private NativeList<int> tris;
    private NativeArray<bool> faces;

    public struct Face
    {
        public bool draw;
        public float3 vert1;
        public float3 vert2;
        public float3 vert3;
        public float3 vert4;
    }
    [BurstCompile]
    public struct ChunkInnerMeshJob : IJobParallelFor
    {
        [ReadOnly]
        public NativeList<BlockElement> blocks;
        [NativeDisableParallelForRestriction]
        public NativeArray<bool> faces;
        public void Execute(int index)
        {
            int3 pos = IndexToChunkPos(index % 512);
            int chunkStartIndex = index - index % 512;
            faces[index * 6    ] = blocks[index].value >= 0 && (pos.x < 1 || blocks[chunkStartIndex + PosToChunkIndex(pos.x - 1, pos.y, pos.z)].value < 0);
            faces[index * 6 + 1] = blocks[index].value >= 0 && (pos.x > 6 || blocks[chunkStartIndex + PosToChunkIndex(pos.x + 1, pos.y, pos.z)].value < 0);
            faces[index * 6 + 2] = blocks[index].value >= 0 && (pos.y < 1 || blocks[chunkStartIndex + PosToChunkIndex(pos.x, pos.y - 1, pos.z)].value < 0);
            faces[index * 6 + 3] = blocks[index].value >= 0 && (pos.y > 6 || blocks[chunkStartIndex + PosToChunkIndex(pos.x, pos.y + 1, pos.z)].value < 0);
            faces[index * 6 + 4] = blocks[index].value >= 0 && (pos.z < 1 || blocks[chunkStartIndex + PosToChunkIndex(pos.x, pos.y, pos.z - 1)].value < 0);
            faces[index * 6 + 5] = blocks[index].value >= 0 && (pos.z > 6 || blocks[chunkStartIndex + PosToChunkIndex(pos.x, pos.y, pos.z + 1)].value < 0);
        }
    }
    [BurstCompile]
    public struct ChunkMeshCombineJob : IJob
    {
        public NativeArray<bool> faces;
        public NativeList<float3> verts;
        public NativeList<float2> uvs;
        public NativeList<int> tris;
        [BurstCompile]
        public void Execute()
        {
            for (int i = 0; i < faces.Length; i++)
            {
                int3 metaChunkPos = IndexToMetaChunkPos(i / 6);
                if(i % 6 == 1 && metaChunkPos.x % 8 == 7 && metaChunkPos.x < 63)
                {
                    if (faces[i] && faces[MetaChunkPosToIndex(metaChunkPos.x + 1, metaChunkPos.y, metaChunkPos.z) * 6])
                    {
                        faces[i] = false;
                        faces[MetaChunkPosToIndex(metaChunkPos.x + 1, metaChunkPos.y, metaChunkPos.z) * 6] = false;
                    }
                }
                if (i % 6 == 3 && metaChunkPos.y % 8 == 7 && metaChunkPos.y < 63)
                {
                    if (faces[i] && faces[MetaChunkPosToIndex(metaChunkPos.x, metaChunkPos.y + 1, metaChunkPos.z) * 6 + 2])
                    {
                        faces[i] = false;
                        faces[MetaChunkPosToIndex(metaChunkPos.x, metaChunkPos.y + 1, metaChunkPos.z) * 6 + 2] = false;
                    }
                }
                if (i % 6 == 5 && metaChunkPos.z % 8 == 7 && metaChunkPos.z < 63)
                {
                    if (faces[i] && faces[MetaChunkPosToIndex(metaChunkPos.x, metaChunkPos.y, metaChunkPos.z + 1) * 6 + 4])
                    {
                        faces[i] = false;
                        faces[MetaChunkPosToIndex(metaChunkPos.x, metaChunkPos.y, metaChunkPos.z + 1) * 6 + 4] = false;
                    }
                }
            }
            for(int i = 0; i < faces.Length; i++)
            {
                if (faces[i])
                {
                    DrawFace(i);
                }
            }
        }
        [BurstCompile]
        private void DrawFace(int index)
        {
            int3 blockOrigin = IndexToMetaChunkPos(index / 6);
            switch (index % 6)
            {
                case 0:
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y    , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y    , blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y + 1, blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y + 1, blockOrigin.z));
                    break;
                case 1:
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y    , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y + 1, blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y + 1, blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y    , blockOrigin.z + 1));
                    break;
                case 2:
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y    , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y    , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y    , blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y    , blockOrigin.z + 1));
                    break;
                case 3:
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y + 1, blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y + 1, blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y + 1, blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y + 1, blockOrigin.z));
                    break;
                case 4:
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y    , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y    , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y + 1, blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y + 1, blockOrigin.z));
                    break;
                case 5:
                    verts.Add(new float3(blockOrigin.x + 1, blockOrigin.y    , blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x  + 1, blockOrigin.y + 1, blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y + 1, blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x    , blockOrigin.y    , blockOrigin.z + 1));
                    break;
                default:
                    Debug.Log("This shouldn't happen!");
                    break;
            }
            uvs.Add(new float2(0, 0));
            uvs.Add(new float2(0, 1));
            uvs.Add(new float2(1, 1));
            uvs.Add(new float2(1, 0));
            int vertCount = verts.Length;
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 1);
        }
    }
    private static int PosToChunkIndex(int x, int y, int z)
    {
        return x * 64 + y * 8 + z;
    }

    private static int MetaChunkPosToIndex(int x, int y, int z)
    {
        int chunkIndex = PosToChunkIndex(x / 8, y / 8, z / 8) * 512;
        int blockIndex = PosToChunkIndex(x % 8, y % 8, z % 8);
        return chunkIndex + blockIndex;
    }

    private static int3 IndexToMetaChunkPos(int index)
    {
        int3 chunkPos = IndexToChunkPos(index / 512) * 8;
        int3 blockPos = IndexToChunkPos(index % 512);
        return chunkPos + blockPos;
    }

    private static int3 IndexToChunkPos(int index)
    {
        return new int3(index / 64, index / 8 % 8, index % 8);
    }

    [BurstCompile]
    protected override void OnCreate()
    {
        queryNeeded = GetEntityQuery(typeof(RenderNeededComponent));
        blocks = new NativeList<BlockElement>(512 * 512, Allocator.Persistent);
        verts = new NativeList<float3>(6 * 4 * 512 * 512, Allocator.Persistent);
        uvs = new NativeList<float2>(6 * 4 * 512 * 512, Allocator.Persistent);
        tris = new NativeList<int>(6 * 6 * 512 * 512, Allocator.Persistent);
        faces = new NativeArray<bool>(6 * 512 * 512, Allocator.Persistent);
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        EntitiesGraphicsSystem egs = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
        if(combineJobHandle.IsCompleted)
        {
            if (EntityManager.Exists(metaChunkEntity))
            {
                innerMeshJobHandle.Complete();
                combineJobHandle.Complete();
                if (verts.Length > 0)
                {
                    Mesh mesh = new Mesh();
                    mesh.indexFormat = IndexFormat.UInt32;
                    mesh.SetVertices(verts.ToArray(Allocator.Temp));
                    mesh.SetUVs(1, uvs.ToArray(Allocator.Temp));
                    mesh.SetTriangles(tris.ToArray(Allocator.Temp).ToArray(), 0);
                    mesh.RecalculateNormals();
                    mesh.RecalculateBounds();
                    BatchMeshID meshID = egs.RegisterMesh(mesh);
                    BatchMaterialID materialID = egs.RegisterMaterial(material);
                    EntityManager.SetComponentData(metaChunkEntity, new MaterialMeshInfo { MeshID = meshID, MaterialID = materialID });
                    EntityManager.SetComponentData(metaChunkEntity, new RenderBounds { Value = new AABB { Center = new float3(32, 32, 32), Extents = new float3(64, 64, 64) } });
                }
                else
                {
                    EntityManager.SetComponentEnabled<MaterialMeshInfo>(metaChunkEntity, false);
                }
                verts.Clear();
                uvs.Clear();
                tris.Clear();
            }
        }
        else
        {
            return;
        }
        NativeArray<Entity> entitiesNeeded = queryNeeded.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < entitiesNeeded.Length && i < 1; i++)
        {
            metaChunkEntity = entitiesNeeded[i];
            MetaChunkComponent metaChunkComponent = EntityManager.GetComponentData<MetaChunkComponent>(entitiesNeeded[i]);
            material = metaChunkComponent.material;
            blocks.Clear();
            NativeArray<ChunkElement> chunkElements = EntityManager.GetBuffer<ChunkElement>(entitiesNeeded[i]).ToNativeArray(Allocator.Temp);
            for (int j = 0; j < chunkElements.Length; j++)
            {
                DynamicBuffer<BlockElement> blockElements = EntityManager.GetBuffer<BlockElement>(chunkElements[j].chunkEntity);
                NativeArray<BlockElement> chunkBlockArray = blockElements.ToNativeArray(Allocator.Temp);
                blocks.AddRange(chunkBlockArray);
            }
            ChunkInnerMeshJob innerMeshJob = new ChunkInnerMeshJob { blocks = blocks, faces = faces };
            innerMeshJobHandle = innerMeshJob.Schedule(512 * 512, 32);
            ChunkMeshCombineJob combineJob = new ChunkMeshCombineJob
            {
                faces = faces,
                verts = verts,
                uvs = uvs,
                tris = tris,
            };
            combineJobHandle = combineJob.Schedule(innerMeshJobHandle); 
            EntityManager.RemoveComponent<RenderNeededComponent>(entitiesNeeded[i]);
        }
    }
    [BurstCompile]
    protected override void OnDestroy()
    {
        innerMeshJobHandle.Complete();
        combineJobHandle.Complete();
        verts.Dispose();
        uvs.Dispose();
        tris.Dispose();
        blocks.Dispose();
        faces.Dispose();
    }
}

public struct RenderNeededComponent : IComponentData
{

}

public struct ChunkDraw
{
    public Material material;
    public Entity metaChunkEntity;
    public JobHandle innerMeshJobHandle;
    public JobHandle combineJobHandle;
    public ChunkMeshGenerateSystem.ChunkMeshCombineJob combinejob;
    public ChunkMeshGenerateSystem.ChunkInnerMeshJob innerMeshJob;
}
public struct RepositionComponent : IComponentData
{
    public float x;
    public float y;
    public float z;
}
