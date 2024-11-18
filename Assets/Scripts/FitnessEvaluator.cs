using UnityEngine;

public class FitnessEvaluator : MonoBehaviour
{
    private Vector3 startPosition; // 初期位置
    private float fitness; // 適応度
    private bool isAlive; // 生存フラグ

    void Start()
    {
        startPosition = transform.position; // 初期位置の設定
        isAlive = true;
    }

    void Update()
    {
        // 飛行距離を計算
        fitness = Vector3.Distance(startPosition, transform.position);
    }

    public float GetFitness()
    {
        return fitness; // 適応度を返す
    }

    // 衝突したときに呼ばれるメソッド
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            isAlive = false;
        }
    }

    public bool IsAlive()
    {
        return isAlive; // 生存フラグを返す
    }
}
