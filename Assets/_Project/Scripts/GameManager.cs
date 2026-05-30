using UnityEngine;
using System;
using System.Collections.Generic;

public enum GameState
{
    BallSelection,
    WaitingThrow,
    BowlSpinning,
    Win,
    Lose
}

[System.Serializable]
public struct LevelConfig
{
    [Tooltip("Nombre descriptivo del nivel")]
    public string LevelName;
    
    [Tooltip("Prefab específico de la zona de victoria para este nivel (dejar vacío para usar la existente en la escena)")]
    public GameObject WinZonePrefab;
    
    [Tooltip("Posición absoluta en el mundo de la zona de victoria (dejar a Vector3.zero para usar la original)")]
    public Vector3 WinZonePosition;
    
    [Tooltip("Fuerza base de lanzamiento para este nivel (dejar en cero para usar la por defecto de la bola)")]
    public Vector3 CustomForce;
}

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static bool HasInstance => _instance != null;
    
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<GameManager>(FindObjectsInactive.Include);
                if (_instance == null)
                {
                    GameObject go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    Debug.Log("[GameManager] Auto-creado GameManager porque no existía en la escena.");
                }
            }
            return _instance;
        }
    }

    [Header("Configuración de Niveles")]
    public List<LevelConfig> Levels = new List<LevelConfig>();
    public int CurrentLevelIndex { get; private set; } = 0;

    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnStateChanged;

    private Vector3 _originalWinZonePos;
    private bool _hasSavedOriginalPos = false;
    private GameObject _instantiatedWinZoneObj;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        if (Levels == null || Levels.Count == 0)
        {
            Levels = new List<LevelConfig>
            {
                new LevelConfig { LevelName = "Nivel 1", WinZonePrefab = null, WinZonePosition = Vector3.zero, CustomForce = Vector3.zero },
                new LevelConfig { LevelName = "Nivel 2", WinZonePrefab = null, WinZonePosition = Vector3.zero, CustomForce = Vector3.zero },
                new LevelConfig { LevelName = "Nivel 3", WinZonePrefab = null, WinZonePosition = Vector3.zero, CustomForce = Vector3.zero },
                new LevelConfig { LevelName = "Nivel 4", WinZonePrefab = null, WinZonePosition = Vector3.zero, CustomForce = Vector3.zero },
                new LevelConfig { LevelName = "Nivel 5", WinZonePrefab = null, WinZonePosition = Vector3.zero, CustomForce = Vector3.zero }
            };
        }
    }

    private void Start()
    {
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (currentScene == "BallSelectionScreen")
        {
            ChangeState(GameState.BallSelection);
        }
        else
        {
            ChangeState(GameState.WaitingThrow);
        }
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        _hasSavedOriginalPos = false;
        _instantiatedWinZoneObj = null;
        ApplyLevelConfig();
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnStateChanged?.Invoke(CurrentState);
    }

    public LevelConfig GetCurrentLevelConfig()
    {
        if (Levels == null || Levels.Count == 0)
        {
            return new LevelConfig { LevelName = "Nivel Default", WinZonePrefab = null, WinZonePosition = Vector3.zero, CustomForce = Vector3.zero };
        }
        int index = Mathf.Clamp(CurrentLevelIndex, 0, Levels.Count - 1);
        return Levels[index];
    }

    public bool HasNextLevel()
    {
        return CurrentLevelIndex < Levels.Count - 1;
    }

    public void AdvanceLevel()
    {
        if (HasNextLevel())
        {
            CurrentLevelIndex++;
        }
        else
        {
            CurrentLevelIndex = 0;
        }
    }

    public void ResetLevels()
    {
        CurrentLevelIndex = 0;
    }

    public void ApplyLevelConfig()
    {
        WinZone existingWinZone = FindFirstObjectByType<WinZone>();
        LevelConfig config = GetCurrentLevelConfig();

        if (config.WinZonePrefab != null)
        {
            if (_instantiatedWinZoneObj == null)
            {
                Vector3 targetPos = config.WinZonePosition;
                if (targetPos == Vector3.zero && existingWinZone != null)
                {
                    targetPos = existingWinZone.transform.position;
                }

                _instantiatedWinZoneObj = Instantiate(config.WinZonePrefab, targetPos, config.WinZonePrefab.transform.rotation);
                _instantiatedWinZoneObj.name = config.WinZonePrefab.name;

                if (existingWinZone != null && existingWinZone.gameObject != _instantiatedWinZoneObj)
                {
                    Destroy(existingWinZone.gameObject);
                }
                
                Debug.Log($"[GameManager] WinZone instanciada desde Prefab '{config.WinZonePrefab.name}' en posición: {targetPos} para {config.LevelName}");
            }
        }
        else if (existingWinZone != null)
        {
            if (!_hasSavedOriginalPos)
            {
                _originalWinZonePos = existingWinZone.transform.position;
                _hasSavedOriginalPos = true;
            }

            existingWinZone.transform.position = (config.WinZonePosition != Vector3.zero) 
                ? config.WinZonePosition 
                : _originalWinZonePos;

            Debug.Log($"[GameManager] WinZone existente re-posicionada en: {existingWinZone.transform.position} para {config.LevelName}");
        }

        BallPhysics ball = FindFirstObjectByType<BallPhysics>();
        if (ball != null)
        {
            if (config.CustomForce != Vector3.zero)
            {
                ball.Force = config.CustomForce;
                Debug.Log($"[GameManager] Fuerza de la bola configurada a: {ball.Force} para {config.LevelName}");
            }
        }
    }
}
