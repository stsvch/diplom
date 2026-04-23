import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { LucideAngularModule } from 'lucide-angular';
import { AuthService } from '../../core/services/auth.service';
import { UsersService } from '../../core/services/users.service';
import { FileService } from '../../core/services/file.service';
import { ToastService } from '../../shared/components/toast/toast.service';
import { User } from '../../core/models/user.model';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [ReactiveFormsModule, LucideAngularModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss',
})
export class ProfileComponent implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly users = inject(UsersService);
  private readonly fileService = inject(FileService);
  private readonly toast = inject(ToastService);

  user = this.auth.currentUser;
  avatarUploading = signal(false);
  savingProfile = signal(false);
  savingPassword = signal(false);

  initials = computed(() => {
    const u = this.user();
    if (!u) return '';
    return (u.firstName[0] ?? '') + (u.lastName[0] ?? '');
  });

  roleLabel = computed(() => {
    const u = this.user();
    if (!u) return '';
    return { Admin: 'Администратор', Teacher: 'Преподаватель', Student: 'Студент' }[u.role] ?? u.role;
  });

  profileForm = this.fb.group({
    firstName: ['', [Validators.required, Validators.minLength(2)]],
    lastName: ['', [Validators.required, Validators.minLength(2)]],
  });

  passwordForm = this.fb.group({
    currentPassword: ['', Validators.required],
    newPassword: ['', [Validators.required, Validators.minLength(6)]],
    repeatPassword: ['', Validators.required],
  });

  ngOnInit(): void {
    const u = this.user();
    if (u) {
      this.profileForm.patchValue({ firstName: u.firstName, lastName: u.lastName });
    } else {
      this.auth.fetchProfile().subscribe({
        next: (fetched) =>
          this.profileForm.patchValue({ firstName: fetched.firstName, lastName: fetched.lastName }),
      });
    }
  }

  onAvatarSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    input.value = '';
    if (!file) return;
    if (!file.type.startsWith('image/')) {
      this.toast.error('Только изображения');
      return;
    }
    if (file.size > 5 * 1024 * 1024) {
      this.toast.error('Максимум 5 МБ');
      return;
    }

    const u = this.user();
    if (!u) return;

    this.avatarUploading.set(true);
    this.fileService.upload(file, 'UserAvatar', u.id).subscribe({
      next: (att) => {
        this.users.updateProfile({
          firstName: this.profileForm.value.firstName!,
          lastName: this.profileForm.value.lastName!,
          avatarUrl: att.fileUrl,
        }).subscribe({
          next: (updated) => {
            this.avatarUploading.set(false);
            this.auth.currentUser.set(updated);
            localStorage.setItem('current_user', JSON.stringify(updated));
            this.toast.success('Аватар обновлён');
          },
          error: () => {
            this.avatarUploading.set(false);
            this.toast.error('Не удалось сохранить аватар');
          },
        });
      },
      error: () => {
        this.avatarUploading.set(false);
        this.toast.error('Не удалось загрузить изображение');
      },
    });
  }

  saveProfile(): void {
    if (this.profileForm.invalid) {
      this.profileForm.markAllAsTouched();
      return;
    }
    const u = this.user();
    if (!u) return;

    this.savingProfile.set(true);
    this.users.updateProfile({
      firstName: this.profileForm.value.firstName!,
      lastName: this.profileForm.value.lastName!,
      avatarUrl: u.avatarUrl ?? null,
    }).subscribe({
      next: (updated) => {
        this.savingProfile.set(false);
        this.auth.currentUser.set(updated);
        localStorage.setItem('current_user', JSON.stringify(updated));
        this.toast.success('Профиль сохранён');
      },
      error: (err) => {
        this.savingProfile.set(false);
        this.toast.error(err?.error?.message ?? 'Не удалось сохранить');
      },
    });
  }

  changePassword(): void {
    if (this.passwordForm.invalid) {
      this.passwordForm.markAllAsTouched();
      return;
    }
    const { currentPassword, newPassword, repeatPassword } = this.passwordForm.value;
    if (newPassword !== repeatPassword) {
      this.toast.error('Пароли не совпадают');
      return;
    }

    this.savingPassword.set(true);
    this.users.changePassword({
      currentPassword: currentPassword!,
      newPassword: newPassword!,
    }).subscribe({
      next: () => {
        this.savingPassword.set(false);
        this.passwordForm.reset();
        this.toast.success('Пароль изменён');
      },
      error: (err) => {
        this.savingPassword.set(false);
        this.toast.error(err?.error?.message ?? 'Не удалось изменить пароль');
      },
    });
  }
}
