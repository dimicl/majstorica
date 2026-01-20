import { NavbarItemString, NavbarItemUserType } from "../enums";
import { NavbarItem } from "../types/navbar-item.type";
import { SharedSvgRoutes } from "./shared_svg_routes";

export const NAVBAR_ITEMS : NavbarItem[] = [
    {
        id: NavbarItemString.HOME,
        icon: SharedSvgRoutes.HOME_ICON,
        label: NavbarItemString.HOME_LABEL,
        userType: NavbarItemUserType.ALL
      },
      {
        id: NavbarItemString.SERVICES,
        icon: SharedSvgRoutes.SERVICES_ICON,
        label: NavbarItemString.SERVICES_LABEL,
        userType: NavbarItemUserType.CLIENT
      },
      {
        id: NavbarItemString.TECHNICIANS,
        icon: SharedSvgRoutes.TECHNICIAN_ICON,
        label: NavbarItemString.TECHNICIANS_LABEL,
        userType: NavbarItemUserType.CLIENT
      },
      {
        id: NavbarItemString.MESSAGES,
        icon: SharedSvgRoutes.MESSAGE_ICON,
        label: NavbarItemString.MESSAGES_LABEL,
        userType: NavbarItemUserType.ALL
      },
      {
        id: NavbarItemString.PROFILE,
        icon: SharedSvgRoutes.PROFILE_ICON,
        label: NavbarItemString.PROFILE_LABEL,
        userType: NavbarItemUserType.ALL,
        isBottomPinned: true
      },
      {
        id: NavbarItemString.REQUESTS,
        icon: '',
        label: NavbarItemString.REQUESTS_LABEL,
        userType: NavbarItemUserType.TECHNICIAN,
        isBottomPinned: false
      }

]