using UnityEngine;
using UnityEngine.AddressableAssets;

public enum ItemCategory
{
    Chair,
    Table,
    Bed,
    Lamp,
    Generic,
    Door,
    Window
}

[CreateAssetMenu(fileName = "NewCatalogItem", menuName = "Home Planner/Catalog Item")]
public class CatalogItem : ScriptableObject
{
    [Header("Info")]
    public string itemName;
    public ItemCategory category;

    [Header("Addressable Assets")]
    public AssetReferenceSprite thumbnailReference;
    public AssetReferenceGameObject modelPrefabReference;
}