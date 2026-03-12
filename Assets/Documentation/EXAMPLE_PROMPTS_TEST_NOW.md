# Ready-to-Test Example Prompts

## 🚀 Copy & Paste These Right Now

---

## Example 1: ChatGPT (Simplest - Just Copy & Paste)

### Go to: https://chat.openai.com/

Paste this entire message:

```
You are an expert math teacher creating a personalized question for a student.

STUDENT PROFILE:
- Name: Alex
- Current Skill Level: Basic (can do 1-digit addition)
- Recent Performance: 90% accuracy
- Recently Solved: 5+3=8, 7+4=11 (both correct)
- Learning Style: Prefers visual/simple explanations

TARGET QUESTION:
Create 1 multiple choice addition question that is slightly harder than what they just solved.
The question should help them build confidence while introducing a small challenge.

REQUIREMENTS:
- Question type: Multiple choice with 4 options
- Difficulty: Medium-Easy (just a step harder than 5+3)
- Include one common mistake as a wrong answer
- Include two other plausible but wrong answers
- Make the correct answer obvious to a smart 1st grader

RETURN FORMAT (STRICT JSON - NOTHING ELSE):

{
  "question": "What is 8 + 6?",
  "options": ["12", "13", "14", "15"],
  "correctAnswer": "14",
  "explanation": "When we add 8 + 6, we can count: 8... 9, 10, 11, 12, 13, 14. That's 6 more. So 8 + 6 = 14",
  "commonMistakeExplanation": "Some students might answer 13 if they only count 5 more instead of 6 more",
  "difficulty": 0.55,
  "skillFocus": "adding numbers with sums 10-20",
  "estimatedTimeSeconds": 25
}
```

**Expected result:** ChatGPT returns a JSON object with a math question

---

## Example 2: Ollama via cURL (Simplest Terminal Test)

**Prerequisites:**
```bash
# Start Ollama in one terminal
ollama serve

# Then in another terminal, run this:
```

**Copy & Paste This Command:**

```bash
curl -X POST http://localhost:11434/api/generate -H "Content-Type: application/json" -d '{
    "model": "mistral",
    "prompt": "You are a math teacher. Create a multiple choice addition question for a student named Alex who recently solved 5+3=8 and 7+4=11 correctly (90% accuracy). Make a question slightly harder, like 8+6 or 9+5. Include 4 options with 1 correct answer and 3 plausible wrong answers. Include a common mistake as one wrong answer. Return ONLY valid JSON with these exact fields: question, options (array of 4 strings), correctAnswer, explanation, commonMistakeExplanation, difficulty (0.0-1.0 scale), skillFocus, estimatedTimeSeconds. No other text.",
    "stream": false,
    "temperature": 0.3
  }' | jq '.response'
```

**Expected output:**
```
{
  "question": "What is 8 + 6?",
  "options": ["12", "13", "14", "15"],
  ...
}
```

---

## Example 3: Two-Digit Addition (Medium Difficulty)

### For ChatGPT:

```
STUDENT PROFILE:
- Name: Jordan
- Skill Level: Intermediate (can do 2-digit addition without carrying)
- Recent Performance: 72% accuracy
- Recently Got Wrong: 27 + 15 (answered 37, forgot to carry)
- Recently Got Right: 23 + 14 = 37, 45 + 23 = 68
- Learning Style: Visual (likes seeing step-by-step)

TARGET QUESTION:
Create a 2-digit addition problem that requires CARRYING from the ones to tens place.
This will help them master the concept they just struggled with.

REQUIREMENTS:
- Both numbers should be 2-digit (10-99)
- The ones place should add to 10 or more (forcing a carry)
- Difficulty: 0.6 (slightly challenging but doable)
- Include distractor showing result of forgetting to carry
- Include distractor off by one
- Include step-by-step explanation

RETURN ONLY JSON:

{
  "question": "What is 38 + 16?",
  "options": ["54", "44", "55", "50"],
  "correctAnswer": "54",
  "explanation": "Step 1: Add ones: 8 + 6 = 14. Step 2: Write 4, carry the 1 to tens. Step 3: Add tens: 3 + 1 + 1 (carried) = 5. Answer: 54",
  "commonMistakeExplanation": "A common mistake is forgetting to carry the 1, which gives 44",
  "difficulty": 0.60,
  "skillFocus": "two-digit addition with carrying/regrouping",
  "solutionSteps": [
    "Add the ones place: 8 + 6 = 14",
    "Since 14 > 10, write down 4 and carry 1",
    "Add the tens place: 3 + 1 (carried) + 1 = 5",
    "Write 5 in the tens place. Answer: 54"
  ],
  "estimatedTimeSeconds": 35
}
```

### For Ollama:

```bash
curl -X POST http://localhost:11434/api/generate \
  -H "Content-Type: application/json" \
  -d '{
    "model": "mistral",
    "prompt": "You are a math teacher. Student Jordan recently got 27+15 wrong (forgot to carry). Create a 2-digit addition question that requires carrying from ones to tens place. Similar difficulty to 27+15. Make it help them learn carrying. Include 4 options: 1 correct, 1 showing result of forgetting to carry, 2 other plausible wrong answers. Return ONLY JSON: {question, options (4 items), correctAnswer, explanation, commonMistakeExplanation, difficulty (0-1), skillFocus, solutionSteps (array), estimatedTimeSeconds}. No other text.",
    "stream": false,
    "temperature": 0.3
  }' | jq '.response'
```

---

## 🧪 Testing Steps

### Step 1: Test Simple Addition (1 minute)

**ChatGPT:**
1. Go to https://chat.openai.com/
2. Paste Example 1 above
3. See what you get

