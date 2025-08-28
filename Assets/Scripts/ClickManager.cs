using UnityEngine;

public class ClickManager : MonoBehaviour
{
    public LayerMask boxLayer; 

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 p = new Vector2(wp.x, wp.y);
            var hit = Physics2D.OverlapPoint(p, boxLayer);
            if (hit)
            {
                var box = hit.GetComponentInParent<Box>();
                if (box != null) box.TryClick();
            }
        }
    }
}