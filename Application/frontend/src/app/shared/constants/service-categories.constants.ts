import { ServiceCardModel } from '../interfaces';

/** Kategorije usluga za glavnu stranicu (main). */
export const SERVICE_CATEGORIES: ServiceCardModel[] = [
  {
    id: 1,
    icon: '⚡',
    title: 'Električar',
    description: 'Ugradnja, popravke, kratki spojevi, rasveta.',
  },
  {
    id: 2,
    icon: '🚰',
    title: 'Vodoinstalater',
    description: 'Curanje, sifoni, ventili, sanitarije.',
  },
  {
    id: 3,
    icon: '🧱',
    title: 'Keramičar',
    description: 'Kupatila, kuhinje, fugovanje i nivelacija.',
  },
  {
    id: 4,
    icon: '🛠️',
    title: 'Majstor za sve',
    description: 'Montaže, sitne popravke, "po kući".',
  },
  {
    id: 5,
    icon: '🎨',
    title: 'Moler',
    description: 'Krečenje, gletovanje, priprema zidova.',
  },
  {
    id: 6,
    icon: '🪚',
    title: 'Stolar',
    description: 'Nameštaj po meri, popravke, vrata.',
  },
];
