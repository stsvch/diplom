export enum CalendarEventType {
  Deadline = 'Deadline',
  Lesson = 'Lesson',
  Quiz = 'Quiz',
  Workshop = 'Workshop',
  Custom = 'Custom',
}

export interface CalendarEventDto {
  id: string;
  title: string;
  description?: string;
  eventDate: string;
  eventTime?: string;
  type: CalendarEventType;
  courseId?: string;
  sourceType?: string;
  sourceId?: string;
}
