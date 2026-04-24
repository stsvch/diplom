# Архитектура оплат, выплат и подписок

Документ фиксирует целевую архитектуру этапа 11: как платформа принимает оплату, как преподаватель подключает выплаты, как хранится история попыток и подписок, почему мы не храним банковские карты, и какую модель нужно выбрать до начала реализации.

Дата: 2026-04-23
Статус: Proposed
Автор: Codex

Связанные документы:
- `implementation_plan.md` — этап 11 `Подписки и оплата`
- `docs/database_logical_model.md` — логическая модель БД

---

## 1. Зачем нужен отдельный дизайн-док

Этап 11 нельзя реализовать как простой `Stripe Checkout + кнопка Купить`, потому что здесь есть как минимум пять разных задач:

1. принять деньги у студента;
2. определить, кто юридически и операционно является продавцом для этого платежа;
3. начислить преподавателю его долю;
4. провести выплату преподавателю;
5. сохранить полную историю успешных, неуспешных, отменённых, возвращённых и подписочных платежей.

Если не зафиксировать архитектуру заранее, то реализация разовой покупки курса почти наверняка придётся переписывать, когда появятся:
- подписки;
- сохранённые способы оплаты;
- возвраты;
- споры по платежам;
- несколько преподавателей в рамках одной монетизации;
- админский и преподавательский учёт выплат.

---

## 2. Нефункциональные ограничения и обязательные правила

### 2.1. Что платформа не делает

- Платформа **не хранит PAN, CVC, полный номер карты, CVV/CVC, банковские реквизиты карты**.
- Платформа **не считает оплату успешной по redirect на success page**.
- Платформа **не выдаёт доступ к платному курсу до подтверждённого webhook события от платёжного провайдера**.

### 2.2. Что платформа обязана делать

- Хранить историю **всех** попыток оплаты: `Succeeded`, `Failed`, `Canceled`, `Expired`, `Refunded`, `Disputed`.
- Уметь показать студенту историю покупок и историю попыток.
- Уметь показать преподавателю историю начислений и выплат.
- Не позволять публиковать **платный** курс, если преподаватель не прошёл payout-onboarding.
- Сохранять платёжную архитектуру **provider-agnostic** на доменном уровне: доменные сущности называются не `StripePayment`, а `PaymentAttempt`, `TeacherPayoutAccount`, `CoursePurchase` и т.д.

### 2.3. Внешнее допущение

В этом документе в качестве референс-провайдера используется Stripe, потому что его документация хорошо описывает marketplace-сценарии (`Checkout`, `Connect`, `saved payment methods`, `webhooks`). Если в production будет другой провайдер, архитектура домена остаётся такой же, а меняется только adapter-интеграция.

---

## 3. Главный архитектурный выбор

### 3.1. Возможные модели

#### Вариант A. Преподаватель — merchant of record

Студент фактически платит преподавателю, а платформа только удерживает комиссию.

Плюсы:
- деньги концептуально ближе к модели `это продажа преподавателя`;
- часть ответственности потенциально может лежать на преподавателе;
- при некоторых провайдерах это может выглядеть как более прямой маршрут денег.

Минусы:
- у одного студента появляются разные payment/customer-контексты у разных преподавателей;
- повторное использование карты между преподавателями становится сложнее;
- единая подписка платформы почти ломает модель;
- единая история покупок на платформе становится искусственной надстройкой;
- труднее централизованно выдавать доступ, делать возвраты, споры и unified reporting.

#### Вариант B. Платформа принимает charge, а деньги сразу уходят преподавателю

Это сценарий уровня `destination charges`: платформа принимает платёж и сразу переводит остаток connected account преподавателя.

Плюсы:
- хорошо подходит для простого сценария `один курс -> один преподаватель`;
- проще стартовый one-time purchase flow.

Минусы:
- хуже подходит для будущих подписок;
- хуже подходит для hold-периода перед выплатой;
- при возвратах и спорах всё равно страдает платформа, а переведённые средства нужно отдельно разворачивать или компенсировать;
- менее удобен для единой внутренней бухгалтерской модели.

#### Вариант C. Платформа принимает charge и отдельно рассчитывает/переводит долю преподавателю

Это сценарий уровня `separate charges and transfers`: платёж создаётся на стороне платформы, а преподавателю создаётся отдельное начисление и затем выплата/transfer.

