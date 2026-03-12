export type KibbeFamily =
  | 'Dramatic'
  | 'Natural'
  | 'Classic'
  | 'Gamine'
  | 'Romantic';

export type KibbeConfidence = 'High' | 'Medium' | 'Low';

export type KibbeOption = 'A' | 'B' | 'C' | 'D' | 'E';

export type KibbeSectionId = 'bone' | 'flesh' | 'face';

// --- Request types ---

export interface KibbeAnswer {
  sectionId: KibbeSectionId;
  questionId: string;
  selectedOption: KibbeOption;
}

export interface KibbeScoreRequest {
  answers: KibbeAnswer[];
}

export interface SaveStyleProfileRequest {
  score: KibbeScoreResponse;
}

export interface OverrideStyleProfileRequest {
  family: KibbeFamily;
}

// --- Response types ---

export interface KibbeSectionDominance {
  bone: KibbeFamily;
  flesh: KibbeFamily;
  face: KibbeFamily;
}

export interface KibbeCounts {
  a: number;
  b: number;
  c: number;
  d: number;
  e: number;
}

export interface KibbeScoreResponse {
  primaryFamily: KibbeFamily;
  runnerUpFamily: KibbeFamily | null;
  sectionDominance: KibbeSectionDominance;
  isMixed: boolean;
  confidence: KibbeConfidence;
  counts: KibbeCounts;
}

export interface StyleProfileResponse {
  primaryFamily: KibbeFamily | null;
  runnerUpFamily: KibbeFamily | null;
  isMixed: boolean;
  confidence: KibbeConfidence | null;
  sectionDominance: KibbeSectionDominance | null;
  isUserOverride: boolean;
  overrideFamily: KibbeFamily | null;
  computedAt: string | null;
  lastUpdatedAt: string | null;
}

// --- Quiz UI types ---

export interface KibbeQuizOption {
  value: KibbeOption;
  label: string;
  description?: string;
}

export interface KibbeQuizQuestion {
  id: string;
  sectionId: KibbeSectionId;
  prompt: string;
  subPrompt?: string;
  options: KibbeQuizOption[];
}

// Maps a primary+runnerUp combination to its named Kibbe mixed type.
// Returns null for combinations not in the classical Kibbe system.
export function getMixedTypeName(
  primary: KibbeFamily,
  runnerUp: KibbeFamily | null | undefined,
): string | null {
  if (!runnerUp) return null;
  const lookup: Partial<Record<string, string>> = {
    'Dramatic+Romantic': 'Soft Dramatic',
    'Natural+Dramatic': 'Flamboyant Natural',
    'Natural+Romantic': 'Soft Natural',
    'Classic+Dramatic': 'Dramatic Classic',
    'Classic+Romantic': 'Soft Classic',
    'Romantic+Dramatic': 'Theatrical Romantic',
    'Gamine+Dramatic': 'Flamboyant Gamine',
    'Gamine+Romantic': 'Soft Gamine',
  };
  return lookup[`${primary}+${runnerUp}`] ?? null;
}

export interface KibbeQuizSection {
  id: KibbeSectionId;
  title: string;
  description: string;
  questions: KibbeQuizQuestion[];
}
