using UnityEngine;

public class PlaneShape : MonoBehaviour
{
    public float wingSpan = 20f; // 翼幅
    public float wingWidth = 1f; // 翼の幅
    public float wingAngle;      // 翼の角度
    public float tailHeight = 1f; // 尾翼の高さ

    public Transform leftWing; // 左翼のTransform
    public Transform rightWing; // 右翼のTransform
    public Transform tail; // 尾翼のTransform

    private Rigidbody rb; // Rigidbodyコンポーネント

    public void ApplyShape()
    {
        // 翼の形状を適用
        leftWing.localScale = new Vector3(wingWidth, leftWing.localScale.y, wingSpan);
        rightWing.localScale = new Vector3(wingWidth, rightWing.localScale.y, wingSpan);

        //// 翼の角度を適用
        //leftWing.localRotation = Quaternion.Euler(0, 0, wingAngle);
        //rightWing.localRotation = Quaternion.Euler(0, 0, -wingAngle);

        // 尾翼の形状を適用
        tail.localScale = new Vector3(tail.localScale.x, tailHeight, tail.localScale.z);

        // 質量を適用
        rb = GetComponent<Rigidbody>(); // Rigidbodyコンポーネントの取得
        rb.mass = 0.3f * wingSpan * wingWidth + 0.1f * tailHeight;
    }

    private void FixedUpdate()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbodyコンポーネントの取得

        // 簡易的な揚力のシミュレーション
        float liftForce = wingSpan * wingAngle * 0.007f;   // 揚力をパラメータに基づき計算
        rb.AddForce(Vector3.up * liftForce);             // 上向きの力を追加

        //// 翼の角度に応じた揚力の調整
        //if (wingAngle > 5.0f)
        //{
        //    rb.AddTorque(Vector3.right * 0.5f);          // 安定化のためのトルク
        //}
    }
}
