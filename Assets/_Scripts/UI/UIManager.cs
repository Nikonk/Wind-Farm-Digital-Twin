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
    private TMP_Text _selectedUnitTitleText;
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
            GameObject buildingButton = GameObject.Instantiate(
                _buildingButtonPrefab,
                _buildingMenu);
            BuildingData buildingData = Globals.BuildingData[i];

            buildingButton.name = buildingData.UnitName;
            buildingButton.transform.Find("Text").GetComponent<TMP_Text>().text = buildingData.UnitName;
            buildingButton.GetComponent<BuildingButton>().Initialize(buildingData);
            Button button = buildingButton.GetComponent<Button>();
            AddBuildingButtonListener(button, i);
            
            _buildingButtons[buildingData.Code] = button;
            
            if (buildingData.CanBuy() == false)
                button.interactable = false;
        }

        Transform infoPanelTransform = _infoPanel.transform;
        _infoPanelTitleText = infoPanelTransform.Find("Content/Title").GetComponent<TMP_Text>();
        _infoPanelDescriptionText = infoPanelTransform.Find("Content/Description").GetComponent<TMP_Text>();
        _infoPanelResourcesCostParent = infoPanelTransform.Find("Content/ResourcesCost");
        ShowInfoPanel(false);

        for (int i = 1; i <= 9; i++)
            ToggleSelectionGroupButton(i, false);
        
        Transform selectedUnitMenuTransform = _selectedUnitMenu.transform;
        _selectedUnitTitleText = selectedUnitMenuTransform
            .Find("Content/Title").GetComponent<TMP_Text>();
        _selectedUnitResourcesProductionParent = selectedUnitMenuTransform
            .Find("Content/ResourcesProduction");
        _selectedUnitActionButtonsParent = selectedUnitMenuTransform
            .Find("SpecificActions");
        
        ShowSelectedUnitMenu(false);

        _gameSettingsPanel.SetActive(false);
        GameParameters[] gameParametersList = Resources.LoadAll<GameParameters>("ScriptableObjects/Parameters");
        _gameParameters = new Dictionary<string, GameParameters>();

        foreach (GameParameters gameParameter in gameParametersList)
            _gameParameters[gameParameter.GetParametersName()] = gameParameter;

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
            Transform transformGameResourceDisplay;

            foreach (ResourceValue resource in data.Cost)
            {
                transformGameResourceDisplay = GameObject.Instantiate(_gameResourceDisplayPrefab, _infoPanelResourcesCostParent).transform;
                transformGameResourceDisplay.Find("Icon").GetComponent<Image>().sprite =
                    Resources.Load<Sprite>($"Textures/GameResources/{resource.Resource}");
                transformGameResourceDisplay.Find("Text").GetComponent<TMP_Text>().text = resource.Amount.ToString();
                Color invalidTextColor = Color.red;

                if (Globals.GameResources[resource.Resource].Amount < resource.Amount)
                    transformGameResourceDisplay.Find("Text").GetComponent<TMP_Text>().color = invalidTextColor;
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

    private void AddBuildingButtonListener(Button button, int i)
    {
        button.onClick.AddListener(() => _buildingPlacer.SelectPlacedBuilding(i));
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
            TMP_Text tmpText = alreadyInstantiatedChild.Find("Count").GetComponent<TMP_Text>();
            int count = int.Parse(tmpText.text);
            tmpText.text = (count + 1).ToString();
        }
        else
        {
            GameObject selectedUnitInfo = GameObject.Instantiate(_selectedUnitInfoPrefab,
                                                                 _selectedUnitsListParent);
            selectedUnitInfo.name = unit.Code;
            Transform transform = selectedUnitInfo.transform;
            transform.Find("Count").GetComponent<TMP_Text>().text = "1";
            transform.Find("UnitType").GetComponent<TMP_Text>().text = unit.Data.UnitName;
        }
    }

    private void RemoveSelectedUnitFromUIList(string code)
    {
        Transform listItem = _selectedUnitsListParent.Find(code);

        if (listItem == null)
            return;

        TMP_Text tmpText = listItem.Find("Count").GetComponent<TMP_Text>();
        int count = int.Parse(tmpText.text);
        count -= 1;

        if (count == 0)
            Destroy(listItem.gameObject);
        else
            tmpText.text = count.ToString();
    }

    private void AddUnitSkillButtonListener(Button button, int i)
    {
        button.onClick.AddListener(() => _selectedUnit.TriggerSkill(i));
    }

    private void SetSelectedUnitMenu(Unit unit)
    {
        _selectedUnit = unit;
        _selectedUnitTitleText.text = unit.Data.UnitName;

        foreach (Transform child in _selectedUnitResourcesProductionParent)
            Destroy(child.gameObject);

        if (unit.Data.IsHasProduction)
        {
            Transform gameResourceCost;

            foreach (var productionModel in unit.Data.ProductionModels)
            {
                foreach (var resource in productionModel.Productions)
                {
                    gameResourceCost = GameObject.Instantiate(
                            _gameResourceCostPrefab, _selectedUnitResourcesProductionParent).transform;
                    gameResourceCost.Find("Text").GetComponent<TMP_Text>().text = $"+{resource.Value}";
                    gameResourceCost.Find("Icon").GetComponent<Image>().sprite =
                                Resources.Load<Sprite>($"Textures/GameResources/{resource.Key}");
                }
            }
        }

        foreach (Transform child in _selectedUnitActionButtonsParent)
            Destroy(child.gameObject);

        if (unit.SkillManagers.Count > 0)
        {
            GameObject unitSkillButton;
            Button button;

            for (int i = 0; i < unit.SkillManagers.Count; i++)
            {
                unitSkillButton = GameObject.Instantiate(
                    _unitSkillButtonPrefab, _selectedUnitActionButtonsParent);
                button = unitSkillButton.GetComponent<Button>();
                unit.SkillManagers[i].SetButton(button);
                unitSkillButton.transform.Find("Text").GetComponent<TMP_Text>().text =
                    unit.SkillManagers[i].Skill.SkillName;
                AddUnitSkillButtonListener(button, i);
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
        GameObject gameSettingsMenuButton; 
        string parametersName;
        List<string> availableMenus = new List<string>();

        foreach (GameParameters parameters in _gameParameters.Values)
        {
            if (parameters.FieldsToShow.Count == 0)
                continue;

            gameSettingsMenuButton = GameObject.Instantiate(_gameSettingsMenuButtonPrefab,
                                                            _gameSettingsMenusParent);
            parametersName = parameters.GetParametersName();
            gameSettingsMenuButton.transform.Find("Text").GetComponent<TMP_Text>().text = parametersName;
            AddGameSettingsPanelMenuListener(gameSettingsMenuButton.GetComponent<Button>(), parametersName);
            availableMenus.Add(parametersName);
        }

        if (availableMenus.Count > 0)
            SetGameSettingsContent(availableMenus[0]);
    }

    private void AddGameSettingsPanelMenuListener(Button button, string menu)
    {
        button.onClick.AddListener(() => SetGameSettingsContent(menu));
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

        foreach (string fieldName in parameters.FieldsToShow)
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
                    Toggle toggle = gEditor.GetComponent<Toggle>();
                    toggle.isOn = (bool)field.GetValue(parameters);

                    toggle.onValueChanged.AddListener(delegate
                    {
                        OnGameSettingsToggleValueChanged(parameters, field, fieldName, toggle);
                    });
                }
                else if (field.FieldType == typeof(int) || field.FieldType == typeof(float))
                {
                    bool isRange = System.Attribute.IsDefined(field, typeof(RangeAttribute), false);

                    if (isRange)
                    {
                        RangeAttribute attr = (RangeAttribute)System.Attribute.GetCustomAttribute(field, typeof(RangeAttribute));
                        gEditor = Instantiate(_sliderPrefab);
                        Slider slider = gEditor.GetComponent<Slider>();
                        slider.minValue = attr.min;
                        slider.maxValue = attr.max;
                        slider.wholeNumbers = field.FieldType == typeof(int);
                        slider.value = field.FieldType == typeof(int)
                            ? (int)field.GetValue(parameters)
                            : (float)field.GetValue(parameters);

                        slider.onValueChanged.AddListener(delegate
                        {
                            OnGameSettingsSliderValueChanged(parameters, field, fieldName, slider);
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
        Vector3 position = (Vector3)values[1];

        if (production == null)
            return;

        foreach (Transform child in _placedBuildingProductionRectTransform.gameObject.transform)
            Destroy(child.gameObject);

        Transform gameResourceCost;

        foreach (KeyValuePair<InGameResource, int> pair in production)
        {
            gameResourceCost = GameObject.Instantiate(_gameResourceCostPrefab,
                                                      _placedBuildingProductionRectTransform.transform)
                                            .transform;
            gameResourceCost.Find("Text").GetComponent<TMP_Text>().text = $"+{pair.Value}";
            gameResourceCost.Find("Icon").GetComponent<Image>().sprite = Resources.Load<Sprite>($"Textures/GameResources/{pair.Key}");            
        }

        _placedBuildingProductionRectTransform.sizeDelta = new Vector2(80, 24 * production.Count);
        _placedBuildingProductionRectTransform.anchoredPosition =
            (Vector2)Camera.main.WorldToScreenPoint(position) / GameManager.Instance.CanvasScaleFactor
            + Vector2.down * 600f;
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
