using System.Reflection;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    private BuildingPlacer _buildingPlacer;
    private Dictionary<InGameResource, TMP_Text> _resourceTexts;
    private Dictionary<string, Button> _buildingButtons;

    [Header("Building")]
    [SerializeField] private Transform _buildingMenu;
    [SerializeField] private GameObject _buildingButtonPrefab;
    [SerializeField] private RectTransform _placedBuildingProductionRectTransform;

    [Header("Resources")]
    [SerializeField] private Transform _resourcesUIParent;
    [SerializeField] private GameObject _gameResourceDisplayPrefab;
    [SerializeField] private GameObject _gameResourceCostPrefab;

    [Header("Info Panel")]
    [SerializeField] private GameObject _infoPanel;
    private TMP_Text _infoPanelTitleText;
    private TMP_Text _infoPanelDescriptionText;
    private Transform _infoPanelResourcesCostParent;

    [Header("Selected Unit")]
    [SerializeField] private Transform _selectedUnitsListParent;
    [SerializeField] private GameObject _selectedUnitInfoPrefab;
    [SerializeField] private GameObject _selectedUnitMenu;
    [SerializeField] private Transform _selectionGroupsParent;
    private RectTransform _selectedUnitContentRectTransform;
    private RectTransform _selectedUnitGlobalButtonsRectTransform;
    private TMP_Text _selectedUnitTitleText;
    private TMP_Text _selectedUnitLevelText;
    private Transform _selectedUnitResourcesProductionParent;
    private Transform _selectedUnitActionButtonsParent;

    [Header("Skill")]
    [SerializeField] private GameObject _unitSkillButtonPrefab;
    private Unit _selectedUnit;

    [Header("Game Settings")]
    [SerializeField] private GameObject _gameSettingsPanel;
    [SerializeField] private Transform _gameSettingsMenusParent;
    [SerializeField] private TMP_Text _gameSettingsContentName;
    [SerializeField] private Transform _gameSettingsContentParent;
    [SerializeField] private GameObject _gameSettingsMenuButtonPrefab;
    [SerializeField] private GameObject _gameSettingsParameterPrefab;
    [SerializeField] private GameObject _sliderPrefab;
    [SerializeField] private GameObject _togglePrefab;
    private Dictionary<string, GameParameters> _gameParameters;

    private void Awake()
    {
        _buildingPlacer = GetComponent<BuildingPlacer>();
        _resourceTexts = new Dictionary<InGameResource, TMP_Text>();

        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GameResources)
        {
            GameObject display = Instantiate(_gameResourceDisplayPrefab, _resourcesUIParent);

            display.name = pair.Key.ToString();
            _resourceTexts[pair.Key] = display.transform.Find("Text").GetComponent<TMP_Text>();
            display.transform.Find("Icon").GetComponent<Image>().sprite = 
                        Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");

            SetResourceText(pair.Key, pair.Value.Amount);
        }

        _buildingButtons = new Dictionary<string, Button>();

        for (int i = 0; i < Globals.BuildingData.Count; i++)
        {
            GameObject button = GameObject.Instantiate(
                _buildingButtonPrefab,
                _buildingMenu);
            BuildingData buildingData = Globals.BuildingData[i];

            button.name = buildingData.UnitName;
            button.transform.Find("Text").GetComponent<TMP_Text>().text = buildingData.UnitName;
            button.GetComponent<BuildingButton>().Initialize(buildingData);
            Button b = button.GetComponent<Button>();
            AddBuildingButtonListener(b, i);
            
            _buildingButtons[buildingData.Code] = b;
            
            if (buildingData.CanBuy() == false)
                b.interactable = false;
        }

        Transform infoPanelTransform = _infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Content/Title").GetComponent<TMP_Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Content/Description").GetComponent<TMP_Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("Content/ResourcesCost");
        ShowInfoPanel(false);

        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);
        
        Transform selectedUnitMenuTransform = _selectedUnitMenu.transform;
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
        
        ShowSelectedUnitMenu(false);

        _gameSettingsPanel.SetActive(false);
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        _gameParameters = new Dictionary<string, GameParameters>();

        foreach (GameParameters p in gameParametersList)
            _gameParameters[p.GetParametersName()] = p;

        SetupGameSettingsPanel();

        _placedBuildingProductionRectTransform.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        EventManager.AddListener("UpdateResourceTexts", OnUpdateResourceTexts);
        EventManager.AddListener("CheckBuildingButtons", OnCheckBuildingButtons);
        EventManager.AddListener("HoverBuildingButton", OnHoverBuildingButton);
        EventManager.AddListener("UnhoverBuildingButton", OnUnhoverBuildingButton);
        EventManager.AddListener("SelectUnit", OnSelectUnit);
        EventManager.AddListener("DeselectUnit", OnDeselectUnit);
        EventManager.AddListener("UpdatePlacedBuildingProduction", OnUpdatePlacedBuildingProduction);
        EventManager.AddListener("PlaceBuildingOn", OnPlaceBuildingOn);
        EventManager.AddListener("PlaceBuildingOff", OnPlaceBuildingOff);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("UpdateResourceTexts", OnUpdateResourceTexts);
        EventManager.RemoveListener("CheckBuildingButtons", OnCheckBuildingButtons);
        EventManager.RemoveListener("HoverBuildingButton", OnHoverBuildingButton);
        EventManager.RemoveListener("UnhoverBuildingButton", OnUnhoverBuildingButton);
        EventManager.RemoveListener("SelectUnit", OnSelectUnit);
        EventManager.RemoveListener("DeselectUnit", OnDeselectUnit);
        EventManager.RemoveListener("UpdatePlacedBuildingProduction", OnUpdatePlacedBuildingProduction);
        EventManager.RemoveListener("PlaceBuildingOn", OnPlaceBuildingOn);
        EventManager.RemoveListener("PlaceBuildingOff", OnPlaceBuildingOff);
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
                g = GameObject.Instantiate(_gameResourceDisplayPrefab, _infoPanelResourcesCostParent);
                t = g.transform;
                t.Find("Icon").GetComponent<Image>().sprite =
                    Resources.Load<Sprite>($"Textures/GameResources/{resource.Resource}");
                t.Find("Text").GetComponent<TMP_Text>().text = resource.Amount.ToString();
                Color invalidTextColor = Color.red;
                if (Globals.GameResources[resource.Resource].Amount < resource.Amount)
                    t.Find("Text").GetComponent<TMP_Text>().color = invalidTextColor;
            }
        }
    }

    public void ToggleSelectionGroupButton(int groupIndex, bool on)
    {
        _selectionGroupsParent.Find(groupIndex.ToString()).gameObject.SetActive(on);
    }

    public void ToggleGameSettingsPanel()
    {
        bool showGameSettingsPanel = !_gameSettingsPanel.activeSelf;
        _gameSettingsPanel.SetActive(showGameSettingsPanel);
        EventManager.TriggerEvent(showGameSettingsPanel ? "PauseGame" : "ResumeGame");
    }

    public void DestroySelectedUnit()
    {
        GameObject selectedUnitGO = _selectedUnit.Transform.gameObject;
        GameManager.Instance.RemoveOperatingUnit(_selectedUnit);

        selectedUnitGO.GetComponent<UnitManager>().Deselect();

        foreach (ResourceValue resource in _selectedUnit.Data.Cost)
        {
            int destroyCompensation = (int)Mathf.Floor(resource.Amount / 2);
            Globals.GameResources[resource.Resource].ChangeAmount(destroyCompensation);
        }

        OnUpdateResourceTexts();
        OnCheckBuildingButtons();

        Destroy(_selectedUnit.Transform.gameObject);
    }

    private void AddBuildingButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
    }  

    private void SetResourceText(InGameResource resource, int value)  
    {
        _resourceTexts[resource].text = value.ToString();
    }

    private void OnUpdateResourceTexts()
    {
        foreach (KeyValuePair<InGameResource, GameResource> pair in Globals.GameResources)
            SetResourceText(pair.Key, pair.Value.Amount);
    }

    private void OnCheckBuildingButtons()
    {
        foreach (BuildingData data in Globals.BuildingData)
            _buildingButtons[data.Code].interactable = data.CanBuy();
    }

    private void OnHoverBuildingButton(object data)
    {
        SetInfoPanel((UnitData)data);
        ShowInfoPanel(true);
    }

    private void OnUnhoverBuildingButton()
    {
        ShowInfoPanel(false);
    }

    private void OnSelectUnit(object data)
    {
        Unit unit = (Unit)data;
        AddSelectedUnitToUIList(unit);
        SetSelectedUnitMenu(unit);
        ShowSelectedUnitMenu(true);
    }

    private void OnDeselectUnit(object data)
    {
        Unit unit = (Unit)data;
        RemoveSelectedUnitFromUIList(unit.Code);

        if (Globals.SelectedUnits.Count == 0)
            ShowSelectedUnitMenu(false);
        else
            SetSelectedUnitMenu(Globals.SelectedUnits[Globals.SelectedUnits.Count - 1].Unit);
    }

    private void ShowInfoPanel(bool show)
    {
        _infoPanel.SetActive(show);
    }

    private void AddSelectedUnitToUIList(Unit unit)
    {
        Transform alreadyInstantiatedChild = _selectedUnitsListParent.Find(unit.Code);

        if (alreadyInstantiatedChild != null)
        {
            TMP_Text t = alreadyInstantiatedChild.Find("Count").GetComponent<TMP_Text>();
            int count = int.Parse(t.text);
            t.text = (count + 1).ToString();
        }
        else
        {
            GameObject g = GameObject.Instantiate(
                _selectedUnitInfoPrefab, _selectedUnitsListParent);
            g.name = unit.Code;
            Transform t = g.transform;
            t.Find("Count").GetComponent<TMP_Text>().text = "1";
            t.Find("UnitType").GetComponent<TMP_Text>().text = unit.Data.UnitName;
        }
    }

    private void RemoveSelectedUnitFromUIList(string code)
    {
        Transform listItem = _selectedUnitsListParent.Find(code);

        if (listItem == null)
            return;

        TMP_Text t = listItem.Find("Count").GetComponent<TMP_Text>();
        int count = int.Parse(t.text);
        count -= 1;

        if (count == 0)
            DestroyImmediate(listItem.gameObject);
        else
            t.text = count.ToString();
    }

    private void AddUnitSkillButtonListener(Button b, int i)
    {
        b.onClick.AddListener(() => _selectedUnit.TriggerSkill(i));
    }

    private void SetSelectedUnitMenu(Unit unit)
    {
        _selectedUnit = unit;
        _selectedUnitTitleText.text = unit.Data.UnitName;

        foreach (Transform child in _selectedUnitResourcesProductionParent)
            Destroy(child.gameObject);

        if (unit.Data.IsHasProduction)
        {
            GameObject g;
            Transform t;

            foreach (var productionModel in unit.Data.ProductionModels)
            {
                foreach (var resource in productionModel.Productions)
                {
                    g = GameObject.Instantiate(
                        _gameResourceCostPrefab, _selectedUnitResourcesProductionParent);
                    t = g.transform;
                    t.Find("Text").GetComponent<TMP_Text>().text = $"+{resource.Value}";
                    t.Find("Icon").GetComponent<Image>().sprite =
                                Resources.Load<Sprite>($"Textures/GameResources/{resource.Key}");
                }
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
                AddUnitSkillButtonListener(b, i);
            }
        }
    }

    private void ShowSelectedUnitMenu(bool show)
    {
        _selectedUnitMenu.SetActive(show);
        _buildingMenu.gameObject.SetActive(!show);
    }

    private void SetupGameSettingsPanel()
    {
        GameObject g; string n;
        List<string> availableMenus = new List<string>();

        foreach (GameParameters parameters in _gameParameters.Values)
        {
            if (parameters.FieldsToShowInGame.Count == 0)
                continue;

            g = GameObject.Instantiate(
                _gameSettingsMenuButtonPrefab, _gameSettingsMenusParent);
            n = parameters.GetParametersName();
            g.transform.Find("Text").GetComponent<TMP_Text>().text = n;
            AddGameSettingsPanelMenuListener(g.GetComponent<Button>(), n);
            availableMenus.Add(n);
        }

        if (availableMenus.Count > 0)
            SetGameSettingsContent(availableMenus[0]);
    }

    private void AddGameSettingsPanelMenuListener(Button b, string menu)
    {
        b.onClick.AddListener(() => SetGameSettingsContent(menu));
    }

    private void SetGameSettingsContent(string menu)
    {
        _gameSettingsContentName.text = menu;

        foreach (Transform child in _gameSettingsContentParent)
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
            gWrapper = GameObject.Instantiate(_gameSettingsParameterPrefab,
                                              _gameSettingsContentParent);
            gWrapper.transform.Find("Text").GetComponent<TMP_Text>().text = fieldName;

            gEditor = null;
            FieldInfo field = ParametersType.GetField(fieldName);

            if (field != null)
            {
                if (field.FieldType == typeof(bool))
                {
                    gEditor = Instantiate(_togglePrefab);
                    Toggle t = gEditor.GetComponent<Toggle>();
                    t.isOn = (bool)field.GetValue(parameters);

                    t.onValueChanged.AddListener(delegate
                    {
                        OnGameSettingsToggleValueChanged(parameters, field, fieldName, t);
                    });
                }
                else if (field.FieldType == typeof(int) || field.FieldType == typeof(float))
                {
                    bool isRange = System.Attribute.IsDefined(field, typeof(RangeAttribute), false);

                    if (isRange)
                    {
                        RangeAttribute attr = (RangeAttribute)System.Attribute.GetCustomAttribute(field, typeof(RangeAttribute));
                        gEditor = Instantiate(_sliderPrefab);
                        Slider s = gEditor.GetComponent<Slider>();
                        s.minValue = attr.min;
                        s.maxValue = attr.max;
                        s.wholeNumbers = field.FieldType == typeof(int);
                        s.value = field.FieldType == typeof(int)
                            ? (int)field.GetValue(parameters)
                            : (float)field.GetValue(parameters);

                        s.onValueChanged.AddListener(delegate
                        {
                            OnGameSettingsSliderValueChanged(parameters, field, fieldName, s);
                        });
                    }
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

        RectTransform rt = _gameSettingsContentParent.GetComponent<RectTransform>();
        Vector2 size = rt.sizeDelta;
        size.y = i * fieldHeight;
        rt.sizeDelta = size;
    }

    private void OnGameSettingsToggleValueChanged(GameParameters parameters,
                                                  FieldInfo field,
                                                  string gameParameter,
                                                  Toggle change)
    {
        field.SetValue(parameters, change.isOn);
        EventManager.TriggerEvent($"UpdateGameParameter:{gameParameter}", change.isOn);
    }

    private void OnGameSettingsSliderValueChanged(GameParameters parameters,
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

    private void OnUpdatePlacedBuildingProduction(object data)
    {
        object[] values = (object[])data;
        Dictionary<InGameResource, int> production = values[0] as Dictionary<InGameResource, int>;
        Vector3 pos = (Vector3)values[1];

        if (production == null)
            return;

        foreach (Transform child in _placedBuildingProductionRectTransform.gameObject.transform)
            Destroy(child.gameObject);

        GameObject g;
        Transform t;

        foreach (KeyValuePair<InGameResource, int> pair in production)
        {
            g = GameObject.Instantiate(
                _gameResourceCostPrefab,
                _placedBuildingProductionRectTransform.transform);
            t = g.transform;
            t.Find("Text").GetComponent<TMP_Text>().text = $"+{pair.Value}";
            t.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");            
        }

        _placedBuildingProductionRectTransform.sizeDelta = new Vector2(80, 24 * production.Count);
    }

    private void OnPlaceBuildingOn()
    {
        _placedBuildingProductionRectTransform.gameObject.SetActive(true);
    }

    private void OnPlaceBuildingOff()
    {
        _placedBuildingProductionRectTransform.gameObject.SetActive(false);
    }
}
