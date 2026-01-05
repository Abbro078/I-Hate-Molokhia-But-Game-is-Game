using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GestureChallengeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI gestureText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI timeLimitText;
    [SerializeField] private Image timerFill;
    [SerializeField] private GameObject challengePanel;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private Button restartButton;

    [Header("Pots")]
    [SerializeField] private GameObject activePotObject;
    [SerializeField] private GameObject losePotObject;

    void Start()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartChallenge);
        }

        StartCoroutine(SubscribeAfterDelay());
    }

    IEnumerator SubscribeAfterDelay()
    {
        yield return null;
        
        if (GestureChallengeManager.Instance != null)
        {
            GestureChallengeManager.Instance.OnNewGesture += OnNewGesture;
            GestureChallengeManager.Instance.OnSuccess += OnSuccess;
            GestureChallengeManager.Instance.OnFail += OnFail;
            GestureChallengeManager.Instance.OnTimerUpdate += OnTimerUpdate;
            
            if (GestureChallengeManager.Instance.IsActive)
            {
                OnNewGesture(GestureChallengeManager.Instance.CurrentGesture);
            }
            UpdateUI();
        }
        else
        {
            Debug.LogWarning("GestureChallengeManager.Instance is null in UI. Make sure the manager exists in the scene.");
        }
    }

    void OnDestroy()
    {
        if (GestureChallengeManager.Instance != null)
        {
            GestureChallengeManager.Instance.OnNewGesture -= OnNewGesture;
            GestureChallengeManager.Instance.OnSuccess -= OnSuccess;
            GestureChallengeManager.Instance.OnFail -= OnFail;
            GestureChallengeManager.Instance.OnTimerUpdate -= OnTimerUpdate;
        }

        if (restartButton != null)
        {
            restartButton.onClick.RemoveListener(RestartChallenge);
        }
    }

    void OnNewGesture(GestureType gesture)
    {
        if (gestureText != null)
        {
            string displayText = GetCleanGestureText(gesture);
            gestureText.text = displayText;
        }
    
        if (timeLimitText != null && GestureChallengeManager.Instance != null)
        {
            timeLimitText.text = $"{GestureChallengeManager.Instance.CurrentTimeLimit:F1}s";
        }
    
        UpdateUI();
    }

    string GetCleanGestureText(GestureType gesture)
    {
        switch (gesture)
        {
            case GestureType.Tap:
                return "Tap";
            case GestureType.SwipeUp:
                return "Swipe Up";
            case GestureType.SwipeDown:
                return "Swipe Down";
            case GestureType.SwirlClockwise:
                return "Swirl Clockwise";
            case GestureType.SwirlCounterClockwise:
                return "Swirl Counter-Clockwise";
            default:
                return gesture.ToString().ToUpper();
        }
    }

    void OnSuccess()
    {
        UpdateUI();
    }

    void OnFail()
    {
        if (resultPanel != null)
        {
            if (challengePanel != null)
            {
                challengePanel.SetActive(false);
            }
        
            resultPanel.SetActive(true);
        
            if (finalScoreText != null && GestureChallengeManager.Instance != null)
            {
                finalScoreText.text = $"{Math.Floor((double)GestureChallengeManager.Instance.Score/3)}";
            }
        }
    
        if (activePotObject != null) activePotObject.SetActive(false);
        if (losePotObject != null) losePotObject.SetActive(true);
    }
    void OnTimerUpdate(float timeRemaining)
    {
        if (GestureChallengeManager.Instance != null)
        {
            float currentTimeLimit = GestureChallengeManager.Instance.CurrentTimeLimit;
            float progress = timeRemaining / currentTimeLimit;
            
            if (timerText != null)
            {
                timerText.text = $"{timeRemaining:F1}";
            }
            
            if (timerFill != null)
            {
                timerFill.fillAmount = progress;
                // Optional: Change color based on time remaining
                timerFill.color = Color.Lerp(Color.red, Color.green, progress);
            }
        }
    }

    void UpdateUI()
    {
        if (scoreText != null && GestureChallengeManager.Instance != null)
        {
            scoreText.text = $"{GestureChallengeManager.Instance.Score}";
        }
    }

    private void RestartChallenge()
    {
        if (resultPanel != null && resultPanel.activeSelf)
        {
            resultPanel.SetActive(false);
        }
    
        if (challengePanel != null)
        {
            challengePanel.SetActive(true);
        }
    
        if (losePotObject != null) losePotObject.SetActive(false);
        if (activePotObject != null) activePotObject.SetActive(true);
    
        if (activePotObject != null)
        {
            PotAnimations potAnim = activePotObject.GetComponent<PotAnimations>();
            if (potAnim != null)
            {
                potAnim.ResetPotForRestart();
            }
        }
    
        if (GestureChallengeManager.Instance != null)
        {
            GestureChallengeManager.Instance.RestartChallenge();
        }
        else
        {
            Debug.LogError("Cannot restart: GestureChallengeManager.Instance is null!");
        }
    }
}