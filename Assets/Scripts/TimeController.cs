using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    public Slider timeSlider;

    void Start()
    {
        // �X���C�h�o�[�̏����l��ݒ�
        timeSlider.value = 1;
        // �X���C�h�o�[�̒l���ς�����Ƃ��ɌĂ΂�郁�\�b�h��ݒ�
        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        // Time.timeScale���X���C�h�o�[�̒l�ɐݒ�
        Time.timeScale = value;
    }
}
