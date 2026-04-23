# Бесплатные источники аудио для уроков

## Freesound — звуковые эффекты, фоновые записи
https://freesound.org — требует регистрации, CC0 и CC-BY лицензии.
Удобно для:
- звуков уведомлений в интерфейсе
- фонового звука в обучающих видео
- аудио-иллюстраций в уроках

## Free Music Archive — музыка и эмбиент
https://freemusicarchive.org — треки под CC-лицензиями.

## BBC Sound Effects (free)
https://sound-effects.bbcrewind.co.uk — 33 000+ эффектов под RemArc-лицензией
(бесплатно для образовательных и персональных проектов).

## Общедоступные подкасты-RSS

Можно парсить RSS и прикреплять эпизоды как аудио-блоки урока:

- **TED Talks Daily** — https://feeds.feedburner.com/TEDTalks_audio
- **BBC Learning English** — https://podcasts.files.bbci.co.uk/p02pc9zl.rss
- **The Daily (NYT)** — https://feeds.simplecast.com/54nAGcIl

## Как использовать в платформе

1. В уроке создаётся блок типа `AudioFile` (если загружается в MinIO) или `AudioUrl` (если внешняя ссылка).
2. Для словаря — поле `audioUrl` в карточке слова.
3. Для занятий — преподаватель может прикреплять аудио-запись разбора темы.

## Примеры прямых MP3-ссылок (для тестовой загрузки в MinIO)

- Kevin MacLeod (CC-BY) — https://incompetech.com/music/royalty-free/
- SoundBible — https://soundbible.com
