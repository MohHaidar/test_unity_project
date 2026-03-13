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
        string response = _ollamaAPI.GenerateSync(prompt, temperature: 0.3f);

        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("[QuestionGenerator] Ollama returned empty response");
            return GetFallbackQuestion();
        }

        IQuestion question = ParseJSONResponse(response);
        if (question == null)
        {
            Debug.LogError("[QuestionGenerator] Failed to parse Ollama response");
            return GetFallbackQuestion();
        }

        // Check if this question was asked recently
        if (IsQuestionRecentlyAsked(player, question))
        {
            Debug.LogWarning("[QuestionGenerator] Question was asked recently, regenerating...");
            // Retry with slightly higher temperature for diversity
            response = _ollamaAPI.GenerateSync(prompt, temperature: 0.5f);
            question = ParseJSONResponse(response) ?? question;
        }

        Debug.Log($"[QuestionGenerator] Generated: {question}");
        return question;
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
        if (challenge.ToLower() == "addition")
        {
            return stepNumber switch
            {
                1 => "- First number: 0-5\n- Second number: 0-5\n- Result: Always <= 10\n- NO CARRYING allowed (e.g., 3+2, 4+1, 5+5)",
                2 => "- First number: 6-9\n- Second number: 6-9\n- Result: Always <= 18\n- NO CARRYING allowed (e.g., 6+2, 7+3, 8+9)",
                3 => "- First number: 10-50\n- Second number: 10-50\n- CRITICAL CONSTRAINT: NO CARRYING required\n  - Ones digits must sum to < 10 (e.g., 3 + 4 = 7, not 8)\n  - Tens digits must sum to < 10\n  - Examples ALLOWED: 12+13=25, 22+14=36, 31+16=47\n  - Examples FORBIDDEN: 15+17 (5+7=12, needs carrying), 27+15 (7+5=12, needs carrying), 38+24 (8+4=12, needs carrying)",
                4 => "- First number: 10-99\n- Second number: 10-99\n- CARRYING IS ALLOWED and expected\n  - Can have ones digits summing to >= 10 (e.g., 8+5=13)\n  - Can have tens digits summing to >= 10 (e.g., 70+50=120)\n  - Examples: 27+15=42, 56+23=79, 48+37=85",
                _ => ""
            };
        }
        else if (challenge.ToLower() == "subtraction")
        {
            return stepNumber switch
            {
                1 => "- First number: 0-5\n- Second number: 0-5\n- Result: Always >= 0",
                2 => "- First number: 6-9\n- Second number: 6-9\n- Result: Always >= 0",
                _ => ""
            };
        }

        return "";
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
