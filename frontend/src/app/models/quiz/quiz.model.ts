export interface QuizAnswer {
  questionId: number;
  answerIndex: number;
}

export interface QuizRequest {
  answers: QuizAnswer[];
}

export interface QuizResponse {
  bodyTypeId: number;
  bodyTypeName: string;
  stature: string;
  build: string;
  resultLabel: string;
  resultDescription: string;
}
