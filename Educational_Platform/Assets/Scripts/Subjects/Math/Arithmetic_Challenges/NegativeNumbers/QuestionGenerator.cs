using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Subjects.Math.Arithmetic_Challenges.NegativeNumbers {
    public static class QuestionGenerator {
        private static List<string> GenerateOptions(int correct) {
            return new List<string> {
                correct.ToString(),
                (correct + Random.Range(1, 5)).ToString(),
                (correct - Random.Range(1, 5)).ToString()
            }.OrderBy(_ => Random.value).ToList();
        }

        public static QuestionModule GenerateNegativeSubtraction() {
            int a = Random.Range(-10, 0);
            int b = Random.Range(1, 10);
            int result = a - b;

            return new QuestionModule {
                Type = "neg_sub",
                Prompt = $"What is {a} - {b}?",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }

        public static QuestionModule GenerateNegativeAddition() {
            int a = Random.Range(-10, 0);
            int b = Random.Range(1, 10);
            int result = a + b;

            return new QuestionModule {
                Type = "neg_add",
                Prompt = $"What is {a} + {b}?",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }

        public static QuestionModule GenerateNegativeComparison() {
            int a = Random.Range(-10, 0);
            int b = Random.Range(-10, 0);
            string correct = a > b ? $"{a}" : $"{b}";

            return new QuestionModule {
                Type = "neg_compare",
                Prompt = $"Which is greater: {a} or {b}?",
                Options = new List<string> { $"{a}", $"{b}", "0" }.OrderBy(_ => Random.value).ToList(),
                CorrectAnswer = correct
            };
        }

        public static QuestionModule GenerateUltimateChallenge() {
            int a = Random.Range(-20, -5);
            int b = Random.Range(-10, 5);
            int result = a + b;

            return new QuestionModule {
                Type = "neg_ultimate",
                Prompt = $"Final challenge! What is {a} + {b}?",
                Options = GenerateOptions(result),
                CorrectAnswer = result.ToString()
            };
        }
    }
}