**Ollama:**
1. Run Example 2 cURL command
2. See what you get

### Step 2: Compare Outputs

**Checklist:**
- [ ] Is the output valid JSON? (paste into https://jsonlint.com/)
- [ ] Does it have a clear question?
- [ ] Are there 4 options?
- [ ] Is exactly 1 marked as correct?
- [ ] Is the explanation clear?
- [ ] Is difficulty between 0.3 and 0.9?
- [ ] Does it make pedagogical sense?

### Step 3: Test Two-Digit Addition (1 minute)

**ChatGPT:** Paste Example 3  
**Ollama:** Use Example 3 cURL

### Step 4: Compare Quality

**Fill this in:**

```
SIMPLE ADDITION:
ChatGPT - Time: ___ sec, Quality: ___/10, JSON Valid: Yes/No
Ollama  - Time: ___ sec, Quality: ___/10, JSON Valid: Yes/No

TWO-DIGIT ADDITION:
ChatGPT - Time: ___ sec, Quality: ___/10, JSON Valid: Yes/No
Ollama  - Time: ___ sec, Quality: ___/10, JSON Valid: Yes/No

WINNER: _____ (faster/better/cheaper)
```

---

## 📊 What Good Output Looks Like

### ✅ Excellent Output (GPT-4)
```json
{
  "question": "What is 38 + 16?",
  "options": ["54", "44", "55", "50"],
  "correctAnswer": "54",
  "explanation": "We add 38 + 16. Starting with the ones place: 8 + 6 = 14. We write down 4 and carry the 1 to the tens place. Then we add the tens: 3 + 1 (carried) + 1 = 5. So our answer is 54. A common mistake is forgetting to carry the 1 from the ones place, which would give us 44.",
  "commonMistakeExplanation": "Forgetting to carry the 1 from 8+6=14 leads to 44",
  "difficulty": 0.60,
  "skillFocus": "two-digit addition with carrying",
  "solutionSteps": [
    "Add ones: 8 + 6 = 14",
    "Write 4, carry 1 to tens",
    "Add tens: 3 + 1 (carried) + 1 = 5",
    "Answer: 54"
  ],
  "estimatedTimeSeconds": 35
}
```
**Score: 10/10** - Perfect JSON, clear explanation, good pedagogy

### ✅ Good Output (Ollama/Mistral)
```json
{
  "question": "What is 27 + 15?",
  "options": ["42", "37", "45", "35"],
  "correctAnswer": "42",
  "explanation": "7 + 5 = 12, so we write 2 and carry 1. Then 2 + 1 + 1 = 4. Answer is 42",
  "difficulty": 0.58,
  "skillFocus": "carrying in addition",
  "estimatedTimeSeconds": 30
}
```
**Score: 7.5/10** - Valid JSON, correct answer, missing some details

### ❌ Bad Output
```json
{
  "question": "Calculate 27 + 15"
  "options": ["42", "37", "52"],  // Only 3 options, not 4
  "correctAnswer": "42"
  // Missing fields: explanation, difficulty, skillFocus
}
```
**Score: 3/10** - Invalid JSON, incomplete

---

## 🎯 What to Do With Results

### If Both Work Well
Use whichever is faster:
- **Ollama faster** → Use Ollama (free, instant)
- **ChatGPT faster** → Use ChatGPT API ($0.05/question)

### If Only One Works
Use that one, obviously!

### If Neither Works
- Check the error message
- Simplify the prompt
- Try different wording
- Check API keys/connection

---

## 💾 Save Your Best Outputs

When you get good results, save them:

```bash
# Create a folder for test results
mkdir test_results

# Save Ollama output
ollama_response.json

# Save ChatGPT output
chatgpt_response.json

# Later, you can analyze patterns
```

---

## ⚡ Even Faster: Pre-written Prompts

If you want prompts that are guaranteed to work, use these:

### Ollama (Pre-tested with Mistral)
```bash
curl -X POST http://localhost:11434/api/generate \
  -H "Content-Type: application/json" \
  -d '{
    "model": "mistral",
    "prompt": "Generate a simple math addition question. 8+6=? Include 4 multiple choice answers. Return JSON only.",
    "stream": false,
    "temperature": 0.3
  }' | jq '.response'
```

### ChatGPT (Simple)
```
Generate a simple addition question: 8+6=?
Create 4 multiple choice options.
Return only JSON with: question, options (4 items), correctAnswer, explanation
```

---

## 🚀 Quick Decision Tree

```
Do you have Ollama installed?
├─ YES → Run the cURL command (Example 2)
│  └─ Good? → Use Ollama
│  └─ Bad? → Try ChatGPT
│
└─ NO → Go to ChatGPT right now (Example 1)
   └─ Good? → Use ChatGPT API later
   └─ Want to try Ollama? → Install it (10 min setup)
```

---

## 📋 Troubleshooting

### "jq: parse error"
→ Response has text before JSON. Fix:
```bash
# Extract just the JSON part
curl ... | jq -r '.response' | grep -o '{.*}' | jq '.'
```

### "Connection refused"
→ Ollama not running
```bash
ollama serve
```

### "Invalid JSON"
→ Check https://jsonlint.com/ with the response

### "ChatGPT returned 500 error"
→ API overloaded, try again in 1 minute

---

## ✅ Success Checklist

After testing, you should have:
- [ ] At least 1 working ChatGPT prompt output
- [ ] At least 1 working Ollama prompt output
- [ ] Valid JSON from both (or at least one)
- [ ] Understanding of quality differences
- [ ] Decision on which backend to use

---

**Ready? Pick a prompt above and test right now! It takes literally 2 minutes.** 🚀

Post your results and I can help you troubleshoot or integrate into the game!
