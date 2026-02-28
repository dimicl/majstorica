export interface MastersListParams {
  search: string;
  sort: MastersListSort;
  category: string;
  minRating: number | null;
}

export type MastersListSort = 'name-asc' | 'name-desc';

export const MASTERS_LIST_SORT_OPTIONS: {
  value: MastersListSort;
  label: string;
}[] = [
  { value: 'name-asc', label: 'Ime A–Z' },
  { value: 'name-desc', label: 'Ime Z–A' },
];

export const MASTERS_LIST_CATEGORY_OPTIONS: { value: string; label: string }[] =
  [
    { value: '', label: 'Sve kategorije' },
    { value: 'Električar', label: 'Električar' },
    { value: 'Vodoinstalater', label: 'Vodoinstalater' },
    { value: 'Keramičar', label: 'Keramičar' },
    { value: 'Majstor za sve', label: 'Majstor za sve' },
    { value: 'Moler', label: 'Moler' },
    { value: 'Stolar', label: 'Stolar' },
  ];

export const MASTERS_LIST_RATING_OPTIONS: {
  value: number | null;
  label: string;
}[] = [
  { value: null, label: 'Sve ocene' },
  { value: 4, label: 'Ocena 4+' },
  { value: 3, label: 'Ocena 3+' },
  { value: 2, label: 'Ocena 2+' },
  { value: 1, label: 'Ocena 1+' },
];

export const DEFAULT_MASTERS_LIST_PARAMS: MastersListParams = {
  search: '',
  sort: 'name-asc',
  category: '',
  minRating: null,
};
