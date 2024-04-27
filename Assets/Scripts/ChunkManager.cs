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
    private Block[,,] blocks;
    private WorldManager worldManager;
    private Vector2Int chunkCoord;

    public virtual void Initialize(WorldManager worldManager, int x, int y)
    {
        blocks = new Block[WorldManager.chunkSize, WorldManager.chunkSize, WorldManager.chunkSize];
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
        List<Vector3> vertices = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();
        List<int> tris = new List<int>();
        for (int i = 0; i < blocks.GetLength(0); i++)
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                for (int k = 0; k < blocks.GetLength(2); k++)
                {
                    if(Mathf.PerlinNoise((i + chunkCoord.x * WorldManager.chunkSize) / 20f, (k + chunkCoord.y * WorldManager.chunkSize) / 20f) * WorldManager.chunkSize / 2 + 1 > j)
                    {
                        blocks[i, j, k] = new StoneBlock();
                    }
                }
            }
        } 
        for (int i = 0; i < blocks.GetLength(0); i++)
        {
            for (int j = 0; j < blocks.GetLength(1); j++)
            {
                for (int k = 0; k < blocks.GetLength(2); k++)
                {
                    drawBlock(i, j, k, vertices, uvs, tris);
                }
            }
        }
        Mesh newMesh = new Mesh();
        newMesh.vertices = vertices.ToArray();
        newMesh.uv = uvs.ToArray();
        newMesh.triangles = tris.ToArray();
        newMesh.RecalculateBounds();
        newMesh.RecalculateNormals();
        chunkMesh.mesh = newMesh;
    }

    private void drawBlock(int x, int y, int z, List<Vector3> vertices, List<Vector2> uvs, List<int> tris)
    {
        if (blocks[x,y,z] == null)
        {
            return;
        }
        if (x <= 0 || blocks[x - 1,y,z] == null)
        {
            vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
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
        if (x >= WorldManager.chunkSize - 1 || blocks[x + 1, y, z] == null)
        {
            vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
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
        if (y <= 0 || blocks[x, y - 1, z] == null)
        {
            vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
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
        if (y >= WorldManager.chunkSize - 1 || blocks[x, y + 1, z] == null)
        {
            vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
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
        if (z <= 0 || blocks[x, y, z - 1] == null)
        {
            vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z - 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z - 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z - 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z - 0.5f));
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
        if (z >= WorldManager.chunkSize - 1 || blocks[x, y, z + 1] == null)
        {
            vertices.Add(new Vector3(x + 0.5f, y - 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x + 0.5f, y + 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y + 0.5f, z + 0.5f));
            vertices.Add(new Vector3(x - 0.5f, y - 0.5f, z + 0.5f));
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
        if (x < 0 || x >= blocks.GetLength(0)
            || y < 0 || y >= blocks.GetLength(1)
            || z < 0 || z >= blocks.GetLength(2))
        {
            return null;
        }
        return blocks[x, y, z];
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
