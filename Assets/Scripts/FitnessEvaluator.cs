using UnityEngine;

public class FitnessEvaluator : MonoBehaviour
{
    private Vector3 startPosition; // �����ʒu
    private float fitness; // �K���x

    void Start()
    {
        startPosition = transform.position; // �����ʒu�̐ݒ�
    }

    void Update()
    {
        // ��s�������v�Z
        fitness = Vector3.Distance(startPosition, transform.position);
    }

    public float GetFitness()
    {
        return fitness; // �K���x��Ԃ�
    }
}
