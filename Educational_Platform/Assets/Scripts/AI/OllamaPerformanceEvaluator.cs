using System;
using UnityEngine;

/// <summary>
/// Evaluates student answers using Ollama.
/// Works with any IQuestion implementation (not just MultipleChoice).
/// Returns correctness, error type, and updated mastery level.
/// </summary>
public class OllamaPerformanceEvaluator
{
    private OllamaAPI _ollamaAPI;

    public OllamaPerformanceEvaluator(string model = "mistral")
    {
        _ollamaAPI = new OllamaAPI(model);
    }

    /// <summary>
    /// Evaluates a student's answer to a question.
    /// Returns structured evaluation with metrics.
    /// </summary>
    public EvaluationResult Evaluate(Player player, Step step, IQuestion question, string studentAnswer, float timeTakenSeconds)
    {
        if (player == null || step == null || question == null)
        {
            Debug.LogError("[Evaluator] Player, Step, or Question is null");
            return GetFallbackEvaluation(question, studentAnswer);
        }

        string prompt = BuildEvaluationPrompt(player, step, question, studentAnswer, timeTakenSeconds);
        string response = _ollamaAPI.GenerateSync(prompt, temperature: 0.3f);

        if (string.IsNullOrEmpty(response))
        {
            Debug.LogError("[Evaluator] Ollama returned empty response");
            return GetFallbackEvaluation(question, studentAnswer);
        }

        EvaluationResult result = ParseEvaluationResponse(response, question, studentAnswer);
        if (result == null)
        {
            Debug.LogError("[Evaluator] Failed to parse evaluation response");
            return GetFallbackEvaluation(question, studentAnswer);
        }

        Debug.Log($"[Evaluator] Evaluation: {(result.IsCorrect ? "CORRECT" : "INCORRECT")} | Mastery Delta: {result.MasteryDelta:+0.00;-0.00}");
        return result;
    }

    /// <summary>
    /// Builds the prompt sent to Ollama for answer evaluation.
    /// Routes to a verbal communication prompt for ConversationQuestion free-text answers.
    /// </summary>
    private string BuildEvaluationPrompt(Player player, Step step, IQuestion question, string studentAnswer, float timeTakenSeconds)
    {
        // Parlour free-text answers need a verbal-communication-aware prompt
        if (question is ConversationQuestion conv && conv.IsFreeTextAnswer(studentAnswer))
            return BuildParlourEvaluationPrompt(player, step, conv, studentAnswer, timeTakenSeconds);

        float stepMastery = player.GetCurrentStepMastery();

        string prompt = $@"You are an expert math educator analyzing a student's answer.

QUESTION:
{question.QuestionText}

QUESTION DETAILS:
- Correct Answer: {GetCorrectAnswerFromQuestion(question)}
- Difficulty: {question.Difficulty:F2} (0.0=easy, 1.0=hard)
- Expected Time: {GetEstimatedTimeFromQuestion(question)} seconds
- Skill Focus: {question.SkillFocus}

STUDENT ANSWER:
- Given Answer: {studentAnswer}
- Time Taken: {timeTakenSeconds:F1} seconds

STUDENT CONTEXT:
- Name: {player.Name}
- Step {step.Number}: {step.Description}
- Current Mastery (this step): {stepMastery:F2}
- Streak (this step): {player.StreakInCurrentStep}/{step.StreakGoal}
- Questions Answered (this step): {player.QuestionsInCurrentStep}
- Mastery Target: {step.MasteryTarget:F2}

TASK:
1. Determine if the answer is CORRECT or INCORRECT
2. If incorrect, identify error type:
   - conceptual_gap: Doesn't understand the concept
   - careless_mistake: Right concept but arithmetic error
   - timing_issue: Rushed or didn't have time
3. Calculate mastery change:
   - Correct + Fast → +0.05 mastery
   - Correct + Slow → +0.03 mastery
   - Incorrect but close → -0.02 mastery
   - Incorrect conceptual gap → -0.05 mastery
4. Suggest next focus area if there's a gap

RETURN ONLY VALID JSON (no other text, no markdown):

{{
  ""isCorrect"": {(question.CheckAnswer(studentAnswer) ? "true" : "false")},
  ""correctAnswer"": ""{GetCorrectAnswerFromQuestion(question)}"",
  ""studentAnswer"": ""{studentAnswer}"",
  ""errorType"": ""<null if correct, else: conceptual_gap|careless_mistake|timing_issue>"",
  ""errorExplanation"": ""<brief analysis of the error type and root cause — for internal use>"",
  ""studentHint"": ""<If incorrect: 1-2 sentences in second person, teaching tone. Explain the mistake and guide them to the right approach. E.g. 'You got 7, but remember you need to add both numbers together. Try counting up from 3 four more times.' Null if correct.>"",
  ""speedScore"": <0.0-1.0>,
  ""confidenceInPerformance"": <0.0-1.0>,
  ""masteryDelta"": <-0.10 to +0.10>,
  ""nextDifficulty"": ""<increase|same|decrease>"",
  ""nextFocusArea"": ""<what to focus on next or null>""
}}";

        return prompt;
    }

