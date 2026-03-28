export enum NotificationType {
  Grade = 'Grade',
  Deadline = 'Deadline',
  Message = 'Message',
  Course = 'Course',
  Achievement = 'Achievement',
}

export interface NotificationDto {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  isRead: boolean;
  linkUrl?: string;
  createdAt: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
