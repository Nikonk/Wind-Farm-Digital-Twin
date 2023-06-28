using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private float canvasScaleFactor;

    private Ray _ray;
    private RaycastHit _raycastHit;

    [Header("Minimap")]
    [SerializeField] private Collider mapWrapperCollider;


    private List<Unit> _producingUnits = new List<Unit>();
    private Coroutine _producingResourcesCoroutine = null;

    private List<Unit> _consumingUnits = new List<Unit>();
    private Coroutine _consumingResourcesCoroutine = null;

    private List<Unit> _transferUnits = new List<Unit>();
    private Coroutine _transferResourcesCoroutine = null;

    public Collider MapWrapperCollider => mapWrapperCollider;
    public float CanvasScaleFactor => canvasScaleFactor;
    public bool IsGamePaused { get; private set; }

    private void Awake() 
    {
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;

        canvasScaleFactor = canvas.scaleFactor;

        DataHandler.LoadGameData();

        IsGamePaused = false;
    }

    private void Start() 
    {
        // _producingResourcesCoroutine = StartCoroutine(ProducingResources());
        // _consumingResourcesCoroutine = StartCoroutine(ConsumingResources());
        _transferResourcesCoroutine = StartCoroutine(TransferResources());
    }

    private void Update() 
    {
        if (IsGamePaused)
            return;

        CheckUnitsNavigation();
    }

    private void OnEnable()
    {
        EventManager.AddListener("PauseGame", OnPauseGame);
        EventManager.AddListener("ResumeGame", OnResumeGame);
    }

    private void OnDisable()
    {
        EventManager.RemoveListener("PauseGame", OnPauseGame);
        EventManager.RemoveListener("ResumeGame", OnResumeGame);
    }

    private void OnApplicationQuit() 
    {
#if !UNITY_EDITOR
        DataHandler.SaveGameData();
#endif
    }

    public void AddProducingUnits(Unit productionUnit)
    {
        _producingUnits.Add(productionUnit);
    }

    public void AddConsumingUnits(Unit consumptionUnit)
    {
        _consumingUnits.Add(consumptionUnit);
    }

    public void AddTransferUnits(Unit transferUnit)
    {
        _transferUnits.Add(transferUnit);
    }

    public void RemoveOperatingUnit(Unit unit) 
    {
        if ( _producingUnits.Contains(unit) )
            _producingUnits.Remove(unit);
        
        if ( _consumingUnits.Contains(unit) )
            _consumingUnits.Remove(unit);        
        
        if ( _transferUnits.Contains(unit) )
            _transferUnits.Remove(unit);
    }

    private void CheckUnitsNavigation()
    {
        if (Globals.SelectedUnits.Count > 0 && Input.GetMouseButtonUp(1))
        {
            _ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(
                _ray,
                out _raycastHit,
                1000f,
                Globals.TerrainLayerMask
            ))
            {
                foreach (UnitManager um in Globals.SelectedUnits)
                    if (um.GetType() == typeof(CharacterManager))
                        ((CharacterManager)um).MoveTo(_raycastHit.point);
            }
        }
    }

    private void OnPauseGame()
    {
        IsGamePaused = true;
        Time.timeScale = 0;

        // if (_producingResourcesCoroutine != null)
        // {
        //     StopCoroutine(_producingResourcesCoroutine);
        //     _producingResourcesCoroutine = null;
        // }

        // if (_consumingResourcesCoroutine != null)
        // {
        //     StopCoroutine(_consumingResourcesCoroutine);
        //     _consumingResourcesCoroutine = null;
        // }

        if (_transferResourcesCoroutine != null)
        {
            StopCoroutine(_transferResourcesCoroutine);
            _transferResourcesCoroutine = null;
        }
    }

    private void OnResumeGame()
    {
        IsGamePaused = false;
        Time.timeScale = 1;

        // _producingResourcesCoroutine ??= StartCoroutine(ProducingResources());

        // _consumingResourcesCoroutine ??= StartCoroutine(ConsumingResources());

        _transferResourcesCoroutine ??= StartCoroutine(TransferResources());
    }

    private IEnumerator ProducingResources()
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

    private IEnumerator ConsumingResources()
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

    private IEnumerator TransferResources()
    {
        while (true)
        {
            float transferRate = 1f;

            foreach (var unit in _transferUnits)
            {
                foreach (var transferModel in unit.Data.TransferModels)
                {
                    transferModel.Transfer();
                }
            }

            EventManager.TriggerEvent("UpdateResourceTexts");
            yield return new WaitForSeconds(transferRate);
        }
    }
}
