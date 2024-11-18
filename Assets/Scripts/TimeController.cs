using UnityEngine;
using UnityEngine.UI;

public class TimeController : MonoBehaviour
{
    public Slider timeSlider;

    void Start()
    {
        // スライドバーの初期値を設定
        timeSlider.value = 1;
        // スライドバーの値が変わったときに呼ばれるメソッドを設定
        timeSlider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    void OnSliderValueChanged(float value)
    {
        // Time.timeScaleをスライドバーの値に設定
        Time.timeScale = value;
    }
}
