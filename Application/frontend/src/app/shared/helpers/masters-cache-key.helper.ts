import type { MastersListParams } from '../interfaces/masters-list-params.interface';

export function mastersListParamsToCacheKey(params: MastersListParams): string {
  const search = (params.search ?? '').trim().toLowerCase();
  const sort = params.sort ?? 'name-asc';
  const category = (params.category ?? '').trim();
  const minRating = params.minRating ?? '';
  const entityType = params.entityType ?? 'all';
  return `search=${encodeURIComponent(
    search
  )}&sort=${sort}&category=${encodeURIComponent(
    category
  )}&minRating=${minRating}&entityType=${entityType}`;
}
