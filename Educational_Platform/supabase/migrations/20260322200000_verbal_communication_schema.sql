-- ============================================================
-- Migration: Verbal Communication Subject — Parlour System
-- Adds: subjects row, characters table, character_step_assignments,
--       3 parlour challenges (Coffee Shop, Job Interview, Book Club)
--       with 6 steps each, character assignments, and challenge prereqs.
-- ============================================================

-- ── Subject ──────────────────────────────────────────────────────────────────
INSERT INTO subjects (id, name, description) VALUES
  ('a4000000-0000-0000-0000-000000000000', 'Verbal Communication',
   'Learn tone, subtext, register, and reading between the lines through parlour conversations with virtual characters');

-- ── Characters table ──────────────────────────────────────────────────────────
CREATE TABLE IF NOT EXISTS characters (
  id             UUID PRIMARY KEY,
  name           TEXT NOT NULL,
  personality    TEXT NOT NULL,
  speaking_style TEXT NOT NULL,
  avatar_key     TEXT NOT NULL,
  subject_id     UUID REFERENCES subjects(id) ON DELETE SET NULL
);

INSERT INTO characters (id, name, personality, speaking_style, avatar_key, subject_id) VALUES
  ('0c000000-0000-0000-0000-000000000000',
   'Maya',
   'Warm, encouraging, and celebratory. She genuinely delights in the player''s progress and frames every challenge as a shared adventure. She uses analogies from everyday life and is never condescending.',
   'Friendly and casual. Uses "we" and "us" often to create togetherness. Short enthusiastic sentences, occasional exclamation marks. Uses contractions freely. Celebrates even small wins.',
   'maya_placeholder',
   'a4000000-0000-0000-0000-000000000000'),

  ('0c100000-0000-0000-0000-000000000000',
   'Victor',
   'Formal, precise, and mildly impatient. He values professionalism above all and holds the player to a high standard. He is not unkind, but he does not tolerate vagueness or sloppiness in communication.',
   'Structured and professional. No contractions. Full sentences. Measured tone. Uses transitional phrases like "Furthermore" and "With that said". Rarely praises unless deserved.',
   'victor_placeholder',
   'a4000000-0000-0000-0000-000000000000'),

  ('0c200000-0000-0000-0000-000000000000',
   'Zoe',
   'Playful, mischievous, and effortlessly witty. She uses humour as a teaching tool and enjoys testing the player with ambiguous or layered language. She is always friendly but loves to keep people on their toes.',
   'Witty and punchy. Short bursts of speech. Lots of rhetorical questions and sarcasm used kindly. Uses "Sooo..." to introduce something tricky. Casual slang but never rude.',
   'zoe_placeholder',
   'a4000000-0000-0000-0000-000000000000'),

  ('0c300000-0000-0000-0000-000000000000',
   'Dr. Chen',
   'Analytical, curious, and fascinated by nuance. She finds beauty in precise word choice and subtle meaning. She is patient and encouraging but will gently probe when she thinks the player is being superficial.',
   'Measured and thoughtful. Starts observations with "Interesting..." or "Notice how...". Uses academic vocabulary but explains it naturally. Poses Socratic follow-up questions.',
   'dr_chen_placeholder',
   'a4000000-0000-0000-0000-000000000000'),

  ('0c400000-0000-0000-0000-000000000000',
   'Alex',
   'Sarcastic, sharp-tongued, and deceptively insightful. Alex challenges the player by being indirect and testing their ability to read subtext. Behind the dry exterior is a genuine interest in whether the player truly understands.',
   'Dry humour delivered deadpan. Uses understatement and rhetorical questions constantly. Will say the opposite of what they mean to see if the player catches it. Never obviously helpful.',
   'alex_placeholder',
   'a4000000-0000-0000-0000-000000000000');

-- ── Character–Step assignments table ──────────────────────────────────────────
CREATE TABLE IF NOT EXISTS character_step_assignments (
  character_id  UUID NOT NULL REFERENCES characters(id) ON DELETE CASCADE,
  step_id       UUID NOT NULL REFERENCES steps(id)      ON DELETE CASCADE,
  PRIMARY KEY (character_id, step_id)
);

