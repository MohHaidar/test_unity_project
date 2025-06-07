using System;
using System.Collections.Generic;
namespace Subjects.Math.Arithmetic_Challenges.Multiplication
{
    public static class MultiplicationChallenge
    {
        public static Challenge Create()
        {
            var challenge = new Challenge
            {
                ChallengeName = "Multiplication",
                UltimateModule = QuestionGenerator.GenerateUltimateChallenge(),
                ChallengeMMR = 300
            };

            challenge.Modules = new List<Func<QuestionModule>> {
                () => QuestionGenerator.GenerateSingleDigitMultiplication(),
                () => QuestionGenerator.GenerateTensMultiplication()
            };

            return challenge;
        }
    }
}
