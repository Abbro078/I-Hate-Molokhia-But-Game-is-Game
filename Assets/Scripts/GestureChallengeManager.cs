using System;
using System.Collections;
using UnityEngine;

using System;
using System.Collections;
using UnityEngine;

public class GestureChallengeManager : MonoBehaviour
{
    public static GestureChallengeManager Instance;

    public GestureType CurrentGesture { get; private set; }
    public bool IsActive { get; private set; }
    public float CurrentTimeLimit { get; private set; }
    public float TimeRemaining { get; private set; }
    public int Score { get; private set; }
    public int ConsecutiveSuccesses { get; private set; }

    [Header("Challenge Settings")]
    [SerializeField] private float baseTimeLimit = 2f;
    [SerializeField] private float minTimeLimit = 0.5f;
    [SerializeField] private float timeDecrement = 0.1f;
    [SerializeField] private int gesturesBeforeDecrement = 3;
    [SerializeField] private bool autoStartOnEnable = true;

    private Coroutine timerCoroutine;

    public event Action<GestureType> OnNewGesture;
    public event Action OnSuccess;
    public event Action OnFail;
    public event Action<float> OnTimerUpdate;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void OnEnable()
    {
        if (autoStartOnEnable)
        {
            StartChallenge();
        }
    }

    void Start()
    {
        Subscribe();
    }

    void OnDisable()
    {
        Unsubscribe();
        StopChallenge();
    }

    void Subscribe()
    {
        if (TouchManager.Instance == null) return;

        TouchManager.Instance.OnTap += OnTap;
        TouchManager.Instance.OnSwipeUp += OnSwipeUp;
        TouchManager.Instance.OnSwipeDown += OnSwipeDown;
        TouchManager.Instance.OnSwirl += HandleSwirl;
    }

    void Unsubscribe()
    {
        if (TouchManager.Instance == null) return;

        TouchManager.Instance.OnTap -= OnTap;
        TouchManager.Instance.OnSwipeUp -= OnSwipeUp;
        TouchManager.Instance.OnSwipeDown -= OnSwipeDown;
        TouchManager.Instance.OnSwirl -= HandleSwirl;
    }

    void HandleSwirl(bool clockwise)
    {
        Evaluate(clockwise ? GestureType.SwirlClockwise : GestureType.SwirlCounterClockwise);
    }

    public void StartChallenge()
    {
        if (IsActive) return;

        IsActive = true;
        Score = 0;
        ConsecutiveSuccesses = 0;
        CurrentTimeLimit = baseTimeLimit;
        
        StartNewGesture();
    }

    public void StopChallenge()
    {
        IsActive = false;
        
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    // NEW METHOD: Public restart method for UI integration
    public void RestartChallenge()
    {
        // First stop the current challenge
        StopChallenge();
        
        // Wait one frame to ensure everything is cleaned up
        StartCoroutine(RestartAfterFrame());
    }

    IEnumerator RestartAfterFrame()
    {
        yield return null; // Wait one frame
        StartChallenge();
    }

    void StartNewGesture()
    {
        // Stop existing timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
        }
        
        // Set new random gesture
        CurrentGesture = GetRandomGesture();
        
        // Reset timer
        TimeRemaining = CurrentTimeLimit;
        
        // Start countdown
        timerCoroutine = StartCoroutine(GestureTimer());
        
        Debug.Log($"Do gesture: {CurrentGesture} within {CurrentTimeLimit:F1}s");
        OnNewGesture?.Invoke(CurrentGesture);
    }

    IEnumerator GestureTimer()
    {
        while (TimeRemaining > 0)
        {
            yield return null;
            TimeRemaining -= Time.deltaTime;
            OnTimerUpdate?.Invoke(TimeRemaining);
        }
        
        // Time's up - fail
        TimeRemaining = 0;
        OnTimerUpdate?.Invoke(TimeRemaining);
        HandleFailure();
    }

    void Evaluate(GestureType input)
    {
        if (!IsActive) return;

        if (input == CurrentGesture)
        {
            HandleSuccess();
        }
        else
        {
            HandleFailure();
        }
    }

    void HandleSuccess()
    {
        // Stop the timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // Update scores
        Score++;
        ConsecutiveSuccesses++;
        
        Debug.Log($"SUCCESS! Score: {Score}");
        OnSuccess?.Invoke();
        
        // Adjust time limit after every 3 consecutive successes
        if (ConsecutiveSuccesses >= gesturesBeforeDecrement)
        {
            CurrentTimeLimit = Mathf.Max(minTimeLimit, CurrentTimeLimit - timeDecrement);
            ConsecutiveSuccesses = 0; // Reset for next set
            Debug.Log($"Time limit decreased to: {CurrentTimeLimit:F1}s");
        }
        
        // Start next gesture
        StartNewGesture();
    }

    void HandleFailure()
    {
        if (!IsActive) return;
        
        // Stop the timer
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
        
        // Reset consecutive successes but keep total score
        ConsecutiveSuccesses = 0;
        
        Debug.Log($"FAIL! Time's up or wrong gesture. Final Score: {Score}");
        IsActive = false;
        OnFail?.Invoke();
    }

    GestureType GetRandomGesture()
    {
        Array values = Enum.GetValues(typeof(GestureType));
        return (GestureType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
    }

    void OnTap() => Evaluate(GestureType.Tap);
    void OnSwipeUp() => Evaluate(GestureType.SwipeUp);
    void OnSwipeDown() => Evaluate(GestureType.SwipeDown);
}