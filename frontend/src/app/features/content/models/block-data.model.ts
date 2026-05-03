import { LessonBlockType } from './block-type.model';

export interface LessonBlockSettings {
  points: number;
  requiredForCompletion: boolean;
  hint?: string;
  shuffleOptions?: boolean;
  showFeedback?: boolean;
  maxAttempts?: number;
}

// ─── Informational ─────────────────────────────────────────

export interface TextBlockData {
  type: 'Text';
  html: string;
}

export interface VideoBlockData {
  type: 'Video';
  url: string;
  caption?: string;
  posterUrl?: string;
}

export interface AudioBlockData {
  type: 'Audio';
  url: string;
  transcript?: string;
  durationSeconds?: number;
}

export interface ImageBlockData {
  type: 'Image';
  url: string;
  alt?: string;
  caption?: string;
}

export interface BannerBlockData {
  type: 'Banner';
  title: string;
  bgColor?: string;
  textColor?: string;
  imageUrl?: string;
}

export interface FileBlockData {
  type: 'File';
  attachmentId: string;
  displayName?: string;
  description?: string;
}

// ─── Auto-graded ───────────────────────────────────────────

export interface ChoiceOption {
  id: string;
  text: string;
  isCorrect: boolean;
}

export interface SingleChoiceBlockData {
  type: 'SingleChoice';
  instruction?: string;
  question: string;
  imageUrl?: string;
  options: ChoiceOption[];
}

export interface MultipleChoiceBlockData {
  type: 'MultipleChoice';
  instruction?: string;
  question: string;
  imageUrl?: string;
  options: ChoiceOption[];
  partialCredit: boolean;
}

export interface TrueFalseStatement {
  id: string;
  text: string;
  isTrue: boolean;
}

export interface TrueFalseBlockData {
  type: 'TrueFalse';
  instruction?: string;
  statements: TrueFalseStatement[];
}

export interface FillGapSlot {
  id: string;
  correctAnswers: string[];
  caseSensitive: boolean;
}

export interface FillGapSentence {
  id: string;
  template: string;
  gaps: FillGapSlot[];
}

export interface FillGapBlockData {
  type: 'FillGap';
  instruction?: string;
  sentences: FillGapSentence[];
}

export interface DropdownSlot {
  id: string;
  options: string[];
  correct: string;
}

export interface DropdownSentence {
  id: string;
  template: string;
  gaps: DropdownSlot[];
}

export interface DropdownBlockData {
  type: 'Dropdown';
  instruction?: string;
  sentences: DropdownSentence[];
}

export interface WordBankSentence {
  id: string;
  template: string;
  correctAnswers: string[];
}

export interface WordBankBlockData {
  type: 'WordBank';
  instruction?: string;
  bank: string[];
  sentences: WordBankSentence[];
  allowExtraWords: boolean;
}

export interface ReorderItem {
  id: string;
  text: string;
  imageUrl?: string;
}

export interface ReorderBlockData {
  type: 'Reorder';
  instruction?: string;
  items: ReorderItem[];
  correctOrder: string[];
  allOrNothing: boolean;
}

export interface MatchingItem {
  id: string;
  text: string;
  imageUrl?: string;
}

export interface MatchingPair {
  leftId: string;
  rightId: string;
}

export interface MatchingBlockData {
  type: 'Matching';
  instruction?: string;
  leftItems: MatchingItem[];
  rightItems: MatchingItem[];
  correctPairs: MatchingPair[];
}

// ─── Manual-graded ─────────────────────────────────────────

export type OpenTextLengthUnit = 'Chars' | 'Words';

export interface OpenTextBlockData {
  type: 'OpenText';
  instruction: string;
  prompt?: string;
  helperWords: string[];
  minLength?: number;
  maxLength?: number;
  unit: OpenTextLengthUnit;
}

export interface CodeTestCase {
  input: string;
  expectedOutput: string;
  isHidden: boolean;
}

export interface CodeExerciseBlockData {
  type: 'CodeExercise';
  instruction: string;
  language: string;
  starterCode?: string;
  testCases: CodeTestCase[];
  timeoutMs: number;
  memoryLimitMb: number;
  hiddenTests: boolean;
}

// ─── Composite ─────────────────────────────────────────────

export interface QuizBlockData {
  type: 'Quiz';
  testId: string;
}

export interface AssignmentBlockData {
  type: 'Assignment';
  assignmentId: string;
}

// ─── Discriminated union ───────────────────────────────────

export type LessonBlockData =
  | TextBlockData
  | VideoBlockData
  | AudioBlockData
  | ImageBlockData
  | BannerBlockData
  | FileBlockData
  | SingleChoiceBlockData
  | MultipleChoiceBlockData
  | TrueFalseBlockData
  | FillGapBlockData
  | DropdownBlockData
  | WordBankBlockData
  | ReorderBlockData
  | MatchingBlockData
  | OpenTextBlockData
  | CodeExerciseBlockData
  | QuizBlockData
  | AssignmentBlockData;

export type LessonBlockStatus = 'Draft' | 'Ready' | 'Invalid';

export interface LessonBlockDto {
  id: string;
  lessonId: string;
  orderIndex: number;
  type: LessonBlockType;
  data: LessonBlockData;
  settings: LessonBlockSettings;
  /** Бэк-поле soft-валидации: Draft если есть ошибки, Ready если данные полные. */
  status?: LessonBlockStatus;
  /** Список ошибок валидации (если статус Draft/Invalid). */
  validationErrors?: string[];
  createdAt: string;
  updatedAt?: string;
}
