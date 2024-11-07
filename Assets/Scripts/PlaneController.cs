using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    public float initialSpeed = 20f; // 初速度
    public Vector3 initialRotation = new Vector3(-90, 0, 0); // 初期回転角度

    private Rigidbody rb; // Rigidbodyコンポーネント

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbodyコンポーネントの取得

        rb.linearVelocity = new Vector3(-1, 0, 0) * initialSpeed; // 初速度を設定
        rb.angularVelocity = new Vector3(0, 0.2f, 0);   // 軽い回転を加えることで安定化を試みる
        transform.rotation = Quaternion.Euler(initialRotation); // 初期回転角度を設定
    }
}
