using UnityEngine;

public class PlaneShape : MonoBehaviour
{
    public float wingSpan; // 翼幅
    public float wingLength; // 翼の長さ
    public float wingAngle; // 翼角度
    public float wingThickness; // 翼の厚さ

    public Transform leftWing; // 左翼のTransform
    public Transform rightWing; // 右翼のTransform

    [SerializeField] private float initialSpeed; // 初速度
    [SerializeField] private Vector3 initialRotation; // 初期回転角度
    [SerializeField] private float liftScale; // 揚力スケーリングファクター
    [SerializeField] float materialDensity; // 材料の密度 (kg/m^3) - 紙の密度の例

    private Rigidbody rb; // Rigidbodyコンポーネント
    private BoxCollider boxCollider;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.linearVelocity = new Vector3(-1, 0, 0) * initialSpeed; // 初速度を設定
        rb.rotation = Quaternion.Euler(initialRotation); // 初期回転角度を設定
    }

    public void ApplyShape()
    {
        rb = GetComponent<Rigidbody>();

        // 翼の形状を適用
        leftWing.localScale = new Vector3(wingLength, wingSpan, wingThickness);
        rightWing.localScale = new Vector3(wingLength, wingSpan, wingThickness);

        // 翼の角度を適用
        leftWing.localEulerAngles = new Vector3(leftWing.localEulerAngles.x, leftWing.localEulerAngles.y + wingAngle, leftWing.localEulerAngles.z);
        rightWing.localEulerAngles = new Vector3(rightWing.localEulerAngles.x, rightWing.localEulerAngles.y + wingAngle, rightWing.localEulerAngles.z);

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
        Renderer bodyRenderer = transform.GetComponent<Renderer>();
        Vector3 bodyLocalScale = transform.localScale;
        Vector3 bodySize = Vector3.Scale(bodyRenderer.bounds.size, bodyLocalScale);
        //float mass = bodySize.x * bodySize.y * bodySize.z * materialDensity / 10;

        Transform wingTransform = transform.Find("LeftWing");
        Renderer wingRenderer = wingTransform.GetComponent<Renderer>();
        Vector3 wingLocalScale = wingTransform.localScale;
        Vector3 wingSize = Vector3.Scale(wingRenderer.bounds.size, wingLocalScale);
        float mass = wingSize.x * wingSize.y * wingSize.z * materialDensity * 2;
        //float mass = wingSpan * wingLength * wingThickness * materialDensity * 2;

        // 質量をRigidbodyに適用
        rb.mass = mass;
    }
}
