import { NavbarItemUserType } from '../enums';

export interface NavbarItem {
  id: string;
  icon: string;
  label: string;
  userType: NavbarItemUserType[];
  isBottomPinned?: boolean;
}
