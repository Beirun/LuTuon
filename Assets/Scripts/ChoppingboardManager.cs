using UnityEngine;

public class ChoppingboardManager : MonoBehaviour
{
    public bool isOccupied = false;
    KnifeController knife;
    void Start()
    {
        knife = FindFirstObjectByType<KnifeController>();
    }

    void Update()
    {
        
    }
}
