export interface AdminUserDto {
  id: string;
  email: string;
  fullName: string;
  role: string;
  isBlocked: boolean;
  emailConfirmed: boolean;
  createdAt: string;
}

export interface AdminCourseDto {
  id: string;
  title: string;
  teacherId: string;
  teacherName: string;
  disciplineId: string;
  disciplineName: string;
  isPublished: boolean;
  isArchived: boolean;
  archiveReason?: string;
  studentsCount: number;
  modulesCount: number;
  createdAt: string;
}

export interface PlatformSettingsDto {
  registrationOpen: boolean;
  maintenanceMode: boolean;
  platformName: string;
  supportEmail: string;
}

export interface UserStatsDto {
  total: number;
  students: number;
  teachers: number;
  admins: number;
  blocked: number;
  unconfirmedEmail: number;
  newLast7Days: number;
}

export interface CourseStatsDto {
  total: number;
  published: number;
  drafts: number;
  archived: number;
  totalEnrollments: number;
  disciplines: number;
}

export interface DashboardStatsDto {
  users: UserStatsDto;
  courses: CourseStatsDto;
}

export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  password: string;
}

export interface ChangeRoleRequest {
  role: string;
}

export interface ForceArchiveRequest {
  reason: string;
}
