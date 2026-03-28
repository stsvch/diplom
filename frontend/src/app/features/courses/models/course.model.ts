export interface DisciplineDto {
  id: string;
  name: string;
  description?: string;
  imageUrl?: string;
  courseCount: number;
}

export interface CourseListDto {
  id: string;
  title: string;
  description: string;
  imageUrl?: string;
  teacherName: string;
  disciplineName: string;
  level: string;
  price?: number;
  isFree: boolean;
  rating?: number;
  studentsCount: number;
  lessonsCount: number;
  duration?: number;
  progress?: number;
  tags?: string;
}

export interface CourseDetailDto extends CourseListDto {
  modules: CourseModuleDetailDto[];
  isPublished: boolean;
  isArchived: boolean;
  orderType: string;
  hasGrading: boolean;
  createdAt: string;
}

export interface CourseModuleDetailDto {
  id: string;
  title: string;
  description?: string;
  orderIndex: number;
  isPublished: boolean;
  lessons: LessonDetailDto[];
}

export interface LessonDetailDto {
  id: string;
  title: string;
  description?: string;
  orderIndex: number;
  isPublished: boolean;
  duration?: number;
  blocksCount: number;
}

export interface CourseModuleDto {
  id: string;
  title: string;
  description?: string;
  orderIndex: number;
  isPublished: boolean;
  lessonsCount: number;
}

export interface LessonDto {
  id: string;
  title: string;
  description?: string;
  orderIndex: number;
  isPublished: boolean;
  duration?: number;
  blocksCount: number;
}

export interface LessonBlockDto {
  id: string;
  orderIndex: number;
  type: string;
  textContent?: string;
  videoUrl?: string;
  testId?: string;
  assignmentId?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CourseFilters {
  disciplineId?: string;
  isFree?: boolean;
  level?: string;
  search?: string;
  sortBy?: string;
  page?: number;
  pageSize?: number;
}

export type CourseLevel = 'Beginner' | 'Intermediate' | 'Advanced';
export type CourseStatus = 'Draft' | 'Published' | 'Archived';
