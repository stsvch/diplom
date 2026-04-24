export interface StudentDashboardSummaryDto {
  enrolledCourses: number;
  activeCourses: number;
  completedCourses: number;
  completedLessons: number;
  totalLessons: number;
  overallProgressPercent: number;
  averageGradePercent: number;
  upcomingEventsCount: number;
}

export interface StudentDashboardCourseDto {
  courseId: string;
  title: string;
  disciplineName: string;
  teacherName: string;
  imageUrl?: string | null;
  deadline?: string | null;
  completedLessons: number;
  totalLessons: number;
  progressPercent: number;
  isCompleted: boolean;
}

export interface StudentDashboardGradeDto {
  id: string;
  courseId: string;
  courseName: string;
  title: string;
  sourceType: string;
  score: number;
  maxScore: number;
  percent: number;
  gradedAt: string;
}

export interface StudentDashboardUpcomingItemDto {
  id: string;
  courseId?: string | null;
  courseName?: string | null;
  title: string;
  description?: string | null;
  eventDate: string;
  eventTime?: string | null;
  type: string;
  sourceType?: string | null;
  sourceId?: string | null;
  status?: string | null;
}

export interface StudentDashboardDto {
  summary: StudentDashboardSummaryDto;
  courses: StudentDashboardCourseDto[];
  recentGrades: StudentDashboardGradeDto[];
  upcoming: StudentDashboardUpcomingItemDto[];
}

export interface TeacherDashboardSummaryDto {
  totalCourses: number;
  publishedCourses: number;
  activeStudents: number;
  pendingReviewsCount: number;
  averageStudentProgressPercent: number;
  averageGradePercent: number;
  upcomingSessionsCount: number;
}

export interface TeacherDashboardEarningsDto {
  readyForPayoutAmount: number;
  inPayoutAmount: number;
  paidOutAmount: number;
  currency: string;
}

export interface TeacherDashboardCourseDto {
  courseId: string;
  title: string;
  disciplineName: string;
  isPublished: boolean;
  isArchived: boolean;
  activeStudents: number;
  pendingReviewsCount: number;
  averageStudentProgressPercent: number;
  averageGradePercent: number;
}

export interface TeacherDashboardReviewItemDto {
  kind: string;
  sourceId: string;
  reviewId: string;
  courseId?: string | null;
  courseName?: string | null;
  title: string;
  studentId: string;
  studentName: string;
  submittedAt: string;
}

export interface TeacherDashboardSessionDto {
  slotId: string;
  courseId?: string | null;
  title: string;
  courseName?: string | null;
  startTime: string;
  endTime: string;
  status: string;
  isGroupSession: boolean;
  bookingsCount: number;
  maxStudents: number;
}

export interface TeacherDashboardDto {
  summary: TeacherDashboardSummaryDto;
  earnings: TeacherDashboardEarningsDto;
  courses: TeacherDashboardCourseDto[];
  pendingReviews: TeacherDashboardReviewItemDto[];
  upcomingSessions: TeacherDashboardSessionDto[];
}

export interface TeacherCourseReportSummaryDto {
  courseId: string;
  title: string;
  disciplineName: string;
  isPublished: boolean;
  isArchived: boolean;
  activeStudents: number;
  totalLessons: number;
  averageProgressPercent: number;
  completionRatePercent: number;
  averageGradePercent: number;
  pendingReviewsCount: number;
  overdueStudentsCount: number;
  overdueAssignmentsCount: number;
  overdueTestsCount: number;
  upcomingDeadlinesCount: number;
}

export interface TeacherCourseGradeBucketDto {
  label: string;
  count: number;
  sharePercent: number;
}

export interface TeacherCourseRiskStudentDto {
  studentId: string;
  studentName: string;
  completedLessons: number;
  totalLessons: number;
  progressPercent: number;
  averageGradePercent?: number | null;
  overdueAssignmentsCount: number;
  overdueTestsCount: number;
  pendingReviewCount: number;
}

export interface TeacherCourseDeadlineItemDto {
  kind: string;
  sourceId: string;
  title: string;
  deadline: string;
  isOverdue: boolean;
  affectedStudentsCount: number;
  pendingReviewsCount: number;
}

export interface TeacherCourseReportDto {
  summary: TeacherCourseReportSummaryDto;
  gradeDistribution: TeacherCourseGradeBucketDto[];
  atRiskStudents: TeacherCourseRiskStudentDto[];
  deadlines: TeacherCourseDeadlineItemDto[];
}
