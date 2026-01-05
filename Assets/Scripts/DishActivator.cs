using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DishActivator : MonoBehaviour
{
    [SerializeField] private List<GameObject> dishes;
    
    private int currentDishIndex = 0;
    private int lastActivationScore = 0;
    
    void Start()
    {
        DeactivateAllDishes();
        
        StartCoroutine(DelayedSubscribe());
    }
    
    IEnumerator DelayedSubscribe()
    {
        yield return null;
        
        if (GestureChallengeManager.Instance != null)
        {
            GestureChallengeManager.Instance.OnSuccess += OnSuccess;
            GestureChallengeManager.Instance.OnFail += OnFail;
            GestureChallengeManager.Instance.OnNewGesture += OnNewGesture;
            
            if (GestureChallengeManager.Instance.Score > 0)
            {
                ResetAndActivateBasedOnScore(GestureChallengeManager.Instance.Score);
            }
        }
    }
    
    void OnDestroy()
    {
        if (GestureChallengeManager.Instance != null)
        {
            GestureChallengeManager.Instance.OnSuccess -= OnSuccess;
            GestureChallengeManager.Instance.OnFail -= OnFail;
            GestureChallengeManager.Instance.OnNewGesture -= OnNewGesture;
        }
    }
    
    void OnSuccess()
    {
        if (GestureChallengeManager.Instance == null) return;
        
        int currentScore = GestureChallengeManager.Instance.Score;
        
        if (currentScore > 0 && currentScore % 3 == 0 && currentScore > lastActivationScore)
        {
            ActivateNextDish();
            lastActivationScore = currentScore;
        }
    }
    
    void OnFail()
    {
        // Optional: Decide what happens on fail
        // If you want to keep dishes visible, do nothing
        // If you want to hide them, uncomment:
        // ResetDishes();
    }
    
    void OnNewGesture(GestureType gesture)
    {
        if (GestureChallengeManager.Instance != null && 
            GestureChallengeManager.Instance.Score == 0)
        {
            ResetDishes();
        }
    }
    
    void ActivateNextDish()
    {
        if (currentDishIndex < dishes.Count && dishes[currentDishIndex] != null)
        {
            dishes[currentDishIndex].SetActive(true);
            Debug.Log($"Activated dish {currentDishIndex + 1} at score {lastActivationScore}");
            currentDishIndex++;
            
            // Optional: Play a sound or particle effect
            // PlayActivationEffect();
        }
        else if (currentDishIndex >= dishes.Count)
        {
            Debug.Log("All dishes have been activated!");
            
            // Optional: Trigger a celebration
            // AllDishesCompleted();
        }
    }
    
    void DeactivateAllDishes()
    {
        foreach (GameObject dish in dishes)
        {
            if (dish != null)
            {
                dish.SetActive(false);
            }
        }
    }
    
    void ResetAndActivateBasedOnScore(int score)
    {
        DeactivateAllDishes();
        currentDishIndex = 0;
        lastActivationScore = 0;
        
        int dishesToActivate = Mathf.Min(score / 3, dishes.Count);
        
        for (int i = 0; i < dishesToActivate; i++)
        {
            if (i < dishes.Count && dishes[i] != null)
            {
                dishes[i].SetActive(true);
                currentDishIndex++;
            }
        }
        
        lastActivationScore = dishesToActivate * 3;
    }
    
    public void ResetDishes()
    {
        DeactivateAllDishes();
        currentDishIndex = 0;
        lastActivationScore = 0;
    }
    
    public int GetCurrentDishIndex()
    {
        return currentDishIndex;
    }
    
    public int GetTotalDishes()
    {
        return dishes.Count;
    }
    
    public bool AreAllDishesActivated()
    {
        return currentDishIndex >= dishes.Count;
    }
    
    // Optional: Visual feedback methods
    /*
    void PlayActivationEffect()
    {
        // Play particle effect, sound, etc.
        if (dishes[currentDishIndex - 1] != null)
        {
            // Example: dishes[currentDishIndex - 1].GetComponent<ParticleSystem>().Play();
        }
    }
    
    void AllDishesCompleted()
    {
        // Trigger celebration
        Debug.Log("All dishes completed! Celebration time!");
    }
    */
}