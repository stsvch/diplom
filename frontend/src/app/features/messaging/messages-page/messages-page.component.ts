import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
  effect,
  ElementRef,
  ViewChild,
  DestroyRef,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  LucideAngularModule,
  Send,
  Paperclip,
  X,
  MessageCircle,
  Search,
  Trash2,
  Edit,
  UserPlus,
  Users,
  EyeOff,
  Plus,
  ChevronLeft,
  Check,
} from 'lucide-angular';
import { MessagingService } from '../services/messaging.service';
import { ChatSignalRService } from '../../../core/services/chat-signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { FileService } from '../../../core/services/file.service';
import { ChatDto, MessageDto, AttachmentDto, UserSummaryDto } from '../models/messaging.model';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { UserPickerComponent } from '../../../shared/components/user-picker/user-picker.component';
import { parseApiError } from '../../../core/models/api-error.model';

type PickerMode = 'newDirect' | 'addParticipant' | null;

@Component({
  selector: 'app-messages-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule,
    AvatarComponent,
    UserPickerComponent,
  ],
  templateUrl: './messages-page.component.html',
  styleUrl: './messages-page.component.scss',
})
export class MessagesPageComponent implements OnInit, OnDestroy {
  private readonly messagingService = inject(MessagingService);
  private readonly chatSignalR = inject(ChatSignalRService);
  private readonly authService = inject(AuthService);
  private readonly fileService = inject(FileService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  private readonly requestedChatId = signal<string | null>(null);
  private loadingChatById: string | null = null;

  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('messageInput') messageInputRef!: ElementRef<HTMLTextAreaElement>;
  @ViewChild('fileInput') fileInputRef!: ElementRef<HTMLInputElement>;

  readonly SendIcon = Send;
  readonly PaperclipIcon = Paperclip;
  readonly XIcon = X;
  readonly MessageCircleIcon = MessageCircle;
  readonly SearchIcon = Search;
  readonly Trash2Icon = Trash2;
  readonly EditIcon = Edit;
  readonly UserPlusIcon = UserPlus;
  readonly UsersIcon = Users;
  readonly EyeOffIcon = EyeOff;
  readonly PlusIcon = Plus;
  readonly CheckIcon = Check;
  readonly ChevronLeftIcon = ChevronLeft;

  readonly chats = signal<ChatDto[]>([]);
  readonly messages = signal<MessageDto[]>([]);
  readonly selectedChat = signal<ChatDto | null>(null);
  readonly loadingChats = signal(false);
  readonly loadingMessages = signal(false);
  readonly sendingMessage = signal(false);
  readonly messageText = signal('');
  readonly searchQuery = signal('');
  readonly error = signal<string | null>(null);

  readonly pendingAttachments = signal<AttachmentDto[]>([]);
  readonly uploading = signal(false);

  readonly editingId = signal<string | null>(null);
  readonly editingText = signal('');

  readonly pickerMode = signal<PickerMode>(null);
  readonly showParticipants = signal(false);

  readonly currentUser = computed(() => this.authService.currentUser());

  readonly filteredChats = computed(() => {
    const query = this.searchQuery().toLowerCase();
    const chats = this.chats();
    if (!query) return chats;
    return chats.filter((chat) => {
      const chatName = this.getChatName(chat).toLowerCase();
      const lastMsg = (chat.lastMessage ?? '').toLowerCase();
      return chatName.includes(query) || lastMsg.includes(query);
    });
  });

  readonly isOwnerOfSelectedCourseChat = computed(() => {
    const chat = this.selectedChat();
    if (!chat || chat.type !== 'CourseChat') return false;
    return chat.ownerId === this.currentUser()?.id;
  });

  readonly canDeleteSelectedChat = computed(() => {
    const chat = this.selectedChat();
    if (!chat) return false;
    if (chat.type === 'DirectMessage') return false;
    return this.isOwnerOfSelectedCourseChat();
  });

  readonly excludedParticipantIds = computed(() =>
    (this.selectedChat()?.participants ?? []).map((p) => p.userId),
  );

  constructor() {
    effect(() => {
      const msg = this.chatSignalR.lastMessage();
      if (!msg) return;
      this.handleIncomingMessage(msg);
    });

    effect(() => {
      const msg = this.chatSignalR.messageEdited();
      if (!msg) return;
      this.applyEditedMessage(msg);
    });

    effect(() => {
      const event = this.chatSignalR.messageDeleted();
      if (!event) return;
      this.applyDeletedMessage(event.chatId, event.messageId);
    });

    effect(() => {
      const event = this.chatSignalR.messagesRead();
      if (!event) return;
      this.applyMessagesRead(event.chatId, event.userId);
    });

    effect(() => {
      const event = this.chatSignalR.participantAdded();
      if (!event) return;
      this.applyParticipantAdded(event.chatId, event.participant);
    });

    effect(() => {
      const event = this.chatSignalR.participantRemoved();
      if (!event) return;
      this.applyParticipantRemoved(event.chatId, event.userId);
    });

    effect(() => {
      const chatId = this.chatSignalR.removedFromChat();
      if (!chatId) return;
      this.handleRemovedFromChat(chatId);
    });

    effect(() => {
      const chatId = this.chatSignalR.chatArchived();
      if (!chatId) return;
      this.chats.update((list) => list.map((c) => c.id === chatId ? { ...c, isArchived: true } : c));
      const selected = this.selectedChat();
      if (selected?.id === chatId) this.selectedChat.set({ ...selected, isArchived: true });
    });

    effect(() => {
      const chatId = this.chatSignalR.chatDeleted();
      if (!chatId) return;
      this.removeChatLocally(chatId);
      this.refreshUnreadCount();
    });
  }

  ngOnInit(): void {
    this.route.paramMap
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        this.requestedChatId.set(params.get('chatId'));
        this.tryOpenRequestedChat();
      });

