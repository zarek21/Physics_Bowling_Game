using UnityEngine;
using UnityEngine.SceneManagement;

public class BallRotation : MonoBehaviour
{
    [Header("Órbita (Movimiento circular para el trail)")]
    [Tooltip("Velocidad a la que gira alrededor del centro")]
    public float RotationSpeed = 150f;
    [Tooltip("Tamaño del círculo (Radio)")]
    public float RotationSize = 1f;
    [Tooltip("Eje sobre el que hace el círculo")]
    public Vector3 RotationAxis = Vector3.up;

    [Header("Giro sobre sí misma")]
    [Tooltip("Velocidad de rotación de la bola en su propio centro")]
    public float SpinSpeed = 300f;
    [Tooltip("Eje de rotación de la textura de la bola")]
    public Vector3 SpinAxis = Vector3.right;

    private Vector3 _startPosition;
    private float _currentAngle = 0f;
    private Vector3 _orbitOffsetDirection;

    void Start()
    {
        // 1. Si estamos en el juego real, este script se autodestruye
        // para no interferir con las físicas normales del lanzamiento.
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            Destroy(this);
            return;
        }

        // 2. Si estamos en la pantalla de selección:
        _startPosition = transform.position;

        // Calcular una dirección inicial para el radio del círculo
        _orbitOffsetDirection = Vector3.Cross(RotationAxis, Vector3.up).normalized;
        if (_orbitOffsetDirection.sqrMagnitude < 0.001f) 
        {
            _orbitOffsetDirection = Vector3.Cross(RotationAxis, Vector3.right).normalized;
        }

        // 3. Modificar el Rigidbody para que flote sin caerse
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Desactiva las físicas para que quede flotando
            rb.useGravity = false;
        }
    }

    void Update()
    {
        // 1. Movimiento en círculo (Órbita) para mostrar el Trail
        _currentAngle += RotationSpeed * Time.deltaTime;
        Quaternion orbitRotation = Quaternion.AngleAxis(_currentAngle, RotationAxis);
        transform.position = _startPosition + (orbitRotation * _orbitOffsetDirection * RotationSize);

        // 2. Rotación de la bola sobre sí misma
        transform.Rotate(SpinAxis * (SpinSpeed * Time.deltaTime), Space.Self);
    }
}