-- ── Parlour Challenges ─────────────────────────────────────────────────────────
INSERT INTO challenges (id, subject_id, name, slug, description, stage_number, stage_name, difficulty) VALUES
  ('26000000-0000-0000-0000-000000000000',
   'a4000000-0000-0000-0000-000000000000',
   'The Coffee Shop',
   'parlour_coffee_shop',
   'Practice informal language, tone matching, and reading social cues in a relaxed coffee-shop setting',
   1, 'Parlour Foundations', 0.10),

  ('27000000-0000-0000-0000-000000000000',
   'a4000000-0000-0000-0000-000000000000',
   'The Job Interview',
   'parlour_job_interview',
   'Master formal register, professional tone, and controlled language under pressure',
   1, 'Parlour Foundations', 0.30),

  ('28000000-0000-0000-0000-000000000000',
   'a4000000-0000-0000-0000-000000000000',
   'The Book Club',
   'parlour_book_club',
   'Interpret subtext, explore word meaning, and learn to read between the lines of layered language',
   1, 'Parlour Foundations', 0.25);

-- ── Coffee Shop Steps (6 steps) ───────────────────────────────────────────────
INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES
  ('14000000-0000-0000-0000-000000000000',
   '26000000-0000-0000-0000-000000000000',
   1, 'First Impressions',
   'Choose the right informal greeting when entering the coffee shop',
   5, 0.80, 0.08,
   '{"character_id":"0c000000-0000-0000-0000-000000000000","skill_focus":"informal_register","scene":"player walks into a cozy coffee shop on a Monday morning, Maya is behind the counter","topic":"greetings and opening small talk","difficulty_note":"beginner — obvious contrast between formal and casual options"}'),

  ('15000000-0000-0000-0000-000000000000',
   '26000000-0000-0000-0000-000000000000',
   2, 'Reading the Context',
   'Understand implied meaning when Maya asks about your order habits',
   5, 0.80, 0.12,
   '{"character_id":"0c000000-0000-0000-0000-000000000000","skill_focus":"context_reading","scene":"Maya says something like the usual? and the player must interpret what she means and how to respond","topic":"implied meaning and context clues","difficulty_note":"introduce the idea that words carry context beyond their literal meaning"}'),

  ('16000000-0000-0000-0000-000000000000',
   '26000000-0000-0000-0000-000000000000',
   3, 'Tone Matching',
   'Zoe joins the conversation — match her playful energy without being rude',
   5, 0.80, 0.15,
   '{"character_id":"0c200000-0000-0000-0000-000000000000","skill_focus":"tone_matching","scene":"Zoe slides into the conversation with a joke or playful remark, player must respond in kind","topic":"matching conversational tone and energy","difficulty_note":"player must identify Zoe''s tone and select a reply that matches it, not one that is too formal or too dismissive"}'),

  ('17000000-0000-0000-0000-000000000000',
   '26000000-0000-0000-0000-000000000000',
   4, 'Reading the Room',
   'The café gets tense — Maya''s body language shifts and her words carry a double meaning',
   5, 0.80, 0.18,
   '{"character_id":"0c000000-0000-0000-0000-000000000000","skill_focus":"subtext","scene":"the café gets unexpectedly busy, Maya is stressed but trying to stay composed — her words are polite but her subtext is different","topic":"reading subtext behind polite words","difficulty_note":"the options all sound reasonable on the surface — player must identify which one acknowledges the subtext"}'),

  ('18000000-0000-0000-0000-000000000000',
   '26000000-0000-0000-0000-000000000000',
   5, 'Between the Lines',
   'Zoe says something that could mean two different things — figure out what she actually means',
   5, 0.80, 0.20,
   '{"character_id":"0c200000-0000-0000-0000-000000000000","skill_focus":"ambiguity_resolution","scene":"Zoe makes a comment that is deliberately ambiguous, using her signature dry humour","topic":"resolving linguistic ambiguity","difficulty_note":"two options are plausible — player must consider tone and context to pick the correct interpretation"}'),

  ('19000000-0000-0000-0000-000000000000',
   '26000000-0000-0000-0000-000000000000',
   6, 'Graceful Exit',
   'Choose the right way to end a casual conversation without being abrupt or awkward',
   5, 0.80, 0.18,
   '{"character_id":"0c000000-0000-0000-0000-000000000000","skill_focus":"closing_register","scene":"the player needs to leave, Maya and Zoe are still chatting — how does the player wrap up politely and naturally","topic":"conversational closings and politeness","difficulty_note":"contrast overly abrupt exits with natural warm ones"}');

