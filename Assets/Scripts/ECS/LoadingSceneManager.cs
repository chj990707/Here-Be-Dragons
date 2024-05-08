using System.Collections;
using TMPro;
using Unity.Entities;
using Unity.Entities.Serialization;
using Unity.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI _textMeshPro;
    private MetaChunkLoadSystem _metaChunkLoadSystem;
    private ChunkMeshGenerateSystem _metaChunkMeshGenerateSystem;
    [SerializeField]
    private EntitySceneReference _entitySceneReference;

    public void Start()
    {
        StartCoroutine("LoadingCoroutine");
    }
    public void Update()
    {
        
    }
    IEnumerator LoadingCoroutine()
    {
        Entity entityScene = SceneSystem.LoadSceneAsync(World.DefaultGameObjectInjectionWorld.Unmanaged, _entitySceneReference, new SceneSystem.LoadParameters { Flags = SceneLoadFlags.DisableAutoLoad & SceneLoadFlags.NewInstance,  });
        int loadedCount = 0;
        _metaChunkLoadSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<MetaChunkLoadSystem>();
        _metaChunkMeshGenerateSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystemManaged<ChunkMeshGenerateSystem>();
        while (loadedCount < MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * 8)
        {
            loadedCount = _metaChunkLoadSystem.getChunksLoaded();
            _textMeshPro.text = string.Format("Loading Chunks... {0} / {1}", loadedCount, MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * 8);
            yield return null;
        }
        int renderedCount = 0;
        while (renderedCount < MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * 8)
        {
            renderedCount = _metaChunkMeshGenerateSystem.getRenderedNum();
            _textMeshPro.text = string.Format("Rendering Chunks... {0} / {1}", renderedCount, MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * MetaChunkLoadSystem.renderRadius * 8);
            yield return null;
        }
        SceneManager.LoadSceneAsync("SampleSceneWithECS", LoadSceneMode.Single);
    }
}
