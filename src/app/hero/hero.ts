import { Component } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';

@Component({
    selector: 'app-hero',
    imports: [HlmButtonImports],
    templateUrl: './hero.html',
    styleUrl: './hero.scss',
})
export class Hero {}