    this.loadChats();
  }

  ngOnDestroy(): void {}

  private loadChats(): void {
    this.loadingChats.set(true);
    this.messagingService.getChats().subscribe({
      next: (chats) => {
        this.chats.set(chats);
        this.loadingChats.set(false);
        this.tryOpenRequestedChat();
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loadingChats.set(false);
      },
    });
  }

  selectChat(chat: ChatDto, options: { syncRoute?: boolean } = {}): void {
    const syncRoute = options.syncRoute ?? true;
    const current = this.selectedChat();
    const isSameChat = current?.id === chat.id;

    if (syncRoute) {
      this.navigateToChat(chat.id);
    }

    this.selectedChat.set(chat);
    this.editingId.set(null);
    this.editingText.set('');
    this.pendingAttachments.set([]);

    if (!isSameChat) {
      this.messages.set([]);
      this.chatSignalR.joinChat(chat.id);
      this.loadMessages(chat.id);
    }

    this.markChatAsRead(chat.id);
  }

  private loadMessages(chatId: string): void {
    this.loadingMessages.set(true);
    this.messagingService.getChatMessages(chatId, 1, 50).subscribe({
      next: (msgs) => {
        this.messages.set([...msgs].reverse());
        this.loadingMessages.set(false);
        setTimeout(() => this.scrollToBottom(), 50);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loadingMessages.set(false);
      },
    });
  }

  sendMessage(): void {
    const text = this.messageText().trim();
    const attachments = this.pendingAttachments();
    const chat = this.selectedChat();
    if (!chat || chat.isArchived) return;
    if (!text && attachments.length === 0) return;
    if (this.sendingMessage()) return;

    this.sendingMessage.set(true);
    this.messagingService.sendMessage(chat.id, { text, attachments }).subscribe({
      next: (msg) => {
        this.messages.update((msgs) => {
          if (msgs.some((m) => m.id === msg.id)) return msgs;
          return [...msgs, msg];
        });
        this.messageText.set('');
        this.pendingAttachments.set([]);
        this.sendingMessage.set(false);
        setTimeout(() => this.scrollToBottom(), 50);
        this.bumpChatInList(msg);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.sendingMessage.set(false);
      },
    });
  }

  onKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.sendMessage();
    }
  }

  triggerFilePicker(): void {
    this.fileInputRef?.nativeElement.click();
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const files = Array.from(input.files ?? []);
    input.value = '';
    const chat = this.selectedChat();
    if (!chat || files.length === 0) return;

    this.uploading.set(true);
    let remaining = files.length;
    for (const file of files) {
      this.fileService.upload(file, 'ChatMessage', chat.id).subscribe({
        next: (att) => {
          this.pendingAttachments.update((list) => [...list, {
            fileName: att.fileName,
            fileUrl: att.fileUrl,
            contentType: att.contentType,
            fileSize: att.fileSize,
          }]);
          if (--remaining === 0) this.uploading.set(false);
        },
        error: (err) => {
          this.error.set(parseApiError(err).message);
          if (--remaining === 0) this.uploading.set(false);
        },
      });
    }
  }

  removePendingAttachment(index: number): void {
    this.pendingAttachments.update((list) => list.filter((_, i) => i !== index));
  }

  startEditMessage(message: MessageDto): void {
    this.editingId.set(message.id);
    this.editingText.set(message.text);
  }

  cancelEdit(): void {
    this.editingId.set(null);
    this.editingText.set('');
  }

  saveEdit(): void {
    const id = this.editingId();
    const text = this.editingText().trim();
    if (!id || !text) return;
    this.messagingService.editMessage(id, text).subscribe({
      next: (msg) => {
        this.applyEditedMessage(msg);
        this.cancelEdit();
      },
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  onEditKeyDown(event: KeyboardEvent): void {
    if (event.key === 'Escape') {
      event.preventDefault();
      this.cancelEdit();
    } else if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.saveEdit();
    }
  }

  canEdit(message: MessageDto): boolean {
    if (!this.isOwnMessage(message)) return false;
    const sent = new Date(message.sentAt).getTime();
    return Date.now() - sent < 15 * 60 * 1000;
  }

  deleteMessage(messageId: string, event: Event): void {
    event.stopPropagation();
    this.messagingService.deleteMessage(messageId).subscribe({
      next: () => this.applyDeletedMessage(this.selectedChat()!.id, messageId),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  hideChat(): void {
    const chat = this.selectedChat();
    if (!chat) return;
    if (!confirm('Скрыть чат у себя? Чат вернётся при новом сообщении.')) return;
    this.messagingService.hideChat(chat.id).subscribe({
      next: () => {
        this.removeChatLocally(chat.id);
        this.refreshUnreadCount();
      },
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  deleteChat(): void {
    const chat = this.selectedChat();
    if (!chat) return;
    if (!confirm('Удалить чат для всех участников? Это необратимо.')) return;
    this.messagingService.deleteChat(chat.id).subscribe({
      next: () => {
        this.removeChatLocally(chat.id);
        this.refreshUnreadCount();
      },
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  openAddParticipant(): void {
    this.pickerMode.set('addParticipant');
  }

  openNewDirectChat(): void {
    this.pickerMode.set('newDirect');
  }

  closePicker(): void {
    this.pickerMode.set(null);
  }

  onUserPicked(user: UserSummaryDto): void {
    const mode = this.pickerMode();
    this.pickerMode.set(null);
    if (mode === 'newDirect') {
      this.messagingService.createDirectChat({ recipientId: user.id, recipientName: user.fullName }).subscribe({
        next: (chat) => {
          this.chats.update((list) => {
            if (list.some((c) => c.id === chat.id)) return list;
            return [chat, ...list];
          });
          this.selectChat(chat);
        },
        error: (err) => this.error.set(parseApiError(err).message),
      });
    } else if (mode === 'addParticipant') {
      const chat = this.selectedChat();
      if (!chat) return;
      this.messagingService.addParticipant(chat.id, { userId: user.id, userName: user.fullName }).subscribe({
        error: (err) => this.error.set(parseApiError(err).message),
      });
    }
  }

  removeParticipant(userId: string): void {
    const chat = this.selectedChat();
    if (!chat) return;
    if (!confirm('Удалить участника?')) return;
    this.messagingService.removeParticipant(chat.id, userId).subscribe({
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  toggleParticipants(): void {
    this.showParticipants.update((v) => !v);
  }

  deselectChat(syncRoute = true): void {
    if (syncRoute) {
      this.navigateToChat(null);
    }
    this.selectedChat.set(null);
    this.messages.set([]);
    this.editingId.set(null);
    this.pendingAttachments.set([]);
  }

  getChatName(chat: ChatDto): string {
    if (chat.type === 'CourseChat') return chat.courseName ?? 'Групповой чат';
    const userId = this.currentUser()?.id;
    const other = chat.participants.find((p) => p.userId !== userId);
    return other?.name ?? 'Собеседник';
  }

  getChatInitials(chat: ChatDto): string {
    return this.getChatName(chat)
      .split(' ')
      .map((w) => w.charAt(0))
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  isOwnMessage(message: MessageDto): boolean {
    return message.senderId === this.currentUser()?.id;
  }

  formatTime(dateStr: string): string {
    const date = new Date(dateStr);
    return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
  }

  formatLastMessageTime(dateStr?: string): string {
    if (!dateStr) return '';
    const date = new Date(dateStr);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffDays = Math.floor(diffMs / (1000 * 60 * 60 * 24));

    if (diffDays === 0) return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
    if (diffDays === 1) return 'вчера';
    if (diffDays < 7) return date.toLocaleDateString('ru-RU', { weekday: 'short' });
    return date.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit' });
  }

  getMessageInitials(senderName: string): string {
    return senderName
      .split(' ')
      .map((w) => w.charAt(0))
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} Б`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} КБ`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} МБ`;
  }

  getReadLabel(message: MessageDto): string {
    if (!this.isOwnMessage(message)) return '';

    const readerCount = message.readBy.filter((userId) => userId !== message.senderId).length;
    if (readerCount === 0) return '';

    if (this.selectedChat()?.type === 'DirectMessage') {
      return 'прочитано';
    }

    return `прочитали: ${readerCount}`;
  }

  private scrollToBottom(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }

  private handleIncomingMessage(msg: MessageDto): void {
    const selected = this.selectedChat();
    const currentUserId = this.currentUser()?.id;
    const isOwnMessage = msg.senderId === currentUserId;

    if (selected && msg.chatId === selected.id) {
      this.messages.update((msgs) => {
        if (msgs.some((m) => m.id === msg.id)) return msgs;
        return [...msgs, msg];
      });
      setTimeout(() => this.scrollToBottom(), 30);

      if (!isOwnMessage) {
        this.markChatAsRead(msg.chatId, 1);
      }
    }

    this.bumpChatInList(
      msg,
      !isOwnMessage && (!selected || selected.id !== msg.chatId) ? 1 : 0,
    );
  }

  private applyEditedMessage(msg: MessageDto): void {
    this.messages.update((list) => list.map((m) => m.id === msg.id ? msg : m));
    this.syncChatSnapshot(msg.chatId);
  }

  private applyDeletedMessage(chatId: string, messageId: string): void {
    if (this.selectedChat()?.id === chatId) {
      this.messages.update((list) => list.filter((m) => m.id !== messageId));
    }

    this.syncChatSnapshot(chatId);
    this.refreshUnreadCount();
  }

  private applyMessagesRead(chatId: string, userId: string): void {
    if (this.selectedChat()?.id === chatId) {
      this.messages.update((list) =>
        list.map((message) => (
          message.readBy.includes(userId)
            ? message
            : { ...message, readBy: [...message.readBy, userId] }
        )),
      );
    }

    if (userId === this.currentUser()?.id) {
      this.clearUnreadForChat(chatId);
    }
  }

  private applyParticipantAdded(chatId: string, participant: { userId: string; name: string }): void {
    this.chats.update((list) => list.map((c) => {
      if (c.id !== chatId) return c;
      if (c.participants.some((p) => p.userId === participant.userId)) return c;
      return { ...c, participants: [...c.participants, participant] };
    }));
    const sel = this.selectedChat();
    if (sel?.id === chatId) {
      if (sel.participants.some((p) => p.userId === participant.userId)) return;
      this.selectedChat.set({ ...sel, participants: [...sel.participants, participant] });
    }
  }

  private applyParticipantRemoved(chatId: string, userId: string): void {
    if (userId === this.currentUser()?.id) {
      this.handleRemovedFromChat(chatId);
      return;
    }

    this.chats.update((list) => list.map((c) => {
      if (c.id !== chatId) return c;
      return { ...c, participants: c.participants.filter((p) => p.userId !== userId) };
    }));
    const sel = this.selectedChat();
    if (sel?.id === chatId) {
      this.selectedChat.set({ ...sel, participants: sel.participants.filter((p) => p.userId !== userId) });
    }
  }

  private bumpChatInList(msg: MessageDto, unreadDelta = 0): void {
    let found = false;

    this.chats.update((chats) =>
      chats
        .map((chat) => {
          if (chat.id !== msg.chatId) {
            return chat;
          }

          found = true;
          return {
            ...chat,
            lastMessage: this.getMessagePreview(msg),
            lastMessageAt: msg.sentAt,
            unreadCount: Math.max(0, chat.unreadCount + unreadDelta),
          };
        })
        .sort((a, b) => {
          const aTime = a.lastMessageAt ? new Date(a.lastMessageAt).getTime() : 0;
          const bTime = b.lastMessageAt ? new Date(b.lastMessageAt).getTime() : 0;
          return bTime - aTime;
        }),
    );

    if (!found) {
      this.loadChatById(msg.chatId);
    }
  }

  private markChatAsRead(chatId: string, fallbackUnreadDelta = 0): void {
    this.messagingService.markAsRead(chatId).subscribe({
      next: () => this.clearUnreadForChat(chatId, fallbackUnreadDelta),
      error: (err) => this.error.set(parseApiError(err).message),
    });
  }

  private clearUnreadForChat(chatId: string, fallbackUnreadDelta = 0): void {
    let unreadCount = fallbackUnreadDelta;

    this.chats.update((chats) =>
      chats.map((chat) => {
        if (chat.id !== chatId) {
          return chat;
        }

        unreadCount = chat.unreadCount || unreadCount;
        return { ...chat, unreadCount: 0 };
      }),
    );

    const selected = this.selectedChat();
    if (selected?.id === chatId && selected.unreadCount > 0) {
      this.selectedChat.set({ ...selected, unreadCount: 0 });
    }

    if (unreadCount > 0) {
      this.chatSignalR.adjustUnreadCount(-unreadCount);
    }
  }

  private tryOpenRequestedChat(): void {
    const chatId = this.requestedChatId();

    if (!chatId) {
      if (this.selectedChat()) {
        this.deselectChat(false);
      }
      return;
    }

    if (this.selectedChat()?.id === chatId) {
      return;
    }

    const existingChat = this.chats().find((chat) => chat.id === chatId);
    if (existingChat) {
      this.selectChat(existingChat, { syncRoute: false });
      return;
    }

    this.loadChatById(chatId, true);
  }

  private loadChatById(chatId: string, selectAfterLoad = false): void {
    if (this.loadingChatById === chatId) {
      return;
    }

    this.loadingChatById = chatId;

    this.messagingService.getChatById(chatId).subscribe({
      next: (chat) => {
        this.loadingChatById = null;
        this.upsertChat(chat);

        if (selectAfterLoad || this.requestedChatId() === chat.id) {
          this.selectChat(chat, { syncRoute: false });
        }
      },
      error: (err) => {
        this.loadingChatById = null;
        if (err.status === 403 || err.status === 404) {
          this.removeChatLocally(chatId);
          if (this.requestedChatId() === chatId) {
            this.navigateToChat(null);
          }
          this.refreshUnreadCount();
          return;
        }

        this.error.set(parseApiError(err).message);
      },
    });
  }

  private refreshUnreadCount(): void {
    this.messagingService.getUnreadCount().subscribe({
      error: () => this.chatSignalR.setUnreadCount(0),
    });
  }

  private syncChatSnapshot(chatId: string): void {
    this.messagingService.getChatById(chatId).subscribe({
      next: (chat) => this.upsertChat(chat),
      error: (err) => {
        if (err.status === 403 || err.status === 404) {
          this.removeChatLocally(chatId);
          if (this.requestedChatId() === chatId) {
            this.navigateToChat(null);
          }
          this.refreshUnreadCount();
          return;
        }

        this.error.set(parseApiError(err).message);
      },
    });
  }

  private upsertChat(chat: ChatDto): void {
    this.chats.update((list) => {
      const next = list.some((item) => item.id === chat.id)
        ? list.map((item) => (item.id === chat.id ? chat : item))
        : [chat, ...list];

      return this.sortChats(next);
    });

    if (this.selectedChat()?.id === chat.id) {
      this.selectedChat.set(chat);
    }
  }

  private removeChatLocally(chatId: string): void {
    this.clearUnreadForChat(chatId);
    this.chats.update((list) => list.filter((chat) => chat.id !== chatId));

    if (this.selectedChat()?.id === chatId) {
      this.deselectChat();
    }
  }

  private handleRemovedFromChat(chatId: string): void {
    const wasSelected = this.selectedChat()?.id === chatId;
    this.removeChatLocally(chatId);
    this.refreshUnreadCount();

    if (wasSelected) {
      this.error.set('Вы удалены из чата.');
    }
  }

  private navigateToChat(chatId: string | null): void {
    const role = this.authService.userRole();
    const prefix = role === 'Teacher' ? '/teacher/messages' : '/student/messages';
    const target = chatId ? [prefix, chatId] : [prefix];

    this.router.navigate(target, { replaceUrl: true });
  }

  private getMessagePreview(message: MessageDto): string {
    if (message.text.trim()) {
      return message.text;
    }

    if (message.attachments.length > 0) {
      return message.attachments.length === 1
        ? '[вложение]'
        : `[вложений: ${message.attachments.length}]`;
    }

    return 'Новое сообщение';
  }

  private sortChats(chats: ChatDto[]): ChatDto[] {
    return [...chats].sort((a, b) => {
      const aTime = a.lastMessageAt ? new Date(a.lastMessageAt).getTime() : 0;
      const bTime = b.lastMessageAt ? new Date(b.lastMessageAt).getTime() : 0;
      return bTime - aTime;
    });
  }
}
