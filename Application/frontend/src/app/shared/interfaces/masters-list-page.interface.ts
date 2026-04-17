import type { MasterListItem } from './master-list-item.interface';

export interface MastersListPage {
  items: MasterListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}
