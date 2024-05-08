using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine.Rendering;
using Unity.Burst;
using Unity.Physics;
using Unity.VisualScripting;

[BurstCompile]
[UpdateInGroup(typeof(PresentationSystemGroup))]
public partial class ChunkMeshGenerateSystem : SystemBase
{
    private EntityQuery queryNeeded;
    private JobHandle innerMeshJobHandle;
    private JobHandle combineJobHandle;
    private Entity metaChunkEntity;
    private UnityEngine.Material material;
    public NativeList<BlockElement> blocks;
    private NativeList<float3> verts;
    private NativeList<float2> uvs;
    private NativeList<int> tris;
    private NativeList<int3> physicsTris;
    private NativeArray<bool> faces;
    private NativeArray<int2> faceSize;
    private NativeArray<BlobAssetReference<Unity.Physics.Collider>> colliders;
    private int renderedChunks = 0;

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
        public NativeArray<int2> faceSize;
        public NativeList<float3> verts;
        public NativeList<float2> uvs;
        public NativeList<int> tris;
        public NativeList<int3> physicsTris;
        public NativeArray<BlobAssetReference<Unity.Physics.Collider>> colliders;
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
                if (!faces[i])
                {
                    faceSize[i] = new int2(0, 0);
                }
                else
                {
                    faceSize[i] = new int2(1, 1);
                }
            }
            for(int chunk = 0; chunk < 512; chunk++)
            {
                for(int block = 0; block < 512; block++)
                {
                    for(int face = 0; face < 6; face++)
                    {
                        if (faceSize[chunk * 512 * 6 + block * 6 + face].x <= 0) continue;
                        int3 blockPos = IndexToChunkPos(block);
                        int x, y, z;
                        switch (face)
                        {
                            case 0:
                            case 1:
                                for (y = 0; y + blockPos.y < 8; y++)
                                {
                                    int2 nextBlock = faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x, blockPos.y + y, blockPos.z) * 6 + face];
                                    if (nextBlock.x != 1 || nextBlock.y != 1) break;
                                }
                                for (z = 1; z + blockPos.z < 8; z++)
                                {
                                    int y2;
                                    for (y2 = 0; y2 < y; y2++)
                                    {
                                        int2 nextBlock = faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x, blockPos.y + y2, blockPos.z + z) * 6 + face];
                                        if (nextBlock.x != 1 || nextBlock.y != 1) break;
                                    }
                                    if (y2 < y) break;
                                }
                                for (int y2 = 0; y2 < y; y2++)
                                {
                                    for(int z2 = 0; z2 < z; z2++)
                                    {
                                        faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x, blockPos.y + y2, blockPos.z + z2) * 6 + face] = new int2(0,  0);
                                    }
                                }
                                faceSize[chunk * 512 * 6 + block * 6 + face] = new int2(y, z);
                                break;
                            case 2:
                            case 3:
                                for (x = 0; x + blockPos.x < 8; x++)
                                {
                                    int2 nextBlock = faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x + x, blockPos.y, blockPos.z) * 6 + face];
                                    if (nextBlock.x != 1 || nextBlock.y != 1) break;
                                }
                                for (z = 1; z + blockPos.z < 8; z++)
                                {
                                    int x2;
                                    for (x2 = 0; x2 < x; x2++)
                                    {
                                        int2 nextBlock = faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x + x2, blockPos.y, blockPos.z + z) * 6 + face];
                                        if (nextBlock.x != 1 || nextBlock.y != 1) break;
                                    }
                                    if (x2 < x) break;
                                }
                                for (int x2 = 0; x2 < x; x2++)
                                {
                                    for (int z2 = 0; z2 < z; z2++)
                                    {
                                        faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x + x2, blockPos.y, blockPos.z + z2) * 6 + face] = new int2(0, 0);
                                    }
                                }
                                faceSize[chunk * 512 * 6 + block * 6 + face] = new int2(x, z);
                                break;
                            case 4:
                            case 5:
                                for (x = 0; x + blockPos.x < 8; x++)
                                {
                                    int2 nextBlock = faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x + x, blockPos.y, blockPos.z) * 6 + face];
                                    if (nextBlock.x != 1 || nextBlock.y != 1) break;
                                }
                                for (y = 1; y + blockPos.y < 8; y++)
                                {
                                    int x2;
                                    for (x2 = 0; x2 < x; x2++)
                                    {
                                        int2 nextBlock = faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x + x2, blockPos.y + y, blockPos.z) * 6 + face];
                                        if (nextBlock.x != 1 || nextBlock.y != 1) break;
                                    }
                                    if (x2 < x) break;
                                }
                                for (int x2 = 0; x2 < x; x2++)
                                {
                                    for (int y2 = 0; y2 < y; y2++)
                                    {
                                        faceSize[chunk * 512 * 6 + PosToChunkIndex(blockPos.x + x2, blockPos.y + y2, blockPos.z) * 6 + face] = new int2(0, 0);
                                    }
                                }
                                faceSize[chunk * 512 * 6 + block * 6 + face] = new int2(x, y);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            for(int i = 0; i < faceSize.Length; i++)
            {
                if (faceSize[i].x > 0 && faceSize[i].y > 0)
                {
                    DrawFace(i, faceSize[i]);
                }
            }
            colliders[0] = Unity.Physics.MeshCollider.Create(verts.ToArray(Allocator.Temp), physicsTris.ToArray(Allocator.Temp));
    }
        [BurstCompile]
        private void DrawFace(int index, int2 size)
        {
            int3 blockOrigin = IndexToMetaChunkPos(index / 6);
            switch (index % 6)
            {
                case 0:
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y         , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y         , blockOrigin.z + size.y));
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y + size.x, blockOrigin.z + size.y));
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y + size.x, blockOrigin.z));
                    break;
                case 1:
                    verts.Add(new float3(blockOrigin.x + 1     , blockOrigin.y         , blockOrigin.z + size.y));
                    verts.Add(new float3(blockOrigin.x + 1     , blockOrigin.y         , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + 1     , blockOrigin.y + size.x, blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + 1     , blockOrigin.y + size.x, blockOrigin.z + size.y));
                    break;
                case 2:
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y         , blockOrigin.z + size.y));
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y         , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + size.x, blockOrigin.y         , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + size.x, blockOrigin.y         , blockOrigin.z + size.y));
                    break;
                case 3:
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y + 1     , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y + 1     , blockOrigin.z + size.y));
                    verts.Add(new float3(blockOrigin.x + size.x, blockOrigin.y + 1     , blockOrigin.z + size.y));
                    verts.Add(new float3(blockOrigin.x + size.x, blockOrigin.y + 1     , blockOrigin.z));
                    break;
                case 4:
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y         , blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y + size.y, blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + size.x, blockOrigin.y + size.y, blockOrigin.z));
                    verts.Add(new float3(blockOrigin.x + size.x, blockOrigin.y         , blockOrigin.z));
                    break;
                case 5:
                    verts.Add(new float3(blockOrigin.x + size.x, blockOrigin.y         , blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x + size.x, blockOrigin.y + size.y, blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y + size.y, blockOrigin.z + 1));
                    verts.Add(new float3(blockOrigin.x         , blockOrigin.y         , blockOrigin.z + 1));
                    break;
                default:
                    Debug.Log("This shouldn't happen!");
                    break;
            }
            uvs.Add(new float2(0, 0));
            uvs.Add(new float2(0, size.y));
            uvs.Add(new float2(size.x, size.y));
            uvs.Add(new float2(size.x, 0));
            int vertCount = verts.Length;
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 1);
            physicsTris.Add(new int3(vertCount - 4, vertCount - 3, vertCount - 2));
            physicsTris.Add(new int3(vertCount - 4, vertCount - 2, vertCount - 1));
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
        physicsTris = new NativeList<int3>(12 * 512 * 512, Allocator.Persistent);
        faces = new NativeArray<bool>(6 * 512 * 512, Allocator.Persistent);
        faceSize = new NativeArray<int2>(6 * 512 * 512, Allocator.Persistent);
        colliders =new NativeArray<BlobAssetReference<Unity.Physics.Collider>>(1, Allocator.Persistent);
}

    [BurstCompile]
    protected override void OnUpdate()
    {
        EntitiesGraphicsSystem egs = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
        if (!combineJobHandle.IsCompleted) return;
        if (EntityManager.Exists(metaChunkEntity))
        {
            innerMeshJobHandle.Complete();
            combineJobHandle.Complete();
            if (verts.Length > 0)
            {
                Mesh mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(verts.ToArray(Allocator.Temp));
                mesh.SetUVs(0, uvs.ToArray(Allocator.Temp));
                mesh.SetTriangles(tris.ToArray(Allocator.Temp).ToArray(), 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                BatchMeshID meshID = egs.RegisterMesh(mesh);
                BatchMaterialID materialID = egs.RegisterMaterial(material);
                EntityManager.SetComponentData(metaChunkEntity, new MaterialMeshInfo { MeshID = meshID, MaterialID = materialID });
                EntityManager.SetComponentData(metaChunkEntity, new RenderBounds { Value = new AABB { Center = new float3(32, 32, 32), Extents = new float3(64, 64, 64) } });
                EntityManager.RemoveComponent<PhysicsCollider>(metaChunkEntity);
                EntityManager.AddComponentData(metaChunkEntity, new PhysicsCollider { Value = colliders[0] });
            }
            else
            {
                EntityManager.RemoveComponent<PhysicsCollider>(metaChunkEntity);
                EntityManager.SetComponentEnabled<MaterialMeshInfo>(metaChunkEntity, false);
            }
            renderedChunks++;
            verts.Clear();
            uvs.Clear();
            tris.Clear();
            physicsTris.Clear();
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
                faceSize = faceSize,
                verts = verts,
                uvs = uvs,
                tris = tris,
                physicsTris = physicsTris,
                colliders = colliders,
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
        faceSize.Dispose();
        physicsTris.Dispose();
        colliders.Dispose();
    }

    public int getRenderedNum()
    {
        return renderedChunks;
    }
}

public struct RenderNeededComponent : IComponentData
{

}

public struct ChunkDraw
{
    public UnityEngine.Material material;
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
