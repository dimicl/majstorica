export type MasterListKind = 'master' | 'company';

export interface MasterListItem {
  kind?: MasterListKind;
  id: string;
  firstName: string;
  lastName: string;
  username: string;
  category: string | null;
  rating: number | null;
  serviceCategories?: string[] | null;
  companyName?: string | null;
  description?: string | null;
  city?: string | null;
  email?: string | null;
}
