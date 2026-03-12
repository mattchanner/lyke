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
}

export interface KibbeQuizQuestion {
  id: string;
  sectionId: KibbeSectionId;
  prompt: string;
  options: KibbeQuizOption[];
}

export interface KibbeQuizSection {
  id: KibbeSectionId;
  title: string;
  description: string;
  questions: KibbeQuizQuestion[];
}