Плюсы:
- единый customer и единые сохранённые способы оплаты на платформе;
- единая история покупок и подписок;
- можно поддержать both `one-time course purchase` и `platform subscription`;
- можно держать funds hold до окончания refund window;
- можно делать гибкое распределение выручки;
- можно переводить деньги нескольким получателям;
- не ломает будущую subscription-модель.

Минусы:
- сложнее реализация;
- платформа операционно несёт больше ответственности за refunds/chargebacks;
- нужен внутренний ledger начислений и выплат.

### 3.2. Выбранное решение

**Рекомендуемая целевая модель: Вариант C.**

Итоговая формулировка:

- **Платформа является merchant of record для student charge.**
- **Преподаватель не принимает платёж напрямую от студента, а выступает получателем выплат от платформы.**
- **Для преподавателя создаётся connected payout account** у провайдера.
- **Платёжная карта студента живёт только у провайдера** и привязана к customer платформы.
- **Начисление преподавателю и реальный payout — отдельные сущности и отдельные этапы.**

Это решение специально выбрано не под самый простой MVP, а под правильную долгосрочную модель.

---

## 4. Кто какие аккаунты ведёт

### 4.1. Аккаунты и роли

#### 1. Platform Payment Account

Это главный аккаунт платформы у платёжного провайдера.

Он нужен для:
- создания checkout sessions;
- хранения student customers;
- хранения токенизированных payment methods;
- приёма charge;
- приёма webhook-ов;
- управления connected accounts преподавателей;
- создания transfers/payouts/settlements.

#### 2. Teacher Connected Payout Account

Это отдельный connected account преподавателя у провайдера.

Он нужен для:
- KYC/KYB и identity verification;
- привязки банковского счёта преподавателя;
- получения transfer/payout;
- отображения payout history преподавателю;
- блокировки продаж платных курсов, если account не готов.

#### 3. Student Customer

Это customer-объект студента у провайдера.

Он нужен для:
- повторных покупок;
- сохранённых способов оплаты;
- истории платежей;
- подписок в будущем.

#### 4. Saved Payment Method

Это токенизированный способ оплаты у провайдера, прикреплённый к `Student Customer`.

Платформа хранит у себя только безопасные display-данные:
- `providerPaymentMethodId`
- `brand`
- `last4`
- `expMonth`
- `expYear`
- `isDefault`

Полных данных карты у нас нет.

---

## 5. Рекомендуемый пользовательский и денежный поток

## 5.1. Поток A. Подключение преподавателя к выплатам

### Цель

Преподаватель должен пройти payout-onboarding до публикации платного курса.

### Шаги

1. Преподаватель создаёт курс и выставляет `isFree = false`.
2. Система проверяет, есть ли у него `TeacherPayoutAccount`.
3. Если аккаунта нет, backend создаёт connected account у провайдера.
4. Backend генерирует onboarding link.
5. Преподаватель уходит в hosted onboarding flow провайдера.
6. Провайдер собирает KYC/KYB, банковские реквизиты и верификационные данные.
7. Провайдер шлёт webhook/update о состоянии account.
8. Платформа сохраняет состояние account у себя.

### Состояния payout account

| Статус | Смысл |
|--------|-------|
| `NotStarted` | преподаватель ещё не начал подключение выплат |
| `OnboardingStarted` | account создан, onboarding начат |
| `PendingVerification` | данные отправлены, но верификация не завершена |
| `Ready` | account готов к приёму выплат |
| `Restricted` | account временно ограничен или требует новых документов |
| `Rejected` | account отклонён |

### Правило публикации платного курса

- `free course` можно публиковать без payout account;
- `paid course` можно публиковать **только** при `TeacherPayoutAccount.Status = Ready`.

### Если account стал Restricted после публикации курса

Не нужно автоматически закрывать уже купленный доступ студентам. Но нужно:
- запретить **новые покупки**;
- показать преподавателю CTA на до-прохождение onboarding/verification;
- перевести курс в режим `sales paused`.

---

## 5.2. Поток B. Разовая покупка курса студентом

### Цель

Студент покупает доступ к одному платному курсу.

### UI-решение

Для первой версии рекомендуется **hosted checkout page**, а не своя форма и не самодельный iframe.

Причины:
- быстрее и безопаснее интеграция;
- меньше PCI и UX-рисков;
- есть built-in mobile UX, SCA, error handling;
- проще поддержать saved payment methods и local payment methods;
- проще дебажить webhook-driven flow.

