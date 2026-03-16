using System;
using UnityEngine;

/// <summary>
/// Generates multiple choice math questions using Ollama.
/// Adapts difficulty and focus area based on player metrics and current step.
/// </summary>
public class OllamaQuestionGenerator
{
    private OllamaAPI _ollamaAPI;

    public OllamaQuestionGenerator(string model = "mistral")
    {
        _ollamaAPI = new OllamaAPI(model);
    }

    /// <summary>
    /// Generates a new question tailored to the player's current step.
    /// Avoids repeating questions from the last 10 answers.
    /// </summary>
    public IQuestion GenerateQuestion(Player player, Step step)
    {
        if (player == null || step == null)
        {
            Debug.LogError("[QuestionGenerator] Player or Step is null");
            return GetFallbackQuestion();
        }

        string prompt = BuildPrompt(player, step);

        // Retry loop: attempt multiple times before falling back
        int maxAttempts = 4;
        float[] temps = new float[] { 0.3f, 0.5f, 0.7f, 0.9f };

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float temp = temps[Mathf.Clamp(attempt, 0, temps.Length - 1)];
            string response = _ollamaAPI.GenerateSync(prompt, temperature: temp);

            if (string.IsNullOrEmpty(response))
            {
                Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: Ollama returned empty response (temp={temp})");
                continue; // try again with higher temperature
            }

            IQuestion question = ParseJSONResponse(response);
            if (question == null)
            {
                Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: Failed to parse response (temp={temp})");
                continue; // try again
            }

            // If the generated question was asked recently, skip and retry
            if (IsQuestionRecentlyAsked(player, question))
            {
                Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: Question was asked recently, retrying (temp={temp})");
                continue;
            }

            // Strict validation: if it fails, retry
            if (question is MultipleChoiceQuestion mcq)
            {
                if (!ValidateQuestion(mcq))
                {
                    Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: Question failed validation, retrying (temp={temp})");
                    continue;
                }
            }

            Debug.Log($"[QuestionGenerator] Generated on attempt {attempt + 1}: {question}");
            return question;
        }

