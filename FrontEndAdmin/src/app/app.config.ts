import { ApplicationConfig, isDevMode } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideHttpClient, withInterceptors } from '@angular/common/http';

import { routes } from './app.routes';
import { provideClientHydration } from '@angular/platform-browser';
import { providePrimeNG } from 'primeng/config';
import Lara from '@primeuix/themes/lara';
import { definePreset } from '@primeuix/themes';
import { authInterceptor } from './core/interceptors/auth.interceptor';
import { errorInterceptor } from './core/interceptors/error.interceptor';
import { MessageService, ConfirmationService } from 'primeng/api';
import { environment } from '../environments/environment';
import { provideServiceWorker } from '@angular/service-worker';

// Tema customizado com azul institucional
const CustomTheme = definePreset(Lara, {
  semantic: {
    primary: {
      50: '#e6eaf7',
      100: '#b3c2e8',
      200: '#8099d8',
      300: '#4d70c8',
      400: '#2651bc',
      500: '#002060',
      600: '#001d58',
      700: '#001a4e',
      800: '#001744',
      900: '#001133'
    },
    colorScheme: {
      light: {
        primary: {
          color: '#002060',
          contrastColor: '#ffffff',
          hoverColor: '#001d58',
          activeColor: '#001744'
        }
      },
      dark: {
        primary: {
          color: '#2651bc',
          contrastColor: '#ffffff',
          hoverColor: '#4d70c8',
          activeColor: '#001d58'
        }
      }
    }
  }
});

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes), 
    provideClientHydration(),
    provideAnimationsAsync(),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
    MessageService, // PrimeNG MessageService for toast notifications
    ConfirmationService, // PrimeNG ConfirmationService for confirmation dialogs
    providePrimeNG({
      theme: {
        preset: CustomTheme,
        options: {
          prefix: 'p',
          darkModeSelector: 'system',
          cssLayer: false
        }
      }
    }), provideServiceWorker('ngsw-worker.js', {
            enabled: !isDevMode(),
            registrationStrategy: 'registerWhenStable:30000'
          })
  ]
};