Возможный evolution path:
- V1: hosted checkout redirect;
- V2: embedded checkout;
- V3: fully custom payments page только если реально нужен кастомный UX.

### Шаги

1. Студент открывает страницу платного курса.
2. Нажимает `Купить`.
3. Backend создаёт `PaymentAttempt` со статусом `Initiated`.
4. Backend создаёт/reuses `Student Customer` у провайдера.
5. Backend создаёт `Checkout Session` в `mode=payment` с metadata:
   - `courseId`
   - `teacherId`
   - `studentId`
   - `paymentAttemptId`
6. Frontend делает redirect на hosted checkout page.
7. Студент оплачивает.
8. Провайдер шлёт webhook.
9. Только после webhook платформа:
   - переводит `PaymentAttempt` в итоговый статус;
   - создаёт `CoursePurchase`;
   - активирует `Enrollment`;
   - создаёт `TeacherSettlement`;
   - отправляет уведомления;
   - создаёт календарные/чатовые побочные эффекты через существующие модули.

### Важное правило

`success_url` и возврат пользователя на сайт означают только то, что пользователь вернулся.  
Факт оплаты подтверждается только webhook-обработкой.

---

## 5.3. Поток C. Неуспешная / отменённая / истёкшая оплата

### Что обязано происходить

- `PaymentAttempt` остаётся в истории;
- `Enrollment` не создаётся;
- `CoursePurchase` не создаётся как активная покупка;
- студент видит статус попытки и может повторить оплату;
- создаётся новая попытка, а не перетирается старая.

### Основные статусы

| Статус | Когда ставится |
|--------|----------------|
| `Initiated` | checkout session создана |
| `PendingProvider` | платёж отправлен в провайдер |
| `Succeeded` | провайдер подтвердил оплату |
| `Failed` | провайдер вернул неуспешный платёж |
| `Canceled` | пользователь отменил checkout |
| `Expired` | session истекла |
| `Refunded` | платёж полностью возвращён |
| `PartiallyRefunded` | возврат частичный |
| `Disputed` | открыт спор/chargeback |

---

## 5.4. Поток D. Повторное использование карты / сохранённые способы оплаты

### Принцип

Платформа **не хранит карты**, но может хранить **ссылки на сохранённые способы оплаты** у провайдера.

### Как это работает

1. У студента есть `Student Customer` в platform account.
2. Во время оплаты checkout может предложить:
   - сохранить способ оплаты;
   - использовать уже сохранённый способ;
   - использовать Link / аналогичный провайдерский ускоренный checkout.
3. Провайдер сохраняет payment method у customer.
4. Платформа сохраняет только безопасный snapshot для UI.

### Почему это критично проектно

Если charge делать не на платформе, а прямо на аккаунте преподавателя, единая модель сохранённых карт распадается.  
Студенту пришлось бы фактически иметь отдельные payment contexts под разных продавцов, что плохо сочетается с:
- единым кабинетом платформы;
- общими подписками;
- общей историей покупок;
- единым UX повторной оплаты.

### Что храним у себя

| Поле | Храним? |
|------|---------|
| Полный номер карты | нет |
| CVC/CVV | нет |
| Stripe/Provider payment method id | да |
| Brand (`visa`, `mastercard`) | да |
| Last4 | да |
| Exp month/year | да |
| Customer id | да |
| Флаг default | да |

---

## 5.5. Поток E. Возвраты и споры

Это одна из причин, почему нельзя мыслить моделью `деньги сразу ушли учителю и тема закрыта`.

### При возврате

1. Платформа инициирует refund у провайдера.
2. `PaymentAttempt` получает `Refunded` или `PartiallyRefunded`.
3. `CoursePurchase` меняет статус по policy:
   - либо `Revoked`;
   - либо `RefundPendingAccessRemoval`;
   - либо остаётся активной, если refund не должен отзывать доступ.
4. `TeacherSettlement`:
   - если ещё не выплачен — отменяется;
   - если уже выплачен — создаётся reversal/offset.

### При dispute / chargeback

Платформа обязана:
- сохранить событие;
- временно заморозить связанное начисление преподавателю;
- при необходимости удержать сумму из будущих выплат преподавателя;
- показать это в teacher payout history.

### Вывод

