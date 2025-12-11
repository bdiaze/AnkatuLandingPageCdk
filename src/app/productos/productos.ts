import { Component } from '@angular/core';
import { HlmButtonImports } from '@spartan-ng/helm/button';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmBadgeImports } from '@spartan-ng/helm/badge';

@Component({
    selector: 'app-productos',
    imports: [HlmButtonImports, HlmCardImports, HlmBadgeImports],
    templateUrl: './productos.html',
    styleUrl: './productos.scss',
})
export class Productos {}
