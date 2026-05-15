// user.model.ts
// Модели описывают DTO и типы, которыми frontend обменивается с backend API.
export enum UserRole {
  Admin = 'Admin',
  Teacher = 'Teacher',
  Student = 'Student',
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  avatarUrl?: string;
  emailConfirmed: boolean;
}
