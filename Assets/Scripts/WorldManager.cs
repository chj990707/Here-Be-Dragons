using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public const int chunkSize = 16;
    public const int renderDist = 32;
    public const float maxFrameLength = 0.03f;
    [SerializeField]
    private GameObject chunk;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Grid grid;
    private Dictionary<long, ChunkManager> loadedChunks = new Dictionary<long, ChunkManager>();

    public void Start()
    {
        loadedChunks.EnsureCapacity((renderDist + 1) * (renderDist + 1) * 4);
        StartCoroutine("MapLoadingCoroutine");
    }

    IEnumerator MapLoadingCoroutine()
    {
        Queue<ChunkManager> chunkPool = new Queue<ChunkManager>();
        while (true)
        {
            float startTime = Time.realtimeSinceStartup;
            int camX = (int)mainCamera.transform.position.x / chunkSize;
            int camY = (int)mainCamera.transform.position.z / chunkSize;
            List<long> removeKeys = new List<long>();
            foreach (var chunk in loadedChunks)
            {
                if(Time.realtimeSinceStartup - startTime > maxFrameLength)
                {
                    break;
                }
                if((KeyToCoord(chunk.Key) - new Vector2(camX, camY)).magnitude > renderDist)
                {
                    chunkPool.Enqueue(chunk.Value);
                    removeKeys.Add(chunk.Key);
                }
            }
            foreach(long key in removeKeys)
            {
                loadedChunks.Remove(key);
            }
            for(int i = -renderDist; i <= renderDist; i++)
            {
                for (int j = -renderDist; j <= renderDist; j++)
                {
                    if (Time.realtimeSinceStartup - startTime > maxFrameLength)
                    {
                        break;
                    }
                    if((i * i + j * j) < renderDist * renderDist && !loadedChunks.ContainsKey(CoordToKey(camX + i, camY + j)))
                    {
                        ChunkManager newChunk;
                        if(!chunkPool.TryDequeue(out newChunk))
                        {
                            newChunk = Instantiate(chunk).GetComponent<ChunkManager>();
                        }
                        newChunk.Initialize(this, camX + i, camY + j);
                        loadedChunks.Add(CoordToKey(camX + i, camY + j), newChunk);
                    }
                }
            }
            yield return null;
        }
    }

    public void UpdateChunk(int x, int y)
    {
        if(loadedChunks.ContainsKey(CoordToKey(x, y)))
        {
            loadedChunks[(CoordToKey(x, y))].UpdateChunk();
        }
    }

    public Grid getGrid()
    {
        return grid;
    }

    public long CoordToKey(int x, int y)
    {
        return x * 0x100000000 + y;
    }

    public Vector2Int KeyToCoord(long key)
    {
        int x = (int)(key / 0x100000000);
        int y = (int)(key - x * 0x100000000);
        return new Vector2Int(x, y);
    }
}
