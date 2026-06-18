using System;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    private void Start()
    {
        GestureChallengeManager.Instance.StartChallenge();
    }
}
