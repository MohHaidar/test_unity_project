
using System;
using System.Collections.Generic;
using System.Linq;

namespace Subjects.Math.Arithmetic_Challenges.HighNumbers
{
    public class Step5Activities : Step
    {
        public Step5Activities()
        {
            StepId = "Step1-LargeNumbers";

            Generators = new Dictionary<Difficulty, List<Func<Activity>>>
        {
            {
                Difficulty.Easy, new List<Func<Activity>>
                {
                    GenerateCheckDivisibilityRulesActivity,
                    GenerateFailedDivisibilityRuleActivity
                }
            },
            {
                Difficulty.Medium, new List<Func<Activity>>
                {
                    GenerateWordProblemDivisibilityCountActivity,
                    GenerateSortByDivisibilityProfileActivity
                }
            },
            {
                Difficulty.Hard, new List<Func<Activity>>
                {
                    GenerateConstructDivisibleBy235Activity,
                    GenerateComboValidatorActivity

                }
            }
        };
        }
        // Activity 1: Check all that apply – divisibility by 2, 3, 5, 10
        public Activity GenerateCheckDivisibilityRulesActivity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 1000);

            Dictionary<string, bool> ruleCheck = new Dictionary<string, bool>
        {
            { "Divisible by 2", number % 2 == 0 },
            { "Divisible by 3", number.ToString().Sum(c => int.Parse(c.ToString())) % 3 == 0 },
            { "Divisible by 5", number % 5 == 0 },
            { "Divisible by 10", number % 10 == 0 }
        };

            List<string> correctRules = ruleCheck.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();

