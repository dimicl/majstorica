import { Routes } from '@angular/router';
import { AuthComponent } from './pages/auth/auth.component';
import { ChatComponent } from './pages/chat/chat.component';
import { MainComponent } from './pages/main/main.component';
import { ServicesComponent } from './pages/services/services.component';
import { TechniciansComponent } from './pages/technicians/technicians.component';
import { ProfilComponent } from './pages/profil/profil.component';
import { authGuard } from './shared/guards/auth.guard';
import { profileLoadUserResolver } from './shared/resolvers/profile-load-user.resolver';

export const routes: Routes = [
  {
    path: 'login',
    component: AuthComponent,
  },
  {
    path: 'home',
    component: MainComponent,
    resolve: { _: profileLoadUserResolver },
  },
  {
    path: 'chat',
    component: ChatComponent,
    canActivate: [authGuard],
  },
  // Navbar koristi id "messages" (Poruke) → vodi na chat stranicu
  {
    path: 'chat',
    component: ChatComponent,
    canActivate: [authGuard],
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
    canActivate: [authGuard],
    resolve: { _: profileLoadUserResolver },
  },
  {
    path: '',
    redirectTo: 'home',
    pathMatch: 'full',
  },
];
