// scheduling.model.ts
// Модели описывают DTO и типы, которыми frontend обменивается с backend API.
export enum SlotStatus {
  Available = 'Available',
  Booked = 'Booked',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  Full = 'Full',
}

export enum BookingStatus {
  Booked = 'Booked',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  LateCancelled = 'LateCancelled',
}

export enum SessionType {
  Individual = 'Individual',
  Group = 'Group',
}

export enum AvailabilityKind {
  Recurring = 'Recurring',
  OneOff = 'OneOff',
}

export type DayOfWeek =
  | 'Sunday' | 'Monday' | 'Tuesday' | 'Wednesday'
  | 'Thursday' | 'Friday' | 'Saturday';

export interface BookingDto {
  id: string;
  studentId: string;
  studentName: string;
  bookedAt: string;
  status: BookingStatus;
}

export interface ScheduleSlotDto {
  id: string;
  teacherId: string;
  teacherName: string;
  availabilityId?: string;
  title: string;
  description?: string;
  startTime: string;
  endTime: string;
  sessionType: SessionType;
  maxStudents: number;
  requiredCourseId?: string;
  status: SlotStatus;
  meetingLink?: string;
  bookedCount: number;
  bookings: BookingDto[];
}

export interface TeacherAvailabilityDto {
  id: string;
  teacherId: string;
  teacherName: string;
  kind: AvailabilityKind;
  dayOfWeek?: DayOfWeek;
  specificDate?: string;          // 'YYYY-MM-DD'
  startTime: string;              // 'HH:mm:ss'
  endTime: string;                // 'HH:mm:ss'
  slotDurationMinutes: number;
  breakBetweenMinutes: number;
  validFrom: string;              // 'YYYY-MM-DD'
  validUntil?: string;            // 'YYYY-MM-DD'
  sessionType: SessionType;
  maxStudents: number;
  title: string;
  description?: string;
  meetingLink?: string;
  requiredCourseId?: string;
  isActive: boolean;
}

export interface CalendarSlotDto {
  availabilityId: string;
  slotId?: string;
  startTime: string;
  endTime: string;
  sessionType: SessionType;
  maxStudents: number;
  bookedCount: number;
  title: string;
  description?: string;
  requiredCourseId?: string;
  isBookedByCurrentUser: boolean;
  isAvailable: boolean;
}

export interface CreateAvailabilityRequest {
  kind: AvailabilityKind;
  dayOfWeek?: DayOfWeek;
  specificDate?: string;
  startTime: string;
  endTime: string;
  slotDurationMinutes: number;
  breakBetweenMinutes: number;
  validFrom: string;
  validUntil?: string;
  sessionType: SessionType;
  maxStudents: number;
  title: string;
  description?: string;
  meetingLink?: string;
  requiredCourseId?: string;
}

export interface BookSlotRequest {
  availabilityId: string;
  startTime: string;
}

export interface TeacherWithScheduleDto {
  teacherId: string;
  teacherName: string;
  activeRulesCount: number;
  individualRulesCount: number;
  groupRulesCount: number;
}

export interface UpdateSlotRequest {
  title?: string;
  description?: string;
  startTime?: string;
  endTime?: string;
  meetingLink?: string;
  maxStudents?: number;
}
