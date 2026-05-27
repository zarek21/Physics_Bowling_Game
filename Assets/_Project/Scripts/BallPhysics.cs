using UnityEngine;
using UnityEngine.InputSystem;

public class BallPhysics : MonoBehaviour
{
    
    [Header("Escena")]
    public Transform BallMesh;
    public LineRenderer AimLine;

    [Header("Variables Físicas")]
    [Tooltip("Vector de fuerza del lanzamiento")]
    public Vector3 Force;


    [Tooltip("El Coeficiente de fricción cinética")]
    public float Friction;

    public float Radius = 0.11f;


    
    [Tooltip("Gravedad (g)")]
    private const float GRAVITY = 9.81f;
    private Vector3 _velocity;
    private bool _isMoving;

    [Header("Apuntado")]
    public float MouseSensitivity = 2f;
    public float MaxAimAngle = 30f;
    
    private float _currentAimAngle = 0f;
    private bool _isLaunched = false;

    void Start()
    {
        _velocity = Vector3.zero;
        _isMoving = false;
        _isLaunched = false;
    }

    void Update()
    {
        if (_isLaunched) return;

        if (Mouse.current == null) return;

        float _mouseInput = Mouse.current.delta.x.ReadValue();

        _currentAimAngle += _mouseInput * MouseSensitivity * 0.05f;
        _currentAimAngle = Mathf.Clamp(_currentAimAngle, -MaxAimAngle, MaxAimAngle);

        Vector3 _aimDirection = Quaternion.Euler(0, _currentAimAngle, 0) * Force.normalized;
        Debug.DrawRay(transform.position, _aimDirection * 5f, Color.red);
        if(AimLine != null)
        {
            AimLine.enabled = true;
            AimLine.SetPosition(0, transform.position);
            AimLine.SetPosition(1, transform.position + _aimDirection * 5f);
        }
    
    
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            LaunchBall(_aimDirection);
        }
    }

    void FixedUpdate()
    {
        if(_isMoving == false)
        {
            return;
        }

        // Fórmula Dinámica: Desaceleración por fricción 
        // La masa se omite porque se cancela
        float _deceleration = Friction * GRAVITY;

        if(_velocity.magnitude > 0.1f)
        {
            // Fórmula Cinemática: vf = vi + a * t
            _velocity -= _velocity.normalized * _deceleration * Time.fixedDeltaTime;
        }
        else
        {
            _isMoving = false;
            _velocity = Vector3.zero;
        }

        transform.position += _velocity * Time.fixedDeltaTime;

        if (_velocity.magnitude > 0.01f && BallMesh != null)
        {
            float distanceTraveled = _velocity.magnitude * Time.fixedDeltaTime;
            float angleDeg = (distanceTraveled / Radius) * Mathf.Rad2Deg;
            Vector3 rotationAxis = Vector3.Cross(Vector3.up, _velocity.normalized);
            BallMesh.Rotate(rotationAxis, angleDeg, Space.World);
        }
    }

    void LaunchBall(Vector3 direction)
    {
        _velocity = direction * Force.magnitude;
        _isMoving = true;
        _isLaunched = true;

        if(AimLine != null)
        {
            AimLine.enabled = false;
        }
    }
}