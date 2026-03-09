You are Claude Code AI working on the LYKE beta app. Implement a Kibbe-style body type quiz acquisition funnel as described below. Produce:
1) Proposed route structure (web/Ionic) and UI components
2) Backend API endpoints + request/response schemas
3) Data model changes for storing Style Profile on the user
4) Deterministic scoring algorithm (rules-based v1) with unit tests
5) Analytics event instrumentation with consistent schema
6) Minimal copy placeholders and template mapping for guidance per family
7) Feature-flagged feed personalisation hook (optional story)

CONTEXT / GOAL
We are repositioning LYKE as a styling intelligence platform. The growth hook is a free Kibbe-style quiz that users can complete without login, then results are gated behind registration to unlock full details and save to a Style Profile. Teaser results are visible pre-auth.

IMPLEMENTATION REQUIREMENTS
A) Public entry (no login)
- Public route: /style-quiz/kibbe
- Landing page with CTA "Start quiz"
- Quiz is completable without authentication.

B) Primer
- Before Q1 show a primer: what yin/yang means, and guidance that this is about overall impression, not weight/size.
- Track events: primer_view, primer_continue.

C) Quiz engine
- 3 sections: Bone structure, Body flesh, Facial features.
- Each question has 5 options A–E.
- Must support Back/Next and a progress indicator.
- Cannot proceed without selecting an option (unless configured otherwise).
- On completion, produce an answers payload.

D) Scoring service (deterministic v1)
- Create backend endpoint: POST /api/style/quiz/kibbe/score
- Input: answers array including section_id, question_id, selected_option (A–E)
- Algorithm:
  - Count A–E overall and per section.
  - Compute dominant option overall; allow ties.
  - Map dominant option to family:
    A=Dramatic, B=Natural, C=Classic, D=Gamine, E=Romantic
  - Determine section dominance for bone/flesh/face.
  - is_mixed=true if section dominance differs OR if margin between top two overall counts is within a small threshold (define threshold, e.g. <= 2 questions).
  - confidence=High/Medium/Low derived from margin between top 2 counts (define mapping).
  - runner_up_family derived from second-highest count (ties handled).
- Output JSON fields:
  primary_family, runner_up_family (optional), section_dominance {bone,flesh,face}, is_mixed, confidence, counts {A,B,C,D,E}.
- Provide unit tests for:
  - Single dominant (all A)
  - Mixed sections (bone A, flesh E, face C)
  - Ties
  - Near-tie confidence behaviour
  - Empty/invalid payload validation

E) Teaser results (pre-auth)
- After scoring, show a teaser screen without full breakdown:
  - "You’re likely in the {primary_family} family"
  - One line meaning
  - CTA: "Create an account to unlock your full style profile"
- Track: teaser_view, teaser_register_click.

F) Registration gate with return path
- When CTA clicked, route user to Register/Login.
- After successful auth, return user automatically to full results.
- Persist quiz session across auth using either:
  - short-lived server session keyed by session_id, or
  - encrypted local storage with expiry (max 30 mins).
- If session expired, show friendly prompt to retake quiz.
- Track: register_complete_from_quiz / login_complete_from_quiz.

G) Full results + save to Style Profile
- After auth, show full results page and save to user profile:
  - primary_family, runner_up_family, is_mixed, confidence, section_dominance, completed_at timestamp.
- Backend endpoint: PUT /api/profile/style (or similar) to persist style_profile.kibbe.
- Style Profile model:
  user.style_profile.kibbe = {computed_result:{...}, user_override?:{family, set_at}, last_updated_at}
- On profile screen, show stored result without needing to retake.
- Track: full_results_view, style_profile_saved.

H) Retake + Override
- Retake: reruns quiz; overwrites computed_result after confirmation.
- Override: allow user to select a different family; store in user_override with flag self_selected=true.
- UI should clearly label override as “self-selected”.

I) Shareable results
- Generate a share card image (or simple HTML->canvas rendering) with:
  - "My Style Type: {family}"
  - LYKE branding and CTA
- Use native share sheet (mobile) and copy-link fallback (web).
- Track: share_click, share_complete.

J) Analytics schema
- Provide a small analytics helper that emits events with consistent properties:
  - session_id, user_id (if logged in), timestamp, route, section_id (if relevant), time_spent_ms (if available).
- Ensure events: quiz_start, primer_view, primer_continue, section_start, section_complete, quiz_complete, teaser_view, teaser_register_click, register_complete_from_quiz, login_complete_from_quiz, full_results_view, style_profile_saved, share_click, share_complete.

K) Optional: feed personalisation hook (feature flagged)
- If user has style_profile.kibbe.primary_family, pass it as a signal to feed API.
- Add a simple boost mechanism behind a feature flag.
- Ensure no style profile => unchanged behaviour.

DELIVERABLES
- Provide a concise implementation plan with file/module locations.
- Provide the API contract definitions (OpenAPI-like or TypeScript interfaces).
- Provide scoring implementation code + tests.
- Provide minimal UI pseudocode/components for key screens.
- Provide any migration notes for adding style_profile to user records.

Keep everything deterministic, safe, and easy to iterate.