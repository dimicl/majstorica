import { NavbarItemUserType } from "../enums";


export interface NavbarItem {
  id: string;
  icon: string;
  label: string;
  /** Jedna uloga ili više (npr. marketplace za majstora i firmu). */
  userType: NavbarItemUserType | NavbarItemUserType[];
  isBottomPinned?: boolean;
}