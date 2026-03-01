/**
 * Kategorija majstora – usklađeno sa backend Domain.Enums.MasterCategory.
 * Vrednosti 1–6; prikazna imena za UI.
 */
export enum MasterCategory {
  Elektricar = 1,
  Vodoinstalater = 2,
  Keramicar = 3,
  MajstorZaSve = 4,
  Moler = 5,
  Stolar = 6,
}

const displayNames: Record<MasterCategory, string> = {
  [MasterCategory.Elektricar]: 'Električar',
  [MasterCategory.Vodoinstalater]: 'Vodoinstalater',
  [MasterCategory.Keramicar]: 'Keramičar',
  [MasterCategory.MajstorZaSve]: 'Majstor za sve',
  [MasterCategory.Moler]: 'Moler',
  [MasterCategory.Stolar]: 'Stolar',
};

export function getMasterCategoryDisplayName(category: MasterCategory): string {
  return displayNames[category] ?? category.toString();
}

/** Sve kategorije za dropdown (bez "Sve kategorije"). */
export const MASTER_CATEGORY_OPTIONS: { value: MasterCategory; label: string }[] = (
  Object.keys(displayNames) as unknown as MasterCategory[]
).map((value) => ({ value, label: displayNames[value] }));

/**
 * Mapiranje prikaznog imena (iz API-ja) u enum ili null.
 */
export function masterCategoryFromDisplayName(displayName: string | null | undefined): MasterCategory | null {
  if (displayName == null || displayName.trim() === '') return null;
  const entry = MASTER_CATEGORY_OPTIONS.find(
    (o) => o.label.localeCompare(displayName.trim(), undefined, { sensitivity: 'accent' }) === 0
  );
  return entry?.value ?? null;
}

/**
 * Vraća prikazno ime za enum vrednost (za slanje na backend).
 */
export function masterCategoryToDisplayName(category: MasterCategory | null | undefined): string | null {
  if (category == null) return null;
  return getMasterCategoryDisplayName(category);
}
