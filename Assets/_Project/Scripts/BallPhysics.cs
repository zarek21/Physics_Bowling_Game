using UnityEngine;

public class BallPhysics : MonoBehaviour
{
    [Header("Variables Físicas")]
    [Tooltip("Vector de fuerza del lanzamiento")]
    public Vector3 Force;


    [Tooltip("El Coeficiente de fricción cinética")]
    public float Friction;

    
    [Tooltip("Gravedad (g)")]
    private const float GRAVITY = 9.81f;
    private Vector3 _velocity;
    private bool _isMoving;

    void Start()
    {
        _velocity = Force;
        _isMoving = true;
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
    }
}