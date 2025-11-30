using UnityEngine;
using System.Collections.Generic;

public class BowlManager : MonoBehaviour
{
    [Header("References")]
    public BigBowlController bowlController; // Drag your BigBowl here
    public List<Transform> objectsToSync;    // Drag your 2 objects here

    // Internal state tracking
    private bool isCurrentlySynced = false;
    private Dictionary<Transform, Transform> originalParents = new Dictionary<Transform, Transform>();
    private Dictionary<Transform, bool> originalKinematicState = new Dictionary<Transform, bool>();

    void Start()
    {
        // Remember who the parents were at the very start of the game
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

    // We use LateUpdate to prevent "Lag/Jitter". 
    // This runs AFTER the DragController has moved the bowl.
    void Update()
    {
        if (bowlController == null) return;

        // Check if the bowl is currently being dragged OR is animating back/pouring
        // In your DragController, isDragging stays true until ReturnToStart finishes.
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
                // 1. Disable Physics (Fixes the Physics Lag)
                Rigidbody rb = obj.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = true;
                    // Optional: rb.useGravity = false; 
                }

                // 2. Parent them to the bowl
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
                // 1. Restore Parent
                if (originalParents.ContainsKey(obj))
                {
                    obj.SetParent(originalParents[obj]);
                }
                else
                {
                    obj.SetParent(null); // Was a root object
                }

                // 2. Restore Physics
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