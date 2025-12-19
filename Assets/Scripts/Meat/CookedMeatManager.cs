using System.Collections;
using UnityEngine;

public class CookedMeatManager : MonoBehaviour
{
    public GameObject lutoEgg;
    public GameObject hilawEgg;
    public GameObject eggplant;
    public bool isFinished = false;
    

    void Update()
    {
        if (eggplant.activeInHierarchy)
        {
            StartCoroutine(CookMeat());
        }
    }

    IEnumerator CookMeat()
    {
        yield return new WaitForSeconds(5f);

        lutoEgg.SetActive(true);
        hilawEgg.SetActive(false);

        yield return new WaitForSeconds(3f);
        isFinished = true;
    } 
}
