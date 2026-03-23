-- ============================================================
-- Verbal Communication: Character-Based Challenge Restructure
-- Replaces location-based challenges (Coffee Shop, Job Interview,
-- Book Club) with 5 character-based challenges, one per character.
-- Each character has their own personality-matched track of 5 steps.
-- ============================================================

-- ── Delete old parlour data ──────────────────────────────────

-- Clear player references to old steps before deleting them
UPDATE players SET current_step_id = NULL
WHERE current_step_id IN (
    SELECT id FROM steps WHERE challenge_id IN (
        SELECT id FROM challenges WHERE slug IN (
            'parlour_coffee_shop', 'parlour_job_interview', 'parlour_book_club'
        )
    )
);

DELETE FROM step_prerequisites
WHERE step_id IN (
    SELECT id FROM steps WHERE challenge_id IN (
        SELECT id FROM challenges WHERE slug IN (
            'parlour_coffee_shop', 'parlour_job_interview', 'parlour_book_club'
        )
    )
)
OR requires_step_id IN (
    SELECT id FROM steps WHERE challenge_id IN (
        SELECT id FROM challenges WHERE slug IN (
            'parlour_coffee_shop', 'parlour_job_interview', 'parlour_book_club'
        )
    )
);

DELETE FROM challenge_step_prerequisites
WHERE challenge_id IN (
    SELECT id FROM challenges WHERE slug IN (
        'parlour_coffee_shop', 'parlour_job_interview', 'parlour_book_club'
    )
)
OR requires_step_id IN (
    SELECT id FROM steps WHERE challenge_id IN (
        SELECT id FROM challenges WHERE slug IN (
            'parlour_coffee_shop', 'parlour_job_interview', 'parlour_book_club'
        )
    )
);

DELETE FROM character_step_assignments
WHERE step_id IN (
    SELECT id FROM steps WHERE challenge_id IN (
        SELECT id FROM challenges WHERE slug IN (
            'parlour_coffee_shop', 'parlour_job_interview', 'parlour_book_club'
        )
    )
);

DELETE FROM steps WHERE challenge_id IN (
    SELECT id FROM challenges WHERE slug IN (
        'parlour_coffee_shop', 'parlour_job_interview', 'parlour_book_club'
    )
);

DELETE FROM challenges WHERE slug IN (
    'parlour_coffee_shop', 'parlour_job_interview', 'parlour_book_club'
);

-- ── UUID reference ────────────────────────────────────────────
-- Subject:         a4000000 (Verbal Communication)
-- Challenges:      29000000 (Maya)  2a000000 (Victor)  2b000000 (Zoe)
--                  2c000000 (Chen)  2d000000 (Alex)
-- Maya steps:      30000000–34000000
-- Victor steps:    35000000–39000000
-- Zoe steps:       3a000000–3e000000
-- Chen steps:      3f000000–43000000
-- Alex steps:      44000000–48000000

-- ── Insert 5 character-based challenges ─────────────────────

INSERT INTO challenges (id, subject_id, slug, name, description, stage_number, stage_name, difficulty) VALUES

('29000000-0000-0000-0000-000000000000',
 'a4000000-0000-0000-0000-000000000000',
 'parlour_maya',
 'Maya''s Track',
 'Social warmth and genuine human connection. Maya guides you through real moments that require empathy, emotional awareness, and the courage to be present with others.',
 1, 'Parlour', 0.10),

('2a000000-0000-0000-0000-000000000000',
 'a4000000-0000-0000-0000-000000000000',
 'parlour_victor',
 'Victor''s Track',
 'Professional communication under pressure. Victor holds you to a high standard in the boardroom, the office, and wherever words carry real weight.',
 1, 'Parlour', 0.30),

('2b000000-0000-0000-0000-000000000000',
 'a4000000-0000-0000-0000-000000000000',
 'parlour_zoe',
 'Zoe''s Track',
 'Banter, subtext, and reading the room. Zoe tests your ability to catch what''s unspoken, use humour well, and stay sharp in fast-moving social exchanges.',
 1, 'Parlour', 0.25),

('2c000000-0000-0000-0000-000000000000',
 'a4000000-0000-0000-0000-000000000000',
 'parlour_chen',
 'Dr. Chen''s Track',
 'Depth, nuance, and precision in conversation. Dr. Chen leads you into exchanges where what you say and how you say it reveals how carefully you actually think.',
 2, 'Parlour Advanced', 0.45),

