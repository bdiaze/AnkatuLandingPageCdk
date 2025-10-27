import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { Menu } from './menu/menu';
import { Hero } from './hero/hero';
import { Productos } from './productos/productos';
import { Propuesta } from './propuesta/propuesta';
import { QuienesSomos } from './quienes-somos/quienes-somos';
import { Clientes } from './clientes/clientes';
import { CallToAction } from './call-to-action/call-to-action';
import { Footer } from './footer/footer';

@Component({
    selector: 'app-root',
    imports: [
        RouterOutlet,
        Menu,
        Hero,
        Productos,
        Propuesta,
        QuienesSomos,
        Clientes,
        CallToAction,
        Footer,
    ],
    templateUrl: './app.html',
    styleUrl: './app.scss',
})
export class App {
    protected readonly title = signal('ankatu-landing-page');
}
