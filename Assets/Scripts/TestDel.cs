using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDel : MonoBehaviour
{
    public float radius = 0.5f;               // 点击检测半径
    public LayerMask targetLayers = ~0;       // 需要检测的层
    public Camera cam;                        // 可选：指定使用的摄像机

    Vector2 lastClickPos;
    bool hasClick;

    void Awake()
    {
        if (cam == null)
        {
            cam = Camera.main;
        }
    }

    void Update()
    {
        // 按下触发一次，长按（保持按住）持续触发
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButton(0))
        {
            DeleteAtMousePosition();
        }
    }

    void DeleteAtMousePosition()
    {
        Vector3 screenPos = Input.mousePosition;
        if (cam == null)
        {
            return;
        }

        Vector3 worldPos = cam.ScreenToWorldPoint(screenPos);
        Vector2 pos2D = new Vector2(worldPos.x, worldPos.y);
        lastClickPos = pos2D;
        hasClick = true;

        Collider2D[] hits = Physics2D.OverlapCircleAll(pos2D, radius, targetLayers);
        if (hits == null || hits.Length == 0)
        {
            return;
        }

        HashSet<GameObject> processed = new HashSet<GameObject>();
        foreach (Collider2D hit in hits)
        {
            if (hit == null)
            {
                continue;
            }

            GameObject target = hit.attachedRigidbody ? hit.attachedRigidbody.gameObject : hit.gameObject;
            if (processed.Contains(target))
            {
                continue;
            }

            processed.Add(target);
            Destroy(target);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!hasClick)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastClickPos, radius);
    }
}