('2d000000-0000-0000-0000-000000000000',
 'a4000000-0000-0000-0000-000000000000',
 'parlour_alex',
 'Alex''s Track',
 'Ambiguity, pressure, and social intelligence. Alex puts you in situations with no obvious right answer where reading subtext and holding your ground is everything.',
 2, 'Parlour Advanced', 0.65);

-- ── Maya's Steps: Social Warmth ──────────────────────────────

INSERT INTO steps (id, challenge_id, number, title, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES

('30000000-0000-0000-0000-000000000000',
 '29000000-0000-0000-0000-000000000000', 1,
 'Making Someone Feel Heard',
 5, 0.7, 0.10,
 '{"skill_focus":"Active Listening","scene":"Maya is in the middle of telling you something personal at a cafe table. She pauses, watching to see if you are really there with her.","difficulty_note":"Focus on responses that invite her to continue rather than redirecting to yourself."}'),

('31000000-0000-0000-0000-000000000000',
 '29000000-0000-0000-0000-000000000000', 2,
 'Reading Emotional Cues',
 5, 0.7, 0.12,
 '{"skill_focus":"Emotional Awareness","scene":"Maya mentions something casually but her tone is off. Something is bothering her, but she has not said it directly.","difficulty_note":"The challenge is noticing the gap between what she says and what she means, and responding to the real feeling."}'),

('32000000-0000-0000-0000-000000000000',
 '29000000-0000-0000-0000-000000000000', 3,
 'Navigating Awkward Silences',
 5, 0.75, 0.15,
 '{"skill_focus":"Comfort with Silence and Recovery","scene":"A silence has fallen between you and Maya. It could tip into awkwardness, or it could become something natural.","difficulty_note":"The right response does not rush to fill the silence, but does not let it become distance."}'),

('33000000-0000-0000-0000-000000000000',
 '29000000-0000-0000-0000-000000000000', 4,
 'Expressing Disagreement Warmly',
 5, 0.75, 0.18,
 '{"skill_focus":"Warm Assertiveness","scene":"Maya says something you genuinely disagree with, but she clearly cares about the topic. You need to push back without pushing her away.","difficulty_note":"The best response holds its ground while keeping the warmth in the relationship intact."}'),

('34000000-0000-0000-0000-000000000000',
 '29000000-0000-0000-0000-000000000000', 5,
 'Deepening a Casual Connection',
 5, 0.8, 0.20,
 '{"skill_focus":"Moving from Small Talk to Depth","scene":"You and Maya have been chatting lightly. There is a natural moment where the conversation could go deeper or stay on the surface.","difficulty_note":"The ideal response opens a door without forcing it. It shows genuine curiosity about her."}');

-- ── Victor's Steps: Professional Excellence ──────────────────

INSERT INTO steps (id, challenge_id, number, title, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES

('35000000-0000-0000-0000-000000000000',
 '2a000000-0000-0000-0000-000000000000', 1,
 'Setting Professional Boundaries',
 5, 0.7, 0.22,
 '{"skill_focus":"Professional Boundary Setting","scene":"Victor assigns you something outside your agreed scope in a meeting. He is matter-of-fact about it, not hostile.","difficulty_note":"The right response is clear and professional without being confrontational or apologetic."}'),

('36000000-0000-0000-0000-000000000000',
 '2a000000-0000-0000-0000-000000000000', 2,
 'Delivering Feedback Diplomatically',
 5, 0.7, 0.26,
 '{"skill_focus":"Constructive Feedback","scene":"Victor has produced work that has a significant flaw. He expects your honest assessment. You can see he is invested in it.","difficulty_note":"Honest, specific, and delivered without softening it to meaninglessness."}'),

('37000000-0000-0000-0000-000000000000',
 '2a000000-0000-0000-0000-000000000000', 3,
 'Asserting Without Aggressing',
 5, 0.75, 0.28,
 '{"skill_focus":"Assertive Communication","scene":"Victor interrupts and dismisses your point in a group setting. Others are watching. You need to reclaim the floor.","difficulty_note":"Firm but measured. No aggression, no surrender."}'),

('38000000-0000-0000-0000-000000000000',
 '2a000000-0000-0000-0000-000000000000', 4,
 'Navigating Power Dynamics',
 5, 0.8, 0.30,
 '{"skill_focus":"Communicating Across Hierarchy","scene":"Victor uses formal authority to close down a discussion you believe should continue. He is technically right to do so.","difficulty_note":"Acknowledge the hierarchy while keeping your voice heard. Neither submissive nor defiant."}'),

('39000000-0000-0000-0000-000000000000',
 '2a000000-0000-0000-0000-000000000000', 5,
 'Recovering from Miscommunication',
 5, 0.8, 0.30,
 '{"skill_focus":"Repair and Recovery","scene":"Victor has clearly misunderstood something you said in a previous exchange and has drawn the wrong conclusions. He raises it formally.","difficulty_note":"Own what needs owning, clarify without overexplaining, restore confidence without grovelling."}');

-- ── Zoe's Steps: Playful Wit ─────────────────────────────────

INSERT INTO steps (id, challenge_id, number, title, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES

('3a000000-0000-0000-0000-000000000000',
 '2b000000-0000-0000-0000-000000000000', 1,
 'Reading the Room',
 5, 0.7, 0.20,
 '{"skill_focus":"Social Calibration","scene":"Zoe makes a comment that lands differently on different people in the group. She watches you to see if you caught it.","difficulty_note":"The right response shows you read the group dynamic, not just the surface of what she said."}'),

('3b000000-0000-0000-0000-000000000000',
 '2b000000-0000-0000-0000-000000000000', 2,
 'Playful Teasing vs. Overstepping',
 5, 0.7, 0.22,
 '{"skill_focus":"Tonal Calibration in Humour","scene":"Zoe teases you about something that is close to the line. It could be funny or it could sting. She is watching your reaction.","difficulty_note":"The ideal response plays along with the spirit without validating the overreach."}'),

('3c000000-0000-0000-0000-000000000000',
 '2b000000-0000-0000-0000-000000000000', 3,
 'Using Humour to Defuse Tension',
 5, 0.75, 0.25,
 '{"skill_focus":"Deflection with Wit","scene":"There is real tension in the room and Zoe makes a joke that could either release it or make it worse. She looks at you to see if you will follow her lead.","difficulty_note":"Matching her register shows you are reading the room. Ignoring it or playing it straight escalates."}'),

('3d000000-0000-0000-0000-000000000000',
 '2b000000-0000-0000-0000-000000000000', 4,
 'Picking Up on Subtext',
 5, 0.8, 0.27,
 '{"skill_focus":"Subtext and Implication","scene":"Zoe says something that sounds casual but clearly means something else. She has not said it plainly and she never does.","difficulty_note":"Responding to what she actually means is the only way to show you are really listening."}'),

('3e000000-0000-0000-0000-000000000000',
 '2b000000-0000-0000-0000-000000000000', 5,
 'Keeping Conversation Alive',
 5, 0.8, 0.28,
 '{"skill_focus":"Conversational Energy","scene":"The conversation has dipped. Zoe is waiting to see if you will just let it die or find a way back in that feels natural, not forced.","difficulty_note":"The right response is light and genuine. Not a topic pivot, but a real thread pulled from what was already said."}');

-- ── Dr. Chen's Steps: Intellectual Depth ─────────────────────

INSERT INTO steps (id, challenge_id, number, title, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES

('3f000000-0000-0000-0000-000000000000',
 '2c000000-0000-0000-0000-000000000000', 1,
 'Asking the Right Questions',
 5, 0.7, 0.35,
 '{"skill_focus":"Socratic Questioning","scene":"Dr. Chen has just made a complex claim. She pauses, inviting a response. The obvious reply is a statement but the right one is a question.","difficulty_note":"The best question reveals genuine curiosity and invites deeper thinking, rather than challenging or agreeing."}'),

('40000000-0000-0000-0000-000000000000',
 '2c000000-0000-0000-0000-000000000000', 2,
 'Challenging Ideas Without Dismissing People',
 5, 0.7, 0.38,
 '{"skill_focus":"Intellectual Disagreement","scene":"Dr. Chen presents an argument you think is flawed. She is measured and clearly open to pushback, but only if it is substantive.","difficulty_note":"The response must engage the idea, not the person. Vague or emotional pushback will disappoint her."}'),

('41000000-0000-0000-0000-000000000000',
 '2c000000-0000-0000-0000-000000000000', 3,
 'Recognising Assumptions in Conversation',
 5, 0.75, 0.40,
 '{"skill_focus":"Critical Listening","scene":"Dr. Chen makes a statement that rests on an unstated assumption. She is testing whether you notice the gap.","difficulty_note":"The right response names the assumption precisely. Not dismissively, but with genuine analytical interest."}'),

('42000000-0000-0000-0000-000000000000',
 '2c000000-0000-0000-0000-000000000000', 4,
 'Bridging Different Communication Styles',
 5, 0.8, 0.42,
 '{"skill_focus":"Style Flexibility","scene":"Dr. Chen is speaking with someone who communicates very differently from her. She turns to you, subtly inviting you to bridge the gap.","difficulty_note":"The ideal response translates without condescending. It values both styles."}'),

('43000000-0000-0000-0000-000000000000',
 '2c000000-0000-0000-0000-000000000000', 5,
 'The Art of the Thoughtful Pause',
 5, 0.8, 0.45,
 '{"skill_focus":"Deliberate Response Timing","scene":"Dr. Chen asks a question that clearly deserves more than an immediate answer. The pressure to respond quickly is social, not rational.","difficulty_note":"Acknowledging the weight of the question before answering is itself the right communication move."}');

-- ── Alex's Steps: Social Challenge ───────────────────────────

INSERT INTO steps (id, challenge_id, number, title, streak_goal, mastery_target, difficulty, prompt_constraints) VALUES

('44000000-0000-0000-0000-000000000000',
 '2d000000-0000-0000-0000-000000000000', 1,
 'Detecting Mixed Signals',
 5, 0.7, 0.50,
 '{"skill_focus":"Signal Detection","scene":"Alex says one thing but their body language and tone suggest something else entirely. They watch to see if you will take the bait or read beneath it.","difficulty_note":"Responding to the surface is the wrong move. The right response shows you caught the contradiction."}'),

('45000000-0000-0000-0000-000000000000',
 '2d000000-0000-0000-0000-000000000000', 2,
 'Navigating Ambiguity',
 5, 0.7, 0.55,
 '{"skill_focus":"Comfort with Ambiguity","scene":"Alex gives you a response that could mean several different things. They offer no clarification. They are waiting to see what you do with it.","difficulty_note":"The ideal response does not force a premature conclusion. It sits with the ambiguity while keeping the conversation moving."}'),

('46000000-0000-0000-0000-000000000000',
 '2d000000-0000-0000-0000-000000000000', 3,
 'Reading What''s Not Said',
 5, 0.75, 0.58,
 '{"skill_focus":"Reading Omission","scene":"Alex has notably not mentioned something that should have come up by now. The absence is louder than anything they have said.","difficulty_note":"Acknowledging the omission the right way shows social sharpness without being accusatory."}'),

('47000000-0000-0000-0000-000000000000',
 '2d000000-0000-0000-0000-000000000000', 4,
 'Holding Ground Under Social Pressure',
 5, 0.8, 0.62,
 '{"skill_focus":"Social Resilience","scene":"Alex pushes back on something you said, not with logic, but with social pressure and mild dismissal. They are testing whether you will fold.","difficulty_note":"The right response holds the position without becoming defensive or rigid. It stays grounded."}'),

('48000000-0000-0000-0000-000000000000',
 '2d000000-0000-0000-0000-000000000000', 5,
 'The Character Test',
 5, 0.85, 0.65,
 '{"skill_focus":"Integrity Under Observation","scene":"Alex creates a small social test, an easy opportunity to deflect, spin, or take a shortcut. They are watching to see what you actually do.","difficulty_note":"The right response is simply honest. Alex has no patience for performance, only for real."}');

-- ── Unlock prerequisites ─────────────────────────────────────
-- Maya: unlocked from start (no prerequisites)
-- Victor: requires Maya step 2 (Reading Emotional Cues)
-- Zoe: requires Maya step 1 (Making Someone Feel Heard)
-- Dr. Chen: requires Zoe step 3 (Humour to Defuse) + Victor step 2 (Delivering Feedback)
-- Alex: requires Dr. Chen step 3 (Recognising Assumptions) + Victor step 4 (Power Dynamics)

INSERT INTO challenge_step_prerequisites (challenge_id, requires_step_id) VALUES
  ('2a000000-0000-0000-0000-000000000000', '31000000-0000-0000-0000-000000000000'),
  ('2b000000-0000-0000-0000-000000000000', '30000000-0000-0000-0000-000000000000'),
  ('2c000000-0000-0000-0000-000000000000', '3c000000-0000-0000-0000-000000000000'),
  ('2c000000-0000-0000-0000-000000000000', '36000000-0000-0000-0000-000000000000'),
  ('2d000000-0000-0000-0000-000000000000', '41000000-0000-0000-0000-000000000000'),
  ('2d000000-0000-0000-0000-000000000000', '38000000-0000-0000-0000-000000000000');
