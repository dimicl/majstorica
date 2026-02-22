import { NAVBAR_ITEMS } from '../../../../shared/constants/navbar-items.constants';
import { NavbarItemUserType } from '../../../../shared/enums';
import type { FilteredNavbarItems } from '../../../../shared/interfaces';

export class NavbarHelper {
  static getFilteredNavbarItems(userType: NavbarItemUserType): FilteredNavbarItems {
    const allItems = NAVBAR_ITEMS.filter(
      item => item.userType === NavbarItemUserType.ALL || item.userType === userType
    );
  
    return {
      menuItems: allItems.filter(item => !item.isBottomPinned),
      bottomItems: allItems.filter(item => item.isBottomPinned)
    };
  }
}
