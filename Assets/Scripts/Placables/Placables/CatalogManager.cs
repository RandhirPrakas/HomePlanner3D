using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CatalogManager : MonoBehaviour
{
    [SerializeField]
    private List<CatalogItem> allItems;


    public List<CatalogItem> GetItemsByCategory(ItemCategory category)
    {
        return allItems.Where(item => item.category == category).ToList();
    }
}