Даже если бизнес хочет, чтобы деньги `по смыслу` были преподавателя, технически и бухгалтерски это всё равно должно проходить через внутренний ledger платформы.

---

## 5.6. Поток F. Выплата преподавателю

### Ключевая идея

`Успешный student charge` и `реальный payout преподавателю` — не одно и то же событие.

Нужны два отдельных слоя:

1. `TeacherSettlement` — внутреннее начисление преподавателю;
2. `PayoutRecord` / `TransferRecord` — реальный вывод средств через провайдера.

### Почему нельзя схлопывать их в одно

- нужен hold-период под refunds;
- нужен weekly/monthly payout schedule;
- нужны ручные корректировки;
- нужны chargeback reversals;
- нужны subscription allocations в будущем.

### Рекомендуемая денежная формула

Для one-time course purchase:

`teacher_net = gross_amount - provider_fee - platform_commission - corrections`

Где:
- `gross_amount` — сумма, которую заплатил студент;
- `provider_fee` — комиссия платёжного провайдера;
- `platform_commission` — комиссия платформы;
- `corrections` — refunds, disputes, manual adjustments.

Все компоненты должны храниться явно, а не только итоговой суммой.

### Рекомендуемая payout-policy

Для первой версии:
- начисление преподавателю создаётся сразу после `Succeeded`;
- payout происходит не мгновенно, а по расписанию;
- есть configurable hold window, например `7-14 days`.

Если бизнес позже решит делать максимально быстрые выплаты, это меняется policy-конфигом, а не перепроектированием всей системы.

---

## 6. Почему не нужно делать “деньги сразу на счёт преподавателя”

На уровне бизнес-ощущения это звучит просто, но на уровне системы это ломает важные свойства:

1. возврат уже выплаченного платежа становится болезненным;
2. спор по транзакции требует обратного движения денег;
3. подписка платформы не делится естественно “сразу”;
4. повторное использование карты между курсами разных преподавателей становится хуже;
5. единый customer profile платформы разрушается;
6. аналитика и reconciliation усложняются.

Поэтому правильная модель звучит так:

> студент платит платформе, платформа фиксирует покупку, рассчитывает долю преподавателя, начисляет её преподавателю и затем выплачивает её преподавателю по payout-policy.

Это не “деньги украла платформа”, а нормальная marketplace-модель.

---

## 7. Подписки: что с ними делать

## 7.1. Главный вывод

Подписки нельзя проектировать как просто `ещё один тип платежа`.

Перед реализацией нужно выбрать бизнес-модель:

### Вариант S1. Подписка на всю платформу

Студент платит ежемесячно и получает доступ к набору курсов платформы.

Проблема:
- как делить месячную выручку между преподавателями?

### Вариант S2. Подписка на конкретного преподавателя

Студент подписывается на автора/школу преподавателя.

Плюс:
- проще распределение денег.

Минус:
- ломает идею единой платформенной подписки.

### Вариант S3. Подписка на пакет/категорию курсов

Нужна отдельная модель catalog eligibility и revenue allocation.

## 7.2. Почему подписки нельзя делать в первой итерации без policy

Если не определить revenue-share policy, то невозможно честно ответить на вопросы:
- сколько из месячного платежа относится какому преподавателю;
- когда это считается заработанным доходом преподавателя;
- как откатывать распределение при refund/chargeback;
- как считать churn, renewal, failed invoice;
- как показывать историю начислений преподавателю.

## 7.3. Рекомендуемое решение

Целевой revenue-sharing policy всё ещё требует отдельного mini-ADR, но для текущей реализации допустим безопасный промежуточный вариант:

- paid subscription invoice создаёт `SubscriptionAllocationRun`;
- распределение идёт по policy `ProgressWeightedActiveEnrollmentsV1`;
- кандидаты — активные enrollments студента;
- веса считаются по текущему `course progress` студента;
- если прогресс по всем активным курсам нулевой, распределение идёт поровну;
- provider fee для subscription allocation пока считается `0` и не пытается притворяться точным reconciliation-слоем.

Это даёт прозрачный allocation ledger для teacher/admin UI, не ломая текущий one-time payout flow. Финальная payout-интеграция подписочной выручки и revenue-share policy всё ещё остаются следующей фазой.

### Обязательные сущности для подписок

- `SubscriptionPlan`
- `UserSubscription`
- `SubscriptionInvoice`
- `SubscriptionPaymentAttempt`
- `SubscriptionAllocationRun`
- `SubscriptionAllocationLine`

