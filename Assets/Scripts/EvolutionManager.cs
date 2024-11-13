using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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

    void Start()
    {
        InitializePopulation(); // �W�c�̏�����
        StartCoroutine(Evolve()); // �i���v���Z�X�̊J�n
    }

    void InitializePopulation()
    {
        // �W�c�����������A�����_���Ȍ`��p�����[�^�Ŏ���s�@�𐶐�
        for (int i = 0; i < populationSize; i++)
        {
            GameObject plane = Instantiate(planePrefab, initialPosition, Quaternion.identity);
            PlaneShape shape = plane.GetComponent<PlaneShape>();
            shape.wingSpan = Random.Range(5f, 35f); // �����������_���ɐݒ�
            shape.wingWidth = Random.Range(0.2f, 3f); // ���̕��������_���ɐݒ�
            shape.wingAngle = Random.Range(0f, 45f); // ���p�x�������_���ɐݒ�
            shape.wingShape = Random.Range(0.05f, 0.5f); // ���`��������_���ɐݒ�
            shape.centerOfMass = Random.Range(-0.5f, 0.5f); // �d�S�ʒu�������_���ɐݒ�
            shape.mass = Random.Range(0.8f, 1.5f); // ���ʂ������_���ɐݒ�
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

    IEnumerator Evolve()
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
                newPopulation.Add(Mutate(tournamentMembers[0]));
                if (newPopulation.Count < populationSize * mutationRate) newPopulation.Add(Mutate(tournamentMembers[1]));
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

            population = newPopulation; // �W�c���X�V
            IgnoreCollisions();
        }
    }

    void EvaluateFitness(int gen)
    {
        // �e�̂̓K���x��]��
        fitnessScores.Clear();
        foreach (GameObject plane in population)
        {
            float fitness = plane.GetComponent<FitnessEvaluator>().GetFitness(); // �K���x�̎擾
            fitnessScores.Add(fitness); // �K���x�X�R�A���X�g�ɒǉ�
        }

        // �K���x���̕\��
        float avgFitness = fitnessScores.Average();
        int maxIndex = fitnessScores.IndexOf(fitnessScores.Max());
        PlaneShape shape = population[maxIndex].GetComponent<PlaneShape>();
        Debug.Log("Gen " + gen + " [ max: " + fitnessScores.Max().ToString("F3") + ", avg: " + avgFitness.ToString("F3") + " ] wingSpan: " + shape.wingSpan + ", wingWidth: " + shape.wingWidth + ", wingAngle: " + shape.wingAngle + ", wingShape: " + shape.wingShape);
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

    GameObject Mutate(GameObject plane)
    {
        // �ˑR�ψق̓K�p
        var newPlane = plane;
        PlaneShape shape = newPlane.GetComponent<PlaneShape>();

        if (Random.value < mutationRate)
        {
            shape.wingSpan += Random.Range(-5f, 5f); // �����̓ˑR�ψ�
            shape.wingSpan = Mathf.Clamp(shape.wingSpan, 0.1f, 35f); // �����͈̔͐���
        }

        if (Random.value < mutationRate)
        {
            shape.wingWidth += Random.Range(-0.5f, 0.5f); // ���̕��̓ˑR�ψ�
            shape.wingWidth = Mathf.Clamp(shape.wingWidth, 0.1f, 3f); // ���̕��͈̔͐���
        }

        if (Random.value < mutationRate)
        {
            shape.wingAngle += Random.Range(-2f, 2f); // ���p�x�̓ˑR�ψ�
            shape.wingAngle = Mathf.Clamp(shape.wingAngle, 0f, 45f); // ���p�x�͈̔͐���
        }

        if (Random.value < mutationRate)
        {
            shape.wingShape += Random.Range(-0.02f, 0.02f); // ���`��̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.centerOfMass += Random.Range(-0.05f, 0.05f); // �d�S�ʒu�̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.mass += Random.Range(-0.5f, 0.5f); // ���ʂ̓ˑR�ψ�
            shape.mass = Mathf.Clamp(shape.mass, 0.8f, 1.5f); // ���ʂ͈̔͐���
        }

        shape.ApplyShape(); // �`���K�p
        return newPlane;
    }

    (GameObject, GameObject) Crossover(GameObject parent1, GameObject parent2)
    {
        // �����ɂ��V�����̂̐���
        GameObject offspring1 = Instantiate(planePrefab, initialPosition, Quaternion.identity);
        GameObject offspring2 = Instantiate(planePrefab, initialPosition, Quaternion.identity);
        PlaneShape shape1 = parent1.GetComponent<PlaneShape>();
        PlaneShape shape2 = parent2.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring1 = offspring1.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring2 = offspring2.GetComponent<PlaneShape>();

        shapeOffspring1.wingSpan = Random.value < crossoverRate ? shape1.wingSpan : shape2.wingSpan;
        shapeOffspring1.wingWidth = Random.value < crossoverRate ? shape1.wingWidth : shape2.wingWidth;
        shapeOffspring1.wingAngle = Random.value < crossoverRate ? shape1.wingAngle : shape2.wingAngle;
        shapeOffspring1.wingShape = Random.value < crossoverRate ? shape1.wingShape : shape2.wingShape;
        shapeOffspring1.centerOfMass = Random.value < crossoverRate ? shape1.centerOfMass : shape2.centerOfMass;
        shapeOffspring1.mass = Random.value < crossoverRate ? shape1.mass : shape2.mass;

        shapeOffspring2.wingSpan = Random.value < crossoverRate ? shape1.wingSpan : shape2.wingSpan;
        shapeOffspring2.wingWidth = Random.value < crossoverRate ? shape1.wingWidth : shape2.wingWidth;
        shapeOffspring2.wingAngle = Random.value < crossoverRate ? shape1.wingAngle : shape2.wingAngle;
        shapeOffspring2.wingShape = Random.value < crossoverRate ? shape1.wingShape : shape2.wingShape;
        shapeOffspring2.centerOfMass = Random.value < crossoverRate ? shape1.centerOfMass : shape2.centerOfMass;
        shapeOffspring2.mass = Random.value < crossoverRate ? shape1.mass : shape2.mass;

        shapeOffspring1.ApplyShape();
        shapeOffspring2.ApplyShape();

        return (offspring1, offspring2);
    }
}
