using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;

public class EvolutionManager : MonoBehaviour
{
    public GameObject planePrefab;                          // 紙飛行機のプレハブ
    [SerializeField] private int populationSize;            // 集団のサイズ
    [SerializeField] private int generations;               // 世代数
    [SerializeField] private float mutationRate;            // 突然変異率
    [SerializeField] private float crossoverRate;           // 交叉率
    [SerializeField] private int eliteSize;                 // エリート選択のサイズ
    [SerializeField] private int tournamentSelection;       // トーナメント選択のサイズ
    [SerializeField] private Vector3 initialPosition;       // 初期位置

    private List<GameObject> population = new List<GameObject>();   // 現在の集団
    private List<float> fitnessScores = new List<float>();          // 各個体の適応度スコア
    private List<float> maxFitnessScores = new List<float>();       // 各世代の最大適応度スコア
    private List<float> avgFitnessScores = new List<float>();       // 各世代の平均適応度スコア

    void Start()
    {
        InitializePopulation(); // 集団の初期化
        StartCoroutine(Evolve()); // 進化プロセスの開始
    }

    private void InitializePopulation()
    {
        // 集団を初期化し、ランダムな形状パラメータで紙飛行機を生成
        for (int i = 0; i < populationSize; i++)
        {
            GameObject plane = Instantiate(planePrefab, initialPosition, Quaternion.identity);
            PlaneShape shape = plane.GetComponent<PlaneShape>();
            shape.wingSpan = Random.Range(1f, 100f); // 翼幅をランダムに設定
            shape.wingLength = Random.Range(0.2f, 2.5f); // 翼の長さをランダムに設定
            shape.wingAngle = Random.Range(0f, 3f); // 翼角度をランダムに設定
            shape.wingThickness = Random.Range(0.1f, 0.5f); // 翼形状をランダムに設定
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

    private IEnumerator Evolve()
    {
        // 指定された世代数だけ進化を繰り返す
        for (int generation = 0; generation < generations; generation++)
        {
            yield return new WaitForSeconds(5); // シミュレーション時間の待機

            EvaluateFitness(generation); // 適応度の評価
            List<GameObject> newPopulation = new List<GameObject>(); // 新しい集団

            // 新しい集団を生成
            population.Sort(CompareGenes); // 適応度で降順ソート
            for (int i = 0; i < eliteSize; ++i)
            {
                newPopulation.Add(Instantiate(population[i], initialPosition, Quaternion.identity)); // エリート選択
            }

            // トーナメント選択 + 突然変異
            while (newPopulation.Count < populationSize * mutationRate)
            {
                var tournamentMembers = population.AsEnumerable().OrderBy(x => System.Guid.NewGuid()).Take(tournamentSelection).ToList();
                tournamentMembers.Sort(CompareGenes);

                var child1 = Mutate(tournamentMembers[0]);
                newPopulation.Add(Instantiate(child1, initialPosition, Quaternion.identity));
                Destroy(child1);
                if (newPopulation.Count < populationSize * mutationRate)
                {
                    var child2 = Mutate(tournamentMembers[1]);
                    newPopulation.Add(Instantiate(child2, initialPosition, Quaternion.identity));
                    Destroy(child2);
                }
            }

            // トーナメント選択 + 交叉
            while (newPopulation.Count < populationSize)
            {
                var tournamentMembers = population.AsEnumerable().OrderBy(x => System.Guid.NewGuid()).Take(tournamentSelection).ToList();
                tournamentMembers.Sort(CompareGenes);

                GameObject child1, child2;
                (child1, child2) = Crossover(tournamentMembers[0], tournamentMembers[1]);
                newPopulation.Add(Instantiate(child1, initialPosition, Quaternion.identity));
                if (newPopulation.Count < populationSize) newPopulation.Add(Instantiate(child2, initialPosition, Quaternion.identity));
                Destroy(child1);
                Destroy(child2);
            }

            // 古い集団を破棄
            foreach (GameObject plane in population)
            {
                Destroy(plane);
            }

            // 集団を更新
            population = newPopulation;
            IgnoreCollisions();
        }
        SaveFitnessDataToFile();

        // End Unity App
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void EvaluateFitness(int gen)
    {
        // 各個体の適応度を評価
        float maxFitness = 0, sumFitness = 0;
        int maxIndex = 0;
        for (int i = 0; i < populationSize; i++)
        {
            var plane = population[i];
            float fitness = plane.GetComponent<FitnessEvaluator>().GetFitness(); // 適応度の取得
            if (fitness > maxFitness)
            {
                maxFitness = fitness;
                maxIndex = i;
            }
            sumFitness += fitness;
        }

        // 適応度情報の表示
        float avgFitness = sumFitness / populationSize;
        PlaneShape shape = population[maxIndex].GetComponent<PlaneShape>();
        Debug.Log("\tGen " + gen + "\t[ max: " + maxFitness.ToString("F3") + ", avg: " + avgFitness.ToString("F3") + " ]\twingSpan: " + shape.wingSpan.ToString("F2") + ", wingLength: " + shape.wingLength.ToString("F2") + ", wingAngle: " + shape.wingAngle.ToString("F2") + ", wingThickness: " + shape.wingThickness.ToString("F2"));

        // 適応度情報の保存
        maxFitnessScores.Add(maxFitness);
        avgFitnessScores.Add(avgFitness);
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

    private GameObject Mutate(GameObject plane)
    {
        // 突然変異の適用
        PlaneShape shape = plane.GetComponent<PlaneShape>();

        if (Random.value < mutationRate)
        {
            shape.wingSpan += Random.Range(-5f, 5f); // 翼幅の突然変異
            shape.wingSpan = Mathf.Clamp(shape.wingSpan, 1f, 100f); // 翼幅の範囲制限
        }

        if (Random.value < mutationRate)
        {
            shape.wingLength += Random.Range(-0.4f, 0.4f); // 翼の長さの突然変異
            shape.wingLength = Mathf.Clamp(shape.wingLength, 0.2f, 2.5f); // 翼の長さの範囲制限
        }

        if (Random.value < mutationRate)
        {
            shape.wingAngle += Random.Range(-0.8f, 0.8f); // 翼角度の突然変異
            shape.wingAngle = Mathf.Clamp(shape.wingAngle, 0f, 3f); // 翼角度の範囲制限
        }

        if (Random.value < mutationRate)
        {
            shape.wingThickness += Random.Range(-0.08f, 0.08f); // 翼の厚さの突然変異
            shape.wingThickness = Mathf.Clamp(shape.wingThickness, 0.1f, 0.5f); // 翼の厚さの範囲制限
        }

        // 形状を適用
        GameObject newPlane = Instantiate(planePrefab, initialPosition, Quaternion.identity);
        PlaneShape newShape = newPlane.GetComponent<PlaneShape>();
        newShape.wingSpan = shape.wingSpan;
        newShape.wingLength = shape.wingLength;
        newShape.wingAngle = shape.wingAngle;
        newShape.wingThickness = shape.wingThickness;
        newShape.ApplyShape();

        return newPlane;
    }

    private (GameObject, GameObject) Crossover(GameObject parent1, GameObject parent2)
    {
        // 交叉による新しい個体の生成
        GameObject offspring1 = Instantiate(planePrefab, initialPosition, Quaternion.identity);
        GameObject offspring2 = Instantiate(planePrefab, initialPosition, Quaternion.identity);
        PlaneShape shape1 = parent1.GetComponent<PlaneShape>();
        PlaneShape shape2 = parent2.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring1 = offspring1.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring2 = offspring2.GetComponent<PlaneShape>();

        shapeOffspring1.wingSpan = Random.value < crossoverRate ? shape1.wingSpan : shape2.wingSpan;
        shapeOffspring1.wingLength = Random.value < crossoverRate ? shape1.wingLength : shape2.wingLength;
        shapeOffspring1.wingAngle = Random.value < crossoverRate ? shape1.wingAngle : shape2.wingAngle;
        shapeOffspring1.wingThickness = Random.value < crossoverRate ? shape1.wingThickness : shape2.wingThickness;

        shapeOffspring2.wingSpan = Random.value < crossoverRate ? shape1.wingSpan : shape2.wingSpan;
        shapeOffspring2.wingLength = Random.value < crossoverRate ? shape1.wingLength : shape2.wingLength;
        shapeOffspring2.wingAngle = Random.value < crossoverRate ? shape1.wingAngle : shape2.wingAngle;
        shapeOffspring2.wingThickness = Random.value < crossoverRate ? shape1.wingThickness : shape2.wingThickness;

        shapeOffspring1.ApplyShape();
        shapeOffspring2.ApplyShape();

        return (offspring1, offspring2);
    }
    private void SaveFitnessDataToFile()
    {
        // 現在の日付と時刻を取得
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");

        // データを書き出すファイルパス
        string directoryPath = Path.Combine(Application.dataPath, "Outputs/Fitness/");
        string fileName = $"{timestamp}.txt";
        string filePath = Path.Combine(directoryPath, fileName);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int i = 0; i < maxFitnessScores.Count; i++)
            {
                writer.WriteLine($"{maxFitnessScores[i]},{avgFitnessScores[i]}");
            }
        }

        Debug.Log("Saved fitness data: " + filePath);
    }
}
