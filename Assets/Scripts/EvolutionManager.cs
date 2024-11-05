using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class EvolutionManager : MonoBehaviour
{
    public GameObject planePrefab; // ����s�@�̃v���n�u
    public int populationSize = 10; // �W�c�̃T�C�Y
    public int generations = 50; // ���㐔
    public float mutationRate = 0.01f; // �ˑR�ψٗ�

    private List<GameObject> population = new List<GameObject>(); // ���݂̏W�c
    private List<float> fitnessScores = new List<float>(); // �e�̂̓K���x�X�R�A

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
            GameObject plane = Instantiate(planePrefab, new Vector3(0, 1, 0), Quaternion.identity);
            PlaneShape shape = plane.GetComponent<PlaneShape>();
            shape.wingSpan = Random.Range(1f, 3f); // �����������_���ɐݒ�
            shape.wingWidth = Random.Range(0.5f, 1.5f); // ���̕��������_���ɐݒ�
            shape.tailHeight = Random.Range(0.5f, 1.5f); // �����̍����������_���ɐݒ�
            shape.ApplyShape(); // �`���K�p
            population.Add(plane);
        }
    }

    IEnumerator Evolve()
    {
        // �w�肳�ꂽ���㐔�����i�����J��Ԃ�
        for (int generation = 0; generation < generations; generation++)
        {
            yield return new WaitForSeconds(5); // �V�~�����[�V�������Ԃ̑ҋ@

            EvaluateFitness(); // �K���x�̕]��
            List<GameObject> newPopulation = new List<GameObject>(); // �V�����W�c

            // �V�����W�c�𐶐�
            for (int i = 0; i < populationSize; i++)
            {
                GameObject parent1 = SelectParent(); // �e1�̑I��
                GameObject parent2 = SelectParent(); // �e2�̑I��
                GameObject offspring = Crossover(parent1, parent2); // �����ɂ��q�̐���
                Mutate(offspring); // �ˑR�ψق̓K�p
                newPopulation.Add(offspring); // �V�����W�c�ɒǉ�
            }

            // �Â��W�c��j��
            foreach (GameObject plane in population)
            {
                Destroy(plane);
            }

            population = newPopulation; // �W�c���X�V
        }
    }

    void EvaluateFitness()
    {
        // �e�̂̓K���x��]��
        fitnessScores.Clear();
        foreach (GameObject plane in population)
        {
            float fitness = plane.GetComponent<FitnessEvaluator>().GetFitness(); // �K���x�̎擾
            fitnessScores.Add(fitness); // �K���x�X�R�A���X�g�ɒǉ�
        }
    }

    GameObject SelectParent()
    {
        // �K���x�Ɋ�Â��Đe��I�����郋�[���b�g�I��
        float totalFitness = 0;
        foreach (float score in fitnessScores)
        {
            totalFitness += score; // ���K���x�̌v�Z
        }

        float randomPoint = Random.Range(0, totalFitness); // �����_���ȑI���|�C���g
        float runningSum = 0;

        for (int i = 0; i < populationSize; i++)
        {
            runningSum += fitnessScores[i];
            if (runningSum > randomPoint)
            {
                return population[i]; // �I�����ꂽ�e��Ԃ�
            }
        }

        return population[populationSize - 1]; // �f�t�H���g�ōŌ�̌̂�Ԃ�
    }

    GameObject Crossover(GameObject parent1, GameObject parent2)
    {
        // �����ɂ��V�����̂̐���
        GameObject offspring = Instantiate(planePrefab, new Vector3(0, 1, 0), Quaternion.identity);
        PlaneShape shape1 = parent1.GetComponent<PlaneShape>();
        PlaneShape shape2 = parent2.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring = offspring.GetComponent<PlaneShape>();

        // �e�̌`��p�����[�^�𕽋ς��Ďq�ɐݒ�
        shapeOffspring.wingSpan = (shape1.wingSpan + shape2.wingSpan) / 2;
        shapeOffspring.wingWidth = (shape1.wingWidth + shape2.wingWidth) / 2;
        shapeOffspring.tailHeight = (shape1.tailHeight + shape2.tailHeight) / 2;

        shapeOffspring.ApplyShape(); // �`���K�p

        return offspring;
    }

    void Mutate(GameObject plane)
    {
        // �ˑR�ψق̓K�p
        PlaneShape shape = plane.GetComponent<PlaneShape>();

        if (Random.value < mutationRate)
        {
            shape.wingSpan += Random.Range(-0.1f, 0.1f); // �����̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.wingWidth += Random.Range(-0.1f, 0.1f); // ���̕��̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.tailHeight += Random.Range(-0.1f, 0.1f); // �����̍����̓ˑR�ψ�
        }

        shape.ApplyShape(); // �`���K�p
    }
}