            return new Activity
            {
                ActivityId = "Step5-CheckDivisibility-Easy",
                Prompt = $"Which of the following are true for {number}?",
                Options = ruleCheck.Keys.ToList(),
                CorrectAnswer = string.Join(", ", correctRules),
                ActivityType = "MultiSelect",
                Modality = "Text",
                DifficultyLevel = 0,
                Tags = new List<string> { "MultiRule", "DivisibilityCheck", "RuleOf2", "RuleOf3", "RuleOf5", "RuleOf10" },
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "CorrectRules", correctRules }
            }
            };
        }

        // Activity 2: Multi-choice - which rule does it fail?
        public Activity GenerateFailedDivisibilityRuleActivity()
        {
            Random rng = new Random();
            int number;
            Dictionary<string, bool> ruleCheck;
            List<string> allRules = new List<string> { "Divisible by 2", "Divisible by 3", "Divisible by 5", "Divisible by 10" };
            List<string> failedRules;

            do
            {
                number = rng.Next(100, 999);
                ruleCheck = new Dictionary<string, bool>
            {
                { "Divisible by 2", number % 2 == 0 },
                { "Divisible by 3", number.ToString().Sum(c => int.Parse(c.ToString())) % 3 == 0 },
                { "Divisible by 5", number % 5 == 0 },
                { "Divisible by 10", number % 10 == 0 }
            };
                failedRules = ruleCheck.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();
            }
            while (failedRules.Count == 0); // ensure at least one failure

            string correct = failedRules[rng.Next(failedRules.Count)];

            return new Activity
            {
                ActivityId = "Step5-FailedDivisibilityRule-Easy",
                Prompt = $"Which divisibility rule does {number} fail?",
                Options = allRules,
                CorrectAnswer = correct,
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = 0,
                Tags = new List<string> { "Divisibility", "MultiRule", "FailureAnalysis" },
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "FailedRule", correct },
                { "RuleResults", ruleCheck }
            }
            };
        }


        // Activity 3: Word problem - how many are divisible by 2 or 5?
        public Activity GenerateWordProblemDivisibilityCountActivity()
        {
            Random rng = new Random();
            List<int> numbers = new List<int>();
            while (numbers.Count < 5)
            {
                int num = rng.Next(100, 999);
                if (!numbers.Contains(num)) numbers.Add(num);
            }

            int count = numbers.Count(n => n % 2 == 0 || n % 5 == 0);

            return new Activity
            {
                ActivityId = "Step5-CountDivisibleBy2or5-Medium",
                Prompt = $"In the list: {string.Join(", ", numbers)}, how many numbers are divisible by 2 or 5?",
                Options = Enumerable.Range(0, 6).Select(n => n.ToString()).ToList(),
                CorrectAnswer = count.ToString(),
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = 1,
                Tags = new List<string> { "Divisibility", "WordProblem", "CountMultiples" },
                Metadata = new Dictionary<string, object>
            {
                { "Numbers", numbers },
                { "DivisibleCount", count }
            }
            };
        }


        // Activity 4: Sort 6-digit numbers by divisibility profile
        public Activity GenerateSortByDivisibilityProfileActivity()
        {
            Random rng = new Random();
            List<int> numbers = new List<int>();
            while (numbers.Count < 6)
            {
                int num = rng.Next(100000, 999999);
                if (!numbers.Contains(num)) numbers.Add(num);
            }

            Dictionary<string, List<int>> profiles = new Dictionary<string, List<int>>
        {
            { "Divisible by 2", new List<int>() },
            { "Divisible by 3", new List<int>() },
            { "Divisible by 5", new List<int>() },
            { "Divisible by 2 and 5", new List<int>() },
            { "Divisible by 3 and 5", new List<int>() },
            { "Divisible by none", new List<int>() }
        };

            foreach (int num in numbers)
            {
                bool by2 = num % 2 == 0;
                bool by3 = num.ToString().Sum(c => int.Parse(c.ToString())) % 3 == 0;
                bool by5 = num % 5 == 0;

                if (by2 && by5)
                    profiles["Divisible by 2 and 5"].Add(num);
                else if (by3 && by5)
                    profiles["Divisible by 3 and 5"].Add(num);
                else if (by2)
                    profiles["Divisible by 2"].Add(num);
                else if (by3)
                    profiles["Divisible by 3"].Add(num);
                else if (by5)
                    profiles["Divisible by 5"].Add(num);
                else
                    profiles["Divisible by none"].Add(num);
            }

            Dictionary<string, string> matchPairs = new Dictionary<string, string>();
            foreach (var kvp in profiles)
            {
                foreach (int n in kvp.Value)
                    matchPairs[n.ToString()] = kvp.Key;
            }

            return new Activity
            {
                ActivityId = "Step5-SortDivisibilityProfiles-Medium",
                Prompt = "Sort each number by its divisibility profile.",
                MatchPairs = matchPairs,
                ActivityType = "DragAndDrop",
                Modality = "Text",
                DifficultyLevel = 1,
                Tags = new List<string> { "Divisibility", "Sorting", "MultiRule" },
                Metadata = new Dictionary<string, object>
            {
                { "Profiles", profiles }
            }
            };
        }


        // Activity 5: Construct a number divisible by 2, 3, and 5
        public Activity GenerateConstructDivisibleBy235Activity()
        {
            Random rng = new Random();
            // LCM of 2, 3, 5 is 30 — ensure divisibility by all
            int baseNum = rng.Next(4, 34); // 4*30 = 120, 33*30 = 990
            int correctNumber = baseNum * 30;

            List<int> distractors = new List<int>();
            while (distractors.Count < 3)
            {
                int num = rng.Next(100, 999);
                if (num != correctNumber && (num % 2 != 0 || num % 3 != 0 || num % 5 != 0))
                    distractors.Add(num);
            }

            var options = distractors.Append(correctNumber).OrderBy(x => rng.Next()).ToList();

            return new Activity
            {
                ActivityId = "Step5-ConstructDivisibleBy235-Hard",
                Prompt = "Which of these numbers is divisible by 2, 3, and 5?",
                Options = options.Select(n => n.ToString()).ToList(),
                CorrectAnswer = correctNumber.ToString(),
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = 2,
                Tags = new List<string> { "Divisibility", "ConstructNumber", "LCM" },
                Metadata = new Dictionary<string, object>
            {
                { "CorrectNumber", correctNumber },
                { "Options", options }
            }
            };
        }


        // Activity 6: Combo validator - apply multiple divisibility rules to a single number
        public Activity GenerateComboValidatorActivity()
        {
            Random rng = new Random();
            int number = rng.Next(100, 999);

            bool by2 = number % 2 == 0;
            bool by3 = number.ToString().Sum(c => int.Parse(c.ToString())) % 3 == 0;
            bool by5 = number % 5 == 0;
            bool by10 = number % 10 == 0;

            var responses = new Dictionary<string, bool>
        {
            { "Divisible by 2", by2 },
            { "Divisible by 3", by3 },
            { "Divisible by 5", by5 },
            { "Divisible by 10", by10 }
        };

            return new Activity
            {
                ActivityId = "Step5-ComboValidator-Hard",
                Prompt = $"Apply all relevant divisibility rules to {number}. Mark each as true or false.",
                MatchPairs = responses.ToDictionary(kvp => kvp.Key, kvp => kvp.Value ? "True" : "False"),
                ActivityType = "TrueFalseMatrix",
                Modality = "Text",
                DifficultyLevel = 2,
                Tags = new List<string> { "Divisibility", "MultiRule", "Validator" },
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "ExpectedResponses", responses }
            }
            };
        }

    }
}