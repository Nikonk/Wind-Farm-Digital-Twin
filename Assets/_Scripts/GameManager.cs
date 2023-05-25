using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Canvas")]
    public Canvas canvas;
    public float canvasScaleFactor;

    private Ray _ray;
    private RaycastHit _raycastHit;

    public GameGlobalParameters gameGlobalParameters;

    [HideInInspector] public bool gameIsPaused;

    private List<Unit> _producingUnits = new List<Unit>();
    private Coroutine _producingResourcesCoroutine = null;

    private List<Unit> _consumingUnits = new List<Unit>();
    private Coroutine _consumingResourcesCoroutine = null;

    [Header("Minimap")]
    public Collider mapWrapperCollider;

    private void Awake() 
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        canvasScaleFactor = canvas.scaleFactor;

        DataHandler.LoadGameData();
        GetComponent<DayAndNightCycler>().enabled = gameGlobalParameters.enableDayAndNightCycle;

        Globals.NAV_MESH_SURFACE = GameObject.Find("Terrain").GetComponent<NavMeshSurface>();
        Globals.UpdateNavMeshSurface();

        gameIsPaused = false;
    }

    private void Start() 
    {
        _producingResourcesCoroutine = StartCoroutine("_ProducingResources");
        _consumingResourcesCoroutine = StartCoroutine("_ConsumingResources");
    }

    private void Update() 
    {
        if (gameIsPaused) return;
        _CheckUnitsNavigation();
    }

    private void OnEnable()
    {
        EventManager.AddListener("PauseGame", _OnPauseGame);
        EventManager.AddListener("ResumeGame", _OnResumeGame);
        EventManager.AddListener("UpdateGameParameter:enableDayAndNightCycle", _OnUpdateDayAndNightCycle);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PauseGame", _OnPauseGame);
        EventManager.RemoveListener("ResumeGame", _OnResumeGame);
        EventManager.RemoveListener("UpdateGameParameter:enableDayAndNightCycle", _OnUpdateDayAndNightCycle);
    }

    private void OnApplicationQuit() 
    {
#if !UNITY_EDITOR
        DataHandler.SaveGameData();
#endif
    }

    private void _CheckUnitsNavigation()
    {
        if (Globals.SELECTED_UNITS.Count > 0 && Input.GetMouseButtonUp(1))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(
                _ray,
                out _raycastHit,
                1000f,
                Globals.TERRAIN_LAYER_MASK
            ))
            {
                foreach (UnitManager um in Globals.SELECTED_UNITS)
                    if (um.GetType() == typeof(CharacterManager))
                        ((CharacterManager)um).MoveTo(_raycastHit.point);
            }
        }
    }

    private void _OnPauseGame()
    {
        gameIsPaused = true;
        Time.timeScale = 0;

        if (_producingResourcesCoroutine != null)
        {
            StopCoroutine(_producingResourcesCoroutine);
            _producingResourcesCoroutine = null;
        }
        if (_consumingResourcesCoroutine != null)
        {
            StopCoroutine(_consumingResourcesCoroutine);
            _consumingResourcesCoroutine = null;
        }
    }

    private void _OnResumeGame()
    {
        gameIsPaused = false;
        Time.timeScale = 1;
        
        if (_producingResourcesCoroutine == null)
            _producingResourcesCoroutine = StartCoroutine("_ProducingResources");
        if (_consumingResourcesCoroutine == null)
            _consumingResourcesCoroutine = StartCoroutine("_ConsumingResources");
    }

    private void _OnUpdateDayAndNightCycle(object data)
    {
        bool dayAndNightIsOn = (bool)data;
        GetComponent<DayAndNightCycler>().enabled = dayAndNightIsOn;
    }

    public void AddProducingUnits(Unit productionUnit)
    {
        _producingUnits.Add(productionUnit);
    }

    public void AddConsumingUnits(Unit consumptionUnit)
    {
        _consumingUnits.Add(consumptionUnit);
    }

    public void RemoveOperatingUnit(Unit unit) 
    {
        if ( _producingUnits.Contains(unit) )
            _producingUnits.Remove(unit);
        
        if ( _consumingUnits.Contains(unit) )
            _consumingUnits.Remove(unit); 
    }

    private IEnumerator _ProducingResources()
    {
        while (true)
        {
            float producingRate = 2f;
            foreach (var unit in _producingUnits)
            {
                foreach (var producingModel in unit.Data.ProductionModels)
                    producingModel.Produce();
            }
            EventManager.TriggerEvent("UpdateResourceTexts");
            yield return new WaitForSeconds(producingRate);
        }
    }

    private IEnumerator _ConsumingResources()
    {
        while (true)
        {
            
            float consumingRate = 2f;
            foreach (var unit in _consumingUnits)
            {
                foreach (var consumptionModel in unit.Data.ConsumptionModels)
                    consumptionModel.Consume();
            }
            EventManager.TriggerEvent("UpdateResourceTexts");
            yield return new WaitForSeconds(consumingRate);
        }
    }
}
