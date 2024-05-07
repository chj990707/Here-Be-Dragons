using TMPro;
using Unity.Entities;
using UnityEngine;

public class LoadStateCounter : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textMeshPro;
    private MetaChunkLoadSystem _metaChunkLoadSystem;
    private ChunkMeshGenerateSystem _metaChunkMeshGenerateSystem;
    private bool _loaded;
    private bool _rendered;
    // Start is called before the first frame update
    void Start()
    {
        _metaChunkLoadSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<MetaChunkLoadSystem>();
        _metaChunkMeshGenerateSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ChunkMeshGenerateSystem>();
        _loaded = false;
    }

    // Update is called once per frame
    void Update()
    {
        int loadedCount = _metaChunkLoadSystem.getChunksLoaded();
        if(loadedCount < MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * 8)
        {
            _textMeshPro.text = string.Format("Loading... {0} / {1}", loadedCount, MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * 8);
            return;
        }
        int renderedCount = _metaChunkMeshGenerateSystem.getRenderedNum();
        if(renderedCount < MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * 8)
        {
            _textMeshPro.text = string.Format("Rendering... {0} / {1}", renderedCount, MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * 8);
            return;
        }
        if (!_loaded)
        {
            _textMeshPro.text = string.Format("Load complete at {0:F} seconds", Time.realtimeSinceStartup);
            _loaded = true;
        }
    }
}
