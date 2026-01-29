export interface User {
  id: string;
  email: string;
  password: string;
  username: string;
  role: 'client' | 'master' | 'admin';

  // Dodaj ostala polja po potrebi
}

