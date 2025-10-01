using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ModelLoaderPanel : MonoBehaviour
{
    [SerializeField] private GameObject _parent;

    [SerializeField] private GameObject _models;
    [SerializeField] private GameObject _modelCategory;

    [SerializeField] private Button _modelBackBtn;
    [SerializeField] private Button _modelCategoryBackBtn;

    public TMP_Text _modelTitle; // type chair, Bed etcs


    [Header("UI Elements")]
    [SerializeField] private Transform _modelCategoryButtonContainer;
    [SerializeField] private Transform _modelItemContainer;

    [Header("Prefabs")]
    [SerializeField] private GameObject categoryButtonPrefab;
    [SerializeField] private GameObject _modelItemButtonPrefab;

    private void Start()
    {
        if (_parent == null)
            _parent = this.gameObject;


        CreateCategoryButtons();
    }

    private void OnEnable()
    {
        _models.SetActive(false);
        _modelCategory.SetActive(true);

        _modelCategoryBackBtn.onClick.AddListener(CloseModelSelectionUI);
        _modelBackBtn.onClick.AddListener(BackToModelCategory);
    }

    private void OnDisable()
    {
        _modelCategoryBackBtn.onClick.RemoveListener(CloseModelSelectionUI);
        _modelBackBtn.onClick.RemoveListener(BackToModelCategory);
    }

    private void CreateCategoryButtons()
    {
        foreach (ItemCategory category in Enum.GetValues(typeof(ItemCategory)))
        {
            if (category == ItemCategory.Door || category == ItemCategory.Window)
                continue;
            GameObject buttonGO = Instantiate(categoryButtonPrefab, _modelCategoryButtonContainer);
            buttonGO.GetComponentInChildren<TMP_Text>().text = category.ToString();

            buttonGO.GetComponent<Button>().onClick.AddListener(() => OnCategorySelected(category));
        }
    }

    private void OnCategorySelected(ItemCategory category)
    {
        _models.SetActive(true);
        _modelCategory.SetActive(false);

        _modelTitle.text = category.ToString();
        List<CatalogItem> items = GameManager.Instance.GetCatalogManager().GetItemsByCategory(category);
        PopulateItemGrid(items);
    }

    private void PopulateItemGrid(List<CatalogItem> items)
    {
        foreach (Transform child in _modelItemContainer) Destroy(child.gameObject);

        foreach (var item in items)
        {
            Debug.Log($"Creating button for '{item.itemName}'. Thumbnail address is '{item.thumbnailReference.AssetGUID}'");
            GameObject buttonGO = Instantiate(_modelItemButtonPrefab, _modelItemContainer);
            buttonGO.GetComponent<AddressableImageLoader>().LoadImage(item.thumbnailReference);
            buttonGO.GetComponent<Button>().onClick.AddListener(() =>
            {
                GameManager.Instance._uiManager._placementManager.InitiatePlacement(item);
            });
        }
    }


    private void CloseModelSelectionUI()
    {
        _parent.SetActive(false);
        GameManager.Instance.GetSubStateManager().SetOrthoIdleState();
    }

    private void BackToModelCategory()
    {
        _modelCategory.SetActive(true);
        _models.SetActive(false);
    }

    
}
