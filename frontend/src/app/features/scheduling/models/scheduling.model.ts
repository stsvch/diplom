export enum SlotStatus {
  Available = 'Available',
  Booked = 'Booked',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
}

export enum BookingStatus {
  Booked = 'Booked',
  Completed = 'Completed',
  Cancelled = 'Cancelled',
}

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
  courseId?: string;
  courseName?: string;
  title: string;
  description?: string;
  startTime: string;
  endTime: string;
  isGroupSession: boolean;
  maxStudents: number;
  status: SlotStatus;
  meetingLink?: string;
  bookedCount: number;
  bookings: BookingDto[];
}

export interface CreateSlotRequest {
  courseId?: string;
  courseName?: string;
  title: string;
  description?: string;
  startTime: string;
  endTime: string;
  isGroupSession: boolean;
  maxStudents: number;
  meetingLink?: string;
}

export interface UpdateSlotRequest {
  title?: string;
  description?: string;
  startTime?: string;
  endTime?: string;
  meetingLink?: string;
  maxStudents?: number;
}
