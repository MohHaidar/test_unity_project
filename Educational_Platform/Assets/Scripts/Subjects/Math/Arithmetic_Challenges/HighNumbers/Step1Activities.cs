using System;
using System.Collections.Generic;
using System.Linq;
namespace Subjects.Math.Arithmetic_Challenges.HighNumbers
{
    public class Step1Activities : Step
    {
        public Step1Activities()
        {
            StepId = "Step1-LargeNumbers";

            AllActivities = new List<Activity>
        {
            new Activity
            {
                ActivityId = "Step1-GreaterThan100K-Easy",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Easy,
                Tags = new List<string> { "PlaceValue", "NumberComparison", "NaturalNumbers" },
                QuestionGenerator = GenerateSelectGreaterThan100KQuestion
            },
            new Activity
            {
                ActivityId = "Step1-ArrangeSixDigitNumbers-Easy",
                ActivityType = "SortOrder",
                Modality = "Text",
                DifficultyLevel = Difficulty.Easy,
                Tags = new List<string> { "NumberOrdering", "PlaceValue", "NaturalNumbers" },
                QuestionGenerator = GenerateArrangeSixDigitNumbersActivity
            },
            new Activity
            {
                ActivityId = "Step1-HighlightPlaceValue-Medium",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Medium,
                Tags = new List<string> { "PlaceValue", "DigitIdentification", "NaturalNumbers" },
                QuestionGenerator = GenerateHighlightPlaceValueActivity
            },
            new Activity
            {
                ActivityId = "Step1-EstimateMagnitude-Medium",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Medium,
                Tags = new List<string> { "Estimation", "PlaceValue", "Rounding" },
                QuestionGenerator = GenerateEstimateMagnitudeActivity
            },
            new Activity
            {
                ActivityId = "Step1-CompareSpecificDigit-Hard",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Hard,
                Tags = new List<string> { "DigitComparison", "PlaceValue", "NaturalNumbers" },
                QuestionGenerator = GenerateCompareSpecificDigitActivity
            },
            new Activity
            {
                ActivityId = "Step1-SpotFakeSixDigitNumber-Hard",
                ActivityType = "MultipleChoice",
                Modality = "Text",
                DifficultyLevel = Difficulty.Hard,
                Tags = new List<string> { "PlaceValue", "NumberLength", "NaturalNumbers" },
                QuestionGenerator = GenerateSpotFakeSixDigitActivity
            },
        };
        }

        // Activity 1: Select the number greater than 100,000
        public Question GenerateSelectGreaterThan100KQuestion()
        {
            var rng = new Random();
            int correct = rng.Next(100001, 999999);
            var distractors = new HashSet<int>();
            while (distractors.Count < 3)
                distractors.Add(rng.Next(10000, 100000));

            var options = distractors.ToList();
            options.Add(correct);
            options = options.OrderBy(x => rng.Next()).ToList();

            return new Question
            {
                Prompt = "Which number is greater than 100,000?",
                Options = options.Select(n => n.ToString()).ToList(),
                CorrectAnswer = correct.ToString(),
                Metadata = new Dictionary<string, object> { { "GreaterThan", 100000 } }
            };
        }

        // Activity 2: Arrange 6-digit numbers by size
        public Activity GenerateArrangeSixDigitNumbersActivity()
        {
            Random rng = new Random();
            List<int> numbers = new List<int>();
            while (numbers.Count < 4)
            {
                int num = rng.Next(100000, 999999);
                if (!numbers.Contains(num))
                    numbers.Add(num);
            }

            var sorted = numbers.OrderBy(n => n).Select(n => n.ToString()).ToList();

            return new Question
            {
                Prompt = "Arrange these numbers from smallest to largest:",
                Options = numbers.Select(n => n.ToString()).ToList(),
                CorrectAnswer = string.Join(", ", sorted),
                Metadata = new Dictionary<string, object> { { "SortedOrder", sorted } }
            };
        }


