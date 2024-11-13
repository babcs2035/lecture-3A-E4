using UnityEngine;

public class PlaneShape : MonoBehaviour
{
    public float wingSpan; // 翼幅
    public float wingLength; // 翼の長さ
    public float wingAngle; // 翼角度
    public float wingShape; // 翼形状
    public float liftScale; // 揚力スケーリングファクター

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

        ApplyShape(); // 初期形状の適用
    }

    public void ApplyShape()
    {
        rb = GetComponent<Rigidbody>();

        // 翼の形状を適用
        leftWing.localScale = new Vector3(wingLength, leftWing.localScale.y, wingSpan);
        rightWing.localScale = new Vector3(wingLength, rightWing.localScale.y, wingSpan);

        // 翼の角度を適用
        leftWing.localEulerAngles = new Vector3(leftWing.localEulerAngles.x, leftWing.localEulerAngles.y, leftWing.localEulerAngles.z);
        rightWing.localEulerAngles = new Vector3(rightWing.localEulerAngles.x, leftWing.localEulerAngles.y, rightWing.localEulerAngles.z);

        // 翼形状を適用（例として、翼の厚さを変更）
        leftWing.localScale = new Vector3(leftWing.localScale.x, wingShape, leftWing.localScale.z);
        rightWing.localScale = new Vector3(rightWing.localScale.x, wingShape, rightWing.localScale.z);

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

        Transform wingTransform = transform.Find("LeftWing");
        Renderer wingRenderer = wingTransform.GetComponent<Renderer>();
        Vector3 wingLocalScale = wingTransform.localScale;
        Vector3 wingSize = Vector3.Scale(wingRenderer.bounds.size, wingLocalScale);

        float speed = rb.linearVelocity.magnitude;
        float angleOfAttackRad = wingAngle * Mathf.Deg2Rad;
        float liftCoefficient = liftCoefficient0 + liftCoefficientAlpha * angleOfAttackRad;
        float wingArea = wingSize.x * wingSize.z; // 翼の面積 (m^2)
        float lift = 0.5f * airDensity * speed * speed * wingArea * liftCoefficient * liftScale;

        // 揚力を適用
        rb.AddForce(Vector3.up * lift);
    }

    private void CalcMass()
    {
        const float materialDensity = 800f; // 材料の密度 (kg/m^3) - 紙の密度の例

        Renderer bodyRenderer = transform.GetComponent<Renderer>();
        Vector3 bodyLocalScale = transform.localScale;
        Vector3 bodySize = Vector3.Scale(bodyRenderer.bounds.size, bodyLocalScale);
        float mass = bodySize.x * bodySize.y * bodySize.z * materialDensity / 10;

        Transform wingTransform = transform.Find("LeftWing");
        Renderer wingRenderer = wingTransform.GetComponent<Renderer>();
        Vector3 wingLocalScale = wingTransform.localScale;
        Vector3 wingSize = Vector3.Scale(wingRenderer.bounds.size, wingLocalScale);
        mass += wingSize.x * wingSize.y * wingSize.z * materialDensity * 2;

        // 質量をRigidbodyに適用
        rb.mass = mass;
    }
}
