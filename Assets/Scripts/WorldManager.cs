using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public const int chunkSize = 32;
    public const int renderDist = 48;
    public const float maxFrameLength = 0.02f;
    [SerializeField]
    private GameObject chunk;
    [SerializeField]
    private Camera mainCamera;
    [SerializeField]
    private Grid grid;
    private Dictionary<long, ChunkManager> loadedChunks = new Dictionary<long, ChunkManager>();
    private Queue<ChunkManager> chunkPool = new Queue<ChunkManager>(128);

    public void Start()
    {
        loadedChunks.EnsureCapacity((renderDist + 1) * (renderDist + 1) * 4);
        grid.cellSize = new Vector3(chunkSize, chunkSize, chunkSize);
        StartCoroutine("MapLoadingCoroutine");
    }

    IEnumerator MapLoadingCoroutine()
    {
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
            for(int i = 0; i < renderDist; i++)
            {
                for(int j = 0; j <= i * 8; j++)
                {
                    int loadX ,loadY ;
                    if(j <= 2 * i)
                    {
                        loadX = j - i;
                        loadY = -i;
                    }
                    else if(j <= 4 * i)
                    {
                        loadX = i;
                        loadY = j - 3 * i;
                    }
                    else if(j <= 6 * i)
                    {
                        loadX = 5 * i - j;
                        loadY = i;
                    }
                    else
                    {
                        loadX= -i;
                        loadY= 7 * i - j;
                    }
                    if (Time.realtimeSinceStartup - startTime > maxFrameLength)
                    {
                        break;
                    }
                    ChunkManager loadChunk;
                    if(!loadedChunks.TryGetValue(CoordToKey(camX + loadX, camY + loadY), out loadChunk))
                    {
                        if ((loadX * loadX + loadY * loadY) < renderDist * renderDist)
                        {
                            ChunkManager newChunk;
                            if (!chunkPool.TryDequeue(out newChunk))
                            {
                                newChunk = Instantiate(chunk).GetComponent<ChunkManager>();
                            }
                            newChunk.Initialize(this, camX + loadX, camY + loadY);
                            loadedChunks.Add(CoordToKey(camX + loadX, camY + loadY), newChunk);
                        }
                    }
                    else if ((loadX * loadX + loadY * loadY) < renderDist * renderDist / 16 && loadChunk.currentSize > 1)
                    {
                        loadChunk.drawChunk(1);
                    }
                    else if ((loadX * loadX + loadY * loadY) < renderDist * renderDist / 4 && loadChunk.currentSize > 2)
                    {
                        loadChunk.drawChunk(2);
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
