using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[RequireComponent(typeof(Image))]
public class AddressableImageLoader : MonoBehaviour
{
    [SerializeField] private Image _image;
    private AsyncOperationHandle<Sprite> _loadHandle;

    void Awake()
    {
        if (_image == null)
            _image = GetComponent<Image>();
    }

    public void LoadImage(AssetReferenceSprite spriteRef)
    {
         if (spriteRef == null)
        {
            // This check is good, keep it.
            return;
        }

        Debug.Log($"'{gameObject.name}' received request to load image at address: '{spriteRef.AssetGUID}'", this.gameObject);

        if (_loadHandle.IsValid())
        {
            Addressables.Release(_loadHandle);
        }

        _loadHandle = Addressables.LoadAssetAsync<Sprite>(spriteRef);

        _loadHandle.Completed += handle =>
        {
            if (this == null || !gameObject.activeInHierarchy)
            {
                if (handle.IsValid()) Addressables.Release(handle);
                return;
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                _image.sprite = handle.Result;
            }
            else
            {
                Debug.LogError($"Failed to load sprite: {spriteRef.AssetGUID}");
            }
        };
    }

    void OnDestroy()
    {
        if (_loadHandle.IsValid())
        {
            Addressables.Release(_loadHandle);
        }
    }
}