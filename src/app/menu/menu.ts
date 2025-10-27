import { Component, HostListener } from '@angular/core';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { lucidePlus } from '@ng-icons/lucide';
import { HlmButton } from '@spartan-ng/helm/button';
import { HlmButtonGroup } from '@spartan-ng/helm/button-group';
import { HlmIcon } from '@spartan-ng/helm/icon';
import { HlmButtonImports } from '@spartan-ng/helm/button';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [NgIcon, HlmIcon, HlmButton, HlmButtonGroup, HlmButtonImports],
    providers: [
        provideIcons({
            lucidePlus,
        }),
    ],
    templateUrl: './menu.html',
    styleUrl: './menu.scss',
})
export class Menu {
    isMenuOpen = false;
    isScrolled = false;

    @HostListener('window:scroll', [])
    onWindowScroll() {
        this.isScrolled = window.scrollY > 20;
    }

    scrollTo(sectionId: string) {
        const element = document.getElementById(sectionId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: 'start' });
            this.isMenuOpen = false;
        }
    }
}
