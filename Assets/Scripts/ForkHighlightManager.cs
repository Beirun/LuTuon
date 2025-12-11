using UnityEngine;

public class ForkHighlightManager : MonoBehaviour
{
    public GameObject Eggplant;
    public GameObject Bowl;
    public TutorialManager TutorialManager;
    public string EggplantHighlightTag = "Untagged";
    public string BowlHighlightTag = "Untagged";

    void Update()
    {
        if(TutorialManager.stepIndex == 8)
        {
            EggplantHighlightTag = Eggplant.tag;
            Eggplant.tag = "Untagged";

        }else if(TutorialManager.stepIndex == 9)
        {
            BowlHighlightTag = Bowl.tag;
            Bowl.tag = "Untagged";
        }
    }
}