        Debug.LogWarning("[QuestionGenerator] All attempts failed, returning fallback question");
        return GetFallbackQuestion();
    }

    /// <summary>
    /// Checks if a question was asked in the last 10 questions.
    /// </summary>
    private bool IsQuestionRecentlyAsked(Player player, IQuestion newQuestion)
    {
        int recentCount = System.Math.Min(10, player.QuestionHistory.Count);
        int startIdx = System.Math.Max(0, player.QuestionHistory.Count - recentCount);

        for (int i = startIdx; i < player.QuestionHistory.Count; i++)
        {
            if (player.QuestionHistory[i].QuestionText == newQuestion.QuestionText)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Builds the prompt sent to Ollama for question generation.
    /// </summary>
    private string BuildPrompt(Player player, Step step)
    {
        string recentPerformance = GetRecentPerformanceText(player);
        float stepMastery = player.GetCurrentStepMastery();
        
        // Build step-specific constraints
        string stepConstraints = GetStepConstraints(player.CurrentChallenge, step.Number);

        string prompt = $@"You are an expert math teacher creating personalized math questions.

STUDENT PROFILE:
- Name: {player.Name}
- Current Subject: {player.CurrentSubject}
- Current Challenge: {player.CurrentChallenge}
- Mastery (this step): {stepMastery:F2} (0.0=beginner, 1.0=expert)
- Current Streak (this step): {player.StreakInCurrentStep}/{step.StreakGoal}
- Questions Answered (this step): {player.QuestionsInCurrentStep}

STEP CONTEXT:
- Step {step.Number}: {step.Description}
- Mastery Target: {step.MasteryTarget:F2}
- Streak Goal: {step.StreakGoal}
- Phase: {step.GetCurrentPhase()}

STEP-SPECIFIC REQUIREMENTS:
{stepConstraints}

RECENT PERFORMANCE AND PREVIOUS QUESTIONS:
{recentPerformance}

TARGET:
Create 1 multiple choice question appropriate for this step.
The question should help the student reach the streak goal.
- IMPORTANT: Do NOT repeat any of the questions listed above

REQUIREMENTS:
- Question should be clear and age-appropriate
- Must have exactly 4 options
- Options should ONLY be NUMERIC ANSWERS (just the number, like ""42"", NOT expressions like ""27 + 15 = 42"")
- Options should include: 1 correct answer + 1 common mistake + 2 plausible wrong answers
- Difficulty should match step level (approximately {step.MasteryTarget:F2})
- Do NOT include the full equation in the options
- AVOID repeating the same question twice

RETURN ONLY VALID JSON (no other text, no markdown):

{{
  ""question"": ""<the math question>"",
  ""options"": [""<number only>"", ""<number only>"", ""<number only>"", ""<number only>""],
  ""correctAnswer"": ""<exact numeric answer matching one option>"",
  ""difficulty"": {step.MasteryTarget:F2},
  ""skillFocus"": ""{step.Description}"",
  ""explanation"": ""<why is the answer correct?>"",
  ""commonMistakeExplanation"": ""<what's the common wrong answer and why?>"",
  ""estimatedTimeSeconds"": 30
}}";

        return prompt;
    }

    /// <summary>
    /// Returns step-specific constraints to guide question generation.
    /// </summary>
    private string GetStepConstraints(string challenge, int stepNumber)
    {
        string normalizedChallenge = challenge
            .Trim()
            .ToLowerInvariant()
            .Replace(" ", "_")
            .Replace("-", "_");

        switch (normalizedChallenge)
        {
            case "addition":
                return stepNumber switch
                {
                    1 => "- Use addition only\n- Both addends: 0-10\n- Total: at most 10\n- No story problems needed",
                    2 => "- Use addition facts that make 10\n- One addend should be between 1 and 9\n- Ask for the missing partner or direct sum that completes 10",
                    3 => "- Use two-digit addition without carrying\n- Each ones-place sum must stay below 10\n- Keep totals below 100",
                    4 => "- Use two-digit addition with carrying\n- At least one ones-place sum must be 10 or more\n- Keep totals below 150",
                    _ => ""
                };

            case "subtraction":
                return stepNumber switch
                {
                    1 => "- Use subtraction only\n- Numbers should stay within 10\n- Result must be 0 or greater\n- Example: 'What is 8 minus 3?'",
                    2 => "- Ask for the missing addend in an addition fact\n- Phrase it as a full question, e.g. 'What number completes 3 + ? = 8?'\n- The question MUST start with 'What' or 'Find'\n- Correct answer must be a whole number from 0 to 10",
                    3 => "- Subtract within 20; one of the numbers may cross the tens boundary (e.g. 16 - 7)\n- Result must be 0 or greater\n- Example: 'What is 16 minus 7?'",
                    4 => "- Subtract a single-digit number from a multiple of 10 (e.g. 30 - 7, 50 - 4)\n- Minuend must be a multiple of 10 between 10 and 90\n- Example: 'What is 40 minus 6?'",
                    5 => "- Use two-digit subtraction without borrowing\n- Ones digit of the minuend must be greater than or equal to the ones digit of the subtrahend\n- Example: 'What is 47 minus 23?'",
                    6 => "- Use two-digit subtraction that requires borrowing (regrouping)\n- At least one borrow must be required\n- Result must stay positive\n- Example: 'What is 52 minus 37?'",
                    _ => ""
                };

            case "multiplication_i":
                return stepNumber switch
                {
                    1 => "- Use multiplication as equal groups\n- Small whole numbers only\n- Formats like 3 groups of 4 or 3 x 4 are allowed",
                    2 => "- Use multiplication by 2 only\n- Other factor between 0 and 12",
                    3 => "- Use multiplication by 5 only\n- Other factor between 0 and 12",
                    4 => "- Use multiplication by 10 only\n- Other factor between 0 and 12",
                    _ => ""
                };

            case "division_i":
                return stepNumber switch
                {
                    1 => "- Use equal sharing or grouping questions\n- Exact division only\n- Small whole numbers only",
                    2 => "- Use division by 2 only\n- Quotient must be a whole number",
                    3 => "- Use division by 5 only\n- Quotient must be a whole number",
                    4 => "- Use division by 10 only\n- Quotient must be a whole number",
                    _ => ""
                };

            case "multiplication_ii":
                return stepNumber switch
                {
                    1 => "- Use multiplication by 3 only\n- Other factor between 1 and 12",
                    2 => "- Use multiplication by 4 only\n- Other factor between 1 and 12",
                    3 => "- Use multiplication by 6 only\n- Other factor between 1 and 12",
                    4 => "- Use multiplication by 7 only\n- Other factor between 1 and 12",
                    _ => ""
                };

            case "division_ii":
                return stepNumber switch
                {
                    1 => "- Use division by 3 only\n- Quotient must be a whole number between 1 and 12",
                    2 => "- Use division by 4 only\n- Quotient must be a whole number between 1 and 12",
                    3 => "- Use division by 6 only\n- Quotient must be a whole number between 1 and 12",
                    4 => "- Use division by 7 only\n- Quotient must be a whole number between 1 and 12",
                    _ => ""
                };

            case "multiplication_iii":
                return stepNumber switch
                {
                    1 => "- Use multiplication by 8 only\n- Other factor between 1 and 12",
                    2 => "- Use multiplication by 9 only\n- Other factor between 1 and 12",
                    3 => "- Use ANY multiplication fact from the 1–9 times tables\n- Mix factors freely",
                    _ => ""
                };

            case "division_iii":
                return stepNumber switch
                {
                    1 => "- Use division by 8 only\n- Quotient must be a whole number between 1 and 12",
                    2 => "- Use division by 9 only\n- Quotient must be a whole number between 1 and 12",
                    3 => "- Use ANY exact division fact (divisors 1–9)\n- Mix divisors freely",
                    _ => ""
                };

            case "arithmetic_review":
                return stepNumber switch
                {
                    1 => "- Use ONLY addition or subtraction\n- Mix question types freely\n- Numbers up to 100",
                    2 => "- Use ONLY multiplication or division\n- Use any facts from the 1–9 times tables\n- Division must be exact",
                    3 => "- Use any of the four operations: +, -, ×, ÷\n- Mix freely\n- All results must be whole numbers",
                    4 => "- Create a two-step mental math question using any two operations\n- Phrase as 'What is ...?' and give a single numeric answer\n- All intermediate and final results must be whole numbers",
                    _ => ""
                };

            case "order_of_operations":
                return stepNumber switch
                {
                    1 => "- Use expressions with multiplication and addition\n- No parentheses\n- The correct solution must multiply before adding",
                    2 => "- Use expressions with multiplication and subtraction\n- No parentheses\n- The correct solution must multiply before subtracting",
                    3 => "- Use parentheses to force the first operation\n- Keep expressions short and numeric",
                    4 => "- Use mixed expressions with 2-3 operations\n- May include parentheses\n- Result must be a whole number",
                    _ => ""
                };

            case "expressions_with_variables":
                return stepNumber switch
                {
                    1 => "- Give a value for x and ask the student to evaluate x + a\n- Use whole numbers only",
                    2 => "- Give a value for x and ask the student to evaluate x - a\n- Result must be 0 or greater",
                    3 => "- Give a value for x and ask the student to evaluate ax\n- Use small coefficients like 2, 3, 5, or 10",
                    4 => "- Give a value for x and ask the student to evaluate x / a\n- Division must be exact",
                    _ => ""
                };

            case "one_step_equations":
                return stepNumber switch
                {
                    1 => "- Solve equations of the form x + a = b\n- Phrase as a question: 'What is the value of x in x + 3 = 7?'\n- Whole-number solution only",
                    2 => "- Solve equations of the form x - a = b\n- Phrase as a question: 'What is the value of x in x - 4 = 2?'\n- Whole-number solution only",
                    3 => "- Solve equations of the form ax = b\n- Phrase as a question: 'What is the value of x in 3x = 12?'\n- Use exact whole-number solutions",
                    4 => "- Solve equations of the form x / a = b\n- Phrase as a question: 'What is the value of x in x / 4 = 5?'\n- Use exact whole-number solutions",
                    _ => ""
                };

            case "two_step_equations":
                return stepNumber switch
                {
                    1 => "- Solve equations of the form ax + b = c\n- Phrase as a question: 'What is the value of x in 2x + 3 = 11?'\n- Use small positive integers\n- Whole-number solution only",
                    2 => "- Solve equations of the form ax - b = c\n- Phrase as a question: 'What is the value of x in 3x - 4 = 8?'\n- Use small positive integers\n- Whole-number solution only",
                    3 => "- Solve equations of the form x / a + b = c\n- Phrase as a question: 'What is the value of x in x / 2 + 3 = 7?'\n- Division must stay exact\n- Whole-number solution only",
                    4 => "- Solve equations of the form x / a - b = c\n- Phrase as a question: 'What is the value of x in x / 3 - 2 = 4?'\n- Division must stay exact\n- Whole-number solution only",
                    _ => ""
                };

            case "systems_of_equations":
                return stepNumber switch
                {
                    1 => "- Use one equation with a known x value and another equation y = x + a\n- Phrase as a question: 'If x = 3 and y = x + 4, what is y?'\n- Whole-number answer only",
                    2 => "- Use a two-equation system where one equation already isolates y\n- Phrase as a question: 'In the system y = x + 2 and y = 7, what is the value of x?'\n- Whole-number solution only",
                    3 => "- Use a two-equation system where one equation already isolates y\n- Phrase as a question: 'In the system y = x + 2 and x = 5, what is the value of y?'\n- Whole-number solution only",
                    4 => "- Use standard-form systems such as x + y = c and x - y = d\n- Phrase as a question: 'In the system x + y = 10 and x - y = 4, what is the value of x?'\n- Whole-number solution only",
                    5 => "- Use standard-form systems such as x + y = c and x - y = d\n- Phrase as a question: 'In the system x + y = 10 and x - y = 4, what is the value of y?'\n- Whole-number solution only",
                    _ => ""
                };

            default:
                return "";
        }
    }

    /// <summary>
    /// Summarizes recent performance for the prompt.
    /// </summary>
    private string GetRecentPerformanceText(Player player)
    {
        if (player.QuestionHistory.Count == 0)
        {
            return "No previous questions answered yet in this step.";
        }

        string recent = $"Last {System.Math.Min(5, player.QuestionHistory.Count)} answers:\n";
        int startIdx = System.Math.Max(0, player.QuestionHistory.Count - 5);
        for (int i = startIdx; i < player.QuestionHistory.Count; i++)
        {
            var answer = player.QuestionHistory[i];
            recent += $"- {(answer.IsCorrect ? "✓" : "✗")} {answer.QuestionText} ({answer.TimeTakenSeconds:F1}s)\n";
        }
        return recent;
    }

    /// <summary>
    /// Parses JSON response from Ollama into a MultipleChoiceQuestion object.
    /// </summary>
    private IQuestion ParseJSONResponse(string jsonText)
    {
        try
        {
            // Try to extract JSON if there's extra text
            int jsonStart = jsonText.IndexOf('{');
            int jsonEnd = jsonText.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                jsonText = jsonText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            // Parse JSON
            QuestionJSON data = JsonUtility.FromJson<QuestionJSON>(jsonText);

            if (data == null || string.IsNullOrEmpty(data.question))
            {
                Debug.LogError("[QuestionGenerator] Invalid JSON structure");
                return null;
            }

            // Convert to MultipleChoiceQuestion
            var question = new MultipleChoiceQuestion
            {
                QuestionText = data.question,
                Options = new System.Collections.Generic.List<string>(data.options),
                CorrectAnswer = data.correctAnswer,
                Difficulty = data.difficulty,
                SkillFocus = data.skillFocus,
                Explanation = data.explanation,
                CommonMistakeExplanation = data.commonMistakeExplanation,
                EstimatedTimeSeconds = data.estimatedTimeSeconds
            };

            // Strict validation
            if (!ValidateQuestion(question))
            {
                Debug.LogError("[QuestionGenerator] Question failed validation");
                return null;
            }

            return question;
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionGenerator] JSON Parse Error: {e.Message}");
            Debug.LogError($"[QuestionGenerator] Raw JSON: {jsonText}");
            return null;
        }
    }

    /// <summary>
    /// Strictly validates a question before returning it.
    /// Returns false if any validation fails.
    /// </summary>
    private bool ValidateQuestion(MultipleChoiceQuestion question)
    {
        // Check 1: Must have exactly 4 options
        if (question.Options.Count != 4)
        {
            Debug.LogError($"[QuestionGenerator] Expected 4 options, got {question.Options.Count}");
            return false;
        }

        // Check 2: Correct answer must be in options
        if (!question.Options.Contains(question.CorrectAnswer))
        {
            Debug.LogError($"[QuestionGenerator] Correct answer '{question.CorrectAnswer}' not in options");
            return false;
        }

        // Check 3: No duplicate options
        var uniqueOptions = new System.Collections.Generic.HashSet<string>(question.Options);
        if (uniqueOptions.Count != 4)
        {
            Debug.LogError("[QuestionGenerator] Duplicate options detected");
            return false;
        }

        // Check 4: All options must be pure numbers (no expressions like "33 + 15")
        foreach (var option in question.Options)
        {
            if (!IsNumericOnly(option))
            {
                Debug.LogError($"[QuestionGenerator] Option '{option}' is not numeric-only");
                return false;
            }
        }

        // Check 5: Correct answer must be a valid number
        if (!int.TryParse(question.CorrectAnswer, out int correctValue))
        {
            Debug.LogError($"[QuestionGenerator] Correct answer '{question.CorrectAnswer}' is not a valid number");
            return false;
        }

        // Check 6: Question text must not be just an equation (must have "What is", "Calculate", etc)
        if (IsQuestionTextJustEquation(question.QuestionText))
        {
            Debug.LogError($"[QuestionGenerator] Question text is just an equation: '{question.QuestionText}'");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a string is purely numeric (0-9 characters only).
    /// </summary>
    private bool IsNumericOnly(string text)
    {
        if (string.IsNullOrEmpty(text)) return false;
        foreach (char c in text)
        {
            if (!char.IsDigit(c)) return false;
        }
        return true;
    }

    /// <summary>
    /// Checks if question text looks like just an equation (e.g., "27 + 15" or "56 + 23").
    /// Good questions should be "What is 27 + 15?" or similar.
    /// </summary>
    private bool IsQuestionTextJustEquation(string text)
    {
        // Remove whitespace and check if it looks like "number operator number"
        string cleaned = text.Trim();
        
        // Should contain words like "what", "calculate", "find", etc
        bool hasQuestionWords = cleaned.ToLower().Contains("what") || 
                                cleaned.ToLower().Contains("calculate") ||
                                cleaned.ToLower().Contains("find") ||
                                cleaned.ToLower().Contains("is") ||
                                cleaned.ToLower().Contains("compute");

        if (!hasQuestionWords)
        {
            Debug.LogWarning($"[QuestionGenerator] Question lacks proper phrasing: '{text}'");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Returns a hardcoded fallback question if generation fails.
    /// </summary>
    private IQuestion GetFallbackQuestion()
    {
        return new MultipleChoiceQuestion
        {
            QuestionText = "What is 5 + 3?",
            Options = new System.Collections.Generic.List<string> { "8", "7", "9", "6" },
            CorrectAnswer = "8",
            Difficulty = 0.3f,
            SkillFocus = "Basic Addition",
            Explanation = "5 + 3 = 8",
            CommonMistakeExplanation = "Some students might say 7 if they miscounted by 1",
            EstimatedTimeSeconds = 20
        };
    }

    /// <summary>
    /// Wrapper for JSON deserialization (matches Ollama response format).
    /// </summary>
    [System.Serializable]
    private class QuestionJSON
    {
        public string question;
        public string[] options;
        public string correctAnswer;
        public float difficulty;
        public string skillFocus;
        public string explanation;
        public string commonMistakeExplanation;
        public int estimatedTimeSeconds;
    }
}
