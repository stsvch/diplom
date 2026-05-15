// tag.model.ts
// Модели описывают DTO и типы, которыми frontend обменивается с backend API.
export interface TagDto {
  id: string;
  slug: string;
  name: string;
  usageCount: number;
}
