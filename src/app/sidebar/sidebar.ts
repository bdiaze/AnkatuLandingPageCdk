import { Component } from '@angular/core';
import { HlmSidebarImports } from '@spartan-ng/helm/sidebar';
import { lucideHouse, lucideBanknoteArrowUp } from '@ng-icons/lucide';
import { NgIcon, provideIcons } from '@ng-icons/core';
import { HlmIcon } from '@spartan-ng/helm/icon';

@Component({
    selector: 'app-sidebar',
    imports: [HlmSidebarImports, NgIcon, HlmIcon],
    providers: [
        provideIcons({
            lucideHouse,
            lucideBanknoteArrowUp,
        }),
    ],
    templateUrl: './sidebar.html',
    styleUrl: './sidebar.scss',
})
export class Sidebar {}
