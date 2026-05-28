using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BallSelector : MonoBehaviour
{
    [System.Serializable]
    public struct BallData
    {
        public string Name;
        public GameObject Prefab;
    }

    // Persistencia de la bola seleccionada
    public static GameObject SelectedBallPrefab { get; private set; }

    [Header("Bolas Disponibles")]
    [SerializeField] private BallData[] balls = new BallData[]
    {
        new BallData { Name = "ICE" },
        new BallData { Name = "FIRE" },
        new BallData { Name = "PLASMA" }
    };

    private int _currentIdx = 0;
    private GameObject _currentBallInstance;

    // Elementos UI
    private Button _prevButton;
    private Button _nextButton;

    private Button _playButton;

    private void Start()
    {
        // 1. Obtener UIDocument adjunto a este GameObject
        var uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("No se encontró el componente UIDocument en este objeto. Asegúrate de agregarlo.");
            return;
        }

        // 2. Buscar elementos en la UI
        var root = uiDocument.rootVisualElement;
        _prevButton = root.Q<Button>("PreviousButton");
        _nextButton = root.Q<Button>("NextButton");
        _playButton = root.Q<Button>("PlayButton");

        // 3. Suscribir los eventos de botones si existen
        if (_prevButton != null) _prevButton.clicked += SelectPrevious;
        if (_nextButton != null) _nextButton.clicked += SelectNext;
        if (_playButton != null) _playButton.clicked += ConfirmSelection;

        // 4. Mostrar la primera bola (o la previamente seleccionada)
        SpawnSelectedBall();
    }

    public void SelectNext()
    {
        if (balls == null || balls.Length == 0) return;
        _currentIdx = (_currentIdx + 1) % balls.Length;
        SpawnSelectedBall();
    }

    public void SelectPrevious()
    {
        if (balls == null || balls.Length == 0) return;
        _currentIdx = (_currentIdx - 1 + balls.Length) % balls.Length;
        SpawnSelectedBall();
    }

    private void SpawnSelectedBall()
    {
        if (balls == null || balls.Length == 0 || _currentIdx >= balls.Length) return;

        // 1. Destruir la bola de previsualización anterior si existe
        if (_currentBallInstance != null)
        {
            Destroy(_currentBallInstance);
        }

        var currentBallData = balls[_currentIdx];

        // 2. Instanciar el nuevo prefab si está asignado para previsualización
        if (currentBallData.Prefab != null)
        {
            _currentBallInstance = Instantiate(currentBallData.Prefab, transform.position, transform.rotation);
            
            // Opcional: Desactivar físicas o scripts de lanzamiento durante la previsualización si interfieren
            var ballPhysics = _currentBallInstance.GetComponent<BallPhysics>();
            if (ballPhysics != null)
            {
                ballPhysics.enabled = false; // Desactivar simulación física en la pantalla de selección
            }
        }
        else
        {
            Debug.LogWarning($"El prefab para {currentBallData.Name} no está asignado en el Inspector de BallSelector.");
        }
    }

    private void ConfirmSelection()
    {
        SelectedBallPrefab = balls[_currentIdx].Prefab;
        Debug.Log($"Selección confirmada. Cargando escena de juego con bola: {balls[_currentIdx].Name}");
        SceneManager.LoadScene("GameScene");
    }

    private void OnDestroy()
    {
        if (_prevButton != null) _prevButton.clicked -= SelectPrevious;
        if (_nextButton != null) _nextButton.clicked -= SelectNext;
        if (_playButton != null) _playButton.clicked -= ConfirmSelection;
    }
}
