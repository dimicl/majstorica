import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { NgIf } from '@angular/common';
import { filter, Subject, takeUntil } from 'rxjs';
import { NavbarComponent } from './components/navbar/navbar.component';
import { AuthSelectorService } from './shared/services/auth-selector.service';
import { ChatService } from './shared/services/chat.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [RouterOutlet, NavbarComponent, NgIf],
})
export class AppComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  private destroy$ = new Subject<void>();
  private auth = inject(AuthSelectorService);
  private chatService = inject(ChatService);

  showNavbar = signal<boolean>(true);
  hasNewMessages = this.chatService.hasNewMessages;

  ngOnInit(): void {
    this.auth.dispatchLoadUser();
    this.updateNavbarVisibility(this.router.url);
    void this.chatService.refreshUnreadIndicator();
    this.router.events
      .pipe(
        filter((e) => e instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe((e: NavigationEnd) => {
        this.updateNavbarVisibility(e.urlAfterRedirects);
        void this.chatService.refreshUnreadIndicator();
      });
  }

  private updateNavbarVisibility(url: string): void {
    const clean = (url || '').split(/[?#]/)[0];
    this.showNavbar.set(
      !(clean.startsWith('/login') || clean.startsWith('/register'))
    );
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
