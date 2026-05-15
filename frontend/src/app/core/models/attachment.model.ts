// attachment.model.ts
// Модели описывают DTO и типы, которыми frontend обменивается с backend API.
export interface AttachmentDto {
  id: string;
  fileName: string;
  fileUrl: string;
  contentType: string;
  fileSize: number;
  entityType: string;
  entityId: string | null;
  uploadedById: string;
  createdAt: string;
}
