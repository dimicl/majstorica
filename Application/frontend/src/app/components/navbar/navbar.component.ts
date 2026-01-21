import { CommonModule } from '@angular/common';
import { Component, ElementRef, HostListener, inject, input, OnInit, signal, ViewChild } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { SvgIconComponent } from 'angular-svg-icon';
import { SharedSvgRoutes } from '../../shared/constants/shared_svg_routes';
import { NavbarItem } from '../../shared/types/navbar-item.type';
import { NavbarItemUserType, NavbarItemString } from '../../shared/enums';
import { NavbarHelper } from './utils/helpers';

@Component({
  selector: 'app-navbar',
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
  imports: [RouterLink, RouterLinkActive, CommonModule, SvgIconComponent],
})
export class NavbarComponent implements OnInit {
  // SVG Icons
  public sharedSvgRoutes = SharedSvgRoutes;

  // Enums 
  public NavbarItemString = NavbarItemString;

  //variables
  public isExpanded = signal<boolean>(false);
  public menuItems : NavbarItem[] = [];
  public bottomItems : NavbarItem[] = [];

  isNewMessages = input<boolean>(false);
  activeNavbarItem: string = NavbarItemString.HOME;

  @ViewChild('navbar') navbar!: ElementRef<HTMLElement>;
  
  @HostListener('document:click', ['$event'])
  onClick(event: MouseEvent): void {
    if (!this.navbar.nativeElement.contains(event.target as Node) && this.isExpanded()) {
      console.log('click outside');
      this.isExpanded.set(false);
    }
  }

  constructor(private router: Router) {}

  ngOnInit(): void {
    this.getNavbarItems();
  }

  expandSidebar(): void {
    this.isExpanded.set(!this.isExpanded());
    
  }

  onItemClick(item: NavbarItem, event: MouseEvent): void {
    event.stopPropagation(); 
    this.activeNavbarItem = item.id;
    this.isExpanded.set(true);
    this.router.navigateByUrl(`/${item.id}`);
  }

  private getNavbarItems() {
    const filteredNavbarItems = NavbarHelper.getFilteredNavbarItems(NavbarItemUserType.CLIENT /*ovde prosledjujes tip*/);
    this.menuItems = filteredNavbarItems.menuItems;
    this.bottomItems = filteredNavbarItems.bottomItems;
  }


}
