import { CommonModule } from '@angular/common';
import {
  Component,
  ElementRef,
  HostListener,
  inject,
  OnDestroy,
  input,
  OnInit,
  signal,
  ViewChild,
} from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { SvgIconComponent } from 'angular-svg-icon';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { NavbarItem } from '../../shared/interfaces';
import { NavbarItemUserType, NavbarItemString } from '../../shared/enums';
import { UserRole } from '../../shared/enums/user-role.enum';
import { NavbarHelper } from './utils/helpers';
import { AuthSelectorService } from '../../shared/services/auth-selector.service';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
  imports: [CommonModule, SvgIconComponent],
})
export class NavbarComponent implements OnInit, OnDestroy {
  // SVG Icons
  public sharedSvgRoutes = SharedSvgRoutes;

  // Enums
  public NavbarItemString = NavbarItemString;

  //variables
  public isExpanded = signal<boolean>(false);
  public menuItems: NavbarItem[] = [];
  public bottomItems: NavbarItem[] = [];
  public activeNavbarItem: string = NavbarItemString.HOME;

  isNewMessages = input<boolean>(false);
  private readonly destroy$ = new Subject<void>();

  @ViewChild('navbar') navbar!: ElementRef<HTMLElement>;

  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    if (
      !this.navbar.nativeElement.contains(event.target as Node) &&
      this.isExpanded()
    ) {
      this.isExpanded.set(false);
    }
  }

  readonly auth = inject(AuthSelectorService);

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.initItems();
  }

  private initItems(): void {
    this.auth.userSelector$.pipe(takeUntil(this.destroy$)).subscribe((user) => {
      this.getNavbarItems(this.mapRoleToNavbarUserType(user?.role));
      this.syncNavbarWithRoute(this.router.url);
    });
    this.syncNavbarWithRoute(this.router.url);
    this.router.events
      .pipe(
        filter((event) => event instanceof NavigationEnd),
        takeUntil(this.destroy$)
      )
      .subscribe((event: NavigationEnd) => {
        this.syncNavbarWithRoute(event.urlAfterRedirects);
      });
  }

  public expandSidebar(): void {
    this.isExpanded.set(!this.isExpanded());
  }

  public onItemClick(item: NavbarItem, event: MouseEvent): void {
    event.stopPropagation();

    // Ako je logout, pokreni logout akciju
    if (item.id === NavbarItemString.LOGOUT) {
      this.auth.dispatchLogout();
      return;
    }

    this.isExpanded.set(true);
    this.router.navigateByUrl(`/${item.id}`);
  }

  private mapRoleToNavbarUserType(role: UserRole | undefined): NavbarItemUserType {
    switch (role) {
      case UserRole.Master:
      case UserRole.CompanyWorker:
        return NavbarItemUserType.MASTER;
      case UserRole.CompanyOwner:
        return NavbarItemUserType.COMPANY_OWNER;
      case UserRole.Admin:
        return NavbarItemUserType.ADMIN;
      case UserRole.Client:
      default:
        return NavbarItemUserType.CLIENT;
    }
  }

  private getNavbarItems(userType: NavbarItemUserType): void {
    const filtered = NavbarHelper.getFilteredNavbarItems(userType);
    this.menuItems = filtered.menuItems;
    this.bottomItems = filtered.bottomItems;
  }

  private syncNavbarWithRoute(url: string): void {
    const normalizedUrl = (url || '').split(/[?#]/)[0].replace(/^\/+/, '');

    const routeSegment = normalizedUrl.split('/')[0] || NavbarItemString.HOME;

    const matchingItem = [...this.menuItems, ...this.bottomItems].find(
      (item) => item.id === routeSegment
    );

    this.activeNavbarItem = matchingItem
      ? matchingItem.id
      : NavbarItemString.HOME;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