### Что подписка должна уметь хранить

- история активаций;
- история продлений;
- история `invoice paid / invoice failed`;
- история пауз, отмен и окончаний;
- история распределения выручки между преподавателями.

---

## 8. Предлагаемая доменная модель

## 8.1. Phase 1: one-time course payments

| Сущность | Назначение |
|----------|------------|
| `TeacherPayoutAccount` | payout account преподавателя у провайдера |
| `PaymentAttempt` | любая попытка оплаты курса |
| `CoursePurchase` | успешная покупка доступа к курсу |
| `PaymentMethodRef` | сохранённый tokenized payment method |
| `TeacherSettlement` | начисление преподавателю по покупке |
| `PayoutRecord` | фактическая выплата преподавателю |
| `RefundRecord` | возврат по платежу |
| `DisputeRecord` | спор/chargeback |
| `ProcessedWebhookEvent` | idempotency и дедупликация webhook-ов |

## 8.2. Phase 2: subscriptions

| Сущность | Назначение |
|----------|------------|
| `SubscriptionPlan` | тариф подписки |
| `UserSubscription` | активная/историческая подписка пользователя |
| `SubscriptionInvoice` | счёт на продление |
| `SubscriptionPaymentAttempt` | попытка оплаты инвойса |
| `SubscriptionAllocationRun` | один запуск распределения подписочной выручки |
| `SubscriptionAllocationLine` | строка начисления преподавателю по allocation run |

---

## 9. Draft API-контракты

## 9.1. Student

- `POST /api/payments/course-checkout`
  - вход: `courseId`, `savePaymentMethod?`
  - выход: `checkoutUrl`, `paymentAttemptId`

- `GET /api/payments/me/history`
  - история всех payment attempts

- `GET /api/payments/me/purchases`
  - список купленных курсов

- `GET /api/payments/me/payment-methods`
  - сохранённые способы оплаты

- `DELETE /api/payments/me/payment-methods/{id}`
  - отвязка способа оплаты

- `GET /api/payments/attempts/{id}`
  - статус конкретной попытки после возврата с checkout

## 9.2. Teacher

- `GET /api/payments/teacher/payout-account`
- `POST /api/payments/teacher/payout-account/onboarding-link`
- `POST /api/payments/teacher/payout-account/dashboard-link`
- `GET /api/payments/teacher/settlements`
- `GET /api/payments/teacher/payouts`

## 9.3. Admin

- `GET /api/admin/payments`
- `GET /api/admin/refunds`
- `GET /api/admin/disputes`
- `POST /api/admin/refunds/{paymentAttemptId}`

## 9.4. Webhooks

- `POST /api/payments/webhooks/provider`

---

## 10. Какие webhook-и нужны

Ниже список для Stripe-like провайдера. Конкретный набор зависит от выбранного PSP, но классы событий такие же.

### Для разовых платежей

- `checkout.session.completed`
- `checkout.session.expired`
- `payment_intent.succeeded`
- `payment_intent.payment_failed`
- `charge.refunded`

### Для connected accounts / payouts

- `account.updated`
- `payout.paid`
- `payout.failed`
- `transfer.created`
- `transfer.reversed`

### Для risk scenarios

- `charge.dispute.created`
- `charge.dispute.closed`

### Для подписок

- `customer.subscription.created`
- `customer.subscription.updated`
- `customer.subscription.deleted`
- `invoice.paid`
- `invoice.payment_failed`

### Правило обработки webhook

Каждый webhook должен:
- верифицироваться подписью провайдера;
- обрабатываться идемпотентно;
- сохраняться в `ProcessedWebhookEvent`;
- не падать из-за повторной доставки.

---

## 11. Требования к UI

## 11.1. Student-side

### Course detail

- если курс бесплатный: кнопка `Записаться`;
- если курс платный и не куплен: кнопка `Купить`;
- если есть активная покупка: `Продолжить обучение`;
- если есть незавершённая попытка: показать статус и `Повторить оплату`.

### Payments page

Нужно показывать:
- история покупок;
- история попыток оплаты;
- сохранённые способы оплаты;
- возвраты;
- в будущем — подписки.

## 11.2. Teacher-side

### Payout onboarding page

Нужно показывать:
- статус payout account;
- причины блокировки;
- CTA `Подключить выплаты`;
- CTA `Продолжить верификацию`;
- список опубликованных платных курсов, поставленных на паузу из-за payout restrictions.

