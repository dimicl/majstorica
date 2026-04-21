import { NavbarItemUserType } from '../enums/navbar-item-user-type.enum';
import { UserRole } from '../enums/user-role.enum';

/**
 * Mapira ulogu korisnika na tip koji navbar koristi za filtriranje stavki.
 * Pokriva string enum (API), kao i slučaj kada role ponekad dođe kao broj.
 */
export function resolveNavbarItemUserTypeFromRole(
  role: unknown
): NavbarItemUserType {
  if (role === null || role === undefined) {
    return NavbarItemUserType.CLIENT;
  }

  if (typeof role === 'number') {
    switch (role) {
      case 1:
        return NavbarItemUserType.CLIENT;
      case 2:
        return NavbarItemUserType.MASTER;
      case 3:
        return NavbarItemUserType.COMPANY;
      case 4:
        // CompanyWorker – tretira se kao klijent u navbaru dok je worker isključen iz „majstorske“ grupe
        // return NavbarItemUserType.MASTER;
        return NavbarItemUserType.CLIENT;
      case 5:
        return NavbarItemUserType.ADMIN;
      default:
        return NavbarItemUserType.CLIENT;
    }
  }

  const s = String(role).trim();

  if (s === UserRole.Master) return NavbarItemUserType.MASTER;
  // if (s === UserRole.CompanyWorker) return NavbarItemUserType.MASTER;
  if (s === UserRole.CompanyOwner) return NavbarItemUserType.COMPANY;
  if (s === UserRole.Admin) return NavbarItemUserType.ADMIN;
  if (s === UserRole.Client) return NavbarItemUserType.CLIENT;

  return NavbarItemUserType.CLIENT;
}
