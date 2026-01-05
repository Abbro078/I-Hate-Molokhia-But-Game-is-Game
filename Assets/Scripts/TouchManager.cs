using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems; // Add this

public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance;

    public event Action OnTap;
    public event Action OnSwipeUp;
    public event Action OnSwipeDown;
    public event Action<bool> OnSwirl; // true = CW, false = CCW

    [SerializeField] float minSwipeDistance = 100f;
    [SerializeField] float maxTapTime = 0.2f;
    [SerializeField] float maxTapMovement = 20f;

    Vector2 startPos;
    float startTime;
    List<Vector2> path = new List<Vector2>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);
        
        // ADD THIS CHECK - prevent touch if over UI
        if (IsTouchOverUI(touch)) return;

        switch (touch.phase)
        {
            case TouchPhase.Began:
                startPos = touch.position;
                startTime = Time.time;
                path.Clear();
                path.Add(startPos);
                break;

            case TouchPhase.Moved:
                path.Add(touch.position);
                break;

            case TouchPhase.Ended:
                EvaluateGesture(touch.position);
                break;
        }
    }
    
    // ADD THIS METHOD
    bool IsTouchOverUI(Touch touch)
    {
        // Check if touch is over any UI element
        if (EventSystem.current == null) return false;
        
        // Create a PointerEventData to check
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = touch.position;
        
        // Raycast all UI elements
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        
        // Return true if we hit any UI element (excluding the EventSystem itself)
        return results.Count > 0;
    }

    void EvaluateGesture(Vector2 endPos)
    {
        float duration = Time.time - startTime;
        float distance = Vector2.Distance(startPos, endPos);

        // TAP
        if (duration <= maxTapTime && distance <= maxTapMovement)
        {
            OnTap?.Invoke();
            return;
        }

        Vector2 delta = endPos - startPos;

        // SWIPE
        if (distance >= minSwipeDistance)
        {
            if (Mathf.Abs(delta.y) > Mathf.Abs(delta.x))
            {
                if (delta.y > 0) OnSwipeUp?.Invoke();
                else OnSwipeDown?.Invoke();
                return;
            }
        }

        // SWIRL
        if (DetectSwirl(path, out bool clockwise))
        {
            OnSwirl?.Invoke(clockwise);
        }
    }
    
    bool DetectSwirl(List<Vector2> points, out bool clockwise)
    {
        clockwise = false;
        if (points.Count < 10) return false;

        Vector2 center = Vector2.zero;
        foreach (var p in points) center += p;
        center /= points.Count;

        float totalAngle = 0f;

        for (int i = 1; i < points.Count - 1; i++)
        {
            Vector2 a = (points[i] - center).normalized;
            Vector2 b = (points[i + 1] - center).normalized;
            totalAngle += Vector2.SignedAngle(a, b);
        }

        if (Mathf.Abs(totalAngle) > 270f)
        {
            clockwise = totalAngle < 0;
            return true;
        }

        return false;
    }
}