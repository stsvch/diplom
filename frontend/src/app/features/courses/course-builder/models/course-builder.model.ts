// ── Course Builder DTOs (повторяют backend/Host/Models/Courses/CourseBuilderDto.cs) ──

export type CourseItemType =
  | 'Lesson'
  | 'Test'
  | 'Assignment'
  | 'LiveSession'
  | 'Resource'
  | 'ExternalLink';

export type CourseItemStatus =
  | 'Draft'
  | 'NeedsContent'
  | 'Ready'
  | 'Published'
  | 'Archived';

export interface CourseBuilderItemDto {
  /** Id записи в таблице CourseItems. Может быть null для inline-сущностей до синка. */
  courseItemId: string | null;
  /** Id source-сущности (Lesson/Test/Assignment/ScheduleSlot/CourseItem.Id для standalone) */
  sourceId: string;
  sectionId: string | null;
  type: CourseItemType;
  title: string;
  description?: string | null;
  url?: string | null;
  attachmentId?: string | null;
  resourceKind?: string | null;
  orderIndex: number;
  status: CourseItemStatus;
  isPublished: boolean;
  isRequired: boolean;
  points?: number | null;
  deadline?: string | null;
  availableFrom?: string | null;
  durationMinutes?: number | null;
  blocksCount?: number | null;
  questionsCount?: number | null;
  attemptsCount?: number | null;
  submissionsCount?: number | null;
  startTime?: string | null;
  endTime?: string | null;
  maxStudents?: number | null;
  bookedCount?: number | null;
  meetingLink?: string | null;
}

export interface CourseBuilderSectionDto {
  id: string;
  title: string;
  description?: string | null;
  orderIndex: number;
  isPublished: boolean;
  items: CourseBuilderItemDto[];
}

export interface CourseBuilderCourseDto {
  id: string;
  disciplineId: string;
  disciplineName: string;
  teacherId: string;
  teacherName: string;
  title: string;
  description: string;
  imageUrl?: string | null;
  level: 'Beginner' | 'Intermediate' | 'Advanced' | string;
  price?: number | null;
  isFree: boolean;
  isPublished: boolean;
  isArchived: boolean;
  archiveReason?: string | null;
  orderType: string;
  hasGrading: boolean;
  hasCertificate: boolean;
  deadline?: string | null;
  tags?: string | null;
  createdAt: string;
  studentsCount: number;
  sectionsCount: number;
  lessonsCount: number;
  testsCount: number;
  assignmentsCount: number;
  liveSessionsCount: number;
}

export interface CourseBuilderReadinessIssueDto {
  severity: 'Error' | 'Warning';
  code: string;
  message: string;
  itemType?: string | null;
  sourceId?: string | null;
  sectionId?: string | null;
}

export interface CourseBuilderReadinessDto {
  totalItems: number;
  readyItems: number;
  readyPercent: number;
  errorCount: number;
  warningCount: number;
  issues: CourseBuilderReadinessIssueDto[];
}

export interface CourseBuilderDto {
  course: CourseBuilderCourseDto;
  sections: CourseBuilderSectionDto[];
  unsectionedItems: CourseBuilderItemDto[];
  readiness: CourseBuilderReadinessDto;
}

// ── Standalone item requests (Resource/ExternalLink) ──

export interface CreateStandaloneCourseItemRequest {
  type: 'Resource' | 'ExternalLink';
  sectionId?: string | null;
  title: string;
  description?: string | null;
  url?: string | null;
  attachmentId?: string | null;
  resourceKind?: string | null;
  isRequired?: boolean;
  points?: number | null;
  availableFrom?: string | null;
  deadline?: string | null;
}

export interface UpdateStandaloneCourseItemRequest {
  title: string;
  description?: string | null;
  url?: string | null;
  attachmentId?: string | null;
  resourceKind?: string | null;
}

export interface UpdateCourseItemMetadataRequest {
  isRequired: boolean;
  points?: number | null;
  availableFrom?: string | null;
  deadline?: string | null;
  status: CourseItemStatus;
}

export interface MoveCourseItemRequest {
  sectionId?: string | null;
  orderIndex: number;
}

export interface ReorderCourseItemsRequest {
  sectionId?: string | null;
  itemIds: string[];
}

export interface CourseItemDto {
  id: string;
  courseId: string;
  sectionId?: string | null;
  type: CourseItemType;
  sourceId: string;
  title: string;
  description?: string | null;
  url?: string | null;
  attachmentId?: string | null;
  resourceKind?: string | null;
  orderIndex: number;
  status: CourseItemStatus;
  isRequired: boolean;
  points?: number | null;
  availableFrom?: string | null;
  deadline?: string | null;
}

export interface CourseItemBackfillDto {
  createdItemsCount: number;
  lessonsCount: number;
  testsCount: number;
  assignmentsCount: number;
  liveSessionsCount: number;
}

// ── UI selection ──

export type Selection =
  | { kind: 'none' }
  | { kind: 'course_info' }
  | { kind: 'item'; sectionId: string | null; itemId: string };

// ── Конфиг типов для UI ──

export const COURSE_ITEM_TYPE_LABELS: Record<CourseItemType, string> = {
  Lesson: 'Урок',
  Test: 'Тест',
  Assignment: 'Задание',
  LiveSession: 'Live-занятие',
  Resource: 'Материал',
  ExternalLink: 'Ссылка',
};

export const COURSE_ITEM_TYPE_DESCRIPTIONS: Record<CourseItemType, string> = {
  Lesson: 'Текст, видео, изображения, файлы',
  Test: 'Вопросы с баллами и проходным порогом',
  Assignment: 'Практическая работа с ручной проверкой',
  LiveSession: 'Онлайн-встреча по расписанию',
  Resource: 'Файл, PDF, презентация',
  ExternalLink: 'Ресурс вне платформы',
};
