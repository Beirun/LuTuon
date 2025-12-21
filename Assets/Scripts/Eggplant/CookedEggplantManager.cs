using System.Collections;
using UnityEngine;

public class CookedEggplantManager : MonoBehaviour
{
    public GameObject lutoEgg;
    public GameObject hilawEgg;
    public GameObject eggplant;
    public bool isFinished = false;
        public Material cookedEggPlantWithOnionMaterial;
    public Material cookedEggPlantWithTomatoMaterial;
    public Material cookedEggPlantWithBothMaterial;
    public ChoppedTomatoController tomato;
    public DicedOnionsController onion;
    public MeshRenderer mesh;

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
        if (onion != null && tomato != null)
        {
            if (tomato.isPlaced)
            {
                if (onion.isInPot) mesh.material = cookedEggPlantWithBothMaterial;
                else mesh.material = cookedEggPlantWithTomatoMaterial;
            }
            else mesh.material = cookedEggPlantWithOnionMaterial;
        }
        lutoEgg.SetActive(true);
        hilawEgg.SetActive(false);

        yield return new WaitForSeconds(3f);
        isFinished = true;
    } 
}
