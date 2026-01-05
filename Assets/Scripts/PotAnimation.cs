using System.Collections;
using UnityEngine;

public class PotAnimations : MonoBehaviour
{
    [Header("Animation Parameters")]
    [SerializeField] private Animator potAnimator;
    
    [Header("Animation Names")]
    [SerializeField] private string garlicAnimation = "Garlic";
    [SerializeField] private string chickenAnimation = "Chicken";
    [SerializeField] private string tomatoAnimation = "Tomato";
    [SerializeField] private string poisonAnimation = "Poison";
    [SerializeField] private string clockwiseAnimation = "Clockwise";
    [SerializeField] private string counterClockwiseAnimation = "CounterClockwise";
    [SerializeField] private string failAnimation = "Fail";
    [SerializeField] private string idleAnimation = "Idle";
    
    [Header("Settings")]
    [SerializeField] private float baseAnimationSpeed = 1f;
    [SerializeField] private float maxAnimationSpeed = 2f;
    [SerializeField] private float speedIncrement = 0.1f;
    [SerializeField] private float chickenChance = 0.5f;
    
    private float currentAnimationSpeed;
    private bool isPlayingAnimation = false;
    private Coroutine resetAnimationCoroutine;
    private int successCountSinceLastSpeedIncrease = 0;
    private int gesturesBeforeSpeedIncrease = 3;
    
    void Start()
    {
        // Make sure animator is set
        if (potAnimator == null)
        {
            potAnimator = GetComponent<Animator>();
            if (potAnimator == null)
            {
                Debug.LogError("PotAnimations: No Animator component found on this GameObject!");
            }
        }
        
        // Initialize animation speed
        currentAnimationSpeed = baseAnimationSpeed;
        UpdateAnimatorSpeed();
        
        SubscribeToEvents();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    void SubscribeToEvents()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.OnTap += PlayPoisonAnimation;
            TouchManager.Instance.OnSwipeUp += PlayGarlicOrChickenAnimation;
            TouchManager.Instance.OnSwipeDown += PlayTomatoAnimation;
            TouchManager.Instance.OnSwirl += PlaySwirlAnimation;
        }
        
