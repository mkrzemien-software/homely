import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-auth-links',
  imports: [
    CommonModule,
    RouterLink
  ],
  templateUrl: './auth-links.component.html',
  styleUrl: './auth-links.component.scss'
})
export class AuthLinksComponent {
  // No additional logic needed - just navigation links
}
