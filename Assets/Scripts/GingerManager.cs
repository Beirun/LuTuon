using UnityEngine;

public class GingerManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [Header("Onion Parts")]
    public GameObject Ginger;
    public GameObject Ginger1;
    public GameObject Ginger2;
    public GameObject Ginger3;

    [Header("Chopped Ginger")]
    public GameObject choppedGinger_1;
    public GameObject choppedGinger_2;
    public GameObject choppedGinger_3;
    public GameObject choppedGinger_4;


    [Header("Knife Controller")]
    public KnifeController knifeController;

    [Header("Ginger Controller")]
    public GingerController gingerController;

    [Header("Chopped Ginger Controller")]
    public ChoppedGingerController choppedGingerController;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 onionPos = Ginger.transform.position;

        if (gingerController.isPlaced)
        {
            if (knifeController.cutsMade == 1)
            {
                Ginger1.transform.position = onionPos;
                Ginger.SetActive(false);
                Ginger1.SetActive(true);
                Ginger2.SetActive(false);
                Ginger3.SetActive(false);
                choppedGinger_1.SetActive(true);
                choppedGinger_2.SetActive(false);
                choppedGinger_3.SetActive(false);
                choppedGinger_4.SetActive(false);
            }
            else if (knifeController.cutsMade == 2)
            {
                Ginger2.transform.position = onionPos;
                Ginger1.SetActive(false);
                Ginger2.SetActive(true);
                Ginger3.SetActive(false);
                choppedGinger_1.SetActive(true);
                choppedGinger_2.SetActive(true);
                choppedGinger_3.SetActive(false);
                choppedGinger_4.SetActive(false);
            }
            else if (knifeController.cutsMade == 3)
            {
                Ginger3.transform.position = onionPos;
                Ginger1.SetActive(false);
                Ginger2.SetActive(false);
                Ginger3.SetActive(true);
                choppedGinger_1.SetActive(true);
                choppedGinger_2.SetActive(true);
                choppedGinger_3.SetActive(true);
                choppedGinger_4.SetActive(false);
            }
            else if (knifeController.cutsMade >= 4)
            {
                Ginger1.SetActive(false);
                Ginger2.SetActive(false);
                Ginger3.SetActive(false);
                choppedGinger_1.SetActive(true);
                choppedGinger_2.SetActive(true);
                choppedGinger_3.SetActive(true);
                choppedGinger_4.SetActive(true);
                gingerController.enabled = false;
                knifeController.cutsMade = 0;
                choppedGingerController.startPos = new Vector3(gingerController.startPos.x + 0.2f, gingerController.startPos.y + 0.05f, gingerController.startPos.z - 0.32f);
                StartCoroutine(choppedGingerController.AnimatePlacement(choppedGingerController.startPos, Quaternion.Euler(9.4f, 22f, -3.2f), 0.75f));
                gingerController.isPlaced = false;
            }
        }

    }
}
