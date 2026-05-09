/**
 * Минимальный markdown→HTML рендерер под формат, который генерирует
 * RichTextEditorComponent: **жирный**, *курсив*, > цитата, • список,
 * 1. нумерованный список, --- разделитель, `inline code`.
 *
 * HTML, который уже есть в строке (legacy-данные TipTap), пропускается
 * как есть — пометки маркдауна применяются построчно и не разрушают теги.
 */

const HTML_TAG_RE = /<\/?[a-z][\s\S]*?>/i;

export function renderMarkdown(input: string): string {
  if (!input) return '';
  if (HTML_TAG_RE.test(input)) {
    // Legacy HTML (например, из старого TipTap) — отдаём как есть.
    return input;
  }

  const lines = input.replace(/\r\n/g, '\n').split('\n');
  const out: string[] = [];

  type ListKind = 'ul' | 'ol' | null;
  let list: ListKind = null;
  let inQuote = false;
  let para: string[] = [];

  const closeList = () => {
    if (list) {
      out.push(`</${list}>`);
      list = null;
    }
  };

  const closeQuote = () => {
    if (inQuote) {
      out.push('</blockquote>');
      inQuote = false;
    }
  };

  const flushPara = () => {
    if (para.length === 0) return;
    out.push(`<p>${inlineFormat(para.join(' '))}</p>`);
    para = [];
  };

  for (const rawLine of lines) {
    const line = rawLine.trimEnd();
    const trimmed = line.trimStart();

    if (trimmed === '') {
      flushPara();
      closeList();
      closeQuote();
      continue;
    }

    if (/^(?:---|\*\*\*|___)$/.test(trimmed)) {
      flushPara();
      closeList();
      closeQuote();
      out.push('<hr>');
      continue;
    }

    if (trimmed.startsWith('> ')) {
      flushPara();
      closeList();
      if (!inQuote) {
        out.push('<blockquote>');
        inQuote = true;
      }
      out.push(`<p>${inlineFormat(trimmed.slice(2).trim())}</p>`);
      continue;
    } else {
      closeQuote();
    }

    const ulMatch = /^(?:•|-|\*)\s+(.*)$/.exec(trimmed);
    if (ulMatch) {
      flushPara();
      if (list !== 'ul') {
        closeList();
        out.push('<ul>');
        list = 'ul';
      }
      out.push(`<li>${inlineFormat(ulMatch[1])}</li>`);
      continue;
    }

    const olMatch = /^\d+\.\s+(.*)$/.exec(trimmed);
    if (olMatch) {
      flushPara();
      if (list !== 'ol') {
        closeList();
        out.push('<ol>');
        list = 'ol';
      }
      out.push(`<li>${inlineFormat(olMatch[1])}</li>`);
      continue;
    }

    closeList();
    para.push(trimmed);
  }

  flushPara();
  closeList();
  closeQuote();

  return out.join('\n');
}

function inlineFormat(text: string): string {
  let html = escapeHtml(text);
  html = html.replace(/`([^`]+)`/g, '<code>$1</code>');
  html = html.replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>');
  html = html.replace(/(^|[^*])\*([^*\n]+)\*(?!\*)/g, '$1<em>$2</em>');
  return html;
}

function escapeHtml(s: string): string {
  return s
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;');
}
