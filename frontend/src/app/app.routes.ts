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
        loadComponent: () =>
          import('./features/public/landing/landing.component').then(
            (m) => m.LandingComponent,
          ),
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
        path: 'welcome',
        redirectTo: '',
        pathMatch: 'full',
      },
    ],
  },
  {
    path: 'messages/:chatId',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/messaging/message-link-redirect/message-link-redirect.component').then(
        (m) => m.MessageLinkRedirectComponent,
      ),
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
          import('./features/reports/student-dashboard/student-dashboard.component').then(
            (m) => m.StudentDashboardComponent,
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
          import('./features/courses/lesson-view-host/lesson-view-host.component').then(
            (m) => m.LessonViewHostComponent,
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
        path: 'messages/:chatId',
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
          import('./features/payments/student-payments/student-payments.component').then(
            (m) => m.StudentPaymentsComponent,
          ),
      },
      {
        path: 'glossary',
        loadComponent: () =>
          import('./features/tools/glossary-page/glossary-page.component').then(
            (m) => m.GlossaryPageComponent,
          ),
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./features/profile/profile.component').then((m) => m.ProfileComponent),
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
          import('./features/reports/teacher-dashboard/teacher-dashboard.component').then(
            (m) => m.TeacherDashboardComponent,
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
        path: 'courses/new',
        loadComponent: () =>
          import('./features/courses/course-new/course-new.component').then(
            (m) => m.CourseNewComponent,
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
        path: 'courses/:id/editor',
        loadComponent: () =>
          import('./features/courses/course-editor/course-editor.component').then(
            (m) => m.CourseEditorComponent,
          ),
      },
      {
        path: 'courses/:id/builder',
        loadComponent: () =>
          import('./features/courses/course-builder/course-builder.component').then(
            (m) => m.CourseBuilderComponent,
          ),
      },
      {
        path: 'courses/:id/preview',
        loadComponent: () =>
          import('./features/courses/course-preview/course-preview-host.component').then(
            (m) => m.CoursePreviewHostComponent,
          ),
      },
      {
        path: 'lesson-preview/:id',
        loadComponent: () =>
          import('./features/courses/course-preview/lesson-preview-host.component').then(
            (m) => m.LessonPreviewHostComponent,
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
        path: 'lesson/:id/review',
        loadComponent: () =>
          import('./features/content/code-run-review/code-run-review.component').then(
            (m) => m.CodeRunReviewComponent,
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
        path: 'calendar',
        loadComponent: () =>
          import('./features/calendar/calendar-page/calendar-page.component').then(
            (m) => m.CalendarPageComponent,
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
          import('./features/reports/teacher-course-reports/teacher-course-reports.component').then(
            (m) => m.TeacherCourseReportsComponent,
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
        path: 'messages/:chatId',
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
          import('./features/payments/teacher-payouts/teacher-payouts.component').then(
            (m) => m.TeacherPayoutsComponent,
          ),
      },
      {
        path: 'glossary',
        loadComponent: () =>
          import('./features/tools/glossary-page/glossary-page.component').then(
            (m) => m.GlossaryPageComponent,
          ),
      },
      {
        path: 'profile',
        loadComponent: () =>
          import('./features/profile/profile.component').then((m) => m.ProfileComponent),
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
          import('./features/admin/dashboard/admin-dashboard.component').then(
            (m) => m.AdminDashboardComponent,
          ),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/admin/users/admin-users.component').then(
            (m) => m.AdminUsersComponent,
          ),
      },
      {
        path: 'courses',
        loadComponent: () =>
          import('./features/admin/courses/admin-courses.component').then(
            (m) => m.AdminCoursesComponent,
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
          import('./features/admin/payments/admin-payments.component').then(
            (m) => m.AdminPaymentsComponent,
          ),
      },
      {
        path: 'analytics',
        loadComponent: () =>
          import('./features/admin/analytics/admin-analytics.component').then(
            (m) => m.AdminAnalyticsComponent,
          ),
      },
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/admin/settings/admin-settings.component').then(
            (m) => m.AdminSettingsComponent,
          ),
      },
    ],
  },
  {
    path: '**',
    redirectTo: '',
  },
];
