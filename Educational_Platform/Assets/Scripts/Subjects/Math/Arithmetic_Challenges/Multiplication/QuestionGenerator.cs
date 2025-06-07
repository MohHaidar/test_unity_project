using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Subjects.Math.Arithmetic_Challenges.Multiplication {
    public static class QuestionGenerator {
        private static List<string> GenerateOptions(int correct) {
            return new List<string> {
                correct.ToString(),
                (correct + Random.Range(1, 5)).ToString(),
                (correct - Random.Range(1, 5)).ToString()
            }.OrderBy(_ => Random.value).ToList();
        }

        public static QuestionModule GenerateSingleDigitMultiplication() {
            int a = Random.Range(2, 10);
            int b = Random.Range(2, 10);
            int result = a * b;

            return new QuestionModule {
                Type = "mul_basic",
                Prompt = $"What is {a} × {b}?",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }

        public static QuestionModule GenerateTensMultiplication() {
            int a = Random.Range(10, 100);
            int b = 10;
            int result = a * b;

            return new QuestionModule {
                Type = "mul_tens",
                Prompt = $"What is {a} × 10?",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }

        public static QuestionModule GenerateUltimateChallenge() {
            int a = Random.Range(10, 20);
            int b = Random.Range(10, 20);
            int result = a * b;

            return new QuestionModule {
                Type = "mul_ultimate",
                Prompt = $"Final challenge! What is {a} × {b}?",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }
    }
}
