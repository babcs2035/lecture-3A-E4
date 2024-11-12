using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class EvolutionManager : MonoBehaviour
{
    public GameObject planePrefab; // 紙飛行機のプレハブ
    public int populationSize; // 集団のサイズ
    public int generations; // 世代数
    public float mutationRate; // 突然変異率
    public Vector3 initialPosition; // 初期位置

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
            GameObject plane = Instantiate(planePrefab, initialPosition, Quaternion.identity);
            PlaneShape shape = plane.GetComponent<PlaneShape>();
            shape.wingSpan = Random.Range(5f, 30f); // 翼幅をランダムに設定
            shape.wingWidth = Random.Range(0.2f, 2f); // 翼の幅をランダムに設定
            shape.wingAngle = Random.Range(0f, 45f); // 翼角度をランダムに設定
            shape.wingShape = Random.Range(0.05f, 0.5f); // 翼形状をランダムに設定
            shape.centerOfMass = Random.Range(-0.5f, 0.5f); // 重心位置をランダムに設定
            shape.mass = Random.Range(0.8f, 1.4f); // 質量をランダムに設定
            shape.ApplyShape(); // 形状を適用
            population.Add(plane);
        }
        IgnoreCollisions();
    }

    // 紙飛行機同士の衝突を無効化
    private void IgnoreCollisions()
    {
        for (int i = 0; i < populationSize; i++)
        {
            for (int j = 0; j < populationSize; j++)
            {
                if (i != j)
                {
                    Physics.IgnoreCollision(population[i].GetComponent<Collider>(), population[j].GetComponent<Collider>());
                }
            }
        }
    }

    IEnumerator Evolve()
    {
        const int eliteSize = 5; // エリート選択のサイズ
        const float mutate_only = 0.3f;
        const int tournamentSelection = 5; // トーナメント選択のサイズ

        // 指定された世代数だけ進化を繰り返す
        for (int generation = 0; generation < generations; generation++)
        {
            yield return new WaitForSeconds(5); // シミュレーション時間の待機

            EvaluateFitness(); // 適応度の評価
            List<GameObject> newPopulation = new List<GameObject>(); // 新しい集団

            // 新しい集団を生成
            population.Sort(CompareGenes); // 適応度で降順ソート
            for (int i = 0; i < eliteSize; ++i)
            {
                newPopulation.Add(Instantiate(population[i], initialPosition, Quaternion.identity)); // エリート選択
            }

            // トーナメント選択 + 突然変異
            while (newPopulation.Count < populationSize * mutate_only)
            {
                var tournamentMembers = population.AsEnumerable().OrderBy(x => System.Guid.NewGuid()).Take(tournamentSelection).ToList();
                tournamentMembers.Sort(CompareGenes);
                newPopulation.Add(Mutate(tournamentMembers[0]));
                if (newPopulation.Count < populationSize * mutate_only) newPopulation.Add(Mutate(tournamentMembers[1]));
            }

            // トーナメント選択 + 交叉
            while (newPopulation.Count < populationSize)
            {
                var tournamentMembers = population.AsEnumerable().OrderBy(x => System.Guid.NewGuid()).Take(tournamentSelection).ToList();
                tournamentMembers.Sort(CompareGenes);

                GameObject child1, child2;
                (child1, child2) = Crossover(tournamentMembers[0], tournamentMembers[1]);
                newPopulation.Add(child1);
                if (newPopulation.Count < populationSize) newPopulation.Add(child2);
            }

            // 古い集団を破棄
            foreach (GameObject plane in population)
            {
                Destroy(plane);
            }

            population = newPopulation; // 集団を更新
            IgnoreCollisions();
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

        // 最大適応度の plane 情報の表示
        int maxIndex = fitnessScores.IndexOf(fitnessScores.Max());
        PlaneShape shape = population[maxIndex].GetComponent<PlaneShape>();
        Debug.Log("[ " + fitnessScores.Max().ToString("F3") + " ] wingSpan: " + shape.wingSpan + ", wingWidth: " + shape.wingWidth + ", wingAngle: " + shape.wingAngle + ", wingShape: " + shape.wingShape);
    }

    // 適応度で降順ソートするための関数
    private static int CompareGenes(GameObject a, GameObject b)
    {
        float fitness_a = a.GetComponent<FitnessEvaluator>().GetFitness(); // 適応度の取得
        float fitness_b = b.GetComponent<FitnessEvaluator>().GetFitness(); // 適応度の取得
        if (fitness_a > fitness_b) return -1;
        if (fitness_b > fitness_a) return 1;
        return 0;
    }

    GameObject Mutate(GameObject plane)
    {
        // 突然変異の適用
        var newPlane = plane;
        PlaneShape shape = newPlane.GetComponent<PlaneShape>();

        if (Random.value < mutationRate)
        {
            shape.wingSpan += Random.Range(-5f, 5f); // 翼幅の突然変異
            shape.wingSpan = Mathf.Clamp(shape.wingSpan, 0.1f, 30f); // 翼幅の範囲制限
        }

        if (Random.value < mutationRate)
        {
            shape.wingWidth += Random.Range(-0.5f, 0.5f); // 翼の幅の突然変異
            shape.wingWidth = Mathf.Clamp(shape.wingWidth, 0.1f, 2f); // 翼の幅の範囲制限
        }

        if (Random.value < mutationRate)
        {
            shape.wingAngle += Random.Range(-2f, 2f); // 翼角度の突然変異
            shape.wingAngle = Mathf.Clamp(shape.wingAngle, 0f, 45f); // 翼角度の範囲制限
        }

        if (Random.value < mutationRate)
        {
            shape.wingShape += Random.Range(-0.02f, 0.02f); // 翼形状の突然変異
        }

        if (Random.value < mutationRate)
        {
            shape.centerOfMass += Random.Range(-0.05f, 0.05f); // 重心位置の突然変異
        }

        if (Random.value < mutationRate)
        {
            shape.mass += Random.Range(-0.5f, 0.5f); // 質量の突然変異
        }

        shape.ApplyShape(); // 形状を適用
        return newPlane;
    }

    (GameObject, GameObject) Crossover(GameObject parent1, GameObject parent2)
    {
        // 交叉による新しい個体の生成
        GameObject offspring1 = Instantiate(planePrefab, initialPosition, Quaternion.identity);
        GameObject offspring2 = Instantiate(planePrefab, initialPosition, Quaternion.identity);
        PlaneShape shape1 = parent1.GetComponent<PlaneShape>();
        PlaneShape shape2 = parent2.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring1 = offspring1.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring2 = offspring2.GetComponent<PlaneShape>();

        shapeOffspring1.wingSpan = Random.value < 0.5f ? shape1.wingSpan : shape2.wingSpan;
        shapeOffspring1.wingWidth = Random.value < 0.5f ? shape1.wingWidth : shape2.wingWidth;
        shapeOffspring1.wingAngle = Random.value < 0.5f ? shape1.wingAngle : shape2.wingAngle;
        shapeOffspring1.wingShape = Random.value < 0.5f ? shape1.wingShape : shape2.wingShape;
        shapeOffspring1.centerOfMass = Random.value < 0.5f ? shape1.centerOfMass : shape2.centerOfMass;
        shapeOffspring1.mass = Random.value < 0.5f ? shape1.mass : shape2.mass;

        shapeOffspring2.wingSpan = Random.value < 0.5f ? shape1.wingSpan : shape2.wingSpan;
        shapeOffspring2.wingWidth = Random.value < 0.5f ? shape1.wingWidth : shape2.wingWidth;
        shapeOffspring2.wingAngle = Random.value < 0.5f ? shape1.wingAngle : shape2.wingAngle;
        shapeOffspring2.wingShape = Random.value < 0.5f ? shape1.wingShape : shape2.wingShape;
        shapeOffspring2.centerOfMass = Random.value < 0.5f ? shape1.centerOfMass : shape2.centerOfMass;
        shapeOffspring2.mass = Random.value < 0.5f ? shape1.mass : shape2.mass;

        shapeOffspring1.ApplyShape();
        shapeOffspring2.ApplyShape();

        return (offspring1, offspring2);
    }
}
