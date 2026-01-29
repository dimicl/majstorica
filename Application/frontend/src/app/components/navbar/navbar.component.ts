import { CommonModule } from '@angular/common';
import { Component, ElementRef, HostListener, OnDestroy, input, OnInit, signal, ViewChild } from '@angular/core';
import { ActivatedRouteSnapshot, NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SvgIconComponent } from 'angular-svg-icon';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { NavbarItem } from '../../shared/interfaces';
import { NavbarItemUserType, NavbarItemString } from '../../shared/enums';
import { NavbarHelper } from './utils/helpers';
import { Subject } from 'rxjs';
import { filter, takeUntil } from 'rxjs/operators';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
  imports: [RouterLink, RouterLinkActive, CommonModule, SvgIconComponent],
})
export class NavbarComponent implements OnInit, OnDestroy {
  // SVG Icons
  public sharedSvgRoutes = SharedSvgRoutes;

  // Enums 
  public NavbarItemString = NavbarItemString;

  //variables
  public isExpanded = signal<boolean>(false);
  public menuItems : NavbarItem[] = [];
  public bottomItems : NavbarItem[] = [];
  public  activeNavbarItem: string = NavbarItemString.HOME;

  private currentUrl = "";


  isNewMessages = input<boolean>(false);
  private readonly destroy$ = new Subject<void>();

  @ViewChild('navbar') navbar!: ElementRef<HTMLElement>;
  
  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    if (!this.navbar.nativeElement.contains(event.target as Node) && this.isExpanded()) {
      this.isExpanded.set(false);
    }
  }

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.getNavbarItems();
    this.syncNavbarWithRoute(this.router.url);
    // svaka promena rute
    this.router.events
    .pipe(
      filter(event => event instanceof NavigationEnd),
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
    console.log('item', item);
    this.isExpanded.set(true);
    this.router.navigateByUrl(`/${item.id}`);
  }



  private getNavbarItems() {
    const filteredNavbarItems = NavbarHelper.getFilteredNavbarItems(NavbarItemUserType.CLIENT /*ovde prosledjujes tip*/);
    this.menuItems = filteredNavbarItems.menuItems;
    this.bottomItems = filteredNavbarItems.bottomItems;
  }

  private syncNavbarWithRoute(url: string): void {
    const normalizedUrl = (url || '')
      .split(/[?#]/)[0]
      .replace(/^\/+/, '');
  
    const routeSegment =
      normalizedUrl.split('/')[0] || NavbarItemString.HOME;
  
    const matchingItem = [...this.menuItems, ...this.bottomItems]
      .find(item => item.id === routeSegment);
  
    this.activeNavbarItem = matchingItem
      ? matchingItem.id
      : NavbarItemString.HOME;
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

}
