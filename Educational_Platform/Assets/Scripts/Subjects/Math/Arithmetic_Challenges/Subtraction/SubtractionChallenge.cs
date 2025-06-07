using System;
using System.Collections.Generic;
namespace Subjects.Math.Arithmetic_Challenges.Subtraction
{
    public static class SubtractionChallenge
    {
        public static Challenge Create()
        {
            var challenge = new Challenge
            {
                ChallengeName = "Subtraction",
                UltimateModule = QuestionGenerator.GenerateUltimateChallenge(),
                ChallengeMMR = 500
            };

            challenge.Modules = new List<Func<QuestionModule>> {
                // () => QuestionGenerator.GenerateBasicSubtraction(),
                () => QuestionGenerator.GenerateWithZero()
            };

            return challenge;
        }
    }
}
