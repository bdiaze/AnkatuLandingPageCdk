import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Propuesta } from './propuesta';

describe('Propuesta', () => {
  let component: Propuesta;
  let fixture: ComponentFixture<Propuesta>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Propuesta]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Propuesta);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
