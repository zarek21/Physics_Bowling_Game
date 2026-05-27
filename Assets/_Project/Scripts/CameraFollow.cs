using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform Target;
    public Vector3 Offset = new Vector3(0f, 2f, -5f);

    void LateUpdate()
    {
        if (Target != null)
        {
            transform.position = Target.position + Offset;
        }
    }
}
