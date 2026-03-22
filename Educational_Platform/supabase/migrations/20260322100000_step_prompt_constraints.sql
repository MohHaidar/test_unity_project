-- ============================================================
--  Migration: Add prompt_constraints to steps table
--
--  Purpose: Store Ollama generation rules per step in the DB.
--  When populated, the game uses these directly without requiring
--  any code changes in OllamaQuestionGenerator.
--
--  After this migration, new challenges + steps can be added
--  entirely through the DB (no C# code changes needed).
-- ============================================================

ALTER TABLE steps ADD COLUMN IF NOT EXISTS prompt_constraints TEXT NOT NULL DEFAULT '';

-- ── Addition ─────────────────────────────────────────────────
UPDATE steps SET prompt_constraints = '- Use addition only\n- Both addends: 0-10\n- Total: at most 10\n- No story problems needed'
  WHERE id = 'c1000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use addition facts that make 10\n- One addend should be between 1 and 9\n- Ask for the missing partner or direct sum that completes 10'
  WHERE id = 'c2000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use two-digit addition without carrying\n- Each ones-place sum must stay below 10\n- Keep totals below 100'
  WHERE id = 'c3000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use two-digit addition with carrying\n- At least one ones-place sum must be 10 or more\n- Keep totals below 150'
  WHERE id = 'c4000000-0000-0000-0000-000000000000';

-- ── Subtraction ───────────────────────────────────────────────
UPDATE steps SET prompt_constraints = '- Use subtraction only\n- Numbers stay within 0-10\n- Result must be 0 or greater\n- Example: ''What is 8 minus 3?'''
  WHERE id = 'c5000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Ask for the missing addend in an addition fact\n- Phrase as a full question, e.g. ''What number completes 3 + _____ = 8?''\n- The question MUST start with ''What'' or ''Find''\n- Correct answer must be a whole number from 0 to 10'
  WHERE id = 'c6000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Practise ALL subtraction facts within 0-10 for fluency\n- Vary the pairs freely across the full range (not just small numbers)\n- Example: ''What is 9 minus 4?'' or ''Solve: 10 - 6 = _____''\n- Result must be 0 or greater'
  WHERE id = 'fc000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Subtract within 20; one of the numbers may cross the tens boundary (e.g. 16 - 7)\n- Result must be 0 or greater\n- Example: ''What is 16 minus 7?'''
  WHERE id = 'fd000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Subtract a single digit from a multiple of ten (e.g. 30 - 6)\n- Result must be 0 or greater'
  WHERE id = 'c9000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use two-digit subtraction without borrowing\n- Ones digit of minuend must be >= ones digit of subtrahend\n- Keep numbers below 100'
  WHERE id = 'ca000000-0000-0000-0000-000000000000';

-- ── Multiplication — Equal Groups (steps 1–5) ─────────────────
UPDATE steps SET prompt_constraints = '- Show EXACTLY 2 equal groups of objects\n- 2 to 6 items per group\n- INCLUDE a visual in the question text using this format: "[● ● ●]  [● ● ●]" where each ● is one item\n- Ask: "How many items are there in total?"\n- Correct answer = groups x items'
  WHERE id = 'cb000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Show EXACTLY 3 equal groups of objects\n- 2 to 5 items per group\n- INCLUDE a visual: "[● ●]  [● ●]  [● ●]"\n- Ask: "How many items are there in total?"\n- Correct answer = groups x items'
  WHERE id = '10000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Show EXACTLY 4 equal groups of objects\n- 2 to 4 items per group\n- INCLUDE a visual showing 4 bracket-groups\n- Ask: "How many items are there in total?"\n- Correct answer = groups x items'
  WHERE id = '11000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Show EXACTLY 5 equal groups of objects\n- 2 to 4 items per group\n- INCLUDE a visual showing 5 bracket-groups\n- Ask: "How many items are there in total?"\n- Correct answer = groups x items'
  WHERE id = '12000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Show 2 to 5 equal groups (vary between questions)\n- 2 to 5 items per group\n- INCLUDE a visual showing the groups\n- May also show a rectangular array (rows x columns) as an alternative\n- Ask: "How many items are there in total?" or "What multiplication fact does this show?"'
  WHERE id = '13000000-0000-0000-0000-000000000000';

-- ── Multiplication — Times tables (steps 6–15) ───────────────
UPDATE steps SET prompt_constraints = '- Use multiplication by 10 only\n- Other factor between 0 and 12'
  WHERE id = 'cc000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use multiplication by 2 only\n- Other factor between 0 and 12'
  WHERE id = 'cd000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use multiplication by 5 only\n- Other factor between 0 and 12'
  WHERE id = 'ce000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use multiplication by 4 only\n- Other factor between 1 and 12'
  WHERE id = 'ea000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use multiplication by 3 only\n- Other factor between 1 and 12'
  WHERE id = 'eb000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use multiplication by 8 only\n- Other factor between 1 and 12'
  WHERE id = 'ec000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use multiplication by 6 only\n- Other factor between 1 and 12'
  WHERE id = 'ed000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use multiplication by 7 only\n- Other factor between 1 and 12'
  WHERE id = 'ee000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use multiplication by 9 only\n- Other factor between 1 and 12'
  WHERE id = 'ef000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use ANY multiplication fact from the 1-10 times tables\n- Mix factors freely'
  WHERE id = 'f0000000-0000-0000-0000-000000000000';

-- ── Division (steps 1–11) ─────────────────────────────────────
UPDATE steps SET prompt_constraints = '- Use equal sharing or grouping questions\n- Exact division only\n- Small whole numbers only'
  WHERE id = 'cf000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 10 only\n- Quotient must be a whole number'
  WHERE id = 'd3000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 2 only\n- Quotient must be a whole number'
  WHERE id = 'd1000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 5 only\n- Quotient must be a whole number'
  WHERE id = 'd2000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 4 only\n- Quotient must be a whole number between 1 and 12'
  WHERE id = 'f2000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 3 only\n- Quotient must be a whole number between 1 and 12'
  WHERE id = 'f1000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 8 only\n- Quotient must be a whole number between 1 and 12'
  WHERE id = 'f5000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 6 only\n- Quotient must be a whole number between 1 and 12'
  WHERE id = 'f3000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 7 only\n- Quotient must be a whole number between 1 and 12'
  WHERE id = 'f4000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use division by 9 only\n- Quotient must be a whole number between 1 and 12'
  WHERE id = 'f6000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use ANY exact division fact (divisors 1-10)\n- Mix divisors freely'
  WHERE id = 'f7000000-0000-0000-0000-000000000000';

-- ── Order of Operations ──────────────────────────────────────
UPDATE steps SET prompt_constraints = '- Expression must have exactly one multiplication and one addition\n- Multiplication MUST be evaluated first\n- Example: ''What is 3 × 4 + 2?'''
  WHERE id = 'd4000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Expression must have exactly one multiplication and one subtraction\n- Multiplication MUST be evaluated first\n- Example: ''What is 5 × 2 - 3?'''
  WHERE id = 'd5000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Expression must use parentheses; contents of parentheses evaluated first\n- Example: ''What is (3 + 2) × 4?'''
  WHERE id = 'd6000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Mix all operation types (+, -, x, /) with or without parentheses\n- Keep numbers under 100\n- Always exactly one correct numeric answer'
  WHERE id = 'd7000000-0000-0000-0000-000000000000';

-- ── Arithmetic Review ─────────────────────────────────────────
UPDATE steps SET prompt_constraints = '- Mix addition and subtraction freely\n- Use two-digit numbers\n- Vary the difficulty'
  WHERE id = 'f8000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Mix multiplication and division freely\n- Use single-digit multipliers/divisors\n- Exact division only'
  WHERE id = 'f9000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Use all four operations (+, -, x, /)\n- May include one pair of parentheses\n- Keep results as whole numbers'
  WHERE id = 'fa000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Multi-step mental math across all four operations\n- 2-3 operations per expression\n- Keep numbers manageable for mental calculation'
  WHERE id = 'fb000000-0000-0000-0000-000000000000';

-- ── Expressions with Variables ────────────────────────────────
UPDATE steps SET prompt_constraints = '- Give x a specific value, ask to evaluate x + a\n- x and a are single-digit whole numbers\n- State x''s value clearly in the question'
  WHERE id = 'd8000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Give x a specific value, ask to evaluate x - a\n- Result must be >= 0\n- State x''s value clearly in the question'
  WHERE id = 'd9000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Give x a specific value, ask to evaluate a*x (coefficient times variable)\n- x and a are single-digit whole numbers\n- State x''s value clearly in the question'
  WHERE id = 'da000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Give x a specific value, ask to evaluate x / a exactly\n- x must be exactly divisible by a\n- State x''s value clearly in the question'
  WHERE id = 'db000000-0000-0000-0000-000000000000';

-- ── One-Step Equations ────────────────────────────────────────
UPDATE steps SET prompt_constraints = '- Equation form: x + a = b\n- Solve for x\n- All values whole numbers, x > 0'
  WHERE id = 'dc000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Equation form: x - a = b\n- Solve for x\n- All values whole numbers, x > 0'
  WHERE id = 'dd000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Equation form: a*x = b\n- Solve for x\n- x must be a whole number'
  WHERE id = 'de000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Equation form: x / a = b\n- Solve for x\n- x = a * b must be a whole number'
  WHERE id = 'df000000-0000-0000-0000-000000000000';

-- ── Two-Step Equations ────────────────────────────────────────
UPDATE steps SET prompt_constraints = '- Equation form: a*x + b = c\n- Solve for x\n- x must be a positive whole number'
  WHERE id = 'e1000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Equation form: a*x - b = c\n- Solve for x\n- x must be a positive whole number'
  WHERE id = 'e2000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Equation form: x/a + b = c\n- Solve for x\n- x = a*(c-b) must be a positive whole number'
  WHERE id = 'e3000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Equation form: x/a - b = c\n- Solve for x\n- x = a*(c+b) must be a positive whole number'
  WHERE id = 'e4000000-0000-0000-0000-000000000000';

-- ── Systems of Equations ──────────────────────────────────────
UPDATE steps SET prompt_constraints = '- Provide a value for x, ask player to evaluate y = x + a using that x\n- Both x and a are single-digit whole numbers'
  WHERE id = 'e5000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Simple 2-equation system solvable by substitution\n- Ask for the value of x\n- All values whole numbers'
  WHERE id = 'e6000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Simple 2-equation system solvable by substitution\n- Ask for the value of y\n- All values whole numbers'
  WHERE id = 'e7000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Two-variable system in standard form (ax + by = c)\n- Solvable with small whole-number substitution\n- Ask for the value of x'
  WHERE id = 'e8000000-0000-0000-0000-000000000000';
UPDATE steps SET prompt_constraints = '- Two-variable system in standard form (ax + by = c)\n- Solvable with small whole-number substitution\n- Ask for the value of y'
  WHERE id = 'e9000000-0000-0000-0000-000000000000';
