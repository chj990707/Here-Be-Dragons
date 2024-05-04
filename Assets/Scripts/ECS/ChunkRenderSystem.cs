using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Rendering;
using UnityEngine.Rendering;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities.UniversalDelegates;

[BurstCompile]
public partial class ChunkMeshGenerateSystem : SystemBase
{
    private EntityQuery queryNeeded;
    private List<ChunkDraw> drawList;

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
        public NativeArray<BlockElement> blocks;
        [NativeDisableParallelForRestriction]
        public NativeArray<Face> faces;
        public void Execute(int index)
        {
            if (blocks[index].value < 0) return;
            int3 pos = IndexToPos(index % 512);
            int3 chunkPos = IndexToPos(index / 512);
            int chunkStartIndex = index - index % 512;
            if (pos.x < 1 || blocks[chunkStartIndex + PosToIndex(pos.x - 1, pos.y, pos.z)].value < 0)
            {
                faces[index * 6] = new Face
                {
                    draw = true,
                    vert1 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z),
                    vert2 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z + 1),
                    vert3 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z + 1),
                    vert4 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z),
                };
            }
            if (pos.x > 6 || blocks[chunkStartIndex + PosToIndex(pos.x + 1, pos.y, pos.z)].value < 0)
            {
                faces[index * 6 + 1] = new Face
                {
                    draw = true,
                    vert1 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z),
                    vert2 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z),
                    vert3 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z + 1),
                    vert4 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z + 1),
                };
            }
            if (pos.y < 1 || blocks[chunkStartIndex + PosToIndex(pos.x, pos.y - 1, pos.z)].value < 0)
            {
                faces[index * 6 + 2] = new Face
                {
                    draw = true,
                    vert1 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z),
                    vert2 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z),
                    vert3 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z + 1),
                    vert4 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z + 1),
                };
            }
            if (pos.y > 6 || blocks[chunkStartIndex + PosToIndex(pos.x, pos.y + 1, pos.z)].value < 0)
            {
                faces[index * 6 + 3] = new Face
                {
                    draw = true,
                    vert1 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z),
                    vert2 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z + 1),
                    vert3 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z + 1),
                    vert4 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z),
                };
            }
            if (pos.z < 1 || blocks[chunkStartIndex + PosToIndex(pos.x, pos.y, pos.z - 1)].value < 0)
            {
                faces[index * 6 + 4] = new Face
                {
                    draw = true,
                    vert1 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z),
                    vert2 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z),
                    vert3 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z),
                    vert4 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z),
                };
            }
            if (pos.z > 6 || blocks[chunkStartIndex + PosToIndex(pos.x, pos.y, pos.z + 1)].value < 0)
            {
                faces[index * 6 + 5] = new Face
                {
                    draw = true,
                    vert1 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z + 1),
                    vert2 = new float3(chunkPos.x * 8 + pos.x + 1, chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z + 1),
                    vert3 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y + 1, chunkPos.z * 8 + pos.z + 1),
                    vert4 = new float3(chunkPos.x * 8 + pos.x    , chunkPos.y * 8 + pos.y    , chunkPos.z * 8 + pos.z + 1),
                };
            }
        }
    }
    public struct ChunkMeshCombineJob : IJob
    {
        public NativeArray<Face> faces;
        public NativeList<float3> verts;
        public NativeList<float2> uvs;
        public NativeList<int> tris;
        public void Execute()
        {
            for (int i = 0; i < faces.Length; i++)
            {
                int3 chunkPos = IndexToPos(i / 6 / 512);
                int3 blockPos = IndexToPos(i / 6 % 512);
                if (i % 6 == 0 && blockPos.x == 0 && chunkPos.x != 0)
                {
                    int block_x_minus = (PosToIndex(chunkPos.x - 1, chunkPos.y, chunkPos.z) * 512 + PosToIndex(7, blockPos.y, blockPos.z)) * 6 + 1;
                    if (faces[block_x_minus].draw && faces[i].draw)
                    {
                        faces[block_x_minus] = new Face { draw = false };
                        faces[i] = new Face { draw = false };
                    }
                }
                if (i % 6 == 2 && blockPos.y == 0 && chunkPos.y != 0)
                {
                    int block_y_minus = (PosToIndex(chunkPos.x, chunkPos.y - 1, chunkPos.z) * 512 + PosToIndex(blockPos.x, 7, blockPos.z)) * 6 + 3;
                    if (faces[block_y_minus].draw && faces[i].draw)
                    {
                        faces[block_y_minus] = new Face { draw = false };
                        faces[i] = new Face { draw = false };
                    }
                }
                if (i % 6 == 4 && blockPos.z == 0 && chunkPos.z != 0)
                {
                    int block_z_minus = (PosToIndex(chunkPos.x, chunkPos.y, chunkPos.z - 1) * 512 + PosToIndex(blockPos.x, blockPos.y, 7)) * 6 + 5;
                    if (faces[block_z_minus].draw && faces[i].draw)
                    {
                        faces[block_z_minus] = new Face { draw = false };
                        faces[i] = new Face { draw = false };
                    }
                }
                if (i % 6 == 1 && blockPos.x == 7 && chunkPos.x != 7)
                {
                    int block_x_plus = (PosToIndex(chunkPos.x + 1, chunkPos.y, chunkPos.z) * 512 + PosToIndex(0, blockPos.y, blockPos.z)) * 6;
                    if (faces[block_x_plus].draw && faces[i].draw)
                    {
                        faces[block_x_plus] = new Face { draw = false };
                        faces[i] = new Face { draw = false };
                    }
                }
                if (i % 6 == 3 && blockPos.y == 7 && chunkPos.y != 7)
                {
                    int block_y_plus = (PosToIndex(chunkPos.x, chunkPos.y + 1, chunkPos.z) * 512 + PosToIndex(blockPos.x, 0, blockPos.z)) * 6 + 2;
                    if (faces[block_y_plus].draw && faces[i].draw)
                    {
                        faces[block_y_plus] = new Face { draw = false };
                        faces[i] = new Face { draw = false };
                    }
                }
                if (i % 6 == 5 && blockPos.z == 7 && chunkPos.z != 7)
                {
                    int block_z_plus = (PosToIndex(chunkPos.x, chunkPos.y, chunkPos.z + 1) * 512 + PosToIndex(blockPos.x, blockPos.y, 0)) * 6 + 4;
                    if (faces[block_z_plus].draw && faces[i].draw)
                    {
                        faces[block_z_plus] = new Face { draw = false };
                        faces[i] = new Face { draw = false };
                    }
                }
                if (faces[i].draw)
                {
                    DrawFace(faces[i]);
                }
            }
        }
        private void DrawFace(Face face)
        {
            verts.Add(face.vert1);
            verts.Add(face.vert2);
            verts.Add(face.vert3);
            verts.Add(face.vert4);
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
    private static int PosToIndex(int x, int y, int z)
    {
        return x * 64 + y * 8 + z;
    }
    private static int3 IndexToPos(int index)
    {
        return new int3(index / 64, index / 8 % 8, index % 8);
    }

    [BurstCompile]
    protected override void OnCreate()
    {
        queryNeeded = GetEntityQuery(typeof(RenderNeededComponent));
        drawList = new List<ChunkDraw>();
    }

    [BurstCompile]
    protected override void OnUpdate()
    {
        EntitiesGraphicsSystem egs = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
        for(int i = 0; i < drawList.Count;)
        {
            if (!drawList[i].combineJobHandle.IsCompleted)
            { 
                i++;
                continue;
            }
            drawList[i].innerMeshJobHandle.Complete();
            drawList[i].innerMeshJob.blocks.Dispose();
            JobHandle combineJobHandle = drawList[i].combineJobHandle;
            ChunkMeshCombineJob combineJob = drawList[i].combinejob;
            combineJobHandle.Complete();
            if (combineJob.verts.Length > 0)
            {
                Mesh mesh = new Mesh();
                mesh.indexFormat = IndexFormat.UInt32;
                mesh.SetVertices(combineJob.verts.ToArray(Allocator.Temp));
                mesh.SetUVs(1, combineJob.uvs.ToArray(Allocator.Temp));
                mesh.SetTriangles(combineJob.tris.ToArray(Allocator.Temp).ToArray(), 0);
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                BatchMeshID meshID = egs.RegisterMesh(mesh);
                BatchMaterialID materialID = egs.RegisterMaterial(drawList[i].material);
                EntityManager.SetComponentData(drawList[i].metaChunkEntity, new MaterialMeshInfo { MeshID = meshID, MaterialID = materialID });
                EntityManager.SetComponentData(drawList[i].metaChunkEntity, new RenderBounds { Value = new AABB { Center = new float3(32, 32, 32), Extents = new float3(64, 64, 64) } });
            }
            else
            {
                EntityManager.SetComponentEnabled<MaterialMeshInfo>(drawList[i].metaChunkEntity, false);
            }
            combineJob.verts.Dispose();
            combineJob.uvs.Dispose();
            combineJob.tris.Dispose();
            combineJob.faces.Dispose();
            drawList.RemoveAt(i);
        }
        NativeArray<Entity> entitiesNeeded = queryNeeded.ToEntityArray(Allocator.Temp);
        for (int i = 0; i < entitiesNeeded.Length; i++)
        {
            MetaChunkComponent metaChunkComponent = EntityManager.GetComponentData<MetaChunkComponent>(entitiesNeeded[i]);
            NativeArray<ChunkElement> chunkElements = EntityManager.GetBuffer<ChunkElement>(entitiesNeeded[i]).ToNativeArray(Allocator.Temp);
            NativeArray<JobHandle> innerJobHandles = new NativeArray<JobHandle>(512, Allocator.Persistent);
            NativeArray<ChunkInnerMeshJob> innerJobs = new NativeArray<ChunkInnerMeshJob>(512, Allocator.Persistent);
            NativeArray<Face> faces = new NativeArray<Face>(6 * 512 * 512, Allocator.Persistent);
            NativeList<BlockElement> blockArray = new NativeList<BlockElement>(Allocator.Temp);
            for (int j = 0; j < chunkElements.Length; j++)
            {
                DynamicBuffer<BlockElement> blockElements = EntityManager.GetBuffer<BlockElement>(chunkElements[j].chunkEntity);
                NativeArray<BlockElement> chunkBlockArray = blockElements.ToNativeArray(Allocator.Temp);
                blockArray.AddRange(chunkBlockArray);
            }
            ChunkInnerMeshJob innerMeshJob = new ChunkInnerMeshJob { blocks = blockArray.ToArray(Allocator.Persistent), faces = faces };
            JobHandle innerMeshJobHandle = innerMeshJob.Schedule(512 * 512, 32);
            ChunkMeshCombineJob combineJob = new ChunkMeshCombineJob
            {
                faces = faces,
                verts = new NativeList<float3>(6 * 4 * 512 * 512, Allocator.Persistent),
                uvs = new NativeList<float2>(6 * 4 * 512 * 512, Allocator.Persistent),
                tris = new NativeList<int>(6 * 6 * 512 * 512, Allocator.Persistent),
            };
            JobHandle combineJobHandle = combineJob.Schedule(innerMeshJobHandle); 
            drawList.Add(new ChunkDraw { metaChunkEntity = entitiesNeeded[i], combinejob = combineJob, combineJobHandle = combineJobHandle, innerMeshJobHandle = innerMeshJobHandle, innerMeshJob = innerMeshJob, material = metaChunkComponent.material });
        }
        EntityManager.RemoveComponent<RenderNeededComponent>(entitiesNeeded);
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
