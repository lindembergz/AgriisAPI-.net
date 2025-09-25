import { Component, inject, signal } from '@angular/core';
import { FormBuilder, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';

import { CardModule } from 'primeng/card';
import { InputTextModule } from 'primeng/inputtext';
import { PasswordModule } from 'primeng/password';
import { CheckboxModule } from 'primeng/checkbox';
import { ButtonModule } from 'primeng/button';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { ValidationService } from '../../shared/services/validation.service';
import { emailValidator } from '../../shared/utils/field-validators.util';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    InputTextModule,
    PasswordModule,
    CheckboxModule,
    ButtonModule,
    ProgressSpinnerModule,
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);
  private notificationService = inject(NotificationService);
  private validationService = inject(ValidationService);

  // Loading state management with signals
  isLoading = signal<boolean>(false);

  loginForm = this.fb.group({
    username: ['', [Validators.required, emailValidator()]],
    password: ['', [Validators.required]],
    rememberMe: [false],
  });

  onSubmit() {
    if (this.loginForm.valid) {
      this.isLoading.set(true);
      const { username, password } = this.loginForm.value;
      
      this.authService.login(username!, password!).subscribe({
        next: (response) => {
          this.isLoading.set(false);
          this.notificationService.showLoginSuccess();
        },
        error: (error) => {
          this.isLoading.set(false);
          this.handleLoginError(error);
        }
      });
    } else {
      this.validationService.markFormGroupTouched(this.loginForm);
      this.notificationService.showValidationWarning('Verifique os dados informados');
    }
  }

  /**
   * Handle login errors with appropriate messages
   */
  private handleLoginError(error: any): void {
    let errorMessage = 'Erro ao realizar login';
    
    if (error?.status === 401) {
      errorMessage = 'Credenciais inválidas';
    } else if (error?.status === 0) {
      errorMessage = 'Erro de conexão com o servidor';
    } else if (error?.message) {
      errorMessage = error.message;
    }
    
    this.notificationService.showCustomError(errorMessage);
  }

  /**
   * Get field error message
   */
  getFieldError(fieldName: string): string {
    const field = this.loginForm.get(fieldName);
    if (!field) return '';
    
    return this.validationService.getErrorMessage(field, fieldName);
  }

  /**
   * Check if field has error
   */
  hasFieldError(fieldName: string): boolean {
    const field = this.loginForm.get(fieldName);
    if (!field) return false;
    
    return this.validationService.shouldShowError(field);
  }
}
