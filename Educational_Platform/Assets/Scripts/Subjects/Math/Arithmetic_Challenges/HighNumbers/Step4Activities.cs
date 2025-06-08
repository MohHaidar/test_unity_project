
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subjects.Math.Arithmetic_Challenges.HighNumbers
{
    public class Step4Activities : Step
    {
        public Step4Activities()
        {
            StepId = "Step1-LargeNumbers";

            Generators = new Dictionary<Difficulty, List<Func<Activity>>>
        {
            {
                Difficulty.Easy, new List<Func<Activity>>
                {
                    GenerateDigitSumActivity,
                    GenerateDigitSumDivisibleBy3Activity
                }
            },
            {
                Difficulty.Medium, new List<Func<Activity>>
                {
                    GenerateSelectDivisibleBy3Activity,
                    GenerateRuleApplicationComparisonActivity
                }
            },
            {
                Difficulty.Hard, new List<Func<Activity>>
                {
                    GenerateExplainDivisibleBy3Activity,
                    GenerateCreateDivisibleBy3Activity

                }
            }
        };
        }
        // Activity 1: What’s the digit sum of a number?
        public Activity GenerateDigitSumActivity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 999);
            int digitSum = number.ToString().Sum(c => int.Parse(c.ToString()));

            HashSet<int> options = new HashSet<int> { digitSum };
            while (options.Count < 4)
            {
                int fake = digitSum + rng.Next(-4, 5);
                if (fake > 0) options.Add(fake);
            }

            return new Activity
            {
                ActivityId = "Step4-DigitSum-Easy",
                Prompt = $"What is the digit sum of {number}?",
                Options = options.OrderBy(x => rng.Next()).Select(n => n.ToString()).ToList(),
                CorrectAnswer = digitSum.ToString(),
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = 0,
                Tags = new List<string> { "DigitSum", "DivisibilityBy3", "MentalMath" },
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "DigitSum", digitSum }
            }
            };
        }

        // Activity 2: Does the digit sum divide by 3?
        public Activity GenerateDigitSumDivisibleBy3Activity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 999);
            int digitSum = number.ToString().Sum(c => int.Parse(c.ToString()));
            bool isDivisible = digitSum % 3 == 0;

            return new Activity
            {
                ActivityId = "Step4-DigitSumDivisibleBy3-Easy",
                Prompt = $"Is the digit sum of {number} divisible by 3?",
                Options = new List<string> { "Yes", "No" },
                CorrectAnswer = isDivisible ? "Yes" : "No",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = 0,
                Tags = new List<string> { "DivisibilityBy3", "DigitSum", "RuleCheck" },
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "DigitSum", digitSum },
                { "IsDivisible", isDivisible }
            }
            };
        }


        // Activity 3: Choose all numbers divisible by 3 from a set
        public Activity GenerateSelectDivisibleBy3Activity()
        {
            Random rng = new Random();
            List<int> options = new List<int>();
            HashSet<int> correctAnswers = new HashSet<int>();

            while (options.Count < 5)
            {
                int number = rng.Next(100, 999);
                if (options.Contains(number)) continue;

                options.Add(number);
                int digitSum = number.ToString().Sum(c => int.Parse(c.ToString()));
                if (digitSum % 3 == 0)
                    correctAnswers.Add(number);
            }

            return new Activity
            {
                ActivityId = "Step4-SelectDivisibleBy3-Medium",
                Prompt = "Select all numbers that are divisible by 3.",
                Options = options.Select(n => n.ToString()).ToList(),
                CorrectAnswer = string.Join(", ", correctAnswers.Select(n => n.ToString())),
                ActivityType = "MultiSelect",
                Modality = "Text",
                DifficultyLevel = 1,
                Tags = new List<string> { "DivisibilityBy3", "DigitSum", "FilterSet" },
                Metadata = new Dictionary<string, object>
            {
                { "Options", options },
                { "CorrectSubset", correctAnswers.ToList() }
            }
            };
        }


        // Activity 4: Which rule applies: digit sum or ending digit?
        public Activity GenerateRuleApplicationComparisonActivity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 999);

            string correctRule = (number % 3 == 0) ? "Digit sum rule (divisible by 3)" : "Neither";
            if (number % 2 == 0 || number % 5 == 0)
            {
                correctRule = "Ending digit rule (divisible by 2 or 5)";
            }

            List<string> options = new List<string>
        {
            "Digit sum rule (divisible by 3)",
            "Ending digit rule (divisible by 2 or 5)",
            "Neither"
        };

            return new Activity
            {
                ActivityId = "Step4-RuleComparison-Medium",
                Prompt = $"Which rule applies to determine divisibility for {number}?",
                Options = options,
                CorrectAnswer = correctRule,
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = 1,
                Tags = new List<string> { "Divisibility", "RuleRecognition", "CompareRules" },
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "CorrectRule", correctRule }
            }
            };
        }


        // Activity 5: Explain - Why is the number divisible by 3?
        public Activity GenerateExplainDivisibleBy3Activity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 999);
            int digitSum = number.ToString().Sum(c => int.Parse(c.ToString()));
            bool isDivisible = digitSum % 3 == 0;

            string correctExplanation = isDivisible
                ? $"The digit sum of {number} is {digitSum}, which is divisible by 3."
                : $"The digit sum of {number} is {digitSum}, which is not divisible by 3.";

            var distractors = new List<string>
        {
            $"{number} is divisible by 3 because it ends in 3.",
            $"{number} is divisible by 3 because it's greater than 300.",
            $"{number} has an odd number of digits, so it's divisible by 3."
        }.OrderBy(x => rng.Next()).Take(2).ToList();

            var options = distractors.Append(correctExplanation).OrderBy(x => rng.Next()).ToList();

            return new Activity
            {
                ActivityId = "Step4-ExplainDivisibleBy3-Hard",
                Prompt = $"Why is {number} {(isDivisible ? "" : "not ")}divisible by 3?",
                Options = options,
                CorrectAnswer = correctExplanation,
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = 2,
                Tags = new List<string> { "DivisibilityBy3", "Reasoning", "DigitSum" },
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "DigitSum", digitSum },
                { "IsDivisible", isDivisible },
                { "CorrectExplanation", correctExplanation }
            }
            };
        }


        // Activity 6: Create a number divisible by 3 from given digits
        public Activity GenerateCreateDivisibleBy3Activity()
        {
            Random rng = new Random();
            List<int> digits = Enumerable.Range(1, 9).OrderBy(x => rng.Next()).Take(5).ToList();

            List<string> possibleNumbers = new List<string>();

            foreach (var permutation in GetPermutations(digits, 3))
            {
                string numStr = string.Join("", permutation);
                int num = int.Parse(numStr);
                int digitSum = numStr.Sum(c => int.Parse(c.ToString()));
                if (digitSum % 3 == 0)
                    possibleNumbers.Add(numStr);
            }

            string correct = possibleNumbers.OrderBy(x => rng.Next()).FirstOrDefault() ?? "123";

            return new Activity
            {
                ActivityId = "Step4-CreateDivisibleBy3-Hard",
                Prompt = $"Using these digits: {string.Join(", ", digits)}, form a 3-digit number divisible by 3.",
                CorrectAnswer = correct,
                Options = null,
                ActivityType = "ConstructFromParts",
                Modality = "Text",
                DifficultyLevel = 2,
                Tags = new List<string> { "DigitSum", "NumberConstruction", "DivisibilityBy3" },
                Metadata = new Dictionary<string, object>
            {
                { "AvailableDigits", digits },
                { "CorrectNumber", correct }
            }
            };
        }

        private static IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1)
                .SelectMany(t => list.Where(o => !t.Contains(o)),
                            (t1, t2) => t1.Concat(new T[] { t2 }));
        }

    }
}