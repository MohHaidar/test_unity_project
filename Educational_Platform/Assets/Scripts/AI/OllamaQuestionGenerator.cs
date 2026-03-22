using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates math questions using Ollama.
/// Supports MultipleChoiceQuestion and FillInBlankQuestion based on the step's QuestionMode.
/// Adapts difficulty and focus area based on player metrics and current step.
/// </summary>
public class OllamaQuestionGenerator
{
    private OllamaAPI _ollamaAPI;

    public OllamaQuestionGenerator(string model = "mistral")
    {
        _ollamaAPI = new OllamaAPI(model);
    }

    /// <summary>Exposes the full debug log from the most recent GenerateQuestion() call.</summary>
    public string LastGenerationDebugLog { get; private set; } = "";

    /// <summary>
    /// Generates a new question tailored to the player's current step.
    /// Mode is driven by Step.QuestionMode; Any defaults to MultipleChoice.
    /// Avoids repeating questions from the last 10 answers.
    /// </summary>
    public IQuestion GenerateQuestion(Player player, Step step)
    {
        var debugLog = new System.Text.StringBuilder();
        debugLog.AppendLine($"=== GenerateQuestion: {player?.CurrentChallenge} Step {step?.Number} ({step?.Description}) ===");

        if (player == null || step == null)
        {
            Debug.LogError("[QuestionGenerator] Player or Step is null");
            LastGenerationDebugLog = debugLog.ToString();
            return GetFallbackQuestion(step);
        }

        QuestionMode mode = ResolveMode(step);
        bool isFIB = mode == QuestionMode.FillInBlank || mode == QuestionMode.DragAndDrop;
        debugLog.AppendLine($"Mode: {mode}  |  IsFIB: {isFIB}");

        string prompt = isFIB ? BuildFillInBlankPrompt(player, step, mode) : BuildPrompt(player, step);

        int maxAttempts = 4;
        float[] temps = { 0.3f, 0.5f, 0.7f, 0.9f };

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            float temp = temps[Mathf.Clamp(attempt, 0, temps.Length - 1)];
            debugLog.AppendLine($"\n--- Attempt {attempt + 1} (temp={temp}) ---");

            string response = _ollamaAPI.GenerateSync(prompt, temperature: temp);

            if (string.IsNullOrEmpty(response))
            {
                string msg = $"Empty response from Ollama";
                debugLog.AppendLine($"REJECTED: {msg}");
                Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: {msg} (temp={temp})");
                continue;
            }

            // Show truncated raw response in debug
            string preview = response.Length > 300 ? response.Substring(0, 300) + "…" : response;
            debugLog.AppendLine($"Raw response ({response.Length} chars):\n{preview}");

            IQuestion question = isFIB ? ParseFIBResponse(response) : ParseJSONResponse(response, debugLog);
            if (question == null)
            {
                debugLog.AppendLine("REJECTED: Parse failed (see above)");
                Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: parse failed (temp={temp})");
                continue;
            }

            debugLog.AppendLine($"Parsed: \"{question.QuestionText}\"");

            if (IsQuestionRecentlyAsked(player, question, out string dupText))
            {
                debugLog.AppendLine($"REJECTED: Duplicate of recent question: \"{dupText}\"");
                Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: duplicate (temp={temp}) — \"{question.QuestionText}\"");
                continue;
            }

            if (question is MultipleChoiceQuestion mcq)
            {
                string validationFailReason;
                if (!ValidateQuestion(mcq, out validationFailReason))
                {
                    debugLog.AppendLine($"REJECTED: MC validation failed — {validationFailReason}");
                    Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: MC validation failed — {validationFailReason} (temp={temp})");
                    continue;
                }
            }

