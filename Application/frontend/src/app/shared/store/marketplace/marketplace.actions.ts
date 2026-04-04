import { createActionGroup, props } from '@ngrx/store';
import { JobListItem } from '../../services/job.service';

export const MarketplaceActions = createActionGroup({
  source: 'Marketplace',
  events: {
    'Load Jobs': props<{ page: number; pageSize: number }>(),
    'Load Jobs Success': props<{
      jobs: JobListItem[];
      page: number;
      pageSize: number;
      hasMore: boolean;
    }>(),
    'Load Jobs Failure': props<{ error: string }>(),
  },
});
