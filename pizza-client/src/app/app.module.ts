import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { NavComponent } from './nav/nav.component';
import {NgbDropdown, NgbDropdownModule, NgbModule} from '@ng-bootstrap/ng-bootstrap';
import { RegisterComponent } from './register/register.component';
import {HttpClientModule} from "@angular/common/http";
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import { TextInputComponent } from './_forms/text-input/text-input.component';
import { PizzaListComponent } from './pizzas/pizza-list/pizza-list.component';
import { PizzaItemComponent } from './pizzas/pizza-item/pizza-item.component';
import { PizzaDetailComponent } from './pizzas/pizza-detail/pizza-detail.component';
import { CartComponent } from './cart/cart/cart.component';
import { PizzaMakerPanelComponent } from './pizza-maker/pizza-maker-panel/pizza-maker-panel.component';

@NgModule({
  declarations: [
    AppComponent,
    NavComponent,
    RegisterComponent,
    TextInputComponent,
    PizzaListComponent,
    PizzaItemComponent,
    PizzaDetailComponent,
    CartComponent,
    PizzaMakerPanelComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    NgbModule,
    HttpClientModule,
    FormsModule,
    ReactiveFormsModule,
    NgbDropdownModule,
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
