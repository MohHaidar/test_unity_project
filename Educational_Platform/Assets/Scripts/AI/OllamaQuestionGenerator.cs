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

        Debug.Log($"[QuestionGenerator] Generated: {question}");
        return question;
    }

    /// <summary>
    /// Builds the prompt sent to Ollama for question generation.
    /// </summary>
    private string BuildPrompt(Player player, Step step)
    {
        string recentPerformance = GetRecentPerformanceText(player);
        float stepMastery = player.GetCurrentStepMastery();

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

RECENT PERFORMANCE:
{recentPerformance}

TARGET:
Create 1 multiple choice question appropriate for this step.
The question should help the student reach the streak goal.
Focus on: {step.Description}

REQUIREMENTS:
- Question should be clear and age-appropriate
- Must have exactly 4 options
- Options should include: 1 correct answer + 1 common mistake + 2 plausible wrong answers
- Difficulty should match step level (approximately {step.MasteryTarget:F2})

RETURN ONLY VALID JSON (no other text, no markdown):

{{
  ""question"": ""<the math question>"",
  ""options"": [""<option1>"", ""<option2>"", ""<option3>"", ""<option4>""],
  ""correctAnswer"": ""<exact match to one of the options>"",
  ""difficulty"": {step.MasteryTarget:F2},
  ""skillFocus"": ""{step.Description}"",
  ""explanation"": ""<why is the answer correct?>"",
  ""commonMistakeExplanation"": ""<what's the common wrong answer and why?>"",
  ""estimatedTimeSeconds"": 30
}}";

        return prompt;
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

            // Validate
            if (question.Options.Count != 4)
            {
                Debug.LogWarning($"[QuestionGenerator] Expected 4 options, got {question.Options.Count}");
            }

            if (!question.Options.Contains(question.CorrectAnswer))
            {
                Debug.LogWarning($"[QuestionGenerator] Correct answer not in options list");
                // Try to fix by adding it
                if (question.Options.Count > 0)
                {
                    question.Options[0] = question.CorrectAnswer;
                }
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
