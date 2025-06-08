using System;
using System.Collections.Generic;
namespace Subjects.Math.Arithmetic_Challenges.HighNumbers
{
    public static class HighNumbersChallenge
    {
        public static Challenge Create()
        {
            var challenge = new Challenge
            {
                ChallengeName = "High Numbers"
            };

            challenge.challengeSteps = new List<Step> {
                new Step1Activities(),
                new Step2Activities(),
                new Step3Activities(),
                new Step4Activities(),
                new Step5Activities()
            };

            return challenge;
        }
    }
}