# Урок 1. Первые шаги в JavaScript

**Курс:** JavaScript с нуля до практики
**Модуль:** Введение

## Что такое JavaScript

JavaScript (JS) — язык, который заставляет веб-страницы «оживать»: реагировать на клики,
загружать данные, анимировать элементы. Работает прямо в браузере, не требует установки.

## Первая строка кода

Откройте в браузере любую страницу, нажмите **F12** → вкладка **Console** и введите:

```javascript
console.log("Привет, мир!");
```

## Переменные

```javascript
let name = "Анна";      // изменяемая
const PI = 3.14159;     // неизменяемая
var old = "так писали"; // старый стиль, избегать
```

## Типы данных

| Тип | Пример |
|---|---|
| string | `"текст"` |
| number | `42`, `3.14` |
| boolean | `true`, `false` |
| null | `null` |
| undefined | `undefined` |
| object | `{ name: "Анна" }` |
| array | `[1, 2, 3]` |

## Условия

```javascript
const age = 18;

if (age >= 18) {
    console.log("Совершеннолетний");
} else {
    console.log("Несовершеннолетний");
}
```

## Функция

```javascript
function greet(name) {
    return `Привет, ${name}!`;
}

console.log(greet("Анна"));  // Привет, Анна!
```

## Практика

Напишите функцию `getFullName(first, last)`, которая принимает имя и фамилию
и возвращает их через пробел.

```javascript
// Ваш код
function getFullName(first, last) {
    // ...
}

console.log(getFullName("Иван", "Иванов"));  // Иван Иванов
```
