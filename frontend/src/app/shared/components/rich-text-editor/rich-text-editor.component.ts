import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
  signal,
  inject,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DomSanitizer } from '@angular/platform-browser';
import { Editor } from '@tiptap/core';
import StarterKit from '@tiptap/starter-kit';
import { Underline } from '@tiptap/extension-underline';
import { TextAlign } from '@tiptap/extension-text-align';
import { Placeholder } from '@tiptap/extension-placeholder';
import { Image } from '@tiptap/extension-image';
import { Link } from '@tiptap/extension-link';
import { CodeBlockLowlight } from '@tiptap/extension-code-block-lowlight';
import { Table } from '@tiptap/extension-table';
import { TableRow } from '@tiptap/extension-table-row';
import { TableCell } from '@tiptap/extension-table-cell';
import { TableHeader } from '@tiptap/extension-table-header';
import { createLowlight, common } from 'lowlight';
import { TiptapEditorDirective } from 'ngx-tiptap';
import {
  LucideAngularModule,
  Bold,
  Italic,
  Underline as UnderlineIcon,
  Strikethrough,
  Heading1,
  Heading2,
  Heading3,
  List,
  ListOrdered,
  Quote,
  Code,
  Code2,
  Link as LinkIcon,
  Image as ImageIcon,
  Table as TableIcon,
  Undo,
  Redo,
  Minus,
} from 'lucide-angular';

const lowlight = createLowlight(common);

@Component({
  selector: 'app-rich-text-editor',
  standalone: true,
  imports: [CommonModule, FormsModule, TiptapEditorDirective, LucideAngularModule],
  templateUrl: './rich-text-editor.component.html',
  styleUrl: './rich-text-editor.component.scss',
})
export class RichTextEditorComponent implements OnInit, OnDestroy {
  private _content = '';
  private _placeholder = 'Начните вводить текст...';

  @Input() set content(val: string) {
    this._content = val ?? '';
    if (this.editor && this.editor.getHTML() !== val) {
      this.editor.commands.setContent(val ?? '');
    }
  }
  get content(): string {
    return this._content;
  }

  @Input() set placeholder(val: string) {
    this._placeholder = val ?? 'Начните вводить текст...';
  }

  @Output() contentChange = new EventEmitter<string>();

  editor!: Editor;

  readonly showLinkDialog = signal(false);
  readonly showImageDialog = signal(false);
  linkUrl = '';
  imageUrl = '';

  // Icons
  readonly BoldIcon = Bold;
  readonly ItalicIcon = Italic;
  readonly UnderlineIcon = UnderlineIcon;
  readonly StrikeIcon = Strikethrough;
  readonly H1Icon = Heading1;
  readonly H2Icon = Heading2;
  readonly H3Icon = Heading3;
  readonly BulletListIcon = List;
  readonly OrderedListIcon = ListOrdered;
  readonly BlockquoteIcon = Quote;
  readonly CodeIcon = Code;
  readonly CodeBlockIcon = Code2;
  readonly LinkIcon = LinkIcon;
  readonly ImageIcon = ImageIcon;
  readonly TableIcon = TableIcon;
  readonly UndoIcon = Undo;
  readonly RedoIcon = Redo;
  readonly HrIcon = Minus;

  ngOnInit(): void {
    this.editor = new Editor({
      extensions: [
        StarterKit.configure({
          codeBlock: false,
        }),
        Underline,
        TextAlign.configure({
          types: ['heading', 'paragraph'],
        }),
        Placeholder.configure({
          placeholder: this._placeholder,
        }),
        Image.configure({
          inline: false,
          allowBase64: true,
        }),
        Link.configure({
          openOnClick: false,
          HTMLAttributes: {
            rel: 'noopener noreferrer',
          },
        }),
        CodeBlockLowlight.configure({
          lowlight,
        }),
        Table.configure({
          resizable: false,
        }),
        TableRow,
        TableCell,
        TableHeader,
      ],
      content: this._content,
      onUpdate: ({ editor }) => {
        this.contentChange.emit(editor.getHTML());
      },
    });
  }

  ngOnDestroy(): void {
    this.editor?.destroy();
  }

  // Toolbar actions
  toggleBold(): void { this.editor.chain().focus().toggleBold().run(); }
  toggleItalic(): void { this.editor.chain().focus().toggleItalic().run(); }
  toggleUnderline(): void { this.editor.chain().focus().toggleUnderline().run(); }
  toggleStrike(): void { this.editor.chain().focus().toggleStrike().run(); }
  setH1(): void { this.editor.chain().focus().toggleHeading({ level: 1 }).run(); }
  setH2(): void { this.editor.chain().focus().toggleHeading({ level: 2 }).run(); }
  setH3(): void { this.editor.chain().focus().toggleHeading({ level: 3 }).run(); }
  toggleBulletList(): void { this.editor.chain().focus().toggleBulletList().run(); }
  toggleOrderedList(): void { this.editor.chain().focus().toggleOrderedList().run(); }
  toggleBlockquote(): void { this.editor.chain().focus().toggleBlockquote().run(); }
  toggleCode(): void { this.editor.chain().focus().toggleCode().run(); }
  toggleCodeBlock(): void { this.editor.chain().focus().toggleCodeBlock().run(); }
  undo(): void { this.editor.chain().focus().undo().run(); }
  redo(): void { this.editor.chain().focus().redo().run(); }

  openLinkDialog(): void {
    const prev = this.editor.getAttributes('link')['href'] ?? '';
    this.linkUrl = prev;
    this.showLinkDialog.set(true);
  }

  applyLink(): void {
    if (this.linkUrl) {
      this.editor.chain().focus().setLink({ href: this.linkUrl }).run();
    } else {
      this.editor.chain().focus().unsetLink().run();
    }
    this.showLinkDialog.set(false);
    this.linkUrl = '';
  }

  cancelLink(): void {
    this.showLinkDialog.set(false);
    this.linkUrl = '';
  }

  openImageDialog(): void {
    this.imageUrl = '';
    this.showImageDialog.set(true);
  }

  applyImage(): void {
    if (this.imageUrl) {
      this.editor.chain().focus().setImage({ src: this.imageUrl }).run();
    }
    this.showImageDialog.set(false);
    this.imageUrl = '';
  }

  cancelImage(): void {
    this.showImageDialog.set(false);
    this.imageUrl = '';
  }

  insertTable(): void {
    this.editor.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run();
  }

  isActive(name: string, attrs?: Record<string, unknown>): boolean {
    return this.editor?.isActive(name, attrs) ?? false;
  }
}
