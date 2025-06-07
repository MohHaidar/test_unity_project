using System;
using System.Collections.Generic;
namespace Subjects.Math.Arithmetic_Challenges.NegativeNumbers
{
    public static class NegativeNumbersChallenge
    {
        public static Challenge Create()
        {
            var challenge = new Challenge
            {
                ChallengeName = "Negative Numbers",
                UltimateModule = QuestionGenerator.GenerateUltimateChallenge(),
                ChallengeMMR = 400
            };

            challenge.Modules = new List<Func<QuestionModule>> {
                () => QuestionGenerator.GenerateNegativeAddition(),
                () => QuestionGenerator.GenerateNegativeSubtraction(),
                () => QuestionGenerator.GenerateNegativeComparison()
            };

            return challenge;
        }
    }
}
