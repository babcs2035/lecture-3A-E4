using UnityEngine;

public class PlaneShape : MonoBehaviour
{
    public float wingSpan = 2f; // 翼幅
    public float wingWidth = 1f; // 翼の幅
    public float tailHeight = 1f; // 尾翼の高さ

    public Transform leftWing; // 左翼のTransform
    public Transform rightWing; // 右翼のTransform
    public Transform tail; // 尾翼のTransform

    public void ApplyShape()
    {
        // 翼の形状を適用
        leftWing.localScale = new Vector3(wingSpan, wingWidth, leftWing.localScale.z);
        rightWing.localScale = new Vector3(wingSpan, wingWidth, rightWing.localScale.z);

        // 尾翼の形状を適用
        tail.localScale = new Vector3(tail.localScale.x, tailHeight, tail.localScale.z);
    }
}
