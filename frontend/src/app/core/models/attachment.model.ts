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
