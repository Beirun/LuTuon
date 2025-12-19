using UnityEngine;
using System.Collections.Generic;

public class BowlManager : MonoBehaviour
{
    [Header("References")]
    public BigBowlController bowlController;
    public List<Transform> objectsToSync;   

    private bool isCurrentlySynced = false;
    private Dictionary<Transform, Transform> originalParents = new Dictionary<Transform, Transform>();
    private Dictionary<Transform, bool> originalKinematicState = new Dictionary<Transform, bool>();

    void Start()
    {
        foreach (Transform obj in objectsToSync)
        {
            if (obj != null)
            {
                originalParents[obj] = obj.parent;
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    originalKinematicState[obj] = rb.isKinematic;
                }
            }
        }
    }

    void Update()
    {
        if (bowlController == null) return;

        if (bowlController.isDragging && !isCurrentlySynced)
        {
            Debug.LogWarning("Testat");
            AttachObjects();
        }
        else if (!bowlController.isDragging && isCurrentlySynced)
        {
            Debug.LogWarning("Testdetat");
            DetachObjects();
        }
    }

    void AttachObjects()
    {
        foreach (Transform obj in objectsToSync)
        {
            if (obj != null)
            {
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                }

                obj.SetParent(bowlController.transform);
            }
        }
        isCurrentlySynced = true;
    }

    void DetachObjects()
    {
        foreach (Transform obj in objectsToSync)
        {
            if (obj != null)
            {
                if (originalParents.ContainsKey(obj))
                {
                    obj.SetParent(originalParents[obj]);
                }
                else
                {
                    obj.SetParent(null);
                }

                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null && originalKinematicState.ContainsKey(obj))
                {
                    rb.isKinematic = originalKinematicState[obj];
                }
            }
        }
        isCurrentlySynced = false;
    }
}