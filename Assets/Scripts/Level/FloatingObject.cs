using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FloatingObject : MonoBehaviour
{
    [Header("Repulsion")]
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float acceleration = 10f;

    private Rigidbody _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.useGravity = false;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player") && !other.CompareTag("FloatingObject"))
            return;

        Vector3 awayDir = (transform.position - other.transform.position);
        awayDir.y = 0f;

        if (awayDir.sqrMagnitude < 0.0001f)
            return;

        awayDir.Normalize();

        // Project current speed along away direction
        float currentSpeed = Vector3.Dot(_rb.linearVelocity, awayDir);

        // Increase speed gradually
        currentSpeed += acceleration * Time.fixedDeltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0f, maxSpeed);

        // Immediately redirect velocity, but with gradual speed build-up
        _rb.linearVelocity = awayDir * currentSpeed;
    }
}
