export type LessonBlockType =
  | 'Text'
  | 'Video'
  | 'Audio'
  | 'Image'
  | 'Banner'
  | 'File'
  | 'SingleChoice'
  | 'MultipleChoice'
  | 'TrueFalse'
  | 'FillGap'
  | 'Dropdown'
  | 'WordBank'
  | 'Reorder'
  | 'Matching'
  | 'OpenText'
  | 'CodeExercise'
  | 'Quiz'
  | 'Assignment';

export const INFORMATIONAL_TYPES: LessonBlockType[] = [
  'Text', 'Video', 'Audio', 'Image', 'Banner', 'File',
];

export const AUTO_GRADED_TYPES: LessonBlockType[] = [
  'SingleChoice', 'MultipleChoice', 'TrueFalse',
  'FillGap', 'Dropdown', 'WordBank', 'Reorder', 'Matching',
];

export const MANUAL_GRADED_TYPES: LessonBlockType[] = [
  'OpenText', 'CodeExercise',
];

export const COMPOSITE_TYPES: LessonBlockType[] = [
  'Quiz', 'Assignment',
];

export function isInformational(type: LessonBlockType): boolean {
  return INFORMATIONAL_TYPES.includes(type);
}

export function isAutoGraded(type: LessonBlockType): boolean {
  return AUTO_GRADED_TYPES.includes(type);
}

export function isManualGraded(type: LessonBlockType): boolean {
  return MANUAL_GRADED_TYPES.includes(type);
}

export function isComposite(type: LessonBlockType): boolean {
  return COMPOSITE_TYPES.includes(type);
}

export function isCheckable(type: LessonBlockType): boolean {
  return isAutoGraded(type) || isManualGraded(type) || isComposite(type);
}
