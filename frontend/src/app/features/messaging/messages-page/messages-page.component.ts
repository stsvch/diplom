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
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  LucideAngularModule,
  Send,
  Paperclip,
  X,
  MessageCircle,
  Search,
  Trash2,
  ChevronLeft,
} from 'lucide-angular';
import { MessagingService } from '../services/messaging.service';
import { ChatSignalRService } from '../../../core/services/chat-signalr.service';
import { AuthService } from '../../../core/services/auth.service';
import { ChatDto, MessageDto, AttachmentDto } from '../models/messaging.model';
import { AvatarComponent } from '../../../shared/components/avatar/avatar.component';
import { parseApiError } from '../../../core/models/api-error.model';

@Component({
  selector: 'app-messages-page',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LucideAngularModule,
    AvatarComponent,
  ],
  templateUrl: './messages-page.component.html',
  styleUrl: './messages-page.component.scss',
})
export class MessagesPageComponent implements OnInit, OnDestroy {
  private readonly messagingService = inject(MessagingService);
  private readonly chatSignalR = inject(ChatSignalRService);
  private readonly authService = inject(AuthService);

  @ViewChild('messagesContainer') messagesContainer!: ElementRef<HTMLDivElement>;
  @ViewChild('messageInput') messageInputRef!: ElementRef<HTMLTextAreaElement>;

  readonly SendIcon = Send;
  readonly PaperclipIcon = Paperclip;
  readonly XIcon = X;
  readonly MessageCircleIcon = MessageCircle;
  readonly SearchIcon = Search;
  readonly Trash2Icon = Trash2;
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

  private messageEffectRef: ReturnType<typeof effect> | null = null;
  private readEffectRef: ReturnType<typeof effect> | null = null;

  constructor() {
    // React to new incoming messages via SignalR
    this.messageEffectRef = effect(() => {
      const msg = this.chatSignalR.lastMessage();
      if (!msg) return;

      const selected = this.selectedChat();
      if (selected && msg.chatId === selected.id) {
        this.messages.update((msgs) => {
          // Avoid duplicates
          if (msgs.some((m) => m.id === msg.id)) return msgs;
          return [...msgs, msg];
        });
        this.scrollToBottom();

        // Mark as read
        const userId = this.currentUser()?.id;
        if (userId && msg.senderId !== userId) {
          this.chatSignalR.markAsRead(msg.chatId);
          this.messagingService.markAsRead(msg.chatId).subscribe();
        }
      }

      // Update chat list
      this.chats.update((chats) =>
        chats.map((c) =>
          c.id === msg.chatId
            ? {
                ...c,
                lastMessage: msg.text,
                lastMessageAt: msg.sentAt,
              }
            : c,
        ).sort((a, b) => {
          const aTime = a.lastMessageAt ? new Date(a.lastMessageAt).getTime() : 0;
          const bTime = b.lastMessageAt ? new Date(b.lastMessageAt).getTime() : 0;
          return bTime - aTime;
        })
      );
    });
  }

  ngOnInit(): void {
    this.loadChats();
  }

  ngOnDestroy(): void {
    // Effects clean up automatically
  }

  private loadChats(): void {
    this.loadingChats.set(true);
    this.messagingService.getChats().subscribe({
      next: (chats) => {
        this.chats.set(chats);
        this.loadingChats.set(false);
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
        this.loadingChats.set(false);
      },
    });
  }

  selectChat(chat: ChatDto): void {
    this.selectedChat.set(chat);
    this.messages.set([]);
    this.chatSignalR.joinChat(chat.id);
    this.loadMessages(chat.id);

    // Mark as read
    this.messagingService.markAsRead(chat.id).subscribe({
      next: () => {
        this.chats.update((chats) =>
          chats.map((c) => (c.id === chat.id ? { ...c, unreadCount: 0 } : c)),
        );
      },
    });
  }

  private loadMessages(chatId: string): void {
    this.loadingMessages.set(true);
    this.messagingService.getChatMessages(chatId, 1, 50).subscribe({
      next: (msgs) => {
        // Messages come sorted desc from server, reverse to show oldest first
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
    const chat = this.selectedChat();
    if (!text || !chat || this.sendingMessage()) return;

    this.sendingMessage.set(true);
    // Send via SignalR for real-time delivery to others
    this.chatSignalR.sendMessage(chat.id, text);

    // Also send via REST to get confirmation and to handle REST fallback
    this.messagingService.sendMessage(chat.id, { text }).subscribe({
      next: (msg) => {
        this.messages.update((msgs) => {
          if (msgs.some((m) => m.id === msg.id)) return msgs;
          return [...msgs, msg];
        });
        this.messageText.set('');
        this.sendingMessage.set(false);
        setTimeout(() => this.scrollToBottom(), 50);

        // Update chat list
        this.chats.update((chats) =>
          chats
            .map((c) =>
              c.id === chat.id
                ? { ...c, lastMessage: msg.text, lastMessageAt: msg.sentAt }
                : c,
            )
            .sort((a, b) => {
              const aTime = a.lastMessageAt ? new Date(a.lastMessageAt).getTime() : 0;
              const bTime = b.lastMessageAt ? new Date(b.lastMessageAt).getTime() : 0;
              return bTime - aTime;
            }),
        );
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

  deleteMessage(messageId: string, event: Event): void {
    event.stopPropagation();
    this.messagingService.deleteMessage(messageId).subscribe({
      next: () => {
        this.messages.update((msgs) => msgs.filter((m) => m.id !== messageId));
      },
      error: (err) => {
        this.error.set(parseApiError(err).message);
      },
    });
  }

  deselectChat(): void {
    this.selectedChat.set(null);
    this.messages.set([]);
  }

  getChatName(chat: ChatDto): string {
    if (chat.type === 'CourseChat') {
      return chat.courseName ?? 'Групповой чат';
    }
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
    const userId = this.currentUser()?.id;
    return message.senderId === userId;
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

    if (diffDays === 0) {
      return date.toLocaleTimeString('ru-RU', { hour: '2-digit', minute: '2-digit' });
    } else if (diffDays === 1) {
      return 'вчера';
    } else if (diffDays < 7) {
      return date.toLocaleDateString('ru-RU', { weekday: 'short' });
    } else {
      return date.toLocaleDateString('ru-RU', { day: '2-digit', month: '2-digit' });
    }
  }

  getMessageInitials(senderName: string): string {
    return senderName
      .split(' ')
      .map((w) => w.charAt(0))
      .slice(0, 2)
      .join('')
      .toUpperCase();
  }

  private scrollToBottom(): void {
    if (this.messagesContainer) {
      const el = this.messagesContainer.nativeElement;
      el.scrollTop = el.scrollHeight;
    }
  }
}
