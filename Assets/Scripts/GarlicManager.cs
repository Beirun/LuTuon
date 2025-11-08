using UnityEngine;

public class GarlicManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Garlic Parts")]
    public GameObject Garlic;
    public GameObject Garlic1;
    public GameObject Garlic2;
    public GameObject Garlic3;

    [Header("Chopped Garlic")]
    public GameObject choppedGarlic_1;
    public GameObject choppedGarlic_2;
    public GameObject choppedGarlic_3;
    public GameObject choppedGarlic_4;


    [Header("Knife Controller")]
    public KnifeController knifeController;

    [Header("Garlic Controller")]
    public GarlicController garlicController;

    [Header("Chopped Garlic Controller")]
    public ChoppedGarlicController choppedGarlicController;
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 garlicPos = Garlic.transform.position;

        if (garlicController.isPlaced)
        {
            if (knifeController.cutsMade == 1)
            {
                Garlic1.transform.position = garlicPos;
                Garlic.SetActive(false);
                Garlic1.SetActive(true);
                Garlic2.SetActive(false);
                Garlic3.SetActive(false);
                choppedGarlic_1.SetActive(true);
                choppedGarlic_2.SetActive(false);
                choppedGarlic_3.SetActive(false);
                choppedGarlic_4.SetActive(false);
            }
            else if (knifeController.cutsMade == 2)
            {
                Garlic2.transform.position = garlicPos;
                Garlic1.SetActive(false);
                Garlic2.SetActive(true);
                Garlic3.SetActive(false);
                choppedGarlic_1.SetActive(true);
                choppedGarlic_2.SetActive(true);
                choppedGarlic_3.SetActive(false);
                choppedGarlic_4.SetActive(false);
            }
            else if (knifeController.cutsMade == 3)
            {
                Garlic3.transform.position = garlicPos;
                Garlic1.SetActive(false);
                Garlic2.SetActive(false);
                Garlic3.SetActive(true);
                choppedGarlic_1.SetActive(true);
                choppedGarlic_2.SetActive(true);
                choppedGarlic_3.SetActive(true);
                choppedGarlic_4.SetActive(false);
            }
            else if (knifeController.cutsMade >= 4)
            {
                Garlic1.SetActive(false);
                Garlic2.SetActive(false);
                Garlic3.SetActive(false);
                choppedGarlic_1.SetActive(true);
                choppedGarlic_2.SetActive(true);
                choppedGarlic_3.SetActive(true);
                choppedGarlic_4.SetActive(true);
                garlicController.enabled = false;
                knifeController.cutsMade = 0;
                choppedGarlicController.startPos = new Vector3(garlicController.startPos.x - 0.3f, garlicController.startPos.y - 0.05f , garlicController.startPos.z - 0.05f);
                StartCoroutine(choppedGarlicController.AnimatePlacement(choppedGarlicController.startPos, Quaternion.Euler(0f, 0f, 0f), 0.75f, false));
                garlicController.isPlaced = false;
            }
        }

    }
}
