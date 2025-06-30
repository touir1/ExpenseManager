import { Routes } from '@angular/router';
import { LayoutComponent } from './layout/layout.component';
import { LoginComponent } from './login/login.component';
import { ForbiddenComponent } from './errors/forbidden/forbidden.component';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { HomeComponent } from './home/home.component';

export const routes: Routes = [
    { path: 'login', component: LoginComponent },
    { path: 'forbidden', component: ForbiddenComponent },
    { path: 'not-found', component: NotFoundComponent },
    { 
        path: '', 
        component: LayoutComponent, 
        children: [
            { path: '', redirectTo: 'home', pathMatch: 'full' },
            { path: 'home', component: HomeComponent },
        ]
    },
    { path: '**', redirectTo: 'not-found' }
];
