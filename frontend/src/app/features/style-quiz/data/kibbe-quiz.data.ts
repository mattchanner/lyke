import { KibbeQuizSection, KibbeQuizQuestion } from '../../../models/style/kibbe.models';

export const KIBBE_QUIZ_SECTIONS: KibbeQuizSection[] = [
  {
    id: 'bone',
    title: 'Bone Structure',
    description: 'The architectural foundation of your style type',
    questions: [
      {
        id: 'bone_1',
        sectionId: 'bone',
        prompt: 'How would you describe your overall bone structure?',
        subPrompt: 'Think about sharpness vs softness of angles',
        options: [
          { value: 'A', label: 'Sharp and angular', description: 'Prominent, chiselled angles — edges that catch the light' },
          { value: 'B', label: 'Large and blunt', description: 'Broad and wide with softer angular edges — substantial but not sharp' },
          { value: 'C', label: 'Moderate and symmetrical', description: 'Even, regular proportions — neither prominently sharp nor blunt' },
          { value: 'D', label: 'Small and angular', description: 'Delicate but sharp — fine-boned with defined edges' },
          { value: 'E', label: 'Small and rounded', description: 'Delicate and soft — no prominent angles anywhere' },
        ],
      },
      {
        id: 'bone_2',
        sectionId: 'bone',
        prompt: 'How would you describe your shoulders?',
        options: [
          { value: 'A', label: 'Square or narrow with sharp edges' },
          { value: 'B', label: 'Broad and wide — square with blunt edges' },
          { value: 'C', label: 'Moderate width — slightly tapered or straight' },
          { value: 'D', label: 'Narrow and slightly angular, or tapered' },
          { value: 'E', label: 'Sloped and rounded — soft and narrow' },
        ],
      },
      {
        id: 'bone_3',
        sectionId: 'bone',
        prompt: 'How would you describe your hands and feet?',
        options: [
          { value: 'A', label: 'Long and narrow, or long and broad' },
          { value: 'B', label: 'Large — broad or long with a sturdy look' },
          { value: 'C', label: 'Moderate — neither particularly large nor small' },
          { value: 'D', label: 'Small and narrow, slightly angular' },
          { value: 'E', label: 'Small and rounded — delicate and soft' },
        ],
      },
      {
        id: 'bone_4',
        sectionId: 'bone',
        prompt: 'What is your overall vertical line?',
        subPrompt: 'Your sense of height and elongation relative to your actual height',
        options: [
          { value: 'A', label: 'Elongated — I appear tall regardless of actual height' },
          { value: 'B', label: 'Broad — I appear wide and substantial' },
          { value: 'C', label: 'Moderate — I appear neither very elongated nor broad' },
          { value: 'D', label: 'Compact — I appear small and contained' },
          { value: 'E', label: 'Petite — I appear small and delicate' },
        ],
      },
      {
        id: 'bone_5',
        sectionId: 'bone',
        prompt: 'How would you describe your jawline?',
        options: [
          { value: 'A', label: 'Narrow and very sharp — strongly defined' },
          { value: 'B', label: 'Wide and blunt — strong but with soft edges' },
          { value: 'C', label: 'Moderate — symmetrical and balanced' },
          { value: 'D', label: 'Narrow and sharp, or pointy — defined but small' },
          { value: 'E', label: 'Rounded — soft, with little definition' },
        ],
      },
    ],
  },
  {
    id: 'flesh',
    title: 'Body Flesh',
    description: 'How muscle and soft tissue express themselves on your frame',
    questions: [
      {
        id: 'flesh_1',
        sectionId: 'flesh',
        prompt: 'How would you describe your overall body flesh?',
        options: [
          { value: 'A', label: 'Taut and sinewy', description: 'Lean muscular tissue, little soft flesh — lithe or wiry' },
          { value: 'B', label: 'Straight and muscular', description: 'Athletic build, moderate muscle, tends to remain straight not curvy' },
          { value: 'C', label: 'Evenly distributed', description: 'Proportionate bust, waist, hips — slightly lithe and controlled' },
          { value: 'D', label: 'Small and fine', description: 'Delicate and light — minimal flesh, stays lean' },
          { value: 'E', label: 'Soft and rounded', description: 'Lush and fleshy — curves are the defining feature' },
        ],
      },
      {
        id: 'flesh_2',
        sectionId: 'flesh',
        prompt: 'How would you describe your bust?',
        options: [
          { value: 'A', label: 'Small or flat — not a defining feature' },
          { value: 'B', label: 'Moderate — present but not prominent' },
          { value: 'C', label: 'Moderate and proportionate to hips' },
          { value: 'D', label: 'Small — may be flat or very slight' },
          { value: 'E', label: 'Full and rounded — a prominent feature' },
        ],
      },
      {
        id: 'flesh_3',
        sectionId: 'flesh',
        prompt: 'How is your waistline naturally defined?',
        options: [
          { value: 'A', label: 'Minimal — my torso is relatively straight' },
          { value: 'B', label: 'Slightly defined but stays straight overall' },
          { value: 'C', label: 'Moderately defined — proportionate to bust and hips' },
          { value: 'D', label: 'Small — defined but compact' },
          { value: 'E', label: 'Strongly defined — clearly visible hourglass curve' },
        ],
      },
      {
        id: 'flesh_4',
        sectionId: 'flesh',
        prompt: 'How would you describe your hips?',
        options: [
          { value: 'A', label: 'Narrow — not a prominent feature' },
          { value: 'B', label: 'Straight and even with shoulders' },
          { value: 'C', label: 'Moderate and proportionate' },
          { value: 'D', label: 'Small and narrow or slightly rounded' },
          { value: 'E', label: 'Full and rounded — a prominent feature' },
        ],
      },
      {
        id: 'flesh_5',
        sectionId: 'flesh',
        prompt: 'When you gain weight, where does it primarily accumulate?',
        options: [
          { value: 'A', label: 'Upper thighs and hips — body stays relatively lean above' },
          { value: 'B', label: 'Becomes more square/stocky — body remains fairly straight' },
          { value: 'C', label: 'Evenly distributed throughout the body' },
          { value: 'D', label: 'Stays proportional — remains small overall' },
          { value: 'E', label: 'The fleshiest parts (bust, hips, waist) become more pronounced' },
        ],
      },
    ],
  },
  {
    id: 'face',
    title: 'Facial Features',
    description: 'The overall impression of your face, not individual features',
    questions: [
      {
        id: 'face_1',
        sectionId: 'face',
        prompt: 'How would you describe the overall impression of your facial features?',
        options: [
          { value: 'A', label: 'Sleek and angular', description: 'Sharp, sculpted, chiselled — geometric precision' },
          { value: 'B', label: 'Open and blunt', description: 'Open and strong — irregular or bold in an uncontrived way' },
          { value: 'C', label: 'Symmetrical and balanced', description: 'Even, regular, classically proportioned' },
          { value: 'D', label: 'Small and sharp, or angular-cute', description: 'High-contrast — angular features in a compact face' },
          { value: 'E', label: 'Soft and rounded', description: 'Lush and curved — fullness in all the features' },
        ],
      },
      {
        id: 'face_2',
        sectionId: 'face',
        prompt: 'How would you describe your eyes?',
        options: [
          { value: 'A', label: 'Narrow, sloe or almond-shaped — elongated' },
          { value: 'B', label: 'Broad or straight — can be large and open or straight and strong' },
          { value: 'C', label: 'Moderate — evenly sized and symmetrical' },
          { value: 'D', label: 'Large relative to face, or sharp and defined' },
          { value: 'E', label: 'Large, round and soft — a defining feature' },
        ],
      },
      {
        id: 'face_3',
        sectionId: 'face',
        prompt: 'How would you describe your lips?',
        options: [
          { value: 'A', label: 'Narrow and thin, or firmly set' },
          { value: 'B', label: 'Slightly irregular or wide — natural, uncontrived' },
          { value: 'C', label: 'Moderate and symmetrically shaped' },
          { value: 'D', label: 'Small, possibly defined or slightly quirky' },
          { value: 'E', label: 'Full, rounded and lush' },
        ],
      },
      {
        id: 'face_4',
        sectionId: 'face',
        prompt: 'How would you describe your cheeks and nose?',
        options: [
          { value: 'A', label: 'Taut cheeks, prominent or refined nose — angular' },
          { value: 'B', label: 'Broad nose, fleshy or flat cheeks — open and strong' },
          { value: 'C', label: 'Symmetrical nose, moderately fleshed cheeks' },
          { value: 'D', label: 'Small nose, flat or lightly defined cheeks' },
          { value: 'E', label: 'Small, soft nose — fleshy, full cheeks' },
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
