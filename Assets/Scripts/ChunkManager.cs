using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkManager : MonoBehaviour
{
    [SerializeField]
    private Grid grid;
    [SerializeField]
    protected GameObject initBlock;
    [SerializeField]
    protected MeshFilter chunkMesh;
    private Block[] blocks;
    private WorldManager worldManager;
    private Vector2Int chunkCoord;
    public int currentSize { get; private set; }

    public virtual void Initialize(WorldManager worldManager, int x, int y)
    {
        blocks = new Block[WorldManager.chunkSize * WorldManager.chunkSize * WorldManager.chunkSize];
        this.worldManager = worldManager;
        chunkCoord = new Vector2Int(x, y);
        transform.localPosition = worldManager.getGrid().CellToWorld(new Vector3Int(x, 0, y));
        createRock();
    }

    public Grid getGrid()
    {
        return grid;
    }

    public void createRock()
    {
        for (int i = 0; i < WorldManager.chunkSize; i++)
        {
            for (int j = 0; j < WorldManager.chunkSize; j++)
            {
                for (int k = 0; k < WorldManager.chunkSize; k++)
                {
                    if(Mathf.PerlinNoise((i + chunkCoord.x * WorldManager.chunkSize) / 200f, (k + chunkCoord.y * WorldManager.chunkSize) / 200f) * WorldManager.chunkSize + 1 > j)
                    {
                        blocks[i * WorldManager.chunkSize * WorldManager.chunkSize + j * WorldManager.chunkSize + k] = new StoneBlock();
                    }
                }
            }
        }
        drawChunk(4);
    }

    public void drawChunk(int size)
    {
        if (size <= 0 || size > WorldManager.chunkSize) return;
        currentSize = size;
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        vertices.Capacity = 0x8000;
        uvs.Capacity = 0x8000;
        tris.Capacity = 0x8000;
        for (int i = 0; i < WorldManager.chunkSize; i += size)
        {
            for (int j = 0; j < WorldManager.chunkSize; j += size)
            {
                for (int k = 0; k < WorldManager.chunkSize; k += size)
                {
                    drawBlock(i, j, k, vertices, uvs, tris, size);
                }
            }
        }
        Mesh newMesh = new Mesh();
        newMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
        newMesh.vertices = vertices.ToArray();
        newMesh.uv = uvs.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        chunkMesh.mesh = newMesh;
    }

    private void drawBlock(int x, int y, int z, List<Vector3> vertices, List<Vector2> uvs, List<int> tris, int size)
    {
        if (getBlock(x,y,z) == null)
        {
            return;
        }
        if (x < size || getBlock(x - size, y, z) == null)
        {
            vertices.Add(new Vector3(x, y , z));
            vertices.Add(new Vector3(x, y + size, z));
            vertices.Add(new Vector3(x, y + size, z + size));
            vertices.Add(new Vector3(x, y , z + size));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            int vertCount = vertices.Count;
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 1);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 4);
        }
        if (x > WorldManager.chunkSize - size - 1|| getBlock(x + size, y, z) == null)
        {
            vertices.Add(new Vector3(x + size, y, z + size));
            vertices.Add(new Vector3(x + size, y + size, z + size));
            vertices.Add(new Vector3(x + size, y + size, z));
            vertices.Add(new Vector3(x + size, y, z));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            int vertCount = vertices.Count;
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 1);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 4);
        }
        if (y < size || getBlock(x, y - size, z) == null)
        {
            vertices.Add(new Vector3(x, y, z));
            vertices.Add(new Vector3(x, y, z + size));
            vertices.Add(new Vector3(x + size, y, z + size));
            vertices.Add(new Vector3(x + size, y, z));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            int vertCount = vertices.Count;
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 1);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 4);
        }
        if (y > WorldManager.chunkSize - size - 1|| getBlock(x, y + size, z) == null)
        {
            vertices.Add(new Vector3(x + size, y + size, z));
            vertices.Add(new Vector3(x + size, y + size, z + size));
            vertices.Add(new Vector3(x, y + size, z + size));
            vertices.Add(new Vector3(x, y + size, z));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            int vertCount = vertices.Count;
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 1);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 4);
        }
        if (z < size || getBlock(x, y, z - size) == null)
        {
            vertices.Add(new Vector3(x , y, z));
            vertices.Add(new Vector3(x, y + size, z));
            vertices.Add(new Vector3(x + size, y + size, z));
            vertices.Add(new Vector3(x + size, y, z));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            int vertCount = vertices.Count;
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 1);
        }
        if (z > WorldManager.chunkSize - size - 1|| getBlock(x, y, z + size) == null)
        {
            vertices.Add(new Vector3(x + size, y, z + size));
            vertices.Add(new Vector3(x + size, y + size, z + size));
            vertices.Add(new Vector3(x, y + size, z + size));
            vertices.Add(new Vector3(x, y, z + size));
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(1, 1));
            uvs.Add(new Vector2(0, 1));
            int vertCount = vertices.Count;
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 3);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 4);
            tris.Add(vertCount - 2);
            tris.Add(vertCount - 1);
        }
    }

    public Block getBlock(int x, int y, int z)
    {
        if (x < 0 || x >= WorldManager.chunkSize
            || y < 0 || y >= WorldManager.chunkSize
            || z < 0 || z >= WorldManager.chunkSize )
        {
            return null;
        }
        return blocks[x * WorldManager.chunkSize * WorldManager.chunkSize + y * WorldManager.chunkSize + z];
    }

    public Block getBlock(Vector3Int pos)
    {
        return getBlock(pos.x, pos.y, pos.z);
    }

    public Vector2Int getChunkCoord()
    {
        return chunkCoord;
    }

    public void UpdateChunk()
    {

    }
    public void OnDestroy()
    {
        worldManager.UpdateChunk(chunkCoord.x + 1, chunkCoord.y);
        worldManager.UpdateChunk(chunkCoord.x - 1, chunkCoord.y);
        worldManager.UpdateChunk(chunkCoord.x, chunkCoord.y + 1);
        worldManager.UpdateChunk(chunkCoord.x, chunkCoord.y - 1);
    }
}
