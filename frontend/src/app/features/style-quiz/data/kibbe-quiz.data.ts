import { KibbeQuizSection, KibbeQuizQuestion } from '../../../models/style/kibbe.models';

export const KIBBE_QUIZ_SECTIONS: KibbeQuizSection[] = [
  {
    id: 'bone',
    title: 'Bone Structure',
    description:
      'Think about your skeleton — the underlying framework beneath the flesh. Focus on length, width, and sharpness of your bones, not your size or weight.',
    questions: [
      {
        id: 'bone_1',
        sectionId: 'bone',
        prompt: 'How would you describe your overall height and bone length?',
        options: [
          { value: 'A', label: 'Tall and long-limbed — elongated overall impression' },
          { value: 'B', label: 'Moderate to tall with broad, spacious proportions' },
          { value: 'C', label: 'Moderate height with even, symmetrical proportions' },
          { value: 'D', label: 'Petite to moderate with a compact, condensed look' },
          { value: 'E', label: 'Petite with a small, delicate bone structure' },
        ],
      },
      {
        id: 'bone_2',
        sectionId: 'bone',
        prompt: 'How do your shoulders appear?',
        options: [
          { value: 'A', label: 'Straight, wide, and very angular — squared edge' },
          { value: 'B', label: 'Broad and slightly sloped — relaxed squareness' },
          { value: 'C', label: 'Moderate width, slightly tapered — neither sharp nor wide' },
          { value: 'D', label: 'Narrow and slightly sloped, with a delicate look' },
          { value: 'E', label: 'Very narrow, sloped, and rounded at the edge' },
        ],
      },
      {
        id: 'bone_3',
        sectionId: 'bone',
        prompt: 'How would you describe your arms and legs?',
        options: [
          { value: 'A', label: 'Long, narrow, and angular — almost bony' },
          { value: 'B', label: 'Long and broad — sturdy, not angular' },
          { value: 'C', label: 'Moderate length, neither particularly angular nor fleshy' },
          { value: 'D', label: 'Short and narrow, or with a lively angular quality' },
          { value: 'E', label: 'Short and delicate — small-boned throughout' },
        ],
      },
      {
        id: 'bone_4',
        sectionId: 'bone',
        prompt: 'How are your hands and feet?',
        options: [
          { value: 'A', label: 'Large and bony with long, angular fingers and toes' },
          { value: 'B', label: 'Large and broad with wide, blunt fingers' },
          { value: 'C', label: 'Moderate size — symmetrical and unremarkable' },
          { value: 'D', label: 'Small and narrow with slight sharpness' },
          { value: 'E', label: 'Very small, dainty, and rounded' },
        ],
      },
      {
        id: 'bone_5',
        sectionId: 'bone',
        prompt: 'What is the overall impression your skeleton gives?',
        options: [
          { value: 'A', label: 'Very angular, elongated, and assertive' },
          { value: 'B', label: 'Broad, sturdy, and relaxed — earthy quality' },
          { value: 'C', label: 'Symmetrical, even, and classically proportioned' },
          { value: 'D', label: 'Small and slightly sharp — a combination of delicate and angular' },
          { value: 'E', label: 'Small, rounded, and delicate throughout' },
        ],
      },
    ],
  },
  {
    id: 'flesh',
    title: 'Body Flesh',
    description:
      'Now look at the soft tissue — skin, muscle definition, and body curves. Focus on the texture and quality of your flesh regardless of weight.',
    questions: [
      {
        id: 'flesh_1',
        sectionId: 'flesh',
        prompt: 'How does your flesh tend to look on your frame?',
        options: [
          { value: 'A', label: 'Taut and flat — very little padding, muscles visible' },
          { value: 'B', label: 'Muscular or sinewy — flesh is broad and firm' },
          { value: 'C', label: 'Smooth and moderately toned — neither overly firm nor soft' },
          { value: 'D', label: 'Variable — can range from wiry to soft' },
          { value: 'E', label: 'Soft and full — curves appear regardless of weight' },
        ],
      },
      {
        id: 'flesh_2',
        sectionId: 'flesh',
        prompt: 'How defined is your waist relative to your bust and hips?',
        options: [
          { value: 'A', label: 'Minimal definition — waist, bust, and hips are similar widths' },
          { value: 'B', label: 'Slight definition — some narrowing but not dramatic' },
          { value: 'C', label: 'Moderate definition — a visible waist but balanced' },
          { value: 'D', label: 'Variable or sharp — can appear quite defined on a small frame' },
          { value: 'E', label: 'Very defined — a pronounced, womanly waist-to-hip curve' },
        ],
      },
      {
        id: 'flesh_3',
        sectionId: 'flesh',
        prompt: 'How would you describe your hips?',
        options: [
          { value: 'A', label: 'Narrow and straight — no pronounced curve' },
          { value: 'B', label: 'Broad, but not particularly curved — wide and even' },
          { value: 'C', label: 'Moderate — gently curved and proportional' },
          { value: 'D', label: 'Narrow but with a slight fleshy quality' },
          { value: 'E', label: 'Full and rounded — prominent curves' },
        ],
      },
      {
        id: 'flesh_4',
        sectionId: 'flesh',
        prompt: 'How would you describe your bust?',
        options: [
          { value: 'A', label: 'Flat or minimal — chest is narrow and taut' },
          { value: 'B', label: 'Broad but not pronounced — wide chest that\'s not curvy' },
          { value: 'C', label: 'Moderate — in proportion with the rest of your body' },
          { value: 'D', label: 'Small and delicate — or slightly rounded on a petite frame' },
          { value: 'E', label: 'Full and prominent — a lush, rounded bust' },
        ],
      },
      {
        id: 'flesh_5',
        sectionId: 'flesh',
        prompt: 'When you gain weight, where does it tend to accumulate?',
        options: [
          { value: 'A', label: 'Little weight gain apparent — stays quite lean' },
          { value: 'B', label: 'Spreads evenly across a broad frame — no dramatic curves appear' },
          { value: 'C', label: 'Distributes evenly, maintaining overall proportion' },
          { value: 'D', label: 'Can become roundly soft, or unevenly distributed' },
          { value: 'E', label: 'Enhances curves — bust, hips, and waist become more pronounced' },
        ],
      },
    ],
  },
  {
    id: 'face',
    title: 'Facial Features',
    description:
      'Consider your facial bones and features — their shape, size, and overall impression. Focus on structure and quality, not attractiveness.',
    questions: [
      {
        id: 'face_1',
        sectionId: 'face',
        prompt: 'What is the overall shape and impression of your face?',
        options: [
          { value: 'A', label: 'Narrow, angular, and chiseled — strong planes and edges' },
          { value: 'B', label: 'Large and open — broad, relaxed features' },
          { value: 'C', label: 'Symmetrical and even — a classically balanced oval or shape' },
          { value: 'D', label: 'Small and angular, or a lively mix of sharp and soft' },
          { value: 'E', label: 'Round and soft — gentle curves, no sharp angles' },
        ],
      },
      {
        id: 'face_2',
        sectionId: 'face',
        prompt: 'How are your eyes?',
        options: [
          { value: 'A', label: 'Narrow, deeply set, or sharply defined — intense look' },
          { value: 'B', label: 'Wide-set, open, and relaxed — a warm, natural look' },
          { value: 'C', label: 'Almond-shaped and evenly spaced — refined and balanced' },
          { value: 'D', label: 'Round or wide — lively and sparkly, possibly kitten-like' },
          { value: 'E', label: 'Large, round, and luminous — soft and doe-like' },
        ],
      },
      {
        id: 'face_3',
        sectionId: 'face',
        prompt: 'How are your lips?',
        options: [
          { value: 'A', label: 'Thin and sculpted — well-defined edges but little volume' },
          { value: 'B', label: 'Broad and relaxed — comfortable and unfussy' },
          { value: 'C', label: 'Moderate — even and proportionate with clear definition' },
          { value: 'D', label: 'Medium to small — can be somewhat angular or slightly pouty' },
          { value: 'E', label: 'Full, lush, and pillowy — soft with rounded curves' },
        ],
      },
      {
        id: 'face_4',
        sectionId: 'face',
        prompt: 'What is the overall quality of your facial features?',
        options: [
          { value: 'A', label: 'Sharp, dramatic, and bold — commanding and angular' },
          { value: 'B', label: 'Broad, blunt, and approachable — earthy and open' },
          { value: 'C', label: 'Refined and symmetrical — quietly elegant and even' },
          { value: 'D', label: 'Lively and perky — an animated mix of small and sharp or soft' },
          { value: 'E', label: 'Soft, delicate, and rounded — feminine and gentle' },
        ],
      },
    ],
  },
];

export const KIBBE_QUESTIONS_BY_ID: Record<string, KibbeQuizQuestion> =
  ([] as KibbeQuizQuestion[]).concat(...KIBBE_QUIZ_SECTIONS.map((s: KibbeQuizSection) => s.questions)).reduce(
    (acc: Record<string, KibbeQuizQuestion>, q: KibbeQuizQuestion) => { acc[q.id] = q; return acc; },
    {} as Record<string, KibbeQuizQuestion>
  );
