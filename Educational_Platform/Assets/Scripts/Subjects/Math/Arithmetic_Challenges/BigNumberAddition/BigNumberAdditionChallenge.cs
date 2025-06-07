using System;
using System.Collections.Generic;
namespace Subjects.Math.Arithmetic_Challenges.BigNumberAddition
{
    public static class BigNumberAdditionChallenge
    {
        public static Challenge Create()
        {
            var challenge = new Challenge
            {
                ChallengeName = "Big Number Addition",
                UltimateModule = QuestionGenerator.GenerateUltimateAdditionChallenge(),
                ChallengeMMR = 200
            };

            challenge.Modules = new List<Func<QuestionModule>> {
                () => QuestionGenerator.GenerateSingleDigitNoCarry(),
                () => QuestionGenerator.GenerateSingleDigitWithCarry(),
                () => QuestionGenerator.GenerateTensNoCarry(),
                () => QuestionGenerator.GenerateTensUnitsWithCarry()
            };

            return challenge;
        }
    }
}