        if (GestureChallengeManager.Instance != null)
        {
            GestureChallengeManager.Instance.OnSuccess += OnSuccess;
            GestureChallengeManager.Instance.OnFail += OnFail;
            
            // Also subscribe to restart to reset speed
            GestureChallengeManager.Instance.OnNewGesture += OnNewGesture;
        }
    }
    
    void UnsubscribeFromEvents()
    {
        if (TouchManager.Instance != null)
        {
            TouchManager.Instance.OnTap -= PlayPoisonAnimation;
            TouchManager.Instance.OnSwipeUp -= PlayGarlicOrChickenAnimation;
            TouchManager.Instance.OnSwipeDown -= PlayTomatoAnimation;
            TouchManager.Instance.OnSwirl -= PlaySwirlAnimation;
        }
        
        if (GestureChallengeManager.Instance != null)
        {
            GestureChallengeManager.Instance.OnSuccess -= OnSuccess;
            GestureChallengeManager.Instance.OnFail -= OnFail;
            GestureChallengeManager.Instance.OnNewGesture -= OnNewGesture;
        }
    }
    
    void OnSuccess()
    {
        // Track successes for speed increase
        successCountSinceLastSpeedIncrease++;
        
        // Increase animation speed every 3 successes (matching the time decrease pattern)
        if (successCountSinceLastSpeedIncrease >= gesturesBeforeSpeedIncrease)
        {
            IncreaseAnimationSpeed();
            successCountSinceLastSpeedIncrease = 0;
        }
        
        // Play a subtle success animation if you have one
        // You can add this if you create a "Success" animation
    }
    
    void OnFail()
    {
        PlayFailAnimation();
        ResetAnimationSpeed(); // Reset speed on failure
    }
    
    void OnNewGesture(GestureType gesture)
    {
        // We don't need to do anything here, but this helps track when challenge restarts
    }
    
    void IncreaseAnimationSpeed()
    {
        // Increase speed but cap at maximum
        currentAnimationSpeed = Mathf.Min(maxAnimationSpeed, currentAnimationSpeed + speedIncrement);
        UpdateAnimatorSpeed();
        
        Debug.Log($"Animation speed increased to: {currentAnimationSpeed:F2}x");
        
        // Optional: Play a visual/audio effect when speed increases
        StartCoroutine(SpeedIncreaseEffect());
    }
    
    IEnumerator SpeedIncreaseEffect()
    {
        // You can add a visual effect here, like a flash or particle effect
        // For example, temporarily increase speed even more for one animation
        float originalSpeed = currentAnimationSpeed;
        float temporarySpeed = currentAnimationSpeed * 1.5f;
        
        potAnimator.speed = temporarySpeed;
        
        // Flash effect (optional - add if you have a way to visualize it)
        // yield return new WaitForSeconds(0.3f);
        
        potAnimator.speed = currentAnimationSpeed;
        yield return null;
    }
    
    void ResetAnimationSpeed()
    {
        // Reset to base speed (called on failure or restart)
        currentAnimationSpeed = baseAnimationSpeed;
        successCountSinceLastSpeedIncrease = 0;
        UpdateAnimatorSpeed();
        
        Debug.Log("Animation speed reset to base");
    }
    
    void UpdateAnimatorSpeed()
    {
        if (potAnimator != null)
        {
            potAnimator.speed = currentAnimationSpeed;
        }
    }
    
    void PlayGarlicOrChickenAnimation()
    {
        if (!isPlayingAnimation && potAnimator != null)
        {
            // Randomly choose between garlic and chicken based on chickenChance
            bool playChicken = Random.Range(0f, 1f) < chickenChance;
            string animationName = playChicken ? chickenAnimation : garlicAnimation;
            
            StartCoroutine(PlayAnimationCoroutine(animationName));
        }
    }
    
    void PlayTomatoAnimation()
    {
        if (!isPlayingAnimation && potAnimator != null)
        {
            StartCoroutine(PlayAnimationCoroutine(tomatoAnimation));
        }
    }
    
    void PlayPoisonAnimation()
    {
        if (!isPlayingAnimation && potAnimator != null)
        {
            StartCoroutine(PlayAnimationCoroutine(poisonAnimation));
        }
    }
    
    void PlaySwirlAnimation(bool clockwise)
    {
        if (!isPlayingAnimation && potAnimator != null)
        {
            string animationName = clockwise ? clockwiseAnimation : counterClockwiseAnimation;
            StartCoroutine(PlayAnimationCoroutine(animationName));
        }
    }
    
    void PlayFailAnimation()
    {
        if (!isPlayingAnimation && potAnimator != null)
        {
            StartCoroutine(PlayAnimationCoroutine(failAnimation));
        }
    }
    
    IEnumerator PlayAnimationCoroutine(string animationName)
    {
        if (potAnimator == null) yield break;
        
        // Stop any existing reset coroutine
        if (resetAnimationCoroutine != null)
        {
            StopCoroutine(resetAnimationCoroutine);
        }
        
        // Play the animation
        isPlayingAnimation = true;
        potAnimator.Play(animationName);
        
        // Calculate wait time based on current speed
        float animationLength = GetAnimationLength(animationName);
        float waitTime = animationLength / currentAnimationSpeed;
        
        // Wait for animation to finish (adjusted for speed)
        yield return new WaitForSeconds(waitTime);
        
        // Reset to idle after a short delay
        resetAnimationCoroutine = StartCoroutine(ResetToIdleAfterDelay(0.3f / currentAnimationSpeed));
    }
    
    IEnumerator ResetToIdleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (potAnimator != null && !string.IsNullOrEmpty(idleAnimation))
        {
            potAnimator.Play(idleAnimation);
        }
        
        isPlayingAnimation = false;
        resetAnimationCoroutine = null;
    }
    
    public void ResetPotForRestart()
    {
        // Reset all states
        isPlayingAnimation = false;
        successCountSinceLastSpeedIncrease = 0;
    
        // Reset animation speed
        ResetAnimationSpeed();
    
        // Stop any running coroutines
        if (resetAnimationCoroutine != null)
        {
            StopCoroutine(resetAnimationCoroutine);
            resetAnimationCoroutine = null;
        }
    
        // Reset to idle animation
        if (potAnimator != null && !string.IsNullOrEmpty(idleAnimation))
        {
            potAnimator.Play(idleAnimation);
        }
    }
    
    float GetAnimationLength(string animationName)
    {
        if (potAnimator == null) return 1f;
        
        // Get animation length from animator
        RuntimeAnimatorController ac = potAnimator.runtimeAnimatorController;
        if (ac == null) return 1f;
        
        for (int i = 0; i < ac.animationClips.Length; i++)
        {
            if (ac.animationClips[i].name == animationName)
            {
                return ac.animationClips[i].length;
            }
        }
        
        // Default fallback
        return 1f;
    }
    
    // Public method to manually trigger animations (for testing or other purposes)
    public void TriggerAnimation(GestureType gesture)
    {
        switch (gesture)
        {
            case GestureType.Tap:
                PlayPoisonAnimation();
                break;
            case GestureType.SwipeUp:
                PlayGarlicOrChickenAnimation();
                break;
            case GestureType.SwipeDown:
                PlayTomatoAnimation();
                break;
            case GestureType.SwirlClockwise:
                PlaySwirlAnimation(true);
                break;
            case GestureType.SwirlCounterClockwise:
                PlaySwirlAnimation(false);
                break;
        }
    }
    
    // Public method to trigger fail animation
    public void TriggerFailAnimation()
    {
        PlayFailAnimation();
    }
    
    // Public method to reset speed (call this when restarting challenge)
    public void ResetSpeed()
    {
        ResetAnimationSpeed();
    }
    
    // Public property to get current speed
    public float CurrentSpeed
    {
        get { return currentAnimationSpeed; }
    }
    
    // Editor helper method to set base animation speed
    public void SetBaseAnimationSpeed(float speed)
    {
        baseAnimationSpeed = Mathf.Clamp(speed, 0.1f, 3f);
        ResetAnimationSpeed();
    }
}