/** Brzi filteri na stranici usluga. */
export const SERVICES_QUICK_FILTERS = [
  'Električar',
  'Vodoinstalater',
  'Keramičar',
  'Klimatske instalacije',
  'Stolar',
  'Moler',
] as const;

/** Statistike u hero sekciji stranice usluga. */
export const SERVICES_HERO_STATS = [
  { value: '1.200+', label: 'zahteva mesečno' },
  { value: '97%', label: 'zadovoljstvo klijenata' },
  { value: '45 min', label: 'prosečan odgovor' },
] as const;

/** Istaknute usluge na stranici usluga. */
export const SERVICES_FEATURED = [
  {
    title: 'Električar',
    description: 'Hitne intervencije, osvetljenje, pametni sistemi i prepravke.',
    tag: 'Hitno',
    rating: '★ 4.9',
    icon: '⚡',
  },
  {
    title: 'Vodoinstalater',
    description: 'Sanitarije, cevovodi, grejni sistemi i zamena bojlera.',
    tag: 'Popularno',
    rating: '★ 4.8',
    icon: '🚰',
  },
  {
    title: 'Keramičar',
    description: 'Postavljanje pločica, fugovanje i nivelacija podova i zidova.',
    tag: 'Preporučeno',
    rating: '★ 4.7',
    icon: '🧱',
  },
  {
    title: 'Servis klima',
    description: 'Sezonsko održavanje, punjenje gasa i čišćenje filtera.',
    tag: 'Sezonski',
    rating: '★ 4.8',
    icon: '❄️',
  },
] as const;

/** Koraci procesa na stranici usluga. */
export const SERVICES_PROCESS_STEPS = [
  {
    index: '01',
    title: 'Napiši šta ti treba',
    text: 'Izaberi kategoriju, opiši problem i dodaj sliku ako možeš.',
  },
  {
    index: '02',
    title: 'Uporedi ponude',
    text: 'Majstori ti šalju procene, rokove i dostupnost. Ti biraš kome veruješ.',
  },
  {
    index: '03',
    title: 'Prati posao',
    text: 'Dogovoreni datum se beleži u kalendaru, a komunikacija ostaje na platformi.',
  },
] as const;