-- ── Job Interview Steps (6 steps) ─────────────────────────────────────────────
INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES
  ('1a000000-0000-0000-0000-000000000000',
   '27000000-0000-0000-0000-000000000000',
   1, 'Making an Entrance',
   'Open a formal job interview with the correct register — no contractions, no casual language',
   5, 0.80, 0.22,
   '{"character_id":"0c100000-0000-0000-0000-000000000000","skill_focus":"formal_register","scene":"Victor greets the player at the start of a formal job interview, expects a professional opening","topic":"formal greetings and professional self-introduction","difficulty_note":"make the contrast between casual and formal very visible — common mistake is informal opener"}'),

  ('1b000000-0000-0000-0000-000000000000',
   '27000000-0000-0000-0000-000000000000',
   2, 'Tell Me About Yourself',
   'Victor evaluates your self-introduction for tone and precision',
   5, 0.80, 0.26,
   '{"character_id":"0c100000-0000-0000-0000-000000000000","skill_focus":"professional_tone","scene":"Victor asks the classic interview question, player must select the response with the best professional tone and structure","topic":"professional self-introduction","difficulty_note":"options include responses that are too casual, too arrogant, too vague — only one is precise and appropriately confident"}'),

  ('1c000000-0000-0000-0000-000000000000',
   '27000000-0000-0000-0000-000000000000',
   3, 'Tricky Questions',
   'Victor asks an indirect question — read what he really wants to know',
   5, 0.80, 0.28,
   '{"character_id":"0c100000-0000-0000-0000-000000000000","skill_focus":"subtext","scene":"Victor asks a question that sounds neutral but actually probes a weakness or sensitive area — player must respond addressing the implicit concern","topic":"reading implicit intent in formal questions","difficulty_note":"the surface question is simple but the correct answer addresses the unstated underlying concern"}'),

  ('1d000000-0000-0000-0000-000000000000',
   '27000000-0000-0000-0000-000000000000',
   4, 'Staying Composed',
   'Victor pushes back on something you said — choose a professional response under pressure',
   5, 0.80, 0.30,
   '{"character_id":"0c100000-0000-0000-0000-000000000000","skill_focus":"composure_under_pressure","scene":"Victor challenges a point the player made, testing how they handle professional disagreement","topic":"maintaining composure and professional register when challenged","difficulty_note":"avoid responses that are defensive, passive-aggressive, or overly apologetic — professional composure is key"}'),

  ('1e000000-0000-0000-0000-000000000000',
   '27000000-0000-0000-0000-000000000000',
   5, 'Reading the Offer',
   'Victor makes an indirect offer — decode what is being communicated between the lines',
   5, 0.80, 0.32,
   '{"character_id":"0c100000-0000-0000-0000-000000000000","skill_focus":"negotiation_register","scene":"Victor phrases a job offer in corporate-speak — player must decode the real meaning and respond appropriately","topic":"decoding formal/corporate language","difficulty_note":"the offer language uses hedging and formal structures that obscure the real meaning — player must read between the lines"}'),

  ('1f000000-0000-0000-0000-000000000000',
   '27000000-0000-0000-0000-000000000000',
   6, 'Closing Statement',
   'End the interview with the right formal sign-off that leaves a positive impression',
   5, 0.80, 0.30,
   '{"character_id":"0c100000-0000-0000-0000-000000000000","skill_focus":"formal_closing","scene":"the interview is wrapping up and Victor expects a professional and memorable closing from the player","topic":"formal conversational closings","difficulty_note":"contrast generic or casual closings with precise, confident, and memorable professional ones"}');

