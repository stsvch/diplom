export type GradeSourceType = 'Test' | 'Assignment';

export interface GradeDto {
  id: string;
  studentId: string;
  courseId: string;
  sourceType: GradeSourceType;
  title: string;
  score: number;
  maxScore: number;
  comment?: string;
  gradedAt: string;
}

export interface StudentGradesDto {
  studentId: string;
  studentName: string;
  grades: GradeDto[];
  averageScore: number;
}

export interface GradebookDto {
  courseId: string;
  courseName: string;
  students: StudentGradesDto[];
}

export interface GradebookStatsDto {
  studentCount: number;
  averageScore: number;
  totalSubmissions: number;
  passingCount: number;
}

export interface CreateGradeDto {
  studentId: string;
  courseId: string;
  sourceType: GradeSourceType;
  testAttemptId?: string;
  assignmentSubmissionId?: string;
  title: string;
  score: number;
  maxScore: number;
  comment?: string;
}

export interface UpdateGradeDto {
  score: number;
  maxScore: number;
  comment?: string;
}