            if (question is FillInBlankQuestion fib)
            {
                string validationFailReason;
                if (!ValidateFillInBlankQuestion(fib, out validationFailReason))
                {
                    debugLog.AppendLine($"REJECTED: FIB validation failed — {validationFailReason}");
                    Debug.LogWarning($"[QuestionGenerator] Attempt {attempt + 1}: FIB validation failed — {validationFailReason} (temp={temp})");
                    continue;
                }
            }

            debugLog.AppendLine($"\n✓ ACCEPTED on attempt {attempt + 1}");
            Debug.Log($"[QuestionGenerator] Generated on attempt {attempt + 1}: {question}");
            LastGenerationDebugLog = debugLog.ToString();
            return question;
        }

        debugLog.AppendLine($"\n✗ All {maxAttempts} attempts failed — using per-step fallback");
        Debug.LogWarning("[QuestionGenerator] All attempts failed, returning fallback question");
        LastGenerationDebugLog = debugLog.ToString();
        return GetFallbackQuestion(step);
    }

    /// <summary>Resolves Any to MultipleChoice; passes through specific modes.</summary>
    private QuestionMode ResolveMode(Step step)
    {
        return step.QuestionMode == QuestionMode.Any ? QuestionMode.MultipleChoice : step.QuestionMode;
    }

    private bool IsQuestionRecentlyAsked(Player player, IQuestion newQuestion, out string matchedText)
    {
        matchedText = null;
        int recentCount = Math.Min(10, player.QuestionHistory.Count);
        int startIdx = Math.Max(0, player.QuestionHistory.Count - recentCount);
        for (int i = startIdx; i < player.QuestionHistory.Count; i++)
        {
            if (player.QuestionHistory[i].QuestionText == newQuestion.QuestionText)
            {
                matchedText = newQuestion.QuestionText;
                return true;
            }
        }
        return false;
    }

    // ─── Fill-In-Blank prompt & parsing ──────────────────────────────────────

    /// <summary>
    /// Builds the prompt for a fill-in-blank question.
    /// The AI returns a compact JSON with a "blanks" array and optional "drag_options".
    /// </summary>
    private string BuildFillInBlankPrompt(Player player, Step step, QuestionMode mode)
    {
        string recentPerformance = GetRecentPerformanceText(player);
        float stepMastery = player.GetCurrentStepMastery();
        string stepConstraints = GetStepConstraints(player.CurrentChallenge, step.Number);
        bool wantDrag = mode == QuestionMode.DragAndDrop;

        string dragInstruction = wantDrag
            ? "- Provide 5-6 short token options in \"drag_options\" (include the correct answer(s) plus plausible distractors)."
            : "- Set \"drag_options\" to an empty array [].";

        string prompt = $@"You are an expert math teacher creating a fill-in-blank question.

STUDENT PROFILE:
- Step {step.Number}: {step.Description}
- Mastery: {stepMastery:F2}  Streak: {player.StreakInCurrentStep}/{step.StreakGoal}

STEP-SPECIFIC REQUIREMENTS:
{stepConstraints}

RECENT QUESTIONS:
{recentPerformance}

REQUIREMENTS:
- Write a clear question sentence ending with _____ for each blank.
- Each blank has a short label (e.g. ""answer"", ""x ="", ""y ="") and an exact correct answer.
- Correct answers must be single integers — no decimals.
- Do NOT repeat any recent question listed above.
{dragInstruction}

RETURN ONLY VALID JSON — no markdown, no extra text:

{{
  ""question"": ""<question text with _____ for each blank>"",
  ""blanks"": [
    {{""label"": ""<short label>"", ""correct"": ""<exact answer>""}}
  ],
  ""drag_options"": [],
  ""difficulty"": {step.MasteryTarget:F2},
  ""skillFocus"": ""{step.Description}""
}}";

        return prompt;
    }

    /// <summary>
    /// Parses the compact FIB JSON from Ollama into a FillInBlankQuestion.
    /// </summary>
    private IQuestion ParseFIBResponse(string jsonText)
    {
        try
        {
            int jsonStart = jsonText.IndexOf('{');
            int jsonEnd = jsonText.LastIndexOf('}');
            if (jsonStart >= 0 && jsonEnd > jsonStart)
                jsonText = jsonText.Substring(jsonStart, jsonEnd - jsonStart + 1);

            FillInBlankJSON data = JsonUtility.FromJson<FillInBlankJSON>(jsonText);

            if (data == null || string.IsNullOrEmpty(data.question) || data.blanks == null || data.blanks.Length == 0)
            {
                Debug.LogError("[QuestionGenerator] FIB: invalid JSON structure");
                return null;
            }

            var question = new FillInBlankQuestion
            {
                QuestionText = data.question,
                Difficulty = data.difficulty,
                SkillFocus = data.skillFocus,
                Blanks = new List<BlankField>(),
                DragOptions = new List<string>()
            };

            foreach (var b in data.blanks)
                question.Blanks.Add(new BlankField { Label = b.label, CorrectAnswer = b.correct });

            if (data.drag_options != null)
                foreach (var opt in data.drag_options)
                    question.DragOptions.Add(opt);

            return question;
        }
        catch (Exception e)
        {
            Debug.LogError($"[QuestionGenerator] FIB parse error: {e.Message}");
            Debug.LogError($"[QuestionGenerator] Raw JSON: {jsonText}");
            return null;
        }
    }

    /// <summary>Validates a FillInBlankQuestion before returning it.</summary>
    private bool ValidateFillInBlankQuestion(FillInBlankQuestion q, out string reason)
    {
        reason = null;

        if (string.IsNullOrWhiteSpace(q.QuestionText))
        {
            reason = "Empty question text";
            Debug.LogError($"[QuestionGenerator] FIB: {reason}");
            return false;
        }
        if (IsQuestionTextJustEquation(q.QuestionText))
        {
            reason = $"Question lacks proper phrasing: '{q.QuestionText}'";
            Debug.LogError($"[QuestionGenerator] FIB: {reason}");
            return false;
        }
        if (q.Blanks == null || q.Blanks.Count == 0)
        {
            reason = "No blanks defined";
            Debug.LogError($"[QuestionGenerator] FIB: {reason}");
            return false;
        }
        foreach (var b in q.Blanks)
        {
            if (string.IsNullOrWhiteSpace(b.CorrectAnswer))
            {
                reason = $"Blank '{b.Label}' has empty correct answer";
                Debug.LogError($"[QuestionGenerator] FIB: {reason}");
                return false;
            }
        }
        return true;
    }

    private bool ValidateFillInBlankQuestion(FillInBlankQuestion q) => ValidateFillInBlankQuestion(q, out _);

    [System.Serializable]
    private class FillInBlankJSON
    {
        public string question;
        public BlankJSON[] blanks;
        public string[] drag_options;
        public float difficulty;
        public string skillFocus;
    }

    [System.Serializable]
    private class BlankJSON
    {
        public string label;
        public string correct;
    }
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
                    1 => "- Use subtraction only\n- Numbers stay within 0–10\n- Result must be 0 or greater\n- Example: 'What is 8 minus 3?'",
                    2 => "- Ask for the missing addend in an addition fact\n- Phrase as a full question, e.g. 'What number completes 3 + _____ = 8?'\n- The question MUST start with 'What' or 'Find'\n- Correct answer must be a whole number from 0 to 10",
                    3 => "- Practise ALL subtraction facts within 0–10 for fluency\n- Vary the pairs freely across the full range (not just small numbers)\n- Example: 'What is 9 minus 4?' or 'Solve: 10 - 6 = _____'\n- Result must be 0 or greater",
                    4 => "- Subtract within 20; one of the numbers may cross the tens boundary (e.g. 16 - 7)\n- Result must be 0 or greater\n- Example: 'What is 16 minus 7?'",
                    5 => "- Subtract a single-digit number from a multiple of 10 (e.g. 30 - 7, 50 - 4)\n- Minuend must be a multiple of 10 between 10 and 90\n- Example: 'What is 40 minus 6?'",
                    6 => "- Use two-digit subtraction without borrowing\n- Ones digit of the minuend must be ≥ ones digit of the subtrahend\n- Example: 'What is 47 minus 23?'",
                    7 => "- Use two-digit subtraction that requires borrowing (regrouping)\n- At least one borrow must be required\n- Result must stay positive\n- Example: 'What is 52 minus 37?'",
                    _ => ""
                };

            case "multiplication":
            case "multiplication_i":  // legacy slug alias
                return stepNumber switch
                {
                    1  => "- Use multiplication as equal groups\n- Small whole numbers only\n- Formats like 3 groups of 4 or 3 × 4 are allowed",
                    2  => "- Use multiplication by 10 only\n- Other factor between 0 and 12",
                    3  => "- Use multiplication by 2 only\n- Other factor between 0 and 12",
                    4  => "- Use multiplication by 5 only\n- Other factor between 0 and 12",
                    5  => "- Use multiplication by 4 only\n- Other factor between 1 and 12",
                    6  => "- Use multiplication by 3 only\n- Other factor between 1 and 12",
                    7  => "- Use multiplication by 8 only\n- Other factor between 1 and 12",
                    8  => "- Use multiplication by 6 only\n- Other factor between 1 and 12",
                    9  => "- Use multiplication by 7 only\n- Other factor between 1 and 12",
                    10 => "- Use multiplication by 9 only\n- Other factor between 1 and 12",
                    11 => "- Use ANY multiplication fact from the 1–10 times tables\n- Mix factors freely",
                    _  => ""
                };

            case "multiplication_ii":   // retired slug — treated as late-range mult steps
                return stepNumber switch
                {
                    1 => "- Use multiplication by 3 only\n- Other factor between 1 and 12",
                    2 => "- Use multiplication by 4 only\n- Other factor between 1 and 12",
                    3 => "- Use multiplication by 6 only\n- Other factor between 1 and 12",
                    4 => "- Use multiplication by 7 only\n- Other factor between 1 and 12",
                    _ => ""
                };

            case "multiplication_iii":  // retired slug — treated as final mult steps
                return stepNumber switch
                {
                    1 => "- Use multiplication by 8 only\n- Other factor between 1 and 12",
                    2 => "- Use multiplication by 9 only\n- Other factor between 1 and 12",
                    3 => "- Use ANY multiplication fact from the 1–10 times tables\n- Mix factors freely",
                    _ => ""
                };

            case "division_i":
            case "division":  // legacy slug alias
                return stepNumber switch
                {
                    1 => "- Use equal sharing or grouping questions\n- Exact division only\n- Small whole numbers only",
                    2 => "- Use division by 2 only\n- Quotient must be a whole number",
                    3 => "- Use division by 5 only\n- Quotient must be a whole number",
                    4 => "- Use division by 10 only\n- Quotient must be a whole number",
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
    private IQuestion ParseJSONResponse(string jsonText, System.Text.StringBuilder debugLog = null)
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
                string msg = "Invalid JSON structure or missing 'question' field";
                debugLog?.AppendLine($"Parse error: {msg}");
                Debug.LogError($"[QuestionGenerator] {msg}");
                return null;
            }

            // Convert to MultipleChoiceQuestion
            var question = new MultipleChoiceQuestion
            {
                QuestionText = data.question,
                Options = new System.Collections.Generic.List<string>(data.options ?? new string[0]),
                CorrectAnswer = data.correctAnswer,
                Difficulty = data.difficulty,
                SkillFocus = data.skillFocus,
                Explanation = data.explanation,
                CommonMistakeExplanation = data.commonMistakeExplanation,
                EstimatedTimeSeconds = data.estimatedTimeSeconds
            };

            return question;
        }
        catch (Exception e)
        {
            string msg = $"JSON exception: {e.Message}";
            debugLog?.AppendLine($"Parse error: {msg}");
            Debug.LogError($"[QuestionGenerator] JSON Parse Error: {e.Message}");
            Debug.LogError($"[QuestionGenerator] Raw JSON: {jsonText}");
            return null;
        }
    }

    /// <summary>
    /// Strictly validates a question before returning it.
    /// Returns false if any validation fails and sets reason to explain why.
    /// </summary>
    private bool ValidateQuestion(MultipleChoiceQuestion question, out string reason)
    {
        reason = null;

        if (question.Options == null || question.Options.Count != 4)
        {
            reason = $"Expected 4 options, got {question.Options?.Count ?? 0}";
            Debug.LogError($"[QuestionGenerator] {reason}");
            return false;
        }

        if (!question.Options.Contains(question.CorrectAnswer))
        {
            reason = $"Correct answer '{question.CorrectAnswer}' not found in options [{string.Join(", ", question.Options)}]";
            Debug.LogError($"[QuestionGenerator] {reason}");
            return false;
        }

        var uniqueOptions = new System.Collections.Generic.HashSet<string>(question.Options);
        if (uniqueOptions.Count != 4)
        {
            reason = $"Duplicate options detected: [{string.Join(", ", question.Options)}]";
            Debug.LogError($"[QuestionGenerator] {reason}");
            return false;
        }

        foreach (var option in question.Options)
        {
            if (!IsNumericOnly(option))
            {
                reason = $"Option '{option}' is not numeric-only";
                Debug.LogError($"[QuestionGenerator] {reason}");
                return false;
            }
        }

        if (!int.TryParse(question.CorrectAnswer, out _))
        {
            reason = $"Correct answer '{question.CorrectAnswer}' is not a valid integer";
            Debug.LogError($"[QuestionGenerator] {reason}");
            return false;
        }

        if (IsQuestionTextJustEquation(question.QuestionText))
        {
            reason = $"Question text lacks proper phrasing: '{question.QuestionText}'";
            Debug.LogError($"[QuestionGenerator] {reason}");
            return false;
        }

        return true;
    }

    // Keep old overload for callers that don't need the reason
    private bool ValidateQuestion(MultipleChoiceQuestion question) => ValidateQuestion(question, out _);

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
    /// Returns a hardcoded fallback question relevant to the current step.
    /// Falls back to a generic addition question if the step is unknown.
    /// </summary>
    private IQuestion GetFallbackQuestion(Step step = null)
    {
        if (step == null)
            return MakeFallbackMC("What is 5 + 3?", "8", new[] { "8", "7", "9", "6" }, "Basic Addition");

        string slug = (step.Challenge ?? "")
            .Trim().ToLowerInvariant()
            .Replace(" ", "_").Replace("-", "_");

        // Use a deterministic but varied index based on questions already answered
        int variant = step.QuestionsCompleted % 3; // cycles through 3 variants

        return slug switch
        {
            // ── Multiplication ───────────────────────────────────────────────
            "multiplication" or "multiplication_i" => step.Number switch
            {
                1  => FallbackEqualGroups(variant),
                2  => FallbackMultBy(10, variant),
                3  => FallbackMultBy(2,  variant),
                4  => FallbackMultBy(5,  variant),
                5  => FallbackMultBy(4,  variant),
                6  => FallbackMultBy(3,  variant),
                7  => FallbackMultBy(8,  variant),
                8  => FallbackMultBy(6,  variant),
                9  => FallbackMultBy(7,  variant),
                10 => FallbackMultBy(9,  variant),
                11 => FallbackMixedMult(variant),
                _  => FallbackMultBy(2,  variant),
            },
            "multiplication_ii" => step.Number switch
            {
                1 => FallbackMultBy(3, variant), 2 => FallbackMultBy(4, variant),
                3 => FallbackMultBy(6, variant), _ => FallbackMultBy(7, variant),
            },
            "multiplication_iii" => step.Number switch
            {
                1 => FallbackMultBy(8, variant), 2 => FallbackMultBy(9, variant),
                _ => FallbackMixedMult(variant),
            },
            // ── Division ─────────────────────────────────────────────────────
            "division_i" or "division" => step.Number switch
            {
                1  => FallbackSharingEqually(variant),
                2  => FallbackDivBy(2,  variant),
                3  => FallbackDivBy(5,  variant),
                4  => FallbackDivBy(10, variant),
                _  => FallbackDivBy(2,  variant),
            },
            "division_ii" => step.Number switch
            {
                1 => FallbackDivBy(3, variant), 2 => FallbackDivBy(4, variant),
                3 => FallbackDivBy(6, variant), _ => FallbackDivBy(7, variant),
            },
            "division_iii" => step.Number switch
            {
                1 => FallbackDivBy(8, variant), 2 => FallbackDivBy(9, variant),
                _ => FallbackMixedDiv(variant),
            },
            // ── Addition ─────────────────────────────────────────────────────
            "addition" => step.Number switch
            {
                1 => MakeFallbackMC("What is 4 + 3?",  "7",  new[] { "7",  "6",  "8",  "5"  }, "Add Within 10"),
                2 => MakeFallbackMC("What is 6 + 4?",  "10", new[] { "10", "9",  "11", "8"  }, "Make 10"),
                3 => MakeFallbackMC("What is 23 + 14?","37", new[] { "37", "36", "38", "35" }, "Two-Digit No Carry"),
                _ => MakeFallbackMC("What is 27 + 15?","42", new[] { "42", "41", "43", "40" }, "Two-Digit With Carry"),
            },
            // ── Subtraction ──────────────────────────────────────────────────
            "subtraction" => step.Number switch
            {
                1 => MakeFallbackMC("What is 9 minus 4?",  "5",  new[] { "5",  "4",  "6",  "3"  }, "Subtract Within 10"),
                2 => MakeFallbackMC("What number completes 3 + _____ = 8?", "5", new[] { "5", "4", "6", "3" }, "Missing Addend"),
                3 => MakeFallbackMC("What is 10 minus 6?", "4",  new[] { "4",  "3",  "5",  "6"  }, "Subtraction Fluency"),
                4 => MakeFallbackMC("What is 14 minus 7?", "7",  new[] { "7",  "6",  "8",  "5"  }, "Subtract Within 20"),
                5 => MakeFallbackMC("What is 40 minus 6?", "34", new[] { "34", "33", "35", "36" }, "Subtract from Tens"),
                6 => MakeFallbackMC("What is 47 minus 23?","24", new[] { "24", "23", "25", "26" }, "Two-Digit No Borrow"),
                _ => MakeFallbackMC("What is 52 minus 37?","15", new[] { "15", "14", "16", "25" }, "Two-Digit With Borrow"),
            },
            _ => MakeFallbackMC("What is 5 + 3?", "8", new[] { "8", "7", "9", "6" }, step.Description ?? "Math"),
        };
    }

    // ── Fallback helpers ─────────────────────────────────────────────────────

    private static readonly int[][] MultFactors = {
        new[] { 3, 4, 7 }, new[] { 6, 8, 9 }, new[] { 2, 5, 11 }
    };

    private IQuestion FallbackMultBy(int multiplier, int variant)
    {
        int[] factors = MultFactors[variant % MultFactors.Length];
        int factor = factors[0];
        int correct = factor * multiplier;
        var distractors = Distractors(correct, 3);
        return MakeFallbackMC(
            $"What is {factor} × {multiplier}?",
            correct.ToString(),
            Shuffle(correct, distractors),
            $"Multiply by {multiplier}");
    }

    private IQuestion FallbackEqualGroups(int variant)
    {
        var (groups, size) = variant switch { 0 => (3, 4), 1 => (5, 2), _ => (4, 3) };
        int correct = groups * size;
        return MakeFallbackMC(
            $"There are {groups} groups of {size} objects. How many objects in total?",
            correct.ToString(),
            Shuffle(correct, Distractors(correct, 3)),
            "Equal Groups");
    }

    private IQuestion FallbackMixedMult(int variant)
    {
        var (a, b) = variant switch { 0 => (6, 7), 1 => (8, 4), _ => (9, 3) };
        int correct = a * b;
        return MakeFallbackMC($"What is {a} × {b}?", correct.ToString(),
            Shuffle(correct, Distractors(correct, 3)), "Mixed Times Tables");
    }

    private IQuestion FallbackSharingEqually(int variant)
    {
        var (total, groups) = variant switch { 0 => (12, 3), 1 => (20, 4), _ => (15, 5) };
        int correct = total / groups;
        return MakeFallbackMC(
            $"{total} apples are shared equally among {groups} children. How many does each child get?",
            correct.ToString(),
            Shuffle(correct, Distractors(correct, 3)),
            "Sharing Equally");
    }

    private IQuestion FallbackDivBy(int divisor, int variant)
    {
        int[] dividends = variant switch {
            0 => new[] { 3 * divisor, 5 * divisor, 7 * divisor },
            1 => new[] { 4 * divisor, 6 * divisor, 8 * divisor },
            _ => new[] { 2 * divisor, 9 * divisor, 11 * divisor },
        };
        int dividend = dividends[0];
        int correct = dividend / divisor;
        return MakeFallbackMC(
            $"What is {dividend} ÷ {divisor}?",
            correct.ToString(),
            Shuffle(correct, Distractors(correct, 3)),
            $"Divide by {divisor}");
    }

    private IQuestion FallbackMixedDiv(int variant)
    {
        var (a, b) = variant switch { 0 => (56, 7), 1 => (48, 6), _ => (81, 9) };
        int correct = a / b;
        return MakeFallbackMC($"What is {a} ÷ {b}?", correct.ToString(),
            Shuffle(correct, Distractors(correct, 3)), "Mixed Division");
    }

    private static MultipleChoiceQuestion MakeFallbackMC(string q, string correct, string[] options, string skill)
    {
        return new MultipleChoiceQuestion
        {
            QuestionText = q,
            CorrectAnswer = correct,
            Options = new System.Collections.Generic.List<string>(options),
            Difficulty = 0.3f,
            SkillFocus = skill,
            Explanation = $"The answer is {correct}.",
            CommonMistakeExplanation = "Check your calculation carefully.",
            EstimatedTimeSeconds = 20
        };
    }

    /// <summary>Generates N distractor integers near the correct answer (no duplicates, no negatives).</summary>
    private static int[] Distractors(int correct, int count)
    {
        var result = new System.Collections.Generic.List<int>();
        int[] offsets = { 1, -1, 2, -2, 5, -5, 3, -3, 10, -10 };
        foreach (int offset in offsets)
        {
            int candidate = correct + offset;
            if (candidate > 0 && candidate != correct && !result.Contains(candidate))
                result.Add(candidate);
            if (result.Count >= count) break;
        }
        while (result.Count < count) result.Add(correct + result.Count + 10);
        return result.ToArray();
    }

    /// <summary>Returns a shuffled 4-element array of [correct, d0, d1, d2].</summary>
    private static string[] Shuffle(int correct, int[] distractors)
    {
        var all = new[] { correct.ToString(), distractors[0].ToString(), distractors[1].ToString(), distractors[2].ToString() };
        // Fisher-Yates with fixed seed based on correct answer for determinism
        var rng = new System.Random(correct * 17 + 3);
        for (int i = all.Length - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (all[i], all[j]) = (all[j], all[i]);
        }
        return all;
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