### Teacher payouts page

Нужно показывать:
- начисления по продажам;
- pending vs paid;
- удержания/refunds/disputes;
- историю payout batches.

---

## 12. Что именно делать в этапе 11

## 12.1. Phase 11A — foundation

Сделать:
- `Payments` модуль;
- `TeacherPayoutAccount`;
- `PaymentAttempt`;
- `CoursePurchase`;
- hosted checkout;
- webhook ingestion;
- student payment history;
- teacher payout onboarding;
- блокировку публикации платных курсов без payout-ready account.

Не делать:
- подписки;
- сложные revenue allocations;
- advanced partial split logic на нескольких получателей.

## 12.2. Phase 11B — settlements

Сделать:
- `TeacherSettlement`;
- `PayoutRecord`;
- teacher payout history;
- refund/reversal handling;
- admin payments/reconciliation views.

## 12.3. Phase 11C — subscriptions

Что уже допустимо в рамках текущей реализации:
- `SubscriptionPlan`, `UserSubscription`, `SubscriptionInvoice`, `SubscriptionPaymentAttempt`;
- renewal/invoice tracking;
- provisional `SubscriptionAllocationRun` и `SubscriptionAllocationLine`.

Что всё ещё требует отдельного policy-документа:
- финальная формула revenue sharing;
- payout-интеграция subscription allocations в teacher payouts;
- reversals/offsets для subscription allocations при refund/dispute/chargeback на production policy.

---

## 13. Рекомендуемое техническое решение по Stripe

При использовании Stripe наиболее логичная связка для этой платформы:

- **Checkout** для student payment UI;
- **hosted checkout page** в первой версии;
- **Connect Express** для payout-onboarding преподавателей;
- **platform-level customers** для студентов;
- **saved payment methods** у customer платформы;
- **indirect charges** на платформе;
- **separate charges and transfers** как базовая учётная модель;
- при необходимости fast-path для простых one-to-one course payments можно настроить почти immediate settlement policy, но не менять доменную модель.

Почему не `direct charges` как базовая модель:
- она хуже ложится на платформенные подписки;
- хуже ложится на единый customer/payment method store;
- делает повторное использование карт и общую историю менее естественными.

---

## 14. Открытые бизнес-вопросы, которые надо закрыть до кода

1. Кто юридически merchant of record в договорной модели платформы?
2. Как считается `platform_commission`?
3. Кто несёт provider fee: платформа, преподаватель или split?
4. Какой refund policy на курсы?
5. Через сколько дней делать payout преподавателю?
6. Нужно ли удерживать reserve у преподавателя на случай refunds/disputes?
7. Как именно будет делиться подписочная выручка между преподавателями?
8. Должен ли полный refund отзывать доступ к курсу автоматически?
9. Можно ли преподавателю публиковать платный курс при `PendingVerification`, или только при `Ready`?

Без ответов на эти вопросы можно делать только foundation-часть, но нельзя считать monetization fully designed.

---

## 15. Итоговое решение в одной формулировке

Для этой платформы правильнее проектировать оплату как **marketplace with platform-led charges and teacher payouts**, а не как прямой приём денег преподавателем.

Практически это означает:

- студент платит платформе;
- карта и сохранённые способы оплаты живут у провайдера;
- платный курс нельзя публиковать без payout-onboarding преподавателя;
- успешная оплата фиксируется только по webhook;
- покупка курса и выплата преподавателю — разные сущности;
- финальная payout-policy и revenue-sharing policy для подписок откладываются до отдельного решения; базовый allocation ledger уже может существовать раньше.

---

## 16. Ссылки на официальные материалы

Ниже материалы, на которых основаны референс-решения в части Stripe:

- Connect account types: https://docs.stripe.com/connect/accounts
- Express onboarding: https://docs.stripe.com/connect/express-accounts
- Connect onboarding options: https://docs.stripe.com/connect/onboarding
- Checkout overview: https://docs.stripe.com/payments/checkout
- How Checkout works: https://docs.stripe.com/payments/checkout/how-checkout-works
- Saving payment details during payment: https://docs.stripe.com/payments/checkout/save-during-payment
- Destination charges: https://docs.stripe.com/connect/destination-charges
- Separate charges and transfers: https://docs.stripe.com/connect/separate-charges-and-transfers
