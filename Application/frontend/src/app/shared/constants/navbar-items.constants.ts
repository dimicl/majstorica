import { NavbarItemString, NavbarItemUserType } from '../enums';
import { NavbarItem } from '../interfaces';
import { SharedSvgRoutes } from './shared_svg_routes';

export const NAVBAR_ITEMS: NavbarItem[] = [
  {
    id: NavbarItemString.HOME,
    icon: SharedSvgRoutes.HOME_ICON,
    label: NavbarItemString.HOME_LABEL,
    userType: NavbarItemUserType.ALL,
  },
  {
    id: NavbarItemString.MASTERS,
    icon: SharedSvgRoutes.MASTER_ICON,
    label: NavbarItemString.MASTERS_LABEL,
    userType: NavbarItemUserType.CLIENT,
  },
  {
    id: NavbarItemString.MARKETPLACE,
    icon: SharedSvgRoutes.MARKETPLACE_ICON,
    label: NavbarItemString.MARKETPLACE_LABEL,
    userType: NavbarItemUserType.MASTER || NavbarItemUserType.COMPANY,
    isBottomPinned: false,
  },
  {
    id: NavbarItemString.MESSAGES,
    icon: SharedSvgRoutes.MESSAGE_ICON,
    label: NavbarItemString.MESSAGES_LABEL,
    userType: NavbarItemUserType.ALL,
  },
  {
    id: NavbarItemString.PROFILE,
    icon: SharedSvgRoutes.PROFILE_ICON,
    label: NavbarItemString.PROFILE_LABEL,
    userType: NavbarItemUserType.ALL,
    isBottomPinned: true,
  },
  {
    id: NavbarItemString.LOGOUT,
    icon: SharedSvgRoutes.LOGOUT_ICON,
    label: NavbarItemString.LOGOUT_LABEL,
    userType: NavbarItemUserType.ALL,
    isBottomPinned: true,
  },
  {
    id: NavbarItemString.REQUESTS,
    icon: SharedSvgRoutes.REQUESTS_ICON,
    label: NavbarItemString.REQUESTS_LABEL,
    userType: NavbarItemUserType.MASTER,
    isBottomPinned: false,
  },
];
