import { Component } from '@angular/core';
import { HlmCardImports } from '@spartan-ng/helm/card';
import { HlmSkeletonImports } from '@spartan-ng/helm/skeleton';

@Component({
    selector: 'app-clientes',
    imports: [HlmCardImports, HlmSkeletonImports],
    templateUrl: './clientes.html',
    styleUrl: './clientes.scss',
})
export class Clientes {}
