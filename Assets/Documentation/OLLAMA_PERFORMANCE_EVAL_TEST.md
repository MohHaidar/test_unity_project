# Ollama Performance Evaluation & Adaptive Context Test

Testing how Ollama can evaluate student answers and adapt the next question based on measurable metrics.

---

## 🎯 Test Goal

Instead of just generating questions, we want to test Ollama's ability to:
1. ✅ Evaluate if a student answered correctly
2. ✅ Calculate performance metrics (accuracy, time taken, difficulty mastery)
3. ✅ Suggest next difficulty level based on performance
4. ✅ Update student context/profile for better personalization
5. ✅ Explain *why* the student struggled (conceptual gap vs careless mistake)

---

## 📊 Test Flow

```
1. Generate Question (what we tested before)
   ↓
2. Student Answers Question (simulated)
   ↓
3. [NEW] Evaluate Answer + Generate Metrics ← Testing this
   ↓
4. [NEW] Analyze Performance & Suggest Next Difficulty ← Testing this
   ↓
5. [NEW] Update Student Profile/Context ← Testing this
   ↓
6. Generate Next Question with Updated Context (with new metrics)
```

---

## 📋 Test 1: Answer Evaluation & Metrics

### Scenario
Student Alex just answered this question:

**Question:** "What is 27 + 15?"  
**Correct Answer:** "42"  
**Student Answer:** "37"  
**Time Taken:** 18 seconds  
**Question Difficulty:** 0.60 (medium)

### Prompt for Ollama

Copy this into your Ollama interface:

```
You are an AI tutor analyzing a student's performance.

QUESTION DETAILS:
- Question: "What is 27 + 15?"
- Correct Answer: "42"
- Student Answer: "37"
- Time Taken: 18 seconds
- Question Difficulty: 0.60 (scale 0-1, where 0.5 = grade level, 1.0 = advanced)
- Max Expected Time: 30 seconds

STUDENT CONTEXT:
- Name: Alex
- Grade Level: 2nd Grade
- Topic: 2-Digit Addition with Carrying
- Recent Accuracy: 90% (9 out of 10 previous questions correct)
- Average Time per Question: 25 seconds
- Mastery Level: 0.75 (75% - nearly mastered)
- Recent Struggles: Forgetting to carry the 1 to the tens place

TASK:
1. Determine if the answer is CORRECT or INCORRECT
2. Identify the TYPE OF ERROR (if wrong):
   - Careless Mistake: Right concept, wrong arithmetic
   - Conceptual Gap: Doesn't understand the concept
   - Timing Issue: Rushed or didn't have enough time
3. Calculate these metrics:
   - Correctness: 1 if right, 0 if wrong
   - Speed Score: 1.0 if time <= expected, 0.5 if +50%, 0.0 if rushed
   - Confidence in Answer Quality: Does this match their skill level?
4. Suggest the next difficulty level:
   - Same difficulty: Keep current level
   - Decrease: Student struggled, needs easier questions
   - Increase: Student mastered this level
5. Recommend ONE specific focus area for the next question

RETURN ONLY JSON (no other text):

{
  "isCorrect": false,
  "studentAnswer": "37",
  "correctAnswer": "42",
  "errorType": "conceptual_gap or careless_mistake",
  "errorExplanation": "Why did they get it wrong?",
  "speedScore": 0.7,
  "speedAnalysis": "Did they rush? Was timing appropriate?",
  "confidenceInPerformance": 0.8,
  "performanceAssessment": "Overall, is this consistent with their skill level?",
  "nextDifficultyRecommendation": "same|increase|decrease",
  "nextDifficultyValue": 0.60,
  "reasonForDifficultyAdjustment": "Why this difficulty?",
  "nextFocusArea": "What concept should the next question target?",
  "updatedMasteryLevel": 0.70,
  "masteryChangeExplanation": "How did this answer affect their mastery?",
  "conceptualGapIdentified": "Did you identify a specific learning gap?",
  "recommendedInterventionBeforeNextQuestion": "Should we do something before the next question (explanation, hint, similar problem)?"
}
```

### Expected Output

