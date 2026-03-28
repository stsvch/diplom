export interface AssignmentDto {
  id: string;
  title: string;
  description: string;
  criteria?: string;
  deadline?: string;
  maxAttempts?: number;
  maxScore: number;
  createdById: string;
  submissionsCount: number;
}

export interface AssignmentDetailDto extends AssignmentDto {
  submissions: SubmissionDto[];
}

export interface SubmissionDto {
  id: string;
  assignmentId: string;
  studentId: string;
  attemptNumber: number;
  content?: string;
  submittedAt: string;
  status: string;
  score?: number;
  maxScore: number;
  teacherComment?: string;
  gradedAt?: string;
}

export interface CreateAssignmentDto {
  title: string;
  description: string;
  criteria?: string;
  deadline?: string;
  maxAttempts?: number;
  maxScore: number;
}

export interface GradeSubmissionDto {
  score: number;
  comment?: string;
  returnForRevision?: boolean;
}
