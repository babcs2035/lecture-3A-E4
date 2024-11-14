using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;

public class EvolutionManager : MonoBehaviour
{
    public GameObject planePrefab;                          // ����s�@�̃v���n�u
    [SerializeField] private int populationSize;            // �W�c�̃T�C�Y
    [SerializeField] private int generations;               // ���㐔
    [SerializeField] private float mutationRate;            // �ˑR�ψٗ�
    [SerializeField] private float crossoverRate;           // ������
    [SerializeField] private int eliteSize;                 // �G���[�g�I���̃T�C�Y
    [SerializeField] private int tournamentSelection;       // �g�[�i�����g�I���̃T�C�Y
    [SerializeField] private Vector3 initialPosition;       // �����ʒu

    private List<GameObject> population = new List<GameObject>();   // ���݂̏W�c
    private List<float> fitnessScores = new List<float>();          // �e�̂̓K���x�X�R�A
    private List<float> maxFitnessScores = new List<float>();       // �e����̍ő�K���x�X�R�A
    private List<float> avgFitnessScores = new List<float>();       // �e����̕��ϓK���x�X�R�A

    void Start()
    {
        InitializePopulation(); // �W�c�̏�����
        StartCoroutine(Evolve()); // �i���v���Z�X�̊J�n
    }

    private void InitializePopulation()
    {
        // �W�c�����������A�����_���Ȍ`��p�����[�^�Ŏ���s�@�𐶐�
        for (int i = 0; i < populationSize; i++)
        {
            GameObject plane = Instantiate(planePrefab, initialPosition, Quaternion.identity);
            PlaneShape shape = plane.GetComponent<PlaneShape>();
            shape.wingSpan = Random.Range(1f, 100f); // �����������_���ɐݒ�
            shape.wingLength = Random.Range(0.2f, 2.5f); // ���̒����������_���ɐݒ�
            shape.wingAngle = Random.Range(0f, 3f); // ���p�x�������_���ɐݒ�
            shape.wingThickness = Random.Range(0.1f, 0.5f); // ���`��������_���ɐݒ�
            shape.ApplyShape(); // �`���K�p
            population.Add(plane);
        }
        IgnoreCollisions();
    }

    // ����s�@���m�̏Փ˂𖳌���
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
        // �w�肳�ꂽ���㐔�����i�����J��Ԃ�
        for (int generation = 0; generation < generations; generation++)
        {
            yield return new WaitForSeconds(5); // �V�~�����[�V�������Ԃ̑ҋ@

            EvaluateFitness(generation); // �K���x�̕]��
            List<GameObject> newPopulation = new List<GameObject>(); // �V�����W�c

            // �V�����W�c�𐶐�
            population.Sort(CompareGenes); // �K���x�ō~���\�[�g
            for (int i = 0; i < eliteSize; ++i)
            {
                newPopulation.Add(Instantiate(population[i], initialPosition, Quaternion.identity)); // �G���[�g�I��
            }

            // �g�[�i�����g�I�� + �ˑR�ψ�
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

            // �g�[�i�����g�I�� + ����
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

            // �Â��W�c��j��
            foreach (GameObject plane in population)
            {
                Destroy(plane);
            }

            // �W�c���X�V
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
        // �e�̂̓K���x��]��
        float maxFitness = 0, sumFitness = 0;
        int maxIndex = 0;
        for (int i = 0; i < populationSize; i++)
        {
            var plane = population[i];
            float fitness = plane.GetComponent<FitnessEvaluator>().GetFitness(); // �K���x�̎擾
            if (fitness > maxFitness)
            {
                maxFitness = fitness;
                maxIndex = i;
            }
            sumFitness += fitness;
        }

        // �K���x���̕\��
        float avgFitness = sumFitness / populationSize;
        PlaneShape shape = population[maxIndex].GetComponent<PlaneShape>();
        Debug.Log("\tGen " + gen + "\t[ max: " + maxFitness.ToString("F3") + ", avg: " + avgFitness.ToString("F3") + " ]\twingSpan: " + shape.wingSpan.ToString("F2") + ", wingLength: " + shape.wingLength.ToString("F2") + ", wingAngle: " + shape.wingAngle.ToString("F2") + ", wingThickness: " + shape.wingThickness.ToString("F2"));

        // �K���x���̕ۑ�
        maxFitnessScores.Add(maxFitness);
        avgFitnessScores.Add(avgFitness);
    }

    // �K���x�ō~���\�[�g���邽�߂̊֐�
    private static int CompareGenes(GameObject a, GameObject b)
    {
        float fitness_a = a.GetComponent<FitnessEvaluator>().GetFitness(); // �K���x�̎擾
        float fitness_b = b.GetComponent<FitnessEvaluator>().GetFitness(); // �K���x�̎擾
        if (fitness_a > fitness_b) return -1;
        if (fitness_b > fitness_a) return 1;
        return 0;
    }

    private GameObject Mutate(GameObject plane)
    {
        // �ˑR�ψق̓K�p
        PlaneShape shape = plane.GetComponent<PlaneShape>();

        if (Random.value < mutationRate)
        {
            shape.wingSpan += Random.Range(-5f, 5f); // �����̓ˑR�ψ�
            shape.wingSpan = Mathf.Clamp(shape.wingSpan, 1f, 100f); // �����͈̔͐���
        }

        if (Random.value < mutationRate)
        {
            shape.wingLength += Random.Range(-0.4f, 0.4f); // ���̒����̓ˑR�ψ�
            shape.wingLength = Mathf.Clamp(shape.wingLength, 0.2f, 2.5f); // ���̒����͈̔͐���
        }

        if (Random.value < mutationRate)
        {
            shape.wingAngle += Random.Range(-0.8f, 0.8f); // ���p�x�̓ˑR�ψ�
            shape.wingAngle = Mathf.Clamp(shape.wingAngle, 0f, 3f); // ���p�x�͈̔͐���
        }

        if (Random.value < mutationRate)
        {
            shape.wingThickness += Random.Range(-0.08f, 0.08f); // ���̌����̓ˑR�ψ�
            shape.wingThickness = Mathf.Clamp(shape.wingThickness, 0.1f, 0.5f); // ���̌����͈̔͐���
        }

        // �`���K�p
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
        // �����ɂ��V�����̂̐���
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
        // ���݂̓��t�Ǝ������擾
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");

        // �f�[�^�������o���t�@�C���p�X
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
