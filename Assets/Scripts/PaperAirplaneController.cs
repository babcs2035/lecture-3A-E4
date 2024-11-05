using UnityEngine;

public class PaperAirplaneController : MonoBehaviour
{
    [SerializeField] private float launchForce = 10f;       // 初速
    [SerializeField] private float liftCoefficient = 0.5f;  // 揚力係数
    [SerializeField] private float dragCoefficient = 0.02f; // 空気抵抗係数

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Launch();
    }

    void Launch()
    {
        // 初速で前方に飛ばす
        rb.AddForce(transform.forward * launchForce, ForceMode.VelocityChange);
    }

    void FixedUpdate()
    {
        ApplyAerodynamicForces();
    }

    void ApplyAerodynamicForces()
    {
        // 揚力の計算
        Vector3 lift = liftCoefficient * rb.linearVelocity.magnitude * transform.up;
        rb.AddForce(lift);

        // 空気抵抗の計算
        Vector3 drag = dragCoefficient * rb.linearVelocity.sqrMagnitude * -rb.linearVelocity.normalized;
        rb.AddForce(drag);
    }
}
