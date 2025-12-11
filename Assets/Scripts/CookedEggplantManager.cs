using System.Collections;
using UnityEngine;

public class CookedEggplantManager : MonoBehaviour
{
    public GameObject lutoEgg;
    public GameObject hilawEgg;
    public GameObject eggplant;
    public bool isFinished = false;
    

    // Update is called once per frame
    void Update()
    {
        if (eggplant.activeInHierarchy)
        {
            StartCoroutine(CookEggplant());
        }
    }

    IEnumerator CookEggplant()
    {
        yield return new WaitForSeconds(5f);

        lutoEgg.SetActive(true);
        hilawEgg.SetActive(false);

        yield return new WaitForSeconds(3f);
        isFinished = true;
    } 
}
