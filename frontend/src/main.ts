// main.ts
import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// Bootstrap запускает корневой Angular-компонент с общей конфигурацией приложения.
bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
