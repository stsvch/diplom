# Аудио-материалы для курсов английского

## Бесплатные источники озвучки слов

### Forvo — произношение носителями
Прямые ссылки на MP3 (API требует ключ, но плеер встраивается):
- https://forvo.com/word/hello/
- https://forvo.com/word/welcome/
- https://forvo.com/word/computer/

### Cambridge Dictionary — UK + US произношение
Каждое слово имеет два аудиофайла (UK/US), ссылки вида:
- https://dictionary.cambridge.org/media/english/uk_pron/u/ukh/ukhel/ukhello018.mp3
- https://dictionary.cambridge.org/media/english/us_pron/h/hel/hello/hello.mp3

### ResponsiveVoice / Web Speech API
Можно генерировать произношение прямо в браузере без загрузки файлов:
```javascript
const utterance = new SpeechSynthesisUtterance("Hello, world!");
utterance.lang = "en-US";
speechSynthesis.speak(utterance);
```

## Подкасты для обучения

- **BBC Learning English — 6 Minute English** — https://www.bbc.co.uk/learningenglish/english/features/6-minute-english
- **Luke's English Podcast** — https://teacherluke.co.uk/
- **All Ears English** — https://www.allearsenglish.com/episodes/

## Аудиокниги (бесплатные)

- **LibriVox** — https://librivox.org/ (public domain, десятки тысяч книг)
- **Project Gutenberg audio** — https://www.gutenberg.org/browse/categories/3

## Для словарного инструмента (dictionary tool)

В модуле Tools можно хранить в карточке слова:
- `word` — "hello"
- `translation` — "привет"
- `audioUrl` — ссылка на Forvo/Cambridge или локально загруженный MP3 в MinIO
- `example` — "Hello, how are you?"
- `transcription` — "/həˈləʊ/"

## Рекомендации

Для стабильной работы лучше загружать аудио в MinIO — внешние ссылки
могут меняться или требовать авторизации. Размер MP3-произношения одного слова ~15–30 KB.
