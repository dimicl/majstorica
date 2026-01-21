import { NavbarItemUserType } from "../enums";


export type NavbarItem = {
    id: string,
    icon: string,
    label: string,
    userType: NavbarItemUserType,
    isBottomPinned?: boolean,
}