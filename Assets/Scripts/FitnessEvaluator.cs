using UnityEngine;

public class FitnessEvaluator : MonoBehaviour
{
    private Vector3 startPosition; // 初期位置
    private float fitness; // 適応度

    void Start()
    {
        startPosition = transform.position; // 初期位置の設定
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
}
