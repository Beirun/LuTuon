using UnityEngine;

public class OnionManager : MonoBehaviour
{
    [Header("Onion Parts")]
    public GameObject wholeOnion;
    public GameObject OnionHalf;
    public GameObject OnionThird;
    public GameObject OnionQuarter;

    [Header("Diced Onions")]
    public GameObject dicedOnion_1;
    public GameObject dicedOnion_2;
    public GameObject dicedOnion_3;
    public GameObject dicedOnion_4;


    [Header("Knife Controller")]
    public KnifeController knifeController;

    [Header("Onion Controller")]
    public OnionController onionController;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 onionPos = wholeOnion.transform.position;
        Quaternion onionRot = Quaternion.Euler(
            wholeOnion.transform.rotation.x, 
            wholeOnion.transform.rotation.y, 
            wholeOnion.transform.rotation.z + 270f);
        if (onionController.isPlaced)
        {
            if (knifeController.cutsMade == 1)
            {
                OnionHalf.transform.position = onionPos;
                OnionHalf.transform.rotation = onionRot;
                wholeOnion.SetActive(false);
                OnionHalf.SetActive(true);
                OnionThird.SetActive(false);
                OnionQuarter.SetActive(false);
                dicedOnion_1.SetActive(true);
                dicedOnion_2.SetActive(false);
                dicedOnion_3.SetActive(false);
                dicedOnion_4.SetActive(false);
            }
            else if (knifeController.cutsMade == 2)
            {
                OnionThird.transform.position = onionPos;
                OnionThird.transform.rotation = onionRot;
                OnionHalf.SetActive(false);
                OnionThird.SetActive(true);
                OnionQuarter.SetActive(false);
                dicedOnion_1.SetActive(true);
                dicedOnion_2.SetActive(true);
                dicedOnion_3.SetActive(false);
                dicedOnion_4.SetActive(false);
            }
            else if (knifeController.cutsMade == 3)
            {
                OnionQuarter.transform.position = onionPos;
                OnionQuarter.transform.rotation = onionRot;
                OnionHalf.SetActive(false);
                OnionThird.SetActive(false);
                OnionQuarter.SetActive(true);
                dicedOnion_1.SetActive(true);
                dicedOnion_2.SetActive(true);
                dicedOnion_3.SetActive(true);
                dicedOnion_4.SetActive(false);
            }
            else if (knifeController.cutsMade >= 4)
            {
                OnionHalf.SetActive(false);
                OnionThird.SetActive(false);
                OnionQuarter.SetActive(false);
                dicedOnion_1.SetActive(true);
                dicedOnion_2.SetActive(true);
                dicedOnion_3.SetActive(true);
                dicedOnion_4.SetActive(true);
                onionController.enabled = false;
            }
        }
    }
}
