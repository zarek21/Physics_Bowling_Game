using UnityEngine;

public class WinZone : MonoBehaviour
{
    public static WinZone Instance { get; private set; }

    [Header("Configuración de Victoria")]
    [Tooltip("Tiempo mínimo en segundos que la bola debe permanecer quieta dentro de la zona para ganar.")]
    public float timeInZoneToWin = 2.5f;

    private bool _ballInside = false;
    private float _timer = 0f;
    private bool _hasWon = false;
    private BallPhysics _ball;
    private bool _hasStartedFilling = false;

    private void Awake()
    {
        Instance = this;
    }

    public bool IsBallInside => _ballInside;
    public bool HasWon => _hasWon;
    public float CurrentTime => _timer;
    public float TargetTime => timeInZoneToWin;

    private void OnTriggerEnter(Collider other)
    {
        var ball = other.GetComponentInParent<BallPhysics>();
        if (ball != null)
        {
            _ball = ball;
            _ballInside = true;
            _timer = 0f;
            _hasStartedFilling = false;
            Debug.Log("La bola ingresó a la zona de victoria.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        var ball = other.GetComponentInParent<BallPhysics>();
        if (ball != null && ball == _ball)
        {
            _ballInside = false;

            
            if (_hasStartedFilling && !_hasWon)
            {
                _timer = 0f;
                _hasStartedFilling = false;
                Debug.Log("[WinZone] La bola empezó a llenar la barra y salió de la zona de victoria. ¡Derrota!");
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ChangeState(GameState.Lose);
                }
            }
            else
            {
                _timer = 0f;
                _hasStartedFilling = false;
            }

            Debug.Log("La bola salió de la zona de victoria.");
        }
    }

    private void Update()
    {
        if (_hasWon) return;

        if (_ballInside && _ball != null)
        {
            Rigidbody rb = _ball.GetComponent<Rigidbody>();
            float speed = rb != null ? rb.linearVelocity.magnitude : 0f;

            
            if (speed < 0.15f)
            {
                _timer += Time.deltaTime;
                if (_timer > 0.05f)
                {
                    _hasStartedFilling = true; 
                }

                if (_timer >= timeInZoneToWin)
                {
                    _timer = timeInZoneToWin; 
                    _hasWon = true;
                    Debug.Log("¡Condición de victoria alcanzada!");
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.ChangeState(GameState.Win);
                    }
                }
            }
            else
            {
                
                _timer = 0f;
            }
        }
    }
}
