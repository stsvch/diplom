export interface TestDto {
  id: string;
  title: string;
  description?: string;
  timeLimitMinutes?: number;
  maxAttempts?: number;
  deadline?: string;
  shuffleQuestions: boolean;
  shuffleAnswers: boolean;
  showCorrectAnswers: boolean;
  maxScore: number;
  questionsCount: number;
  createdById: string;
}

export interface TestDetailDto extends TestDto {
  questions: QuestionDto[];
}

export interface QuestionDto {
  id: string;
  type: string;
  text: string;
  points: number;
  orderIndex: number;
  answerOptions: AnswerOptionDto[];
}

export interface AnswerOptionDto {
  id: string;
  text: string;
  isCorrect: boolean;
  orderIndex: number;
  matchingPairValue?: string;
}

export interface StudentQuestionDto {
  id: string;
  type: string;
  text: string;
  points: number;
  orderIndex: number;
  answerOptions: StudentAnswerOptionDto[];
}

export interface StudentAnswerOptionDto {
  id: string;
  text: string;
  orderIndex: number;
  matchingPairValue?: string;
}

export interface TestAttemptDto {
  id: string;
  testId: string;
  studentId: string;
  attemptNumber: number;
  startedAt: string;
  completedAt?: string;
  score?: number;
  maxScore: number;
  status: string;
}

export interface TestAttemptDetailDto extends TestAttemptDto {
  responses: TestResponseDto[];
  questions?: QuestionDto[];
}

export interface TestResponseDto {
  id: string;
  questionId: string;
  selectedOptionIds?: string[];
  textAnswer?: string;
  isCorrect?: boolean;
  points?: number;
  teacherComment?: string;
}

export interface TestAttemptStartDto {
  attemptId: string;
  questions: StudentQuestionDto[];
  timeLimitMinutes?: number;
  attemptNumber: number;
}
