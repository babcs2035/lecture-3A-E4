using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EvolutionManager : MonoBehaviour
{
    public GameObject planePrefab; // 紙飛行機のプレハブ
    public int populationSize = 10; // 集団のサイズ
    public int generations = 50; // 世代数
    public float mutationRate = 0.01f; // 突然変異率

    private List<GameObject> population = new List<GameObject>(); // 現在の集団
    private List<float> fitnessScores = new List<float>(); // 各個体の適応度スコア

    void Start()
    {
        InitializePopulation(); // 集団の初期化
        StartCoroutine(Evolve()); // 進化プロセスの開始
    }

    void InitializePopulation()
    {
        // 集団を初期化し、ランダムな形状パラメータで紙飛行機を生成
        for (int i = 0; i < populationSize; i++)
        {
            GameObject plane = Instantiate(planePrefab, new Vector3(0, 1, 0), Quaternion.identity);
            PlaneShape shape = plane.GetComponent<PlaneShape>();
            shape.wingSpan = Random.Range(1f, 3f); // 翼幅をランダムに設定
            shape.wingWidth = Random.Range(0.5f, 1.5f); // 翼の幅をランダムに設定
            shape.tailHeight = Random.Range(0.5f, 1.5f); // 尾翼の高さをランダムに設定
            shape.ApplyShape(); // 形状を適用
            population.Add(plane);
        }
    }

    IEnumerator Evolve()
    {
        // 指定された世代数だけ進化を繰り返す
        for (int generation = 0; generation < generations; generation++)
        {
            yield return new WaitForSeconds(5); // シミュレーション時間の待機

            EvaluateFitness(); // 適応度の評価
            List<GameObject> newPopulation = new List<GameObject>(); // 新しい集団

            // 新しい集団を生成
            for (int i = 0; i < populationSize; i++)
            {
                GameObject parent1 = SelectParent(); // 親1の選択
                GameObject parent2 = SelectParent(); // 親2の選択
                GameObject offspring = Crossover(parent1, parent2); // 交叉による子の生成
                Mutate(offspring); // 突然変異の適用
                newPopulation.Add(offspring); // 新しい集団に追加
            }

            // 古い集団を破棄
            foreach (GameObject plane in population)
            {
                Destroy(plane);
            }

            population = newPopulation; // 集団を更新
        }
    }

    void EvaluateFitness()
    {
        // 各個体の適応度を評価
        fitnessScores.Clear();
        foreach (GameObject plane in population)
        {
            float fitness = plane.GetComponent<FitnessEvaluator>().GetFitness(); // 適応度の取得
            fitnessScores.Add(fitness); // 適応度スコアリストに追加
        }
    }

    GameObject SelectParent()
    {
        // 適応度に基づいて親を選択するルーレット選択
        float totalFitness = 0;
        foreach (float score in fitnessScores)
        {
            totalFitness += score; // 総適応度の計算
        }

        float randomPoint = Random.Range(0, totalFitness); // ランダムな選択ポイント
        float runningSum = 0;

        for (int i = 0; i < populationSize; i++)
        {
            runningSum += fitnessScores[i];
            if (runningSum > randomPoint)
            {
                return population[i]; // 選択された親を返す
            }
        }

        return population[populationSize - 1]; // デフォルトで最後の個体を返す
    }

    GameObject Crossover(GameObject parent1, GameObject parent2)
    {
        // 交叉による新しい個体の生成
        GameObject offspring = Instantiate(planePrefab, new Vector3(0, 1, 0), Quaternion.identity);
        PlaneShape shape1 = parent1.GetComponent<PlaneShape>();
        PlaneShape shape2 = parent2.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring = offspring.GetComponent<PlaneShape>();

        // 親の形状パラメータを平均して子に設定
        shapeOffspring.wingSpan = (shape1.wingSpan + shape2.wingSpan) / 2;
        shapeOffspring.wingWidth = (shape1.wingWidth + shape2.wingWidth) / 2;
        shapeOffspring.tailHeight = (shape1.tailHeight + shape2.tailHeight) / 2;

        shapeOffspring.ApplyShape(); // 形状を適用

        return offspring;
    }

    void Mutate(GameObject plane)
    {
        // 突然変異の適用
        PlaneShape shape = plane.GetComponent<PlaneShape>();

        if (Random.value < mutationRate)
        {
            shape.wingSpan += Random.Range(-0.1f, 0.1f); // 翼幅の突然変異
        }

        if (Random.value < mutationRate)
        {
            shape.wingWidth += Random.Range(-0.1f, 0.1f); // 翼の幅の突然変異
        }

        if (Random.value < mutationRate)
        {
            shape.tailHeight += Random.Range(-0.1f, 0.1f); // 尾翼の高さの突然変異
        }

        shape.ApplyShape(); // 形状を適用
    }
}
