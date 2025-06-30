import { Component } from '@angular/core';
import { RouterOutlet, Router, NavigationEnd } from '@angular/router';
import { NgIf } from '@angular/common';
import { Title } from '@angular/platform-browser';
import { filter } from 'rxjs/operators';
import { LayoutComponent } from "./layout/layout.component";

@Component({
    selector: 'app-root',
    imports: [RouterOutlet, NgIf, LayoutComponent],
    templateUrl: './app.component.html',
    styleUrl: './app.component.css'
})
export class AppComponent {

  constructor(private router: Router, private titleService: Title) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => {
        this.updateTitle();
      });

  }

  get isSimplePage(): boolean {
    const simpleRoutes = ['/login', '/not-found', '/forbidden'];
    return simpleRoutes.includes(this.router.url);
  }

  updateTitle() {
    switch (this.router.url) {
      case '/login':
        this.titleService.setTitle('Login - Expenses Manager');
        break;
      case '/not-found':
        this.titleService.setTitle('404 - Not Found');
        break;
      case '/forbidden':
        this.titleService.setTitle('403 - Forbidden');
        break;
      default:
        this.titleService.setTitle('Expenses Manager');
    }
  }
}
