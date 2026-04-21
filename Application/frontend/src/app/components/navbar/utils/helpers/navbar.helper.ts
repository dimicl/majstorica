import { NAVBAR_ITEMS } from '../../../../shared/constants/navbar-items.constants';
import { NavbarItemUserType } from '../../../../shared/enums';
import type { FilteredNavbarItems } from '../../../../shared/interfaces';
import type { NavbarItem } from '../../../../shared/interfaces/navbar-item.interface';

export class NavbarHelper {
  static getFilteredNavbarItems(userType: NavbarItemUserType): FilteredNavbarItems {
    const allItems = NAVBAR_ITEMS.filter((item) =>
      NavbarHelper.itemVisibleForUserType(item, userType)
    );

    return {
      menuItems: allItems.filter((item) => !item.isBottomPinned),
      bottomItems: allItems.filter((item) => item.isBottomPinned),
    };
  }

  private static itemVisibleForUserType(
    item: NavbarItem,
    userType: NavbarItemUserType
  ): boolean {
    if (item.userType === NavbarItemUserType.ALL) return true;
    const allowed = Array.isArray(item.userType)
      ? item.userType
      : [item.userType];
    return allowed.includes(userType);
  }
}
