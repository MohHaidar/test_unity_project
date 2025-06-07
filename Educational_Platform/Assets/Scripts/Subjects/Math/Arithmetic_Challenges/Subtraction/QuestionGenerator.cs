using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Subjects.Math.Arithmetic_Challenges.Subtraction {
    public static class QuestionGenerator {
        private static List<string> GenerateOptions(int correct) {
            return new List<string> {
                correct.ToString(),
                (correct + Random.Range(1, 5)).ToString(),
                (correct - Random.Range(1, 5)).ToString()
            }.OrderBy(_ => Random.value).ToList();
        }

        public static QuestionModule GenerateBasicSubtraction() {
            int a = Random.Range(10, 100);
            int b = Random.Range(1, a);
            int result = a - b;

            return new QuestionModule {
                Type = "sub_basic",
                Prompt = $"What is {a} - {b}?",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }

        public static QuestionModule GenerateWithZero() {
            int a = Random.Range(100, 1000);
            int b = a % 10; // subtract units place
            int result = a - b;

            return new QuestionModule {
                Type = "sub_zeroend",
                Prompt = $"Subtract: {a} - {b}",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }

        public static QuestionModule GenerateUltimateChallenge() {
            int a = Random.Range(500, 999);
            int b = Random.Range(100, 500);
            int result = a - b;

            return new QuestionModule {
                Type = "sub_ultimate",
                Prompt = $"Final challenge! What is {a} - {b}?",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }
    }
}