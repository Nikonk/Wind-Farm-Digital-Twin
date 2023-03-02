using System.Reflection;
using System.Collections.Generic;
using System;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    
    private BuildingPlacer _buildingPlacer;
    private Dictionary<InGameResource, TMP_Text> _resourceTexts;
    private Dictionary<string, Button> _buildingButtons;

    public Transform buildingMenu;
    public GameObject BuildingButtonPrefab;
    public Transform resourcesUIParent;
    public GameObject gameResourceDisplayPrefab;
    
    public GameObject infoPanel;
    private TMP_Text _infoPanelTitleText;
    private TMP_Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;

    public Transform selectedUnitsListParent;
    public GameObject selectedUnitInfoPrefab;

    public Transform selectionGroupsParent;

    public GameObject gameResourceCostPrefab;
    public GameObject selectedUnitMenu;
    private RectTransform _selectedUnitContentRectTransform;
    private RectTransform _selectedUnitGlobalButtonsRectTransform;
    private TMP_Text _selectedUnitTitleText;
    private TMP_Text _selectedUnitLevelText;
    private Transform _selectedUnitResourcesProductionParent;
    private Transform _selectedUnitActionButtonsParent;

    [SerializeField]
    private GameObject _unitSkillButtonPrefab;
    private Unit _selectedUnit;

    public GameObject gameSettingsPanel;
    public Transform gameSettingsMenusParent;
    public TMP_Text gameSettingsContentName;
    public Transform gameSettingsContentParent;
    public GameObject gameSettingsMenuButtonPrefab;
    public GameObject gameSettingsParameterPrefab;
    public GameObject sliderPrefab;
    public GameObject togglePrefab;
    private Dictionary<string, GameParameters> _gameParameters;

    [Header("Placed Building Production")]
    public RectTransform placedBuildingProductionRectTransform;

    private void Awake()
    {
        _buildingPlacer = GetComponent<BuildingPlacer>();
        _resourceTexts = new Dictionary<InGameResource, TMP_Text>();

        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES)
        {
            GameObject display = Instantiate(gameResourceDisplayPrefab, resourcesUIParent);
            display.name = pair.Key.ToString();
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<TMP_Text>();
            display.transform.Find("Icon").GetComponent<Image>().sprite = 
                        Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
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

        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);
        
        Transform selectedUnitMenuTransform = selectedUnitMenu.transform;
        _selectedUnitContentRectTransform = selectedUnitMenuTransform
            .Find("Content").GetComponent<RectTransform>();
        _selectedUnitGlobalButtonsRectTransform = selectedUnitMenuTransform
            .Find("GlobalActions").GetComponent<RectTransform>();
        _selectedUnitTitleText = selectedUnitMenuTransform
            .Find("Content/Title").GetComponent<TMP_Text>();
        _selectedUnitLevelText = selectedUnitMenuTransform
            .Find("Content/Level").GetComponent<TMP_Text>();
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform
            .Find("Content/ResourcesProduction");
        _selectedUnitActionButtonsParent = selectedUnitMenuTransform
            .Find("SpecificActions");
        
        _ShowSelectedUnitMenu(false);

        gameSettingsPanel.SetActive(false);
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>(
            "ScriptableObjects/Parameters");
        _gameParameters = new Dictionary<string, GameParameters>();
        foreach (GameParameters p in gameParametersList)
            _gameParameters[p.GetParametersName()] = p;
        _SetupGameSettingsPanel();

        placedBuildingProductionRectTransform.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons", _OnCheckBuildingButtons);
        EventManager.AddListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
        EventManager.AddListener("SelectUnit", _OnSelectUnit);
        EventManager.AddListener("DeselectUnit", _OnDeselectUnit);
        EventManager.AddListener("UpdatePlacedBuildingProduction", _OnUpdatePlacedBuildingProduction);
        EventManager.AddListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.AddListener("PlaceBuildingOff", _OnPlaceBuildingOff);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", _OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons", _OnCheckBuildingButtons);
        EventManager.RemoveListener("HoverBuildingButton", _OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", _OnUnhoverBuildingButton);
        EventManager.RemoveListener("SelectUnit", _OnSelectUnit);
        EventManager.RemoveListener("DeselectUnit", _OnDeselectUnit);
        EventManager.RemoveListener("UpdatePlacedBuildingProduction", _OnUpdatePlacedBuildingProduction);
        EventManager.RemoveListener("PlaceBuildingOn", _OnPlaceBuildingOn);
        EventManager.RemoveListener("PlaceBuildingOff", _OnPlaceBuildingOff);
    }

    private void _AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }  

    private void _SetResourceText(InGameResource resource, int value)  
    {
        _resourceTexts[resource].text = value.ToString();
    }

    private void _OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GAME_RESOURCES)
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

    private void _OnHoverBuildingButton(object data)
    {
        SetInfoPanel((UnitData)data);
        _ShowInfoPanel(true);
    }

    private void _OnUnhoverBuildingButton()
    {
        _ShowInfoPanel(false);
    }

    private void _OnSelectUnit(object data)
    {
        Unit unit = (Unit)data;
        _AddSelectedUnitToUIList(unit);
        _SetSelectedUnitMenu(unit);
        _ShowSelectedUnitMenu(true);
    }

    private void _OnDeselectUnit(object data)
    {
        Unit unit = (Unit)data;
        _RemoveSelectedUnitFromUIList(unit.Code);
        if (Globals.SELECTED_UNITS.Count == 0)
            _ShowSelectedUnitMenu(false);
        else
            _SetSelectedUnitMenu(Globals.SELECTED_UNITS[Globals.SELECTED_UNITS.Count - 1].Unit);
    }

    private void _ShowInfoPanel(bool show)
    {
        infoPanel.SetActive(show);
    }

    private void _AddSelectedUnitToUIList(Unit unit)
    {
        Transform alreadyInstantiatedChild = selectedUnitsListParent.Find(unit.Code);
        if (alreadyInstantiatedChild != null)
        {
            TMP_Text t = alreadyInstantiatedChild.Find("Count").GetComponent<TMP_Text>();
            int count = int.Parse(t.text);
            t.text = (count + 1).ToString();
        }
        else
        {
            GameObject g = GameObject.Instantiate(
                selectedUnitInfoPrefab, selectedUnitsListParent);
            g.name = unit.Code;
            Transform t = g.transform;
            t.Find("Count").GetComponent<TMP_Text>().text = "1";
            t.Find("UnitType").GetComponent<TMP_Text>().text = unit.Data.UnitName;
        }
    }

    private void _RemoveSelectedUnitFromUIList(string code)
    {
        Transform listItem = selectedUnitsListParent.Find(code);
        if (listItem == null) return;
        TMP_Text t = listItem.Find("Count").GetComponent<TMP_Text>();
        int count = int.Parse(t.text);
        count -= 1;
        if (count == 0)
            DestroyImmediate(listItem.gameObject);
        else
            t.text = count.ToString();
    }

    private void _AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _selectedUnit.TriggerSkill(i));
    }

    private void _SetSelectedUnitMenu(Unit unit)
    {
        _selectedUnit = unit;
        _selectedUnitTitleText.text = unit.Data.UnitName;
        _selectedUnitLevelText.text = $"Level {unit.Level}";

        foreach (Transform child in _selectedUnitResourcesProductionParent)
            Destroy(child.gameObject);
        if (unit.Production.Count > 0)
        {
            GameObject g;
            Transform t;
            foreach (KeyValuePair<InGameResource, int> resource in unit.Production)
            {
                g = GameObject.Instantiate(
                    gameResourceCostPrefab, _selectedUnitResourcesProductionParent);
                t = g.transform;
                t.Find("Text").GetComponent<TMP_Text>().text = $"+{resource.Value}";
                t.Find("Icon").GetComponent<Image>().sprite = 
                            Resources.Load<Sprite>($"Textures/GameResources/{resource.Key}");                
            }
        }

        foreach (Transform child in _selectedUnitActionButtonsParent)
            Destroy(child.gameObject);
        if (unit.SkillManagers.Count > 0)
        {
            GameObject g;
            Transform t;
            Button b;
            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                g = GameObject.Instantiate(
                    _unitSkillButtonPrefab, _selectedUnitActionButtonsParent);
                t = g.transform;
                b = g.GetComponent<Button>();
                unit.SkillManagers[i].SetButton(b);
                t.Find("Text").GetComponent<TMP_Text>().text =
                    unit.SkillManagers[i].Skill.SkillName;
                _AddUnitSkillButtonListener(b, i);
            }
        }
    }

    private void _ShowSelectedUnitMenu(bool show)
    {
        selectedUnitMenu.SetActive(show);
        buildingMenu.gameObject.SetActive(!show);
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

    public void ToggleSelectionGroupButton(int groupIndex, bool on)
    {
        selectionGroupsParent.Find(groupIndex.ToString()).gameObject.SetActive(on);
    }

    public void ToggleGameSettingsPanel()
    {
        bool showGameSettingsPanel = !gameSettingsPanel.activeSelf;
        gameSettingsPanel.SetActive(showGameSettingsPanel);
        EventManager.TriggerEvent(showGameSettingsPanel ? "PauseGame" : "ResumeGame");
    }

    private void _SetupGameSettingsPanel()
    {
        GameObject g; string n;
        List<string> availableMenus = new List<string>();
        foreach (GameParameters parameters in _gameParameters.Values)
        {
            if (parameters.FieldsToShowInGame.Count == 0) continue;

            g = GameObject.Instantiate(
                gameSettingsMenuButtonPrefab, gameSettingsMenusParent);
            n = parameters.GetParametersName();
            g.transform.Find("Text").GetComponent<TMP_Text>().text = n;
            _AddGameSettingsPanelMenuListener(g.GetComponent<Button>(), n);
            availableMenus.Add(n);
        }

        if (availableMenus.Count > 0)
            _SetGameSettingsContent(availableMenus[0]);
    }

    private void _AddGameSettingsPanelMenuListener(Button b, string menu)
    {
        b.onClick.AddListener(() => _SetGameSettingsContent(menu));
    }

    private void _SetGameSettingsContent(string menu)
    {
        gameSettingsContentName.text = menu;

        foreach (Transform child in gameSettingsContentParent)
            Destroy(child.gameObject);

        GameParameters parameters = _gameParameters[menu];
        System.Type ParametersType = parameters.GetType();
        GameObject gWrapper, gEditor;
        RectTransform rtWrapper, rtEditor;
        int i = 0;
        float contentWidth = 534f;
        float parameterNameWidth = 210f;
        float fieldHeight = 32f;
        foreach (string fieldName in parameters.FieldsToShowInGame)
        {
            gWrapper = GameObject.Instantiate(
                gameSettingsParameterPrefab, gameSettingsContentParent);
            gWrapper.transform.Find("Text").GetComponent<TMP_Text>().text =
                fieldName;// Utils.CapitalizeWords(fieldName);

            gEditor = null;
            FieldInfo field = ParametersType.GetField(fieldName);
            if (field.FieldType == typeof(bool))
            {
                gEditor = Instantiate(togglePrefab);
                Toggle t = gEditor.GetComponent<Toggle>();
                t.isOn = (bool)field.GetValue(parameters);

                t.onValueChanged.AddListener(delegate {
                    _OnGameSettingsToggleValueChanged(parameters, field, fieldName, t);
                });
            }
            else if (field.FieldType == typeof(int) || field.FieldType == typeof(float))
            {
                bool isRange = System.Attribute.IsDefined(field, typeof(RangeAttribute), false);
                if (isRange)
                {
                    RangeAttribute attr = (RangeAttribute)System.Attribute.GetCustomAttribute(field, typeof(RangeAttribute));
                    gEditor = Instantiate(sliderPrefab);
                    Slider s = gEditor.GetComponent<Slider>();
                    s.minValue = attr.min;
                    s.maxValue = attr.max;
                    s.wholeNumbers = field.FieldType == typeof(int);
                    s.value = field.FieldType == typeof(int)
                        ? (int)field.GetValue(parameters)
                        : (float)field.GetValue(parameters);
                    
                    s.onValueChanged.AddListener(delegate
                    {
                        _OnGameSettingsSliderValueChanged(parameters, field, fieldName, s);
                    });
                }
            }
            rtWrapper = gWrapper.GetComponent<RectTransform>();
            rtWrapper.anchoredPosition = new Vector2(0f, -i * fieldHeight);
            rtWrapper.sizeDelta = new Vector2(contentWidth, fieldHeight);

            if (gEditor != null)
            {
                gEditor.transform.SetParent(gWrapper.transform);
                rtEditor = gEditor.GetComponent<RectTransform>();
                rtEditor.anchoredPosition = new Vector2((parameterNameWidth + 16f), 0f);
                rtEditor.sizeDelta = new Vector2(rtWrapper.sizeDelta.x - (parameterNameWidth + 16f), 0f);
            }

            i++;
        }

        RectTransform rt = gameSettingsContentParent.GetComponent<RectTransform>();
        Vector2 size = rt.sizeDelta;
        size.y = i * fieldHeight;
        rt.sizeDelta = size;
    }

    private void _OnGameSettingsToggleValueChanged(GameParameters parameters,
                                                   FieldInfo field,
                                                   string gameParameter,
                                                   Toggle change)
    {
        field.SetValue(parameters, change.isOn);
        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameter}", change.isOn);
    }

    private void _OnGameSettingsSliderValueChanged(GameParameters parameters,
                                                   FieldInfo field,
                                                   string gameParameter,
                                                   Slider change)
    {
        if (field.FieldType == typeof(int))
            field.SetValue(parameters, (int)change.value);
        else
            field.SetValue(parameters, change.value);
        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameter}", change.value);
    }

    private void _OnUpdatePlacedBuildingProduction(object data)
    {
        object[] values = (object[])data;
        Dictionary<InGameResource, int> production = (Dictionary<InGameResource, int>)values[0];
        Vector3 pos = (Vector3)values[1];

        if (production == null) return;

        foreach (Transform child in placedBuildingProductionRectTransform.gameObject.transform)
            Destroy(child.gameObject);

        GameObject g;
        Transform t;
        foreach (KeyValuePair<InGameResource, int> pair in production)
        {
            g = GameObject.Instantiate(
                gameResourceCostPrefab,
                placedBuildingProductionRectTransform.transform);
            t = g.transform;
            try
            {
                t.Find("Text").GetComponent<TMP_Text>().text = $"+{pair.Value}";
                t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");
            }
            catch(NullReferenceException ex)
            {
                Debug.Log(ex.ToString());
            }
        }

        placedBuildingProductionRectTransform.sizeDelta = new Vector2(80, 24 * production.Count);
    }

    private void _OnPlaceBuildingOn()
    {
        placedBuildingProductionRectTransform.gameObject.SetActive(true);
    }

    private void _OnPlaceBuildingOff()
    {
        placedBuildingProductionRectTransform.gameObject.SetActive(false);
    }
}
