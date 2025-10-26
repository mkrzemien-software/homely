import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';

// Components
import { LoginFormComponent } from '../components/login-form/login-form.component';
import { AuthLinksComponent } from '../components/auth-links/auth-links.component';
import { ThemeToggleComponent } from '../../../shared/components/theme-toggle/theme-toggle.component';

// Services
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  imports: [
    CommonModule,
    LoginFormComponent,
    AuthLinksComponent,
    ThemeToggleComponent
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);

  ngOnInit(): void {
    // Redirect to dashboard if already authenticated
    if (this.authService.isLoggedIn()) {
      this.router.navigate(['/dashboard']);
    }
  }

  /**
   * Handle successful login
   */
  onLoginSuccess(): void {
    // This will be called when login is successful
    // Navigation is handled in LoginFormComponent, but we can add additional logic here
    console.log('Login successful');
  }
}
