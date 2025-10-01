using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PlacementManager : MonoBehaviour
{
    [SerializeField] private GameObject loadingUI;
    private AsyncOperationHandle<GameObject> _loadHandle;

    public void InitiatePlacement(CatalogItem item)
    {
        if (loadingUI != null) loadingUI.SetActive(true);

        _loadHandle = Addressables.LoadAssetAsync<GameObject>(item.modelPrefabReference);
        _loadHandle.Completed += OnPrefabLoaded;
    }

    private void OnPrefabLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (loadingUI != null) loadingUI.SetActive(false);

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedPrefab = handle.Result;

            GameManager.Instance._uiManager.HideModelSelectionUI();
            GameManager.Instance.SetSubState(new PlaceObjectState(GameManager.Instance.GetOrthoCamera(), loadedPrefab));
        }
        else
        {
            Debug.LogError($"Failed to load model from address: {handle.DebugName}");
        }
    }

    public void ReleaseCurrentPrefab()
    {
        if (_loadHandle.IsValid())
        {
            Addressables.Release(_loadHandle);
        }
    }
}