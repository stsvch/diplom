export interface DictionaryWordDto {
  id: string;
  courseId: string;
  courseTitle: string;
  term: string;
  translation?: string | null;
  definition?: string | null;
  example?: string | null;
  note?: string | null;
  imageUrl?: string | null;
  tags: string[];
  createdById: string;
  isKnown: boolean;
  reviewCount: number;
  hardCount: number;
  repeatLaterCount: number;
  lastReviewedAt?: string | null;
  lastOutcome?: string | null;
  nextReviewAt?: string | null;
  createdAt: string;
  updatedAt?: string | null;
}

export interface GlossaryFilters {
  courseId?: string;
  search?: string;
  knownOnly?: boolean;
}

export interface UpsertDictionaryWordRequest {
  courseId: string;
  term: string;
  translation?: string | null;
  definition?: string | null;
  example?: string | null;
  note?: string | null;
  tags: string[];
}

export type DictionaryReviewOutcome = 'Known' | 'Hard' | 'RepeatLater';
