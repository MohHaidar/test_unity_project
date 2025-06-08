using System;
using System.Collections.Generic;
using UnityEngine;

public class Challenge
{
    public string ChallengeName;
    public List<Step> challengeSteps;
    public bool ChallengeCompleted = false;
    public int CurrentStepIndex = 0;
    public Step CurrentStep => challengeSteps[CurrentStepIndex];

    public void RefreshChallenge()
    {
        CurrentStepIndex = 0;
        ChallengeCompleted = false;
    }
}