    private string GetCorrectAnswerFromQuestion(IQuestion question)
    {
        if (question is MultipleChoiceQuestion mcQuestion)
            return mcQuestion.CorrectAnswer;

        if (question is FillInBlankQuestion fibQuestion)
            return fibQuestion.CorrectAnswerString();

        if (question is ConversationQuestion convQuestion)
            return convQuestion.CorrectAnswer ?? "see evaluation";

        return "unknown";
    }

    private int GetEstimatedTimeFromQuestion(IQuestion question)
    {
        if (question is MultipleChoiceQuestion mcQuestion)
            return mcQuestion.EstimatedTimeSeconds;

        if (question is FillInBlankQuestion fibQuestion)
            return fibQuestion.Blanks != null && fibQuestion.Blanks.Count > 1 ? 45 : 30;

        return 30;
    }

    /// <summary>
    /// Builds an evaluation prompt for parlour free-text answers.
    /// The AI judges the appropriateness of the player's verbal response
    /// rather than checking for an exact string match.
    /// </summary>
    private string BuildParlourEvaluationPrompt(Player player, Step step, ConversationQuestion question, string studentAnswer, float timeTakenSeconds)
    {
        float stepMastery = player.GetCurrentStepMastery();

        return $@"You are an expert verbal communication coach evaluating a student's free-text response.

SCENE:
{question.CharacterDialogue}

QUESTION ASKED:
{question.QuestionText}

SKILL BEING ASSESSED: {question.SkillFocus}
STEP DESCRIPTION: {step.Description}

STUDENT'S FREE-TEXT RESPONSE:
""{studentAnswer}""

IDEAL RESPONSE (for reference): ""{question.CorrectAnswer}""

STUDENT CONTEXT:
- Current mastery (this step): {stepMastery:F2}
- Streak: {player.StreakInCurrentStep}/{step.StreakGoal}

EVALUATION CRITERIA:
Judge the student's response on how well it demonstrates the skill being assessed ({question.SkillFocus}).
A response is CORRECT if it:
  - Uses appropriate register and tone for the scene
  - Demonstrates understanding of the skill being practiced
  - Would be a natural and effective real-world response
A response is INCORRECT if it:
  - Uses the wrong register (too formal/too casual for context)
  - Misreads the tone or intent of the situation
  - Would be socially inappropriate or awkward in the scene

The student does NOT need to match the ideal response exactly — judge on communication quality.

RETURN ONLY VALID JSON (no extra text, no markdown):

{{
  ""isCorrect"": <true or false>,
  ""correctAnswer"": ""{question.CorrectAnswer}"",
  ""studentAnswer"": ""{studentAnswer}"",
  ""errorType"": ""<null if correct, else: wrong_register|tone_mismatch|misread_subtext|inappropriate>"",
  ""errorExplanation"": ""<brief analysis of why the response works or doesn't work>"",
  ""studentHint"": ""<If incorrect: 1-2 sentences guiding them. E.g. 'Your response was too formal for this casual scene. Try matching Maya's friendly, relaxed tone.' Null if correct.>"",
  ""speedScore"": <0.0-1.0, based on {timeTakenSeconds:F1}s — longer is fine for free-text>,
  ""confidenceInPerformance"": <0.0-1.0>,
  ""masteryDelta"": <-0.10 to +0.10>,
  ""nextDifficulty"": ""<increase|same|decrease>"",
  ""nextFocusArea"": ""<what specific communication skill to work on next, or null>""
}}";
    }

