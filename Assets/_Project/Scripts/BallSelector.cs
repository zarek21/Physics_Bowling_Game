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
        public GameObject PreviewPrefab;
        public GameObject GameplayPrefab;
    }

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

    private Button _prevButton;
    private Button _nextButton;
    private Button _playButton;

    private void Start()
    {
        var uiDocument = GetComponent<UIDocument>();

        if (uiDocument == null)
        {
            Debug.LogError("No se encontró el componente UIDocument en este objeto. Asegúrate de agregarlo.");
            return;
        }

        var root = uiDocument.rootVisualElement;
        _prevButton = root.Q<Button>("PreviousButton");
        _nextButton = root.Q<Button>("NextButton");
        _playButton = root.Q<Button>("PlayButton");

        if (_prevButton != null) _prevButton.clicked += SelectPrevious;
        if (_nextButton != null) _nextButton.clicked += SelectNext;
        if (_playButton != null) _playButton.clicked += ConfirmSelection;

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

        if (_currentBallInstance != null)
        {
            Destroy(_currentBallInstance);
        }

        var currentBallData = balls[_currentIdx];

        if (currentBallData.PreviewPrefab != null)
        {
            _currentBallInstance = Instantiate(currentBallData.PreviewPrefab, transform.position, transform.rotation);
            
            var ballPhysics = _currentBallInstance.GetComponent<BallPhysics>();
            if (ballPhysics != null)
            {
                ballPhysics.enabled = false;
            }
        }
        else
        {
            Debug.LogWarning($"El prefab de previsualización para {currentBallData.Name} no está asignado en el Inspector de BallSelector.");
        }
    }

    private void ConfirmSelection()
    {
        SelectedBallPrefab = balls[_currentIdx].GameplayPrefab;
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
