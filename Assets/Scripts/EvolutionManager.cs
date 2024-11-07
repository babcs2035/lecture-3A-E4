using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class EvolutionManager : MonoBehaviour
{
    public GameObject planePrefab; // ����s�@�̃v���n�u
    public int populationSize; // �W�c�̃T�C�Y
    public int generations; // ���㐔
    public float mutationRate; // �ˑR�ψٗ�

    private List<GameObject> population = new List<GameObject>(); // ���݂̏W�c
    private List<float> fitnessScores = new List<float>(); // �e�̂̓K���x�X�R�A

    private Vector3 initialPosition = new Vector3(25, 15, 0); // �����ʒu

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
            shape.wingSpan = Random.Range(5f, 30f); // �����������_���ɐݒ�
            shape.wingWidth = Random.Range(0.3f, 1.2f); // ���̕��������_���ɐݒ�
            shape.wingAngle = Random.Range(-10f, 10f); // ���p�x�������_���ɐݒ�
            shape.wingShape = Random.Range(0.05f, 0.15f); // ���`��������_���ɐݒ�
            shape.tailHeight = Random.Range(0.1f, 2f); // �����̍����������_���ɐݒ�
            shape.centerOfMass = Random.Range(-0.5f, 0.5f); // �d�S�ʒu�������_���ɐݒ�
            shape.mass = Random.Range(0.8f, 1.4f); // ���ʂ������_���ɐݒ�            shape.ApplyShape(); // �`���K�p
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
            IgnoreCollisions();
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

        // �ő�K���x�� plane ���̕\��
        int maxIndex = fitnessScores.IndexOf(fitnessScores.Max());
        PlaneShape shape = population[maxIndex].GetComponent<PlaneShape>();
        Debug.Log("wingSpan: " + shape.wingSpan + ", wingWidth: " + shape.wingWidth + ", wingAngle: " + shape.wingAngle + ", wingShape: " + shape.wingShape + ", tailHeight: " + shape.tailHeight + ", score: " + fitnessScores.Max());
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
        GameObject offspring = Instantiate(planePrefab, initialPosition, Quaternion.identity);
        PlaneShape shape1 = parent1.GetComponent<PlaneShape>();
        PlaneShape shape2 = parent2.GetComponent<PlaneShape>();
        PlaneShape shapeOffspring = offspring.GetComponent<PlaneShape>();

        // �e�̌`��p�����[�^�𕽋ς��Ďq�ɐݒ�
        shapeOffspring.wingSpan = (shape1.wingSpan + shape2.wingSpan) / 2;
        shapeOffspring.wingWidth = (shape1.wingWidth + shape2.wingWidth) / 2;
        shapeOffspring.wingAngle = (shape1.wingAngle + shape2.wingAngle) / 2;
        shapeOffspring.wingShape = (shape1.wingShape + shape2.wingShape) / 2;
        shapeOffspring.tailHeight = (shape1.tailHeight + shape2.tailHeight) / 2;
        shapeOffspring.centerOfMass = (shape1.centerOfMass + shape2.centerOfMass) / 2;
        shapeOffspring.mass = (shape1.mass + shape2.mass) / 2;

        shapeOffspring.ApplyShape(); // �`���K�p

        return offspring;
    }

    void Mutate(GameObject plane)
    {
        // �ˑR�ψق̓K�p
        PlaneShape shape = plane.GetComponent<PlaneShape>();

        if (Random.value < mutationRate)
        {
            shape.wingSpan += Random.Range(-8f, 8f); // �����̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.wingWidth += Random.Range(-0.5f, 0.5f); // ���̕��̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.wingAngle += Random.Range(-1f, 1f); // ���p�x�̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.wingShape += Random.Range(-0.02f, 0.02f); // ���`��̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.tailHeight += Random.Range(-0.5f, 0.5f); // �����̍����̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.centerOfMass += Random.Range(-0.05f, 0.05f); // �d�S�ʒu�̓ˑR�ψ�
        }

        if (Random.value < mutationRate)
        {
            shape.mass += Random.Range(-0.05f, 0.05f); // ���ʂ̓ˑR�ψ�
        }

        shape.ApplyShape(); // �`���K�p
    }
}
