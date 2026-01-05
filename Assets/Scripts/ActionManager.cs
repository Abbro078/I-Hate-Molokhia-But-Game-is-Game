using System;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    private void Start()
    {
        GestureChallengeManager.Instance.StartChallenge();
    }
    // void Start()
    // {
    //     TouchManager.Instance.OnTap += HandleTap;
    //     TouchManager.Instance.OnSwipeUp += Jump;
    //     TouchManager.Instance.OnSwirl += RotateValve;
    // }
    //
    // void OnDisable()
    // {
    //     TouchManager.Instance.OnTap -= HandleTap;
    //     TouchManager.Instance.OnSwipeUp -= Jump;
    //     TouchManager.Instance.OnSwirl -= RotateValve;
    // }
    //
    // void RotateValve(bool clockwise)
    // {
    //     Debug.Log(clockwise ? "Turn Right" : "Turn Left");
    // }
    //
    //
    // public void HandleTap()
    // {
    //     Debug.Log("Tap");
    // }
    //
    // public void Jump()
    // {
    //     Debug.Log("Jump");
    // }
}
