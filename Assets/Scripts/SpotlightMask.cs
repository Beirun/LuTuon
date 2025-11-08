using UnityEngine;
using UnityEngine.UI;

public class SpotlightMask : MonoBehaviour
{
    public Transform target;
    public float radius = 0.2f;

    Material m;
    Camera cam;

    void Start()
    {
        cam = Camera.main;
        var img = GetComponent<Image>();
        m = Instantiate(img.material);
        img.material = m;
        m.SetFloat("_Radius", radius);
    }

    void Update()
    {
        if (!target) return;

        Vector3 p = cam.WorldToScreenPoint(target.position);
        Vector2 uv = new Vector2(p.x / Screen.width, p.y / Screen.height);
        m.SetVector("_Center", new Vector4(uv.x, uv.y, 0, 0));
    }
}
