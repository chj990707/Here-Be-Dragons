using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Block 
{
    private bool initialized = false;
    [SerializeField]
    private bool opaque;
    [SerializeField]
    private bool fullBlock;
    public ChunkManager gridManager { get; protected set; }
    private Vector3Int blockCoord;

    public virtual void Initialize(ChunkManager gridManager, int x, int y, int z)
    {
        if(initialized) return;
        initialized = true;
        this.gridManager = gridManager;
        blockCoord = new Vector3Int(x, y, z);
    }

    public Block[] getAdjacentBlocks()
    {
        Block[] adajcentBlocks = new Block[6]; 
        adajcentBlocks[0] = gridManager.getBlock(blockCoord.x - 1, blockCoord.y, blockCoord.z);
        adajcentBlocks[1] = gridManager.getBlock(blockCoord.x + 1, blockCoord.y, blockCoord.z);
        adajcentBlocks[2] = gridManager.getBlock(blockCoord.x, blockCoord.y - 1, blockCoord.z);
        adajcentBlocks[3] = gridManager.getBlock(blockCoord.x, blockCoord.y + 1, blockCoord.z);
        adajcentBlocks[4] = gridManager.getBlock(blockCoord.x, blockCoord.y, blockCoord.z + 1);
        adajcentBlocks[5] = gridManager.getBlock(blockCoord.x, blockCoord.y, blockCoord.z - 1);
        return adajcentBlocks;
    }

    public Block[] getNearBlocks()
    {
        List<Block> nearBlocks = new List<Block>();
        for(int i = -1; i <= 1; i++)
        {
            for(int j = -1; j <= 1; j++)
            {
                for(int k = -1; k <= 1; k++)
                {
                    if(i == 0 && j == 0 && k == 0) continue;
                    nearBlocks.Add(gridManager.getBlock(blockCoord.x + i, blockCoord.y + j, blockCoord.z + k));
                }
            }
        }
        return nearBlocks.ToArray();
    }
    
    public virtual bool isVisible()
    {
        Block[] adjacentBlocks = getAdjacentBlocks();
        foreach (Block adjacentBlock in adjacentBlocks)
        {
            if (adjacentBlock == null || !adjacentBlock.opaque || !adjacentBlock.fullBlock) return true;
        }
        return false;
    }
    public virtual bool isCollidable()
    {
        Block[] adjacentBlocks = getAdjacentBlocks();
        foreach (Block adjacentBlock in adjacentBlocks)
        {
            if (adjacentBlock == null || !adjacentBlock.fullBlock) return true;
        }
        return false;
    }
}
