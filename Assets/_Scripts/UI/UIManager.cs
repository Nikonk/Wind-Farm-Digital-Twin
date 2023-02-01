using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIManager : MonoBehaviour
{
    
    private BuildingPlacer _buildingPlacer;
    private Dictionary<string, TMP_Text> _resourceTexts;
    private Dictionary<string, Button> _buildingButtons;

    public Transform buildingMenu;
    public GameObject BuildingButtonPrefab;
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    
    public GameObject infoPanel;
    private TMP_Text _infoPanelTitleText;
    private TMP_Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;

    private void Awake()
    {
        _buildingPlacer = GetComponent<BuildingPlacer>();
        _resourceTexts = new Dictionary<string, TMP_Text>();

        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key;
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<TMP_Text>();
            _SetResourceText(pair.Key, pair.Value.Amount);
        }

        _buildingButtons = new Dictionary<string, Button>();
        for (int i = 0; i < Globals.BUILDING_DATA.Length; i++)
        {
            BuildingData data = Globals.BUILDING_DATA[i];
            GameObject button = GameObject.Instantiate(
                BuildingButtonPrefab,
                buildingMenu);
            button.name = data.UnitName;
            button.transform.Find("Text").GetComponent<TMP_Text>().text = data.UnitName;
            button.GetComponent<BuildingButton>().Initialize(Globals.BUILDING_DATA[i]);
            Button b = button.GetComponent<Button>();
            _AddBuildingButtonListener(b, i);
            
            _buildingButtons[data.Code] = b;
            
            if (!Globals.BUILDING_DATA[i].CanBuy())
            {
                b.interactable = false;
            }
        }

        Transform infoPanelTransform = infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Content/Title").GetComponent<TMP_Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Content/Description").GetComponent<TMP_Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("Content/ResourcesCost");
        _ShowInfoPanel(false);
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons", _OnCheckBuildingButtons);
        EventManager.AddTypedListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons", _OnCheckBuildingButtons);
        EventManager.RemoveTypedListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
    }

    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }  

    private void _SetResourceText(string resource, int value)  
    {
        _resourceTexts[resource].text = value.ToString();
    }

    private void _OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<string, GameResource> pair in Globals.GAME_RESOURCES)
        {
            _SetResourceText(pair.Key, pair.Value.Amount);
        }
    }

    private void _OnCheckBuildingButtons()
    {
        foreach (BuildingData data in Globals.BUILDING_DATA)
        {
            _buildingButtons[data.Code].interactable = data.CanBuy();
        }
    }

    private void _OnHoverBuildingButton(CustomEventData data)
    {
        SetInfoPanel(data.unitData);
        _ShowInfoPanel(true);
    }

    private void _OnUnhoverBuildingButton()
    {
        _ShowInfoPanel(false);
    }

    private void _ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }

    public void SetInfoPanel(UnitData data)
    {
        if (data.Code != "")
            _infoPanelTitleText.text = data.Code;
        if (data.Description != "")
            _infoPanelDescriptionText.text = data.Description;
        
        foreach (Transform child in _infoPanelResourcesCostParent)
            Destroy(child.gameObject);
        
        if (data.Cost.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (ResourceValue resource in data.Cost)
            {
                g = GameObject.Instantiate(gameResourceDisplayPrefab, _infoPanelResourcesCostParent);
                t = g.transform;
                t.Find("Text").GetComponent<TMP_Text>().text = resource.amount.ToString();
                Color invalidTextColor = Color.red;
                if (Globals.GAME_RESOURCES[resource.code].Amount < resource.amount)
                    t.Find("Text").GetComponent<TMP_Text>().color = invalidTextColor;
            }
        }
    }
}
