using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Subjects.Math.Arithmetic_Challenges.BigNumberAddition {
    public static class QuestionGenerator {
        private static List<string> GenerateAnswerOptions(string correct, int count) {
            int correctInt = int.Parse(correct);
            HashSet<string> options = new() { correct };
            while (options.Count < count + 1) {
                int distractor = correctInt + Random.Range(-20, 20);
                if (distractor != correctInt && distractor > 0)
                    options.Add(distractor.ToString());
            }
            return options.OrderBy(_ => Random.value).ToList();
        }

        public static QuestionModule GenerateSingleDigitNoCarry() {
            int a = Random.Range(1, 5), b = Random.Range(1, 5);
            string correct = (a + b).ToString();
            return new QuestionModule {
                Type = "1d_nocarry",
                Prompt = $"What is {a} + {b}?",
                Options = GenerateAnswerOptions(correct, 2),
                CorrectAnswer = correct
            };
        }

        public static QuestionModule GenerateSingleDigitWithCarry() {
            int a = Random.Range(6, 9), b = Random.Range(6, 9);
            string correct = (a + b).ToString();
            return new QuestionModule {
                Type = "1d_carry",
                Prompt = $"What is {a} + {b}? (You'll need to carry)",
                Options = GenerateAnswerOptions(correct, 2),
                CorrectAnswer = correct
            };
        }

        public static QuestionModule GenerateTensNoCarry() {
            int a = Random.Range(1, 9) * 10, b = Random.Range(1, 9) * 10;
            string correct = (a + b).ToString();
            return new QuestionModule {
                Type = "tens_nocarry",
                Prompt = $"Add the tens: {a} + {b}",
                Options = GenerateAnswerOptions(correct, 2),
                CorrectAnswer = correct
            };
        }

        public static QuestionModule GenerateTensUnitsWithCarry() {
            int a = Random.Range(11, 99), b = Random.Range(11, 99);
            string correct = (a + b).ToString();
            return new QuestionModule {
                Type = "2d_withcarry",
                Prompt = $"Add: {a} + {b} (Carry from units to tens)",
                Options = GenerateAnswerOptions(correct, 2),
                CorrectAnswer = correct
            };
        }

        public static QuestionModule GenerateUltimateAdditionChallenge() {
            int a = Random.Range(100, 999), b = Random.Range(100, 999);
            string correct = (a + b).ToString();
            return new QuestionModule {
                Type = "ultimate_addition",
                Prompt = $"What is:\n  {a}\n+ {b}\n= ?",
                Options = GenerateAnswerOptions(correct, 2),
                CorrectAnswer = correct
            };
        }
    }
}
