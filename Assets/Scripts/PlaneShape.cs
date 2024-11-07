using UnityEngine;

public class PlaneShape : MonoBehaviour
{
    public float wingSpan; // 翼幅
    public float wingWidth; // 翼の幅
    public float wingAngle; // 翼角度
    public float wingShape; // 翼形状
    public float tailHeight; // 尾翼の高さ
    public float centerOfMass; // 重心位置
    public float mass; // 質量

    public float initialSpeed; // 初速度
    public Vector3 initialRotation; // 初期回転角度

    public Transform leftWing; // 左翼のTransform
    public Transform rightWing; // 右翼のTransform
    public Transform tail; // 尾翼のTransform

    private Rigidbody rb; // Rigidbodyコンポーネント

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = new Vector3(-1, 0, 0) * initialSpeed; // 初速度を設定
        rb.angularVelocity = new Vector3(0, 0.2f, 0);   // 軽い回転を加えることで安定化を試みる
        transform.rotation = Quaternion.Euler(initialRotation); // 初期回転角度を設定

        ApplyShape(); // 初期形状の適用
    }

    public void ApplyShape()
    {
        rb = GetComponent<Rigidbody>();

        // 翼の形状を適用
        leftWing.localScale = new Vector3(wingWidth, leftWing.localScale.y, wingSpan);
        rightWing.localScale = new Vector3(wingWidth, rightWing.localScale.y, wingSpan);

        // 翼の角度を適用
        //leftWing.localEulerAngles = new Vector3(wingAngle, leftWing.localEulerAngles.y, leftWing.localEulerAngles.z);
        //rightWing.localEulerAngles = new Vector3(wingAngle, rightWing.localEulerAngles.y, rightWing.localEulerAngles.z);

        // 翼形状を適用（例として、翼の厚さを変更）
        leftWing.localScale = new Vector3(leftWing.localScale.x, wingShape, leftWing.localScale.z);
        rightWing.localScale = new Vector3(rightWing.localScale.x, wingShape, rightWing.localScale.z);

        // 尾翼の形状を適用
        tail.localScale = new Vector3(tail.localScale.x, tailHeight, tail.localScale.z);

        // 重心位置を適用
        rb.centerOfMass = new Vector3(centerOfMass, rb.centerOfMass.y, rb.centerOfMass.z);

        // 質量を適用
        rb.mass = 0.5f * wingSpan * wingWidth * wingShape + 0.15f * tailHeight;
    }

    private void FixedUpdate()
    {
        // 簡易的な揚力のシミュレーション
        float liftForce = wingSpan * wingAngle * 0.007f;   // 揚力をパラメータに基づき計算
        rb.AddForce(Vector3.up * liftForce);             // 上向きの力を追加

        // 翼の角度に応じた揚力の調整
        if (wingAngle > 5.0f)
        {
            //rb.AddTorque(Vector3.right * 0.05f);          // 安定化のためのトルク
        }
    }
}
