using UnityEngine;

public class FitnessEvaluator : MonoBehaviour
{
    private Vector3 startPosition; // �����ʒu
    private float fitness; // �K���x
    private bool isAlive; // �����t���O

    void Start()
    {
        startPosition = transform.position; // �����ʒu�̐ݒ�
        isAlive = true;
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

    // �Փ˂����Ƃ��ɌĂ΂�郁�\�b�h
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "Ground")
        {
            isAlive = false;
        }
    }

    public bool IsAlive()
    {
        return isAlive; // �����t���O��Ԃ�
    }
}
