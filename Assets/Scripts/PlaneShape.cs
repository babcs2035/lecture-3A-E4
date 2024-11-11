using UnityEngine;

public class PlaneShape : MonoBehaviour
{
    public float wingSpan; // 翼幅
    public float wingWidth; // 翼の幅
    public float wingAngle; // 翼角度
    public float wingShape; // 翼形状
    public float centerOfMass; // 重心位置
    public float mass; // 質量
    public float liftScale = 0.5f; // 揚力スケーリングファクター

    public float initialSpeed; // 初速度
    public Vector3 initialRotation; // 初期回転角度

    public Transform leftWing; // 左翼のTransform
    public Transform rightWing; // 右翼のTransform

    private Rigidbody rb; // Rigidbodyコンポーネント
    private BoxCollider boxCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = new Vector3(-1, 0, 0) * initialSpeed; // 初速度を設定
        rb.rotation = Quaternion.Euler(initialRotation); // 初期回転角度を設定
        //rb.angularVelocity = new Vector3(0, 0.2f, 0);   // 軽い回転を加えることで安定化を試みる
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
        leftWing.localEulerAngles = new Vector3(leftWing.localEulerAngles.x, 45, leftWing.localEulerAngles.z);
        rightWing.localEulerAngles = new Vector3(rightWing.localEulerAngles.x, 45, rightWing.localEulerAngles.z);

        // 翼形状を適用（例として、翼の厚さを変更）
        leftWing.localScale = new Vector3(leftWing.localScale.x, wingShape, leftWing.localScale.z);
        rightWing.localScale = new Vector3(rightWing.localScale.x, wingShape, rightWing.localScale.z);

        // 重心位置を適用
        rb.centerOfMass = new Vector3(centerOfMass, rb.centerOfMass.y, rb.centerOfMass.z);

        // 質量を適用
        CalcMass();
    }

    private void FixedUpdate()
    {
        // 簡易的な揚力のシミュレーション
        UpdateLiftForce();
    }

    private void UpdateLiftForce()
    {
        const float airDensity = 1.225f; // 空気の密度 (kg/m^3)
        const float liftCoefficient0 = 0.2f; // C_L0
        const float liftCoefficientAlpha = 5.7f; // C_Lα

        boxCollider = GetComponent<BoxCollider>();
        Vector3 size = boxCollider.size;
        Vector3 scale = transform.localScale;
        Vector3 dimensions = Vector3.Scale(size, scale);

        float speed = rb.linearVelocity.magnitude;
        float angleOfAttackRad = wingAngle * Mathf.Deg2Rad;
        float liftCoefficient = liftCoefficient0 + liftCoefficientAlpha * angleOfAttackRad;
        float wingArea = dimensions.x * dimensions.z; // 翼の面積 (m^2)
        float lift = 0.5f * airDensity * speed * speed * wingArea * liftCoefficient * liftScale;

        // 揚力を適用
        rb.AddForce(Vector3.up * lift);
    }

    private void CalcMass()
    {
        boxCollider = GetComponent<BoxCollider>();
        Vector3 size = boxCollider.size;
        Vector3 scale = transform.localScale;
        Vector3 dimensions = Vector3.Scale(size, scale);

        const float materialDensity = 800f; // 材料の密度 (kg/m^3) - 紙の密度の例
        float wingThickness = dimensions.y; // 翼の厚さ (m)

        float wingArea = dimensions.x * dimensions.z; // 翼の面積 (m^2)
        float volume = wingArea * wingThickness; // 翼の体積 (m^3)
        float mass = volume * materialDensity; // 質量 (kg)

        // 質量をRigidbodyに適用
        rb.mass = mass;
    }
}
