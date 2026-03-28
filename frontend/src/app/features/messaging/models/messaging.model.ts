export interface ParticipantDto {
  userId: string;
  name: string;
}

export interface AttachmentDto {
  fileName: string;
  fileUrl: string;
  contentType: string;
  fileSize: number;
}

export interface ChatDto {
  id: string;
  type: string; // 'DirectMessage' | 'CourseChat'
  courseId?: string;
  courseName?: string;
  participants: ParticipantDto[];
  lastMessage?: string;
  lastMessageAt?: string;
  unreadCount: number;
}

export interface MessageDto {
  id: string;
  chatId: string;
  senderId: string;
  senderName: string;
  text: string;
  attachments: AttachmentDto[];
  sentAt: string;
  readBy: string[];
  isEdited: boolean;
}

export interface SendMessageRequest {
  text: string;
  attachments?: AttachmentDto[];
}

export interface CreateDirectChatRequest {
  recipientId: string;
  recipientName: string;
}

export interface CreateCourseChatRequest {
  courseId: string;
  courseName: string;
  participantIds: string[];
  participantNames?: string[];
}