        // Activity 3: Highlight digits in place value
        public Activity GenerateHighlightPlaceValueActivity()
        {
            Random rng = new Random();
            int number = rng.Next(100000, 999999);
            string numberStr = number.ToString();

            Dictionary<string, int> placeMap = new Dictionary<string, int>
        {
            { "Hundred Thousands", 0 },
            { "Ten Thousands", 1 },
            { "Thousands", 2 },
            { "Hundreds", 3 },
            { "Tens", 4 },
            { "Ones", 5 }
        };

            var keys = placeMap.Keys.ToList();
            string selectedPlace = keys[rng.Next(keys.Count)];
            int index = placeMap[selectedPlace];
            string correctDigit = numberStr[index].ToString();

            return new Question
            {
                Prompt = $"What is the digit in the {selectedPlace} place of {number}?",
                Options = numberStr.Distinct().OrderBy(x => rng.Next()).Select(c => c.ToString()).Take(4).ToList(),
                CorrectAnswer = correctDigit,
                Metadata = new Dictionary<string, object>
            {
                { "TargetNumber", number },
                { "TargetPlace", selectedPlace },
                { "CorrectDigit", correctDigit }
            }
            };
        }


        // Activity 4: Estimate magnitude (e.g., is 678900 closer to 500k or 1M?)
        public Activity GenerateEstimateMagnitudeActivity()
        {
            Random rng = new Random();
            int number = rng.Next(200000, 950000); // avoid numbers too close to the edges
            int roundedDown = (number / 100000) * 100000;
            int roundedUp = roundedDown + 100000;

            int distanceToDown = Math.Abs(number - roundedDown);
            int distanceToUp = Math.Abs(number - roundedUp);

            int closer = distanceToDown < distanceToUp ? roundedDown : roundedUp;

            var options = new List<int> { roundedDown, roundedUp, rng.Next(100000, 999999) };
            if (!options.Contains(closer)) options[2] = closer; // ensure correct option is in list

            options = options.Distinct().OrderBy(x => rng.Next()).ToList();

            return new Question
            {
                Prompt = $"Which number is {number} closer to?",
                Options = options.Select(n => n.ToString()).ToList(),
                CorrectAnswer = closer.ToString(),
                Metadata = new Dictionary<string, object>
            {
                { "OriginalNumber", number },
                { "RoundedDown", roundedDown },
                { "RoundedUp", roundedUp },
                { "CloserTo", closer }
            }
            };
        }


        // Activity 5: Compare numbers using specific digits (e.g., 4th digit)
        public Activity GenerateCompareSpecificDigitActivity()
        {
            Random rng = new Random();
            List<string> numbers = new List<string>();

            while (numbers.Count < 3)
            {
                int num = rng.Next(100000, 999999);
                string str = num.ToString();
                if (!numbers.Contains(str)) numbers.Add(str);
            }

            int digitPosition = rng.Next(0, 6); // 0 = leftmost
            string positionName = digitPosition switch
            {
                0 => "1st",
                1 => "2nd",
                2 => "3rd",
                3 => "4th",
                4 => "5th",
                5 => "6th",
                _ => "nth"
            };

            string correct = numbers.OrderByDescending(n => n[digitPosition]).First();

            return new Question
            {
                Prompt = $"Which number has the highest {positionName} digit?",
                Options = numbers,
                CorrectAnswer = correct,
                Metadata = new Dictionary<string, object>
            {
                { "DigitIndex", digitPosition },
                { "ComparedDigit", positionName },
                { "TargetDigit", correct[digitPosition].ToString() }
            }
            };
        }


        // Activity 6: Spot the fake - which is not a 6-digit number
        public Activity GenerateSpotFakeSixDigitActivity()
        {
            Random rng = new Random();
            List<string> options = new List<string>();

            // Generate 3 valid 6-digit numbers
            while (options.Count < 3)
            {
                string num = rng.Next(100000, 999999).ToString();
                if (!options.Contains(num)) options.Add(num);
            }

            // Add a "fake" number (not 6 digits)
            string fake = rng.Next(1000, 99999).ToString(); // shorter than 6 digits
            if (rng.NextDouble() > 0.5)
                fake = (rng.Next(1_000_000, 9_999_999)).ToString(); // longer than 6 digits

            options.Add(fake);
            options = options.OrderBy(x => rng.Next()).ToList();

            return new Question
            {
                Prompt = "Which of these is NOT a 6-digit number?",
                Options = options,
                CorrectAnswer = fake,
                Metadata = new Dictionary<string, object>
            {
                { "FakeOption", fake },
                { "AllOptions", options }
            }
            };
        }

    }
}