using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public float thrust = 10f; // 推進力
    public float lift = 5f; // 揚力

    private Rigidbody rb; // Rigidbodyコンポーネント

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbodyコンポーネントの取得
    }

    void FixedUpdate()
    {
        // 前方に推進力を加える
        rb.AddForce(transform.forward * thrust);

        // 上向きに揚力を加える
        rb.AddForce(transform.up * lift);
    }
}