A JSON response that shows Ollama can:
- ✅ Recognize the error is a **conceptual gap** (forgetting to carry)
- ✅ Calculate speed appropriately (18 sec is reasonable)
- ✅ Suggest **staying at same difficulty** (they'll get it with the right concept)
- ✅ Update mastery down slightly (0.75 → 0.70)
- ✅ Recommend **"carrying" focus** for next question

### What Good Output Looks Like:

```json
{
  "isCorrect": false,
  "studentAnswer": "37",
  "correctAnswer": "42",
  "errorType": "conceptual_gap",
  "errorExplanation": "Student did not carry the 1 from 8+7=15 in the ones place. They added 2+1=3 without adding the carried 1, getting 37 instead of 42. This is a classic carrying mistake.",
  "speedScore": 0.85,
  "speedAnalysis": "18 seconds is good - they took time to think, not rushing. They had 30 seconds available.",
  "confidenceInPerformance": 0.85,
  "performanceAssessment": "Consistent with a student who has 75% mastery - they understand addition but sometimes forget the carry step.",
  "nextDifficultyRecommendation": "same",
  "nextDifficultyValue": 0.60,
  "reasonForDifficultyAdjustment": "Stay at same difficulty but reinforce the carrying concept. Once they master carrying, they'll excel at this level.",
  "nextFocusArea": "2-digit addition WITH carrying (reinforce the carry step)",
  "updatedMasteryLevel": 0.72,
  "masteryChangeExplanation": "One wrong answer decreased mastery slightly (0.75 → 0.72), but since they got 9/10 before, we have high confidence they understand this with a small gap.",
  "conceptualGapIdentified": "Forgetting to carry the 1 from ones to tens place when sum exceeds 9",
  "recommendedInterventionBeforeNextQuestion": "Yes - show a worked example: 8+7=15, so write 5 and carry 1. Then 2+1+1=4. Answer 45. Then ask similar question."
}
```

---

## 📊 Test 2: Scenario-Based Evaluation (Multiple Wrong Answers)

### Scenario A: Fast but Wrong (Student Rushed)

```
Question: 27 + 15 = ?
Correct Answer: 42
Student Answer: 35 (wrong)
Time Taken: 8 seconds
Expected Time: 30 seconds
Previous Accuracy: 90%
Mastery Level: 0.75

ANALYZE THIS WITH OLLAMA:
(Use same prompt as Test 1 above, but with these values)
```

**Expected insight:** "Rushed answer - speed score should be low, difficulty should decrease"

### Scenario B: Slow and Wrong (Struggling)

```
Question: 27 + 15 = ?
Correct Answer: 42
Student Answer: 38 (wrong)
Time Taken: 45 seconds (took 50% longer than expected)
Expected Time: 30 seconds
Previous Accuracy: 65%
Mastery Level: 0.45

ANALYZE THIS WITH OLLAMA
```

**Expected insight:** "Struggling with concept - took too long AND got it wrong. Difficulty should decrease."

### Scenario C: Fast and Correct (Mastering)

```
Question: 27 + 15 = ?
Correct Answer: 42
Student Answer: 42 (correct!)
Time Taken: 12 seconds (faster than expected 30)
Expected Time: 30 seconds
Previous Accuracy: 95%
Mastery Level: 0.85

ANALYZE THIS WITH OLLAMA
```

**Expected insight:** "Mastering this level - consider increasing difficulty next time"

---

## 🔄 Test 3: Full Adaptive Loop

### Part A: Evaluate Previous Answer

Use Test 1 prompt to evaluate: "What is 27 + 15?" → Answer "37" → Get metrics

### Part B: Generate Next Question with Updated Context

Once you have the metrics from Part A, use this prompt:

```
You are an adaptive math tutor. Based on the student's last answer evaluation, generate the next question.

STUDENT PROFILE (UPDATED FROM PREVIOUS ANSWER):
- Name: Alex
- Grade Level: 2nd Grade
- Previous Question Result: INCORRECT (27+15, answered 37 instead of 42)
- Previous Error Type: Conceptual gap (forgot to carry)
- Identified Learning Gap: Forgetting to carry 1 from ones to tens place
- Current Mastery Level: 0.72 (slightly decreased from 0.75)
- Overall Accuracy: 89% (9/10, then got this wrong)
- Average Speed: 25 seconds
- Recommended Next Difficulty: SAME (0.60)
- Recommended Focus: 2-digit addition WITH carrying

TASK:
Generate 1 question that:
1. Addresses the identified gap (carrying in addition)
2. Is at the SAME difficulty level (0.60)
3. Is visually/pedagogically different from the previous question
4. Includes 4 multiple choice options
5. Uses similar numbers but structured to reinforce the carry concept

RETURN ONLY JSON:

{
  "question": "...",
  "options": ["...", "...", "...", "..."],
  "correctAnswer": "...",
  "difficulty": 0.60,
  "targetedConcept": "2-digit addition with carrying",
  "explanation": "...",
  "commonMistakeExplanation": "...",
  "pedagogicalReasoning": "Why is this question good for Alex right now?",
  "estimatedTimeSeconds": 30
}
```

**Expected:** Ollama generates a question specifically designed to address the "forgetting to carry" gap. Example: "19 + 13 = ?" (so 9+3=12, need to carry)

---

## 🎮 Test 4: Tracking Performance Over Multiple Questions

### How to Test This

1. **Answer 1:** Evaluate "27 + 15" → "37" (wrong, conceptual gap)
   - Metrics: Accuracy 0/1, Mastery 0.72, Focus: carrying

2. **Question 2:** Generate with updated context
   - Should reinforce carrying

3. **Answer 2:** Simulate answering it correctly this time
   - Metrics: Accuracy 1/1, Mastery increases to 0.76

4. **Question 3:** Generate again with NEW updated context
   - Should now suggest increasing difficulty slightly

**Show Ollama this progression:**

```
STUDENT PERFORMANCE HISTORY (Last 3 Questions):
1. 27 + 15 = ? → Answered 37 (WRONG - forgot carry) - Mastery: 0.75 → 0.72
2. 19 + 13 = ? → Answered 32 (CORRECT - remembered carry!) - Mastery: 0.72 → 0.76
3. Generate next question with this updated context

What should difficulty be for Question 3?
Should we increase, stay, or decrease?
Why?
```

**Expected:** "Since they got the carrying concept right on the second try, increase difficulty slightly to 0.65"

---

## 📈 Key Metrics to Track

For each question answered, Ollama should consider:

| Metric | Range | What It Means |
|--------|-------|---------------|
| **Correctness** | 0 or 1 | Did they get it right? |
| **Speed Score** | 0.0-1.0 | Did they answer in reasonable time? |
| **Mastery Level** | 0.0-1.0 | Overall understanding of concept |
| **Accuracy Trend** | % | Last 5-10 questions correct |
| **Error Type** | conceptual_gap, careless, timing | What kind of mistake? |
| **Confidence** | 0.0-1.0 | Is performance consistent with ability? |
| **Next Difficulty** | decrease, same, increase | Should we go harder/easier? |
| **Learning Gap** | string | What specific concept needs work? |

---

## 🧪 Testing Checklist

- [ ] Test 1: Evaluate single answer → Get metrics JSON
- [ ] Test 2: Evaluate three scenarios (rushed, struggling, mastering)
- [ ] Test 3 Part A: Evaluate previous answer → Get updated metrics
- [ ] Test 3 Part B: Generate next question with updated context
- [ ] Test 3 Full: Run both parts, verify context carries over correctly
- [ ] Test 4: Simulate 3-question sequence, track mastery changes
- [ ] Verify Ollama can identify different error types
- [ ] Verify Ollama adjusts difficulty appropriately
- [ ] Verify JSON output is always valid
- [ ] Compare speed with ChatGPT (if you want to test GPT's eval capabilities too)

---

## 💡 What This Tells Us

After testing, we'll know:

✅ **Can Ollama evaluate answers accurately?**  
✅ **Can it identify conceptual gaps?**  
✅ **Can it calculate appropriate metrics?**  
✅ **Can it suggest difficulty adjustments?**  
✅ **Can it generate adaptive questions based on performance?**  

If YES to all → **Ollama is ready for integration!**  
If NO to some → **We need GPT for evaluation, Ollama for generation** (hybrid approach)

---

## 🔗 Integration Plan (After Testing)

Once you confirm this works, we'll:

1. Create `IPerformanceEvaluator` interface
2. Create `OllamaPerformanceEvaluator` class
3. Store metrics in CSV/database: `player_performance.csv`
   - Columns: player_id, question_id, answer, is_correct, time_taken, mastery_before, mastery_after, error_type
4. On each question submission:
   - Call Ollama to evaluate & get metrics
   - Update player context
   - Call Ollama to generate next question with new context
5. Track mastery trends over time

---

## 🚀 Start Testing Now

Pick one test (I'd suggest Test 1 first):

1. Copy the prompt
2. Paste into Ollama interface
3. Check if output is valid JSON
4. See if Ollama correctly:
   - Identified the error type (carrying mistake)
   - Suggested same difficulty
   - Updated mastery appropriately
   - Recommended focusing on carrying

Post your results and we'll move to Test 2!

---

## 📝 Notes

- Temperature: Keep at 0.3 for consistency (deterministic evaluation)
- Model: Use same Mistral that worked for generation
- Expected response time: 5-10 seconds
- Focus on JSON validity first, then accuracy of recommendations
