import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [CommonModule, ButtonModule],
  template: `
    <div class="error-container">
      <div class="error-content">
        <h1 class="error-code">404</h1>
        <h2 class="error-title">Strona nie znaleziona</h2>
        <p class="error-message">
          Nie możemy znaleźć strony, której szukasz.
        </p>
        <p class="error-description">
          Strona mogła zostać przeniesiona lub usunięta. Sprawdź adres URL lub wróć do strony głównej.
        </p>
        <div class="error-actions">
          <p-button
            label="Powrót do strony głównej"
            icon="pi pi-home"
            (onClick)="goHome()"
            styleClass="p-button-primary">
          </p-button>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .error-container {
      display: flex;
      align-items: center;
      justify-content: center;
      min-height: 100vh;
      padding: 2rem;
      background: var(--surface-ground);
    }

    .error-content {
      text-align: center;
      max-width: 600px;
    }

    .error-code {
      font-size: 8rem;
      font-weight: 700;
      margin: 0;
      color: var(--primary-color);
      line-height: 1;
    }

    .error-title {
      font-size: 2rem;
      font-weight: 600;
      margin: 1rem 0;
      color: var(--text-color);
    }

    .error-message {
      font-size: 1.125rem;
      margin: 1rem 0;
      color: var(--text-color-secondary);
    }

    .error-description {
      font-size: 1rem;
      margin: 1rem 0 2rem;
      color: var(--text-color-secondary);
    }

    .error-actions {
      display: flex;
      gap: 1rem;
      justify-content: center;
      flex-wrap: wrap;
    }
  `]
})
export class NotFoundComponent {
  private router = inject(Router);

  goHome(): void {
    this.router.navigate(['/']);
  }
}
