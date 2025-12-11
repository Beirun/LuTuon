using UnityEngine;

public class ForkHighlightManager : MonoBehaviour
{
    public TutorialManager TutorialManager;
    public ForkController forkController;
    bool hasRemovedEggplantTag = false;
    bool hasRemovedBowlTag = false;
    public string EggplantHighlightTag = "Untagged";
    public string BowlHighlightTag = "Untagged";

    void Update()
    {
        if(TutorialManager.stepIndex == 8 && !hasRemovedEggplantTag)
        {
            forkController.highlightTags.Remove(EggplantHighlightTag);
            hasRemovedEggplantTag = true;
        }
        else if(TutorialManager.stepIndex == 9 && !hasRemovedBowlTag)
        {
            forkController.highlightTags.Remove(BowlHighlightTag);
            forkController.highlightTags.Add(EggplantHighlightTag);
            hasRemovedBowlTag = true;   
        }
    }
}