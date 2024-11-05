using UnityEngine;

public class PlaneController : MonoBehaviour
{
    public float thrust = 10f; // ���i��
    public float lift = 5f; // �g��

    private Rigidbody rb; // Rigidbody�R���|�[�l���g

    void Start()
    {
        rb = GetComponent<Rigidbody>(); // Rigidbody�R���|�[�l���g�̎擾
    }

    void FixedUpdate()
    {
        // �O���ɐ��i�͂�������
        rb.AddForce(transform.forward * thrust);

        // ������ɗg�͂�������
        rb.AddForce(transform.up * lift);
    }
}
