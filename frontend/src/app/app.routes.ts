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
        path: 'test/:testId/play',
        loadComponent: () =>
          import('./features/tests/test-player/test-player.component').then(
            (m) => m.TestPlayerComponent,
          ),
      },
      {
        path: 'test/:testId/result/:attemptId',
        loadComponent: () =>
          import('./features/tests/test-result/test-result.component').then(
            (m) => m.TestResultComponent,
          ),
      },
      {
        path: 'assignment/:id',
        loadComponent: () =>
          import('./features/assignments/assignment-submit/assignment-submit.component').then(
            (m) => m.AssignmentSubmitComponent,
          ),
      },
      {
        path: 'grades',
        loadComponent: () =>
          import('./features/grading/student-grades/student-grades.component').then(
            (m) => m.StudentGradesComponent,
          ),
      },
      {
        path: 'calendar',
        loadComponent: () =>
          import('./features/calendar/calendar-page/calendar-page.component').then(
            (m) => m.CalendarPageComponent,
          ),
      },
      {
        path: 'schedule',
        loadComponent: () =>
          import('./features/scheduling/student-schedule/student-schedule.component').then(
            (m) => m.StudentScheduleComponent,
          ),
      },
      {
        path: 'messages',
        loadComponent: () =>
          import('./features/messaging/messages-page/messages-page.component').then(
            (m) => m.MessagesPageComponent,
          ),
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/notifications/notifications-page/notifications-page.component').then(
            (m) => m.NotificationsPageComponent,
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
        path: 'test/new',
        loadComponent: () =>
          import('./features/tests/test-editor/test-editor.component').then(
            (m) => m.TestEditorComponent,
          ),
      },
      {
        path: 'test/:id/edit',
        loadComponent: () =>
          import('./features/tests/test-editor/test-editor.component').then(
            (m) => m.TestEditorComponent,
          ),
      },
      {
        path: 'test/:testId/submissions',
        loadComponent: () =>
          import('./features/tests/test-submissions/test-submissions.component').then(
            (m) => m.TestSubmissionsComponent,
          ),
      },
      {
        path: 'test/:testId/grade/:attemptId',
        loadComponent: () =>
          import('./features/tests/test-grading/test-grading.component').then(
            (m) => m.TestGradingComponent,
          ),
      },
      {
        path: 'assignments',
        loadComponent: () =>
          import('./features/assignments/assignment-grading/assignment-grading.component').then(
            (m) => m.AssignmentGradingComponent,
          ),
      },
      {
        path: 'assignment/new',
        loadComponent: () =>
          import('./features/assignments/assignment-editor/assignment-editor.component').then(
            (m) => m.AssignmentEditorComponent,
          ),
      },
      {
        path: 'assignment/:id/edit',
        loadComponent: () =>
          import('./features/assignments/assignment-editor/assignment-editor.component').then(
            (m) => m.AssignmentEditorComponent,
          ),
      },
      {
        path: 'gradebook',
        loadComponent: () =>
          import('./features/grading/gradebook/gradebook.component').then(
            (m) => m.GradebookComponent,
          ),
      },
      {
        path: 'schedule',
        loadComponent: () =>
          import('./features/scheduling/teacher-schedule/teacher-schedule.component').then(
            (m) => m.TeacherScheduleComponent,
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
          import('./features/messaging/messages-page/messages-page.component').then(
            (m) => m.MessagesPageComponent,
          ),
      },
      {
        path: 'notifications',
        loadComponent: () =>
          import('./features/notifications/notifications-page/notifications-page.component').then(
            (m) => m.NotificationsPageComponent,
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