-- ── Book Club Steps (6 steps) ─────────────────────────────────────────────────
INSERT INTO steps (id, challenge_id, number, title, description, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES
  ('20000000-0000-0000-0000-000000000000',
   '28000000-0000-0000-0000-000000000000',
   1, 'Opening Discussion',
   'Dr. Chen introduces a theme — pick the word that best captures her meaning',
   5, 0.80, 0.20,
   '{"character_id":"0c300000-0000-0000-0000-000000000000","skill_focus":"vocabulary","scene":"book club opens, Dr. Chen introduces a theme from a passage using careful word choice — player must identify the most accurate synonym or related term","topic":"precise vocabulary and word meaning","difficulty_note":"options include words that are close in meaning but carry different connotations — only one truly captures Dr. Chen''s point"}'),

  ('21000000-0000-0000-0000-000000000000',
   '28000000-0000-0000-0000-000000000000',
   2, 'Word Choice Matters',
   'Two words look similar — Dr. Chen wants to know which one the author really meant',
   5, 0.80, 0.23,
   '{"character_id":"0c300000-0000-0000-0000-000000000000","skill_focus":"vocabulary_nuance","scene":"Dr. Chen presents two near-synonyms and asks which one better fits the author''s intent in context","topic":"connotation and word nuance","difficulty_note":"focus on connotation differences — one word is neutral, one carries emotional weight that fits the passage"}'),

  ('22000000-0000-0000-0000-000000000000',
   '28000000-0000-0000-0000-000000000000',
   3, 'What Did the Author Mean?',
   'A passage has a surface meaning and a deeper one — find the interpretation Dr. Chen is looking for',
   5, 0.80, 0.25,
   '{"character_id":"0c300000-0000-0000-0000-000000000000","skill_focus":"interpretation","scene":"Dr. Chen reads a short passage aloud and asks what the author was really trying to say","topic":"literal vs. figurative meaning and authorial intent","difficulty_note":"include a literal interpretation as a distractor — the correct answer is the deeper figurative or thematic reading"}'),

  ('23000000-0000-0000-0000-000000000000',
   '28000000-0000-0000-0000-000000000000',
   4, 'A Polite Disagreement',
   'Maya gently disagrees with Dr. Chen — sense the underlying tension and pick the response that handles it gracefully',
   5, 0.80, 0.27,
   '{"character_id":"0c000000-0000-0000-0000-000000000000","skill_focus":"subtext_in_disagreement","scene":"Maya offers a different interpretation than Dr. Chen in a polite but loaded way — player must identify what Maya is really saying","topic":"reading subtext in polite disagreement","difficulty_note":"politeness masks real tension — player must identify the implicit disagreement and respond in a way that acknowledges both positions"}'),

  ('24000000-0000-0000-0000-000000000000',
   '28000000-0000-0000-0000-000000000000',
   5, 'Tone and Intent',
   'Dr. Chen makes a pointed observation — recognise that it is a critique, not a compliment',
   5, 0.80, 0.28,
   '{"character_id":"0c300000-0000-0000-0000-000000000000","skill_focus":"tone_recognition","scene":"Dr. Chen delivers what sounds like praise but is actually a polite critique — player must identify the true tone","topic":"recognising tone and intent","difficulty_note":"the phrasing sounds positive — player must detect the critical subtext and choose a response that addresses the actual critique"}'),

  ('25000000-0000-0000-0000-000000000000',
   '28000000-0000-0000-0000-000000000000',
   6, 'Your Turn',
   'Synthesise the whole discussion — Maya asks you to summarise the passage''s message in the most accurate way',
   5, 0.80, 0.28,
   '{"character_id":"0c000000-0000-0000-0000-000000000000","skill_focus":"comprehension_synthesis","scene":"Maya asks the player to summarise what the book club has discussed — picking the most accurate and nuanced summary","topic":"comprehension and synthesis of meaning","difficulty_note":"options range from superficial summaries to precise ones that capture both surface and deeper meaning"}');

-- ── Step prerequisites (within challenges) ───────────────────────────────────
INSERT INTO step_prerequisites (step_id, requires_step_id) VALUES
  -- Coffee Shop chain
  ('15000000-0000-0000-0000-000000000000', '14000000-0000-0000-0000-000000000000'),
  ('16000000-0000-0000-0000-000000000000', '15000000-0000-0000-0000-000000000000'),
  ('17000000-0000-0000-0000-000000000000', '16000000-0000-0000-0000-000000000000'),
  ('18000000-0000-0000-0000-000000000000', '17000000-0000-0000-0000-000000000000'),
  ('19000000-0000-0000-0000-000000000000', '18000000-0000-0000-0000-000000000000'),
  -- Job Interview chain
  ('1b000000-0000-0000-0000-000000000000', '1a000000-0000-0000-0000-000000000000'),
  ('1c000000-0000-0000-0000-000000000000', '1b000000-0000-0000-0000-000000000000'),
  ('1d000000-0000-0000-0000-000000000000', '1c000000-0000-0000-0000-000000000000'),
  ('1e000000-0000-0000-0000-000000000000', '1d000000-0000-0000-0000-000000000000'),
  ('1f000000-0000-0000-0000-000000000000', '1e000000-0000-0000-0000-000000000000'),
  -- Book Club chain
  ('21000000-0000-0000-0000-000000000000', '20000000-0000-0000-0000-000000000000'),
  ('22000000-0000-0000-0000-000000000000', '21000000-0000-0000-0000-000000000000'),
  ('23000000-0000-0000-0000-000000000000', '22000000-0000-0000-0000-000000000000'),
  ('24000000-0000-0000-0000-000000000000', '23000000-0000-0000-0000-000000000000'),
  ('25000000-0000-0000-0000-000000000000', '24000000-0000-0000-0000-000000000000');

-- ── Challenge step prerequisites (cross-challenge unlock, AND semantics) ──────
-- Job Interview: requires completing Coffee Shop step 4 (Reading the Room)
INSERT INTO challenge_step_prerequisites (challenge_id, requires_step_id) VALUES
  ('27000000-0000-0000-0000-000000000000', '17000000-0000-0000-0000-000000000000');

-- Book Club: requires completing Coffee Shop step 3 (Tone Matching)
INSERT INTO challenge_step_prerequisites (challenge_id, requires_step_id) VALUES
  ('28000000-0000-0000-0000-000000000000', '16000000-0000-0000-0000-000000000000');

-- ── Character–Step assignments ────────────────────────────────────────────────
-- Coffee Shop
INSERT INTO character_step_assignments (character_id, step_id) VALUES
  ('0c000000-0000-0000-0000-000000000000', '14000000-0000-0000-0000-000000000000'), -- Maya: First Impressions
  ('0c000000-0000-0000-0000-000000000000', '15000000-0000-0000-0000-000000000000'), -- Maya: Reading the Context
  ('0c200000-0000-0000-0000-000000000000', '16000000-0000-0000-0000-000000000000'), -- Zoe:  Tone Matching
  ('0c000000-0000-0000-0000-000000000000', '17000000-0000-0000-0000-000000000000'), -- Maya: Reading the Room
  ('0c200000-0000-0000-0000-000000000000', '18000000-0000-0000-0000-000000000000'), -- Zoe:  Between the Lines
  ('0c000000-0000-0000-0000-000000000000', '19000000-0000-0000-0000-000000000000'), -- Maya: Graceful Exit
  -- Job Interview
  ('0c100000-0000-0000-0000-000000000000', '1a000000-0000-0000-0000-000000000000'), -- Victor: all 6 steps
  ('0c100000-0000-0000-0000-000000000000', '1b000000-0000-0000-0000-000000000000'),
  ('0c100000-0000-0000-0000-000000000000', '1c000000-0000-0000-0000-000000000000'),
  ('0c100000-0000-0000-0000-000000000000', '1d000000-0000-0000-0000-000000000000'),
  ('0c100000-0000-0000-0000-000000000000', '1e000000-0000-0000-0000-000000000000'),
  ('0c100000-0000-0000-0000-000000000000', '1f000000-0000-0000-0000-000000000000'),
  -- Book Club
  ('0c300000-0000-0000-0000-000000000000', '20000000-0000-0000-0000-000000000000'), -- Dr. Chen: first 2 steps
  ('0c300000-0000-0000-0000-000000000000', '21000000-0000-0000-0000-000000000000'),
  ('0c300000-0000-0000-0000-000000000000', '22000000-0000-0000-0000-000000000000'),
  ('0c000000-0000-0000-0000-000000000000', '23000000-0000-0000-0000-000000000000'), -- Maya: polite disagreement
  ('0c300000-0000-0000-0000-000000000000', '24000000-0000-0000-0000-000000000000'), -- Dr. Chen: tone & intent
  ('0c000000-0000-0000-0000-000000000000', '25000000-0000-0000-0000-000000000000'); -- Maya: your turn
