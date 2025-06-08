
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subjects.Math.Arithmetic_Challenges.HighNumbers
{
    public class Step2Activities : Step
    {
        public Step2Activities()
        {
            StepId = "Step2-LargeNumbers";

            AllActivities = new List<Activity>
        {
            new Activity
            {
                ActivityId = "Step2-EndDigit-Easy",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Easy,
                Tags = new List<string> { "EndDigit", "DivisibilityPatterns", "Multiples" },
                QuestionGenerator = GenerateEndDigitRecognitionActivity
            },
            new Activity
            {
                ActivityId = "Step2-DivisibleBy5-Easy",
                ActivityType = "MultiSelect",
                Modality = "Text",
                DifficultyLevel = Difficulty.Easy,
                Tags = new List<string> { "Divisibility", "Multiples", "RuleOf5" },
                QuestionGenerator = GenerateDivisibleBy5SelectionActivity
            },
            new Activity
            {
                ActivityId = "Step2-MatchMultiplesGroup-Medium",
                ActivityType = "DragAndDrop",
                Modality = "Visual",
                DifficultyLevel = Difficulty.Medium,
                Tags = new List<string> { "Multiples", "Classification", "Divisibility" },
                QuestionGenerator = GenerateMultipleGroupMatchActivity
            },
            new Activity
            {
                ActivityId = "Step2-DivisibleBy5And10-Medium",
                ActivityType = "MultiSelect",
                Modality = "Text",
                DifficultyLevel = Difficulty.Medium,
                Tags = new List<string> { "Divisibility", "RuleOf5", "RuleOf10", "MultipleClassification" },
                QuestionGenerator = GenerateDivisibleBy5And10Activity
            },
            new Activity
            {
                ActivityId = "Step2-EstimateNearMultipleOf10-Hard",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Hard,
                Tags = new List<string> { "Estimation", "Multiples", "Rounding" },
                QuestionGenerator = GenerateEstimateNearMultipleOf10Activity
            },
            new Activity
            {
                ActivityId = "Step2-CategorizeMultiples-Hard",
                ActivityType = "DragAndDrop",
                Modality = "Text",
                DifficultyLevel = Difficulty.Hard,
                Tags = new List<string> { "Multiples", "Classification", "DivisibilityRules" },
                QuestionGenerator = GenerateCategorizeMultiplesActivity
            },
        };
        }

        // Activity 1: What does this number end with?
        public Activity GenerateEndDigitRecognitionActivity()
        {
            Random rng = new Random();
            int number = rng.Next(1000, 99999);
            string numberStr = number.ToString();
            string lastDigit = numberStr[^1].ToString();

            // Generate distractors
            List<string> digits = new List<string> { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            digits.Remove(lastDigit);
            var distractors = digits.OrderBy(x => rng.Next()).Take(3).ToList();

            var options = distractors.Append(lastDigit).OrderBy(x => rng.Next()).ToList();

            return new Question
            {
                Prompt = $"What digit does the number {number} end with?",
                Options = options,
                CorrectAnswer = lastDigit,
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "LastDigit", lastDigit }
            }
            };
        }

        // Activity 2: Which of these are divisible by 5?
        public Activity GenerateDivisibleBy5SelectionActivity()
        {
            Random rng = new Random();
            List<int> options = new List<int>();
            HashSet<int> correctAnswers = new HashSet<int>();

            while (options.Count < 4)
            {
                int num = rng.Next(100, 999);
                if (options.Contains(num)) continue;

                options.Add(num);
                if (num % 5 == 0)
                    correctAnswers.Add(num);
            }

            return new Question
            {
                Prompt = "Which of these numbers are divisible by 5?",
                Options = options.Select(n => n.ToString()).ToList(),
                CorrectAnswer = string.Join(", ", correctAnswers.Select(n => n.ToString())),
                Metadata = new Dictionary<string, object>
            {
                { "Options", options },
                { "CorrectSubset", correctAnswers.ToList() }
            }
            };
        }


        // Activity 3: Drag-and-drop - match number to multiple group (2, 5, 10)
        public Activity GenerateMultipleGroupMatchActivity()
        {
            Random rng = new Random();
            Dictionary<string, List<int>> groups = new Dictionary<string, List<int>>
        {
            { "Multiples of 2", new List<int>() },
            { "Multiples of 5", new List<int>() },
            { "Multiples of 10", new List<int>() }
        };

            HashSet<int> used = new HashSet<int>();

            while (groups["Multiples of 2"].Count < 2)
            {
                int num = rng.Next(100, 500);
                if (num % 2 == 0 && used.Add(num))
                    groups["Multiples of 2"].Add(num);
            }

            while (groups["Multiples of 5"].Count < 2)
            {
                int num = rng.Next(100, 500);
                if (num % 5 == 0 && used.Add(num))
                    groups["Multiples of 5"].Add(num);
            }

            while (groups["Multiples of 10"].Count < 2)
            {
                int num = rng.Next(100, 500);
                if (num % 10 == 0 && used.Add(num))
                    groups["Multiples of 10"].Add(num);
            }

            Dictionary<string, string> matchPairs = new Dictionary<string, string>();
            foreach (var group in groups)
            {
                foreach (int n in group.Value)
                    matchPairs.Add(n.ToString(), group.Key);
            }

            return new Question
            {
                Prompt = "Drag each number to its correct multiple group.",
                MatchPairs = matchPairs,
                Metadata = new Dictionary<string, object>
            {
                { "Groups", groups }
            }
            };
        }


        // Activity 4: Pick all divisible by both 5 and 10
        public Activity GenerateDivisibleBy5And10Activity()
        {
            Random rng = new Random();
            List<int> options = new List<int>();
            HashSet<int> correctAnswers = new HashSet<int>();

            while (options.Count < 4)
            {
                int num = rng.Next(100, 1000);
                if (options.Contains(num)) continue;

                options.Add(num);
                if (num % 5 == 0 && num % 10 == 0)
                    correctAnswers.Add(num);
            }

            return new Question
            {
                Prompt = "Select all numbers that are divisible by BOTH 5 and 10.",
                Options = options.Select(n => n.ToString()).ToList(),
                CorrectAnswer = string.Join(", ", correctAnswers.Select(n => n.ToString())),
                Metadata = new Dictionary<string, object>
            {
                { "Options", options },
                { "CorrectSubset", correctAnswers.ToList() }
            }
            };
        }


        // Activity 5: Estimate which number is near a multiple of 10
        public Activity GenerateEstimateNearMultipleOf10Activity()
        {
            Random rng = new Random();

            int baseMultiple = rng.Next(10, 100) * 10; // e.g., 400, 560
            int near1 = baseMultiple + rng.Next(1, 5);
            int near2 = baseMultiple - rng.Next(1, 5);
            int far = baseMultiple + rng.Next(6, 15);

            List<int> options = new List<int> { near1, near2, far };
            options = options.OrderBy(x => rng.Next()).ToList();

            return new Question
            {
                Prompt = "Which number is closest to a multiple of 10?",
                Options = options.Select(n => n.ToString()).ToList(),
                CorrectAnswer = (Math.Abs(near1 - baseMultiple) < Math.Abs(near2 - baseMultiple)) ? near1.ToString() : near2.ToString(),
                Metadata = new Dictionary<string, object>
            {
                { "BaseMultiple", baseMultiple },
                { "Options", options }
            }
            };
        }


        // Activity 6: Categorize numbers as multiples of 2, 5, 10, or none
        public Activity GenerateCategorizeMultiplesActivity()
        {
            Random rng = new Random();
            Dictionary<string, List<int>> categories = new Dictionary<string, List<int>>
        {
            { "Multiple of 2", new List<int>() },
            { "Multiple of 5", new List<int>() },
            { "Multiple of 10", new List<int>() },
            { "None", new List<int>() }
        };

            HashSet<int> used = new HashSet<int>();

            while (categories["Multiple of 2"].Count < 2)
            {
                int num = rng.Next(100, 999);
                if (num % 2 == 0 && num % 10 != 0 && used.Add(num))
                    categories["Multiple of 2"].Add(num);
            }

            while (categories["Multiple of 5"].Count < 2)
            {
                int num = rng.Next(100, 999);
                if (num % 5 == 0 && num % 10 != 0 && num % 2 != 0 && used.Add(num))
                    categories["Multiple of 5"].Add(num);
            }

            while (categories["Multiple of 10"].Count < 2)
            {
                int num = rng.Next(100, 999);
                if (num % 10 == 0 && used.Add(num))
                    categories["Multiple of 10"].Add(num);
            }

            while (categories["None"].Count < 2)
            {
                int num = rng.Next(101, 999);
                if (num % 2 != 0 && num % 5 != 0 && used.Add(num))
                    categories["None"].Add(num);
            }

            Dictionary<string, string> matchPairs = new Dictionary<string, string>();
            foreach (var kvp in categories)
            {
                foreach (var n in kvp.Value)
                    matchPairs[n.ToString()] = kvp.Key;
            }

            return new Question
            {
                Prompt = "Categorize each number according to its divisibility.",
                MatchPairs = matchPairs,
                Metadata = new Dictionary<string, object>
            {
                { "Categories", categories }
            }
            };
        }

    }
}