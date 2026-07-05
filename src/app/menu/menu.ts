import { Component, HostListener } from '@angular/core';
import { HlmButton } from '@spartan-ng/helm/button';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmSeparatorImports } from '@spartan-ng/helm/separator';

@Component({
    selector: 'app-menu',
    standalone: true,
    imports: [HlmButtonImports, HlmSeparatorImports],
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

    scrollTo(sectionId: string, block: ScrollLogicalPosition = 'center') {
        const element = document.getElementById(sectionId);
        if (element) {
            element.scrollIntoView({ behavior: 'smooth', block: block });
            this.isMenuOpen = false;
        }
    }
}
