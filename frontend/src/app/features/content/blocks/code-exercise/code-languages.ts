// code-languages.ts
// Файл содержит типы и вспомогательные данные для соответствующего feature-блока.
export interface CodeLanguageOption {
  value: string;
  label: string;
  monacoLanguage: string;
}

export const CODE_LANGUAGE_OPTIONS: CodeLanguageOption[] = [
  { value: 'plaintext', label: 'Plain text', monacoLanguage: 'plaintext' },
  { value: 'javascript', label: 'JavaScript', monacoLanguage: 'javascript' },
  { value: 'typescript', label: 'TypeScript', monacoLanguage: 'typescript' },
  { value: 'python', label: 'Python', monacoLanguage: 'python' },
  { value: 'csharp', label: 'C#', monacoLanguage: 'csharp' },
  { value: 'java', label: 'Java', monacoLanguage: 'java' },
  { value: 'cpp', label: 'C++', monacoLanguage: 'cpp' },
  { value: 'c', label: 'C', monacoLanguage: 'c' },
  { value: 'go', label: 'Go', monacoLanguage: 'go' },
  { value: 'php', label: 'PHP', monacoLanguage: 'php' },
  { value: 'ruby', label: 'Ruby', monacoLanguage: 'ruby' },
  { value: 'kotlin', label: 'Kotlin', monacoLanguage: 'kotlin' },
  { value: 'swift', label: 'Swift', monacoLanguage: 'swift' },
  { value: 'rust', label: 'Rust', monacoLanguage: 'rust' },
  { value: 'sql', label: 'SQL', monacoLanguage: 'sql' },
  { value: 'html', label: 'HTML', monacoLanguage: 'html' },
  { value: 'css', label: 'CSS', monacoLanguage: 'css' },
  { value: 'json', label: 'JSON', monacoLanguage: 'json' },
  { value: 'xml', label: 'XML', monacoLanguage: 'xml' },
  { value: 'markdown', label: 'Markdown', monacoLanguage: 'markdown' },
  { value: 'shell', label: 'Shell', monacoLanguage: 'shell' },
];

const monacoLanguageMap = new Map(
  CODE_LANGUAGE_OPTIONS.map((language) => [language.value, language.monacoLanguage]),
);

const languageLabelMap = new Map(
  CODE_LANGUAGE_OPTIONS.map((language) => [language.value, language.label]),
);

export function toMonacoLanguage(language: string | null | undefined): string {
  if (!language) return 'plaintext';
  return monacoLanguageMap.get(language) ?? 'plaintext';
}

export function getCodeLanguageLabel(language: string | null | undefined): string {
  if (!language) return 'Plain text';
  return languageLabelMap.get(language) ?? language;
}
