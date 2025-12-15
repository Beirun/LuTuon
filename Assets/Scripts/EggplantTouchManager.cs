using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // Needed to detect button clicks

public class EggplantTouchManager : MonoBehaviour
{
    public Camera cam;
    public Button tmpBtn;
    public float offset = 0.55f;
    public bool isFinished = false;
    public bool isDragging = false;
    public bool isPerforming = false;
    public string targetTag = "Eggplant";

    DragManager dragManager;
    Transform target;
    private void Start()
    {
        enabled = false;
        dragManager = FindFirstObjectByType<DragManager>();
    }

    void Awake()
    {
        if (!cam) cam = Camera.main;
        if (tmpBtn) tmpBtn.gameObject.SetActive(false);
    }

    void Update()
    {
        if (Input.touchCount > 0 && !isPerforming && !dragManager.areDisabled)
        {
            Touch t = Input.GetTouch(0);

            if (IsPointerOverUI(t)) return;

            if (t.phase == TouchPhase.Ended)
            {
                HandleTouch(t.position);
            }
        }

        if (target) Position();
    }

    void HandleTouch(Vector2 pos)
    {
        Ray r = cam.ScreenPointToRay(pos);

        if (Physics.Raycast(r, out RaycastHit h))
        {
            if (h.transform.CompareTag(targetTag))
            {
                target = h.transform;
                tmpBtn.gameObject.SetActive(true);
                Position();
            }
            else
            {
                HideButton();
            }
        }
        else
        {
            HideButton();
        }
    }

    public void HideButton()
    {
        target = null;
        tmpBtn.gameObject.SetActive(false);
        if(!isPerforming) isDragging = false;
    }

    void Position()
    {
        Vector3 p = target.position + cam.transform.right * offset;
        Vector3 screen = cam.WorldToScreenPoint(p);
        if (screen.z < 0) screen = cam.WorldToScreenPoint(target.position);
        tmpBtn.transform.position = screen;
        isDragging = true;  
    }

    bool IsPointerOverUI(Touch t)
    {
        if (EventSystem.current == null) return false;
        return EventSystem.current.IsPointerOverGameObject(t.fingerId);
    }
}