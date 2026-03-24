import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { roleGuard } from './core/guards/role.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./layouts/guest-layout/guest-layout.component').then(
        (m) => m.GuestLayoutComponent,
      ),
    children: [
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'login',
      },
      {
        path: 'login',
        loadComponent: () =>
          import('./features/auth/login/login.component').then(
            (m) => m.LoginComponent,
          ),
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./features/auth/register/register.component').then(
            (m) => m.RegisterComponent,
          ),
      },
      {
        path: 'forgot-password',
        loadComponent: () =>
          import('./features/auth/forgot-password/forgot-password.component').then(
            (m) => m.ForgotPasswordComponent,
          ),
      },
      {
        path: 'confirm-email',
        loadComponent: () =>
          import('./features/auth/confirm-email/confirm-email.component').then(
            (m) => m.ConfirmEmailComponent,
          ),
      },
      {
        path: 'reset-password',
        loadComponent: () =>
          import('./features/auth/reset-password/reset-password.component').then(
            (m) => m.ResetPasswordComponent,
          ),
      },
    ],
  },
  {
    path: 'student',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Student' },
    loadComponent: () =>
      import('./layouts/app-layout/app-layout.component').then(
        (m) => m.AppLayoutComponent,
      ),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'courses',
        loadComponent: () =>
          import('./features/courses/my-courses/my-courses.component').then(
            (m) => m.MyCoursesComponent,
          ),
      },
      {
        path: 'catalog',
        loadComponent: () =>
          import('./features/courses/catalog/catalog.component').then(
            (m) => m.CatalogComponent,
          ),
      },
      {
        path: 'course/:id',
        loadComponent: () =>
          import('./features/courses/course-detail/course-detail.component').then(
            (m) => m.CourseDetailComponent,
          ),
      },
      {
        path: 'lesson/:id',
        loadComponent: () =>
          import('./features/courses/lesson-view/lesson-view.component').then(
            (m) => m.LessonViewComponent,
          ),
      },
      {
        path: 'calendar',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'messages',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'payments',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
    ],
  },
  {
    path: 'teacher',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Teacher' },
    loadComponent: () =>
      import('./layouts/app-layout/app-layout.component').then(
        (m) => m.AppLayoutComponent,
      ),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'courses',
        loadComponent: () =>
          import('./features/courses/teacher-courses/teacher-courses.component').then(
            (m) => m.TeacherCoursesComponent,
          ),
      },
      {
        path: 'courses/create',
        loadComponent: () =>
          import('./features/courses/create-course/create-course.component').then(
            (m) => m.CreateCourseComponent,
          ),
      },
      {
        path: 'courses/edit/:id',
        loadComponent: () =>
          import('./features/courses/create-course/create-course.component').then(
            (m) => m.CreateCourseComponent,
          ),
      },
      {
        path: 'lesson/:id/edit',
        loadComponent: () =>
          import('./features/courses/lesson-editor/lesson-editor.component').then(
            (m) => m.LessonEditorComponent,
          ),
      },
      {
        path: 'assignments',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'gradebook',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'schedule',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'reports',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'messages',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'glossary',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
    ],
  },
  {
    path: 'admin',
    canActivate: [authGuard, roleGuard],
    data: { role: 'Admin' },
    loadComponent: () =>
      import('./layouts/app-layout/app-layout.component').then(
        (m) => m.AppLayoutComponent,
      ),
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'courses',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'disciplines',
        loadComponent: () =>
          import('./features/admin/disciplines/disciplines.component').then(
            (m) => m.DisciplinesComponent,
          ),
      },
      {
        path: 'payments',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'analytics',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/placeholder/placeholder.component').then(
            (m) => m.PlaceholderComponent,
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: 'login',
  },
];
