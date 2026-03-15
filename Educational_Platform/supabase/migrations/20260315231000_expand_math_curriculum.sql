-- Migration 003: expand the seeded Math curriculum toward systems of equations
-- Keeps the catalog aligned with the hardcoded fallback IDs in ChallengeDataManager.

-- Update existing challenge copy for a cleaner curriculum arc.
UPDATE challenges
SET description = 'Build fluency with sums from within 10 through two-digit addition'
WHERE id = 'b1000000-0000-0000-0000-000000000000';

UPDATE challenges
SET description = 'Use subtraction to find differences, missing parts, and two-digit answers'
WHERE id = 'b2000000-0000-0000-0000-000000000000';

-- Add new Math challenges in learning order.
INSERT INTO challenges (id, subject_id, name, slug, description) VALUES
  ('b5000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'Multiplication', 'multiplication', 'Treat multiplication as repeated groups and build fluency with key facts'),
  ('b6000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'Division', 'division', 'Connect division to equal sharing and inverse multiplication facts'),
  ('b7000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'Order of Operations', 'order_of_operations', 'Evaluate short expressions by choosing the correct operation order'),
  ('b8000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'Expressions with Variables', 'expressions_with_variables', 'Evaluate expressions by replacing one variable with a given number'),
  ('b9000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'One-Step Equations', 'one_step_equations', 'Solve equations with one inverse operation'),
  ('ba000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'Two-Step Equations', 'two_step_equations', 'Solve equations by undoing two operations in the correct order'),
  ('bb000000-0000-0000-0000-000000000000', 'a1000000-0000-0000-0000-000000000000', 'Systems of Equations', 'systems_of_equations', 'Use substitution and paired equations to solve for two variables')
ON CONFLICT (id) DO UPDATE
SET subject_id = EXCLUDED.subject_id,
    name = EXCLUDED.name,
    slug = EXCLUDED.slug,
    description = EXCLUDED.description;

-- Math challenge prerequisites form a single ladder.
INSERT INTO challenge_prerequisites (challenge_id, requires_challenge_id) VALUES
  ('b2000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000'),
  ('b5000000-0000-0000-0000-000000000000', 'b2000000-0000-0000-0000-000000000000'),
  ('b6000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000'),
  ('b7000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000'),
  ('b8000000-0000-0000-0000-000000000000', 'b7000000-0000-0000-0000-000000000000'),
  ('b9000000-0000-0000-0000-000000000000', 'b8000000-0000-0000-0000-000000000000'),
  ('ba000000-0000-0000-0000-000000000000', 'b9000000-0000-0000-0000-000000000000'),
  ('bb000000-0000-0000-0000-000000000000', 'ba000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;

-- Refresh the existing arithmetic steps and add the new ones.
INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, require_ultimate) VALUES
  ('c1000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000', 1, 'Add Within 10', 'Addition facts whose sums stay within 10', 5, 0.80, FALSE),
  ('c2000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000', 2, 'Make 10', 'Addition facts that build combinations equal to 10', 5, 0.80, FALSE),
  ('c3000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000', 3, 'Two-Digit No Carry', 'Two-digit addition without carrying', 5, 0.80, FALSE),
  ('c4000000-0000-0000-0000-000000000000', 'b1000000-0000-0000-0000-000000000000', 4, 'Two-Digit With Carry', 'Two-digit addition with carrying', 5, 0.80, FALSE),
  ('c5000000-0000-0000-0000-000000000000', 'b2000000-0000-0000-0000-000000000000', 1, 'Subtract Within 10', 'Subtraction facts within 10', 5, 0.80, FALSE),
  ('c6000000-0000-0000-0000-000000000000', 'b2000000-0000-0000-0000-000000000000', 2, 'Find the Missing Addend', 'Use inverse addition facts to find the missing part', 5, 0.80, FALSE),
  ('c9000000-0000-0000-0000-000000000000', 'b2000000-0000-0000-0000-000000000000', 3, 'Two-Digit No Borrow', 'Two-digit subtraction without borrowing', 5, 0.80, FALSE),
  ('ca000000-0000-0000-0000-000000000000', 'b2000000-0000-0000-0000-000000000000', 4, 'Two-Digit With Borrow', 'Two-digit subtraction with borrowing', 5, 0.80, FALSE),
  ('cb000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 1, 'Equal Groups', 'Interpret multiplication as equal groups', 5, 0.80, FALSE),
  ('cc000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 2, 'Multiply by 2', 'Multiply whole numbers by 2', 5, 0.80, FALSE),
  ('cd000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 3, 'Multiply by 5', 'Multiply whole numbers by 5', 5, 0.80, FALSE),
  ('ce000000-0000-0000-0000-000000000000', 'b5000000-0000-0000-0000-000000000000', 4, 'Multiply by 10', 'Multiply whole numbers by 10', 5, 0.80, FALSE),
  ('cf000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000', 1, 'Sharing Equally', 'Interpret division as equal sharing', 5, 0.80, FALSE),
  ('d1000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000', 2, 'Divide by 2', 'Divide whole numbers by 2', 5, 0.80, FALSE),
  ('d2000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000', 3, 'Divide by 5', 'Divide whole numbers by 5', 5, 0.80, FALSE),
  ('d3000000-0000-0000-0000-000000000000', 'b6000000-0000-0000-0000-000000000000', 4, 'Divide by 10', 'Divide whole numbers by 10', 5, 0.80, FALSE),
  ('d4000000-0000-0000-0000-000000000000', 'b7000000-0000-0000-0000-000000000000', 1, 'Multiply Then Add', 'Evaluate expressions where multiplication happens before addition', 5, 0.80, FALSE),
  ('d5000000-0000-0000-0000-000000000000', 'b7000000-0000-0000-0000-000000000000', 2, 'Multiply Then Subtract', 'Evaluate expressions where multiplication happens before subtraction', 5, 0.80, FALSE),
  ('d6000000-0000-0000-0000-000000000000', 'b7000000-0000-0000-0000-000000000000', 3, 'Parentheses First', 'Evaluate expressions that require parentheses first', 5, 0.80, FALSE),
  ('d7000000-0000-0000-0000-000000000000', 'b7000000-0000-0000-0000-000000000000', 4, 'Mixed Expressions', 'Evaluate short mixed-operation expressions', 5, 0.80, FALSE),
  ('d8000000-0000-0000-0000-000000000000', 'b8000000-0000-0000-0000-000000000000', 1, 'Evaluate x + a', 'Substitute a value for x and add', 5, 0.80, FALSE),
  ('d9000000-0000-0000-0000-000000000000', 'b8000000-0000-0000-0000-000000000000', 2, 'Evaluate x - a', 'Substitute a value for x and subtract', 5, 0.80, FALSE),
  ('da000000-0000-0000-0000-000000000000', 'b8000000-0000-0000-0000-000000000000', 3, 'Evaluate ax', 'Substitute a value for x and multiply', 5, 0.80, FALSE),
  ('db000000-0000-0000-0000-000000000000', 'b8000000-0000-0000-0000-000000000000', 4, 'Evaluate x / a', 'Substitute a value for x and divide exactly', 5, 0.80, FALSE),
  ('dc000000-0000-0000-0000-000000000000', 'b9000000-0000-0000-0000-000000000000', 1, 'Solve x + a = b', 'Solve addition equations with one step', 5, 0.80, FALSE),
  ('dd000000-0000-0000-0000-000000000000', 'b9000000-0000-0000-0000-000000000000', 2, 'Solve x - a = b', 'Solve subtraction equations with one step', 5, 0.80, FALSE),
  ('de000000-0000-0000-0000-000000000000', 'b9000000-0000-0000-0000-000000000000', 3, 'Solve ax = b', 'Solve multiplication equations with one step', 5, 0.80, FALSE),
  ('df000000-0000-0000-0000-000000000000', 'b9000000-0000-0000-0000-000000000000', 4, 'Solve x / a = b', 'Solve division equations with one step', 5, 0.80, FALSE),
  ('e1000000-0000-0000-0000-000000000000', 'ba000000-0000-0000-0000-000000000000', 1, 'Solve ax + b = c', 'Solve two-step equations with addition', 5, 0.80, FALSE),
  ('e2000000-0000-0000-0000-000000000000', 'ba000000-0000-0000-0000-000000000000', 2, 'Solve ax - b = c', 'Solve two-step equations with subtraction', 5, 0.80, FALSE),
  ('e3000000-0000-0000-0000-000000000000', 'ba000000-0000-0000-0000-000000000000', 3, 'Solve x / a + b = c', 'Solve two-step equations that start with exact division', 5, 0.80, FALSE),
  ('e4000000-0000-0000-0000-000000000000', 'ba000000-0000-0000-0000-000000000000', 4, 'Solve x / a - b = c', 'Solve two-step equations with exact division and subtraction', 5, 0.80, FALSE),
  ('e5000000-0000-0000-0000-000000000000', 'bb000000-0000-0000-0000-000000000000', 1, 'Substitute x into y = x + a', 'Use a known value of x to find y', 5, 0.80, FALSE),
  ('e6000000-0000-0000-0000-000000000000', 'bb000000-0000-0000-0000-000000000000', 2, 'Solve a System and Find x', 'Solve a simple substitution-ready system and report x', 5, 0.80, FALSE),
  ('e7000000-0000-0000-0000-000000000000', 'bb000000-0000-0000-0000-000000000000', 3, 'Solve a System and Find y', 'Solve a simple substitution-ready system and report y', 5, 0.80, FALSE),
  ('e8000000-0000-0000-0000-000000000000', 'bb000000-0000-0000-0000-000000000000', 4, 'Standard Form: Find x', 'Solve a two-variable system in standard form and report x', 5, 0.80, FALSE),
  ('e9000000-0000-0000-0000-000000000000', 'bb000000-0000-0000-0000-000000000000', 5, 'Standard Form: Find y', 'Solve a two-variable system in standard form and report y', 5, 0.80, FALSE)
ON CONFLICT (id) DO UPDATE
SET challenge_id = EXCLUDED.challenge_id,
    number = EXCLUDED.number,
    title = EXCLUDED.title,
    description = EXCLUDED.description,
    streak_goal = EXCLUDED.streak_goal,
    mastery_target = EXCLUDED.mastery_target,
    require_ultimate = EXCLUDED.require_ultimate;

INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  ('c2000000-0000-0000-0000-000000000000', 'c1000000-0000-0000-0000-000000000000'),
  ('c3000000-0000-0000-0000-000000000000', 'c2000000-0000-0000-0000-000000000000'),
  ('c4000000-0000-0000-0000-000000000000', 'c3000000-0000-0000-0000-000000000000'),
  ('c6000000-0000-0000-0000-000000000000', 'c5000000-0000-0000-0000-000000000000'),
  ('c9000000-0000-0000-0000-000000000000', 'c6000000-0000-0000-0000-000000000000'),
  ('ca000000-0000-0000-0000-000000000000', 'c9000000-0000-0000-0000-000000000000'),
  ('cc000000-0000-0000-0000-000000000000', 'cb000000-0000-0000-0000-000000000000'),
  ('cd000000-0000-0000-0000-000000000000', 'cc000000-0000-0000-0000-000000000000'),
  ('ce000000-0000-0000-0000-000000000000', 'cd000000-0000-0000-0000-000000000000'),
  ('d1000000-0000-0000-0000-000000000000', 'cf000000-0000-0000-0000-000000000000'),
  ('d2000000-0000-0000-0000-000000000000', 'd1000000-0000-0000-0000-000000000000'),
  ('d3000000-0000-0000-0000-000000000000', 'd2000000-0000-0000-0000-000000000000'),
  ('d5000000-0000-0000-0000-000000000000', 'd4000000-0000-0000-0000-000000000000'),
  ('d6000000-0000-0000-0000-000000000000', 'd5000000-0000-0000-0000-000000000000'),
  ('d7000000-0000-0000-0000-000000000000', 'd6000000-0000-0000-0000-000000000000'),
  ('d9000000-0000-0000-0000-000000000000', 'd8000000-0000-0000-0000-000000000000'),
  ('da000000-0000-0000-0000-000000000000', 'd9000000-0000-0000-0000-000000000000'),
  ('db000000-0000-0000-0000-000000000000', 'da000000-0000-0000-0000-000000000000'),
  ('dd000000-0000-0000-0000-000000000000', 'dc000000-0000-0000-0000-000000000000'),
  ('de000000-0000-0000-0000-000000000000', 'dd000000-0000-0000-0000-000000000000'),
  ('df000000-0000-0000-0000-000000000000', 'de000000-0000-0000-0000-000000000000'),
  ('e2000000-0000-0000-0000-000000000000', 'e1000000-0000-0000-0000-000000000000'),
  ('e3000000-0000-0000-0000-000000000000', 'e2000000-0000-0000-0000-000000000000'),
  ('e4000000-0000-0000-0000-000000000000', 'e3000000-0000-0000-0000-000000000000'),
  ('e6000000-0000-0000-0000-000000000000', 'e5000000-0000-0000-0000-000000000000'),
  ('e7000000-0000-0000-0000-000000000000', 'e6000000-0000-0000-0000-000000000000'),
  ('e8000000-0000-0000-0000-000000000000', 'e7000000-0000-0000-0000-000000000000'),
  ('e9000000-0000-0000-0000-000000000000', 'e8000000-0000-0000-0000-000000000000')
ON CONFLICT DO NOTHING;
