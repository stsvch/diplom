export interface LessonProgressDto {
  id: string;
  lessonId: string;
  studentId: string;
  isCompleted: boolean;
  completedAt?: string;
}

export interface CourseProgressDto {
  courseId: string;
  totalLessons: number;
  completedLessons: number;
  progressPercent: number;
}

export interface MyProgressDto {
  courses: CourseProgressDto[];
}
