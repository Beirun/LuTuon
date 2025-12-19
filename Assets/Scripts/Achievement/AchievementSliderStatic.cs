using UnityEngine;
using UnityEngine.UI;

public class AchievementSliderStatic : MonoBehaviour
{
    public Slider mySlider;

    void Start()
    {
        if (mySlider == null)
        {
            mySlider = GetComponent<Slider>();
        }

        if (mySlider != null)
        {
            mySlider.enabled = false;
        }
    }

    public void EnableSlider()
    {
        if (mySlider != null)
        {
            mySlider.enabled = true;
        }
    }
}