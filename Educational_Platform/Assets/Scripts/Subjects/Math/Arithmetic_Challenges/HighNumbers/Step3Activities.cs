
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subjects.Math.Arithmetic_Challenges.HighNumbers
{
    public class Step3Activities : Step
    {
        public Step3Activities()
        {
            StepId = "Step3-LargeNumbers";

            AllActivities = new List<Activity>
        {
            new Activity
            {
                ActivityId = "Step3-DivisibleBy2YesNo-Easy",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Easy,
                Tags = new List<string> { "Divisibility", "RuleOf2", "EvenOdd" },
                QuestionGenerator = GenerateDivisibleBy2YesNoActivity
            },
            new Activity
            {
                ActivityId = "Step3-MatchEvenEndDigit-Easy",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Easy,
                Tags = new List<string> { "Divisibility", "RuleOf2", "EvenDigit" },
                QuestionGenerator = GenerateMatchEvenEndDigitActivity
            },
            new Activity
            {
                ActivityId = "Step3-ExplainDivisibleBy2-Medium",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Medium,
                Tags = new List<string> { "Divisibility", "Reasoning", "EvenOdd" },
                QuestionGenerator = GenerateExplainDivisibleBy2Activity
            },
            new Activity
            {
                ActivityId = "Step3-DivisibilityRulePuzzle-Medium",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Medium,
                Tags = new List<string> { "Divisibility", "RuleOf2", "RuleOf5", "Classification" },
                QuestionGenerator = GenerateDivisibilityRulePuzzleActivity
            },
            new Activity
            {
                ActivityId = "Step3-ExplainDivisibleBy5-Hard",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Hard,
                Tags = new List<string> { "Divisibility", "Reasoning", "RuleOf5" },
                QuestionGenerator = GenerateExplainDivisibleBy5Activity
            },
            new Activity
            {
                ActivityId = "Step3-SortByDivisibilityRules-Hard",
                ActivityType = "DragAndDrop",
                Modality = "Text",
                DifficultyLevel = Difficulty.Hard,
                Tags = new List<string> { "Divisibility", "Classification", "RuleOf2", "RuleOf5" },
                QuestionGenerator = GenerateSortByDivisibilityRulesActivity
            },
        };
        }

        // Activity 1: Is this number divisible by 2? Yes/No
        public Activity GenerateDivisibleBy2YesNoActivity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 1000);
            bool isDivisible = number % 2 == 0;

            return new Question
            {
                Prompt = $"Is the number {number} divisible by 2?",
                Options = new List<string> { "Yes", "No" },
                CorrectAnswer = isDivisible ? "Yes" : "No",
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "IsDivisible", isDivisible }
            }
            };
        }

        // Activity 2: Match rule - ends in even digit → divisible by 2
        public Activity GenerateMatchEvenEndDigitActivity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 1000);
            string lastDigit = number.ToString().Last().ToString();
            bool isEven = "02468".Contains(lastDigit);

            return new Question
            {
                Prompt = $"Does the number {number} end in an even digit?",
                Options = new List<string> { "Yes", "No" },
                CorrectAnswer = isEven ? "Yes" : "No",
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "LastDigit", lastDigit },
                { "IsEven", isEven }
            }
            };
        }


        // Activity 3: True/False - explain divisibility by 2
        public Activity GenerateExplainDivisibleBy2Activity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 1000);
            bool isDivisible = number % 2 == 0;

            string correctExplanation = isDivisible
                ? $"{number} ends in {number % 10}, which is even, so it's divisible by 2."
                : $"{number} ends in {number % 10}, which is odd, so it's not divisible by 2.";

            var distractors = new List<string>
        {
            $"{number} is even because it’s a large number.",
            $"{number} is divisible by 2 because it’s greater than 100.",
            $"{number} ends in {number % 10}, which is even, so it’s not divisible by 2."
        }.Where(e => e != correctExplanation).OrderBy(x => rng.Next()).Take(2).ToList();

            var options = distractors.Append(correctExplanation).OrderBy(x => rng.Next()).ToList();

            return new Question
            {
                Prompt = $"Why is {number} {(isDivisible ? "" : "not ")}divisible by 2?",
                Options = options,
                CorrectAnswer = correctExplanation,
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "IsDivisible", isDivisible },
                { "CorrectExplanation", correctExplanation }
            }
            };
        }


        // Activity 4: Divisibility puzzle - which rule(s) apply? (2, 5, both, or neither)
        public Activity GenerateDivisibilityRulePuzzleActivity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 1000);
            bool divisibleBy2 = number % 2 == 0;
            bool divisibleBy5 = number % 5 == 0;

            string correctCategory = divisibleBy2 && divisibleBy5
                ? "Divisible by 2 and 5"
                : divisibleBy2 ? "Divisible by 2"
                : divisibleBy5 ? "Divisible by 5"
                : "Divisible by neither";

            List<string> categories = new List<string>
        {
            "Divisible by 2",
            "Divisible by 5",
            "Divisible by 2 and 5",
            "Divisible by neither"
        };

            return new Question
            {
                Prompt = $"Which rule(s) apply to the number {number}?",
                Options = categories,
                CorrectAnswer = correctCategory,
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "DivisibleBy2", divisibleBy2 },
                { "DivisibleBy5", divisibleBy5 }
            }
            };
        }


        // Activity 5: Explain choice - why is number divisible by 5?
        public Activity GenerateExplainDivisibleBy5Activity()
        {
            Random rng = new Random();
            int baseMultiple = rng.Next(20, 200) * 5;
            string lastDigit = baseMultiple.ToString().Last().ToString();

            string correctExplanation = baseMultiple % 10 == 0
                ? $"{baseMultiple} ends in 0, so it's divisible by 10 and 5."
                : $"{baseMultiple} ends in 5, so it's divisible by 5.";

            var distractors = new List<string>
        {
            $"{baseMultiple} is divisible by 5 because it’s an even number.",
            $"{baseMultiple} is divisible by 5 because it’s greater than 100.",
            $"{baseMultiple} ends in {lastDigit}, so it must be a multiple of 2."
        };

            var options = distractors.Append(correctExplanation).OrderBy(x => rng.Next()).ToList();

            return new Question
            {
                Prompt = $"Why is {baseMultiple} divisible by 5?",
                Options = options,
                CorrectAnswer = correctExplanation,
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", baseMultiple },
                { "CorrectExplanation", correctExplanation }
            }
            };
        }


        // Activity 6: Sort by rule - 2-only, 5-only, both, neither
        public Activity GenerateSortByDivisibilityRulesActivity()
        {
            Random rng = new Random();
            List<int> numbers = new List<int>();
            while (numbers.Count < 6)
            {
                int num = rng.Next(100, 999);
                if (!numbers.Contains(num))
                    numbers.Add(num);
            }

            Dictionary<string, List<int>> categories = new Dictionary<string, List<int>>
        {
            { "Divisible by 2 only", new List<int>() },
            { "Divisible by 5 only", new List<int>() },
            { "Divisible by both", new List<int>() },
            { "Divisible by neither", new List<int>() }
        };

            foreach (int num in numbers)
            {
                bool by2 = num % 2 == 0;
                bool by5 = num % 5 == 0;

                if (by2 && by5)
                    categories["Divisible by both"].Add(num);
                else if (by2)
                    categories["Divisible by 2 only"].Add(num);
                else if (by5)
                    categories["Divisible by 5 only"].Add(num);
                else
                    categories["Divisible by neither"].Add(num);
            }

            Dictionary<string, string> matchPairs = new Dictionary<string, string>();
            foreach (var kvp in categories)
            {
                foreach (var n in kvp.Value)
                    matchPairs[n.ToString()] = kvp.Key;
            }

            return new Question
            {
                Prompt = "Sort each number by its divisibility rule.",
                MatchPairs = matchPairs,
                Metadata = new Dictionary<string, object>
            {
                { "Categories", categories }
            }
            };
        }

    }
}