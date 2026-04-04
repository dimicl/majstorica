import { JobListItem } from '../../services/job.service';

export interface MarketplaceState {
  jobs: JobListItem[];
  page: number;
  pageSize: number;
  hasMore: boolean;
  loading: boolean;
  error: string | null;
}

export const initialMarketplaceState: MarketplaceState = {
  jobs: [],
  page: 1,
  pageSize: 10,
  hasMore: false,
  loading: false,
  error: null,
};
