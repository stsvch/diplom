import { LessonBlockType } from './block-type.model';

export interface SingleChoiceAnswer {
  type: 'SingleChoice';
  selectedOptionId: string;
}

export interface MultipleChoiceAnswer {
  type: 'MultipleChoice';
  selectedOptionIds: string[];
}

export interface TrueFalseResponse {
  statementId: string;
  answer: boolean;
}

export interface TrueFalseAnswer {
  type: 'TrueFalse';
  responses: TrueFalseResponse[];
}

export interface FillGapValue {
  gapId: string;
  value: string;
}

export interface FillGapResponse {
  sentenceId: string;
  gaps: FillGapValue[];
}

export interface FillGapAnswer {
  type: 'FillGap';
  responses: FillGapResponse[];
}

export interface DropdownAnswer {
  type: 'Dropdown';
  responses: FillGapResponse[];
}

export interface WordBankResponse {
  sentenceId: string;
  answers: string[];
}

export interface WordBankAnswer {
  type: 'WordBank';
  responses: WordBankResponse[];
}

export interface ReorderAnswer {
  type: 'Reorder';
  order: string[];
}

export interface MatchingAnswerPair {
  leftId: string;
  rightId: string;
}

export interface MatchingAnswer {
  type: 'Matching';
  pairs: MatchingAnswerPair[];
}

export interface OpenTextAnswer {
  type: 'OpenText';
  text: string;
}

export interface CodeTestCaseResult {
  input: string;
  expectedOutput: string;
  actualOutput: string;
  passed: boolean;
  isHidden: boolean;
  error?: string | null;
}

export interface CodeExerciseAnswer {
  type: 'CodeExercise';
  code: string;
  runOutput?: CodeTestCaseResult[];
}

export type LessonBlockAnswer =
  | SingleChoiceAnswer
  | MultipleChoiceAnswer
  | TrueFalseAnswer
  | FillGapAnswer
  | DropdownAnswer
  | WordBankAnswer
  | ReorderAnswer
  | MatchingAnswer
  | OpenTextAnswer
  | CodeExerciseAnswer;

export type LessonBlockAttemptStatus = 'Draft' | 'Submitted' | 'Graded';

export interface LessonBlockAttemptDto {
  id: string;
  blockId: string;
  userId: string;
  answers: LessonBlockAnswer;
  score: number;
  maxScore: number;
  isCorrect: boolean;
  needsReview: boolean;
  attemptsUsed: number;
  status: LessonBlockAttemptStatus;
  submittedAt: string;
  reviewedAt?: string;
  reviewerId?: string;
  reviewerComment?: string;
}

export interface SubmitAttemptResult {
  attemptId: string;
  score: number;
  maxScore: number;
  isCorrect: boolean;
  needsReview: boolean;
  attemptsUsed: number;
  attemptsRemaining?: number | null;
  feedback?: string;
}

export interface LessonProgressDto {
  lessonId: string;
  totalBlocks: number;
  requiredBlocks: number;
  completedBlocks: number;
  totalScore: number;
  maxScore: number;
  percentage: number;
  isCompleted: boolean;
}

export type CodeExerciseRunKind = 'Run' | 'Submission';

export interface CodeExerciseRunDto {
  id: string;
  blockId: string;
  userId: string;
  userName?: string | null;
  attemptId?: string | null;
  kind: CodeExerciseRunKind | string;
  language: string;
  code: string;
  ok: boolean;
  globalError?: string | null;
  results: CodeTestCaseResult[];
  createdAt: string;
  blockOrderIndex?: number | null;
  blockLabel?: string | null;
  attemptStatus?: LessonBlockAttemptStatus | string | null;
  attemptScore?: number | null;
  attemptMaxScore?: number | null;
  attemptNeedsReview?: boolean | null;
  attemptReviewedAt?: string | null;
  attemptReviewerComment?: string | null;
  attemptAttemptsUsed?: number | null;
}
