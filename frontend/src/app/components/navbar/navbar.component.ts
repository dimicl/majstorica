import { CommonModule } from '@angular/common';
import { Component, input, OnInit, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
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

  ngOnInit(): void {
    this.getNavbarItems();
  }

  expandSidebar(): void {
    this.isExpanded.set(!this.isExpanded());
    
  }

  onItemClick(itemId: string): void {
    this.activeNavbarItem = itemId;
    this.isExpanded.set(false);
  }

  private getNavbarItems() {
    const filteredNavbarItems = NavbarHelper.getFilteredNavbarItems(NavbarItemUserType.CLIENT /*ovde prosledjujes tip*/);
    this.menuItems = filteredNavbarItems.menuItems;
    this.bottomItems = filteredNavbarItems.bottomItems;
  }


}
