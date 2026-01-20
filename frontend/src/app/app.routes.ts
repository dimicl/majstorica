import { Routes } from '@angular/router';
import { AuthComponent } from './pages/auth/auth.component';
import { MainComponent } from './pages/main/main.component';
import { ServicesComponent } from './pages/services/services.component';
import { TechniciansComponent } from './pages/technicians/technicians.component';
import { ProfilComponent } from './pages/profil/profil.component';

export const routes: Routes = [
  {
    path: 'login',
    component: AuthComponent,
  },
  {
    path: 'home',
    component: MainComponent,
  },
  {
    path: 'services',
    component: ServicesComponent,
  },
  {
    path: 'technicians',
    component: TechniciansComponent,
  },
  {
    path: 'profile',
    component: ProfilComponent,
  },
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },
];