    /// <summary>
    /// Parses JSON evaluation response from Ollama.
    /// </summary>
    private EvaluationResult ParseEvaluationResponse(string jsonText, IQuestion question, string studentAnswer)
    {
        try
        {
            // Extract JSON if there's extra text
            int jsonStart = jsonText.IndexOf('{');
            int jsonEnd = jsonText.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                jsonText = jsonText.Substring(jsonStart, jsonEnd - jsonStart + 1);
            }

            EvaluationJSON data = JsonUtility.FromJson<EvaluationJSON>(jsonText);

            if (data == null)
            {
                Debug.LogError("[Evaluator] Invalid JSON structure");
                return null;
            }

            // For parlour free-text answers the AI decides correctness (not CheckAnswer)
            bool isCorrect = (question is ConversationQuestion conv && conv.IsFreeTextAnswer(studentAnswer))
                ? data.isCorrect
                : question.CheckAnswer(studentAnswer);

            EvaluationResult result = new EvaluationResult
            {
                IsCorrect = isCorrect,
                ErrorType = data.errorType,
                ErrorExplanation = data.errorExplanation,
                StudentHint = data.studentHint,
                SpeedScore = data.speedScore,
                ConfidenceInPerformance = data.confidenceInPerformance,
                MasteryDelta = data.masteryDelta,
                NextDifficulty = data.nextDifficulty,
                NextFocusArea = data.nextFocusArea
            };

            return result;
        }
        catch (Exception e)
        {
            Debug.LogError($"[Evaluator] Parse Error: {e.Message}");
            Debug.LogError($"[Evaluator] Raw JSON: {jsonText}");
            return null;
        }
    }

    /// <summary>
    /// Returns a fallback evaluation based on simple correctness check.
    /// </summary>
    private EvaluationResult GetFallbackEvaluation(IQuestion question, string studentAnswer)
    {
        bool isCorrect = question != null && question.CheckAnswer(studentAnswer);

        return new EvaluationResult
        {
            IsCorrect = isCorrect,
            ErrorType = isCorrect ? null : "unknown",
            ErrorExplanation = isCorrect ? null : "Answer is incorrect",
            StudentHint = isCorrect ? null : "That's not quite right. Take your time and try working through it step by step.",
            SpeedScore = 0.5f,
            ConfidenceInPerformance = 0.3f,
            MasteryDelta = isCorrect ? 0.03f : -0.03f,
            NextDifficulty = "same",
            NextFocusArea = null
        };
    }

    [System.Serializable]
    private class EvaluationJSON
    {
        public bool isCorrect;
        public string correctAnswer;
        public string studentAnswer;
        public string errorType;
        public string errorExplanation;
        public string studentHint;
        public float speedScore;
        public float confidenceInPerformance;
        public float masteryDelta;
        public string nextDifficulty;
        public string nextFocusArea;
    }
}

/// <summary>
/// Result of answer evaluation.
/// </summary>
public class EvaluationResult
{
    public bool IsCorrect { get; set; }
    public string ErrorType { get; set; }
    public string ErrorExplanation { get; set; }  // internal analysis
    public string StudentHint { get; set; }        // student-facing teaching message
    public float SpeedScore { get; set; } // 0.0-1.0
    public float ConfidenceInPerformance { get; set; } // 0.0-1.0
    public float MasteryDelta { get; set; } // -0.10 to +0.10
    public string NextDifficulty { get; set; } // "increase", "same", "decrease"
    public string NextFocusArea { get; set; } // Specific concept to focus on

    public override string ToString()
    {
        return $"[{(IsCorrect ? "✓" : "✗")}] Mastery: {MasteryDelta:+0.00;-0.00} | Next: {NextDifficulty} | Focus: {NextFocusArea}";
    }
}
