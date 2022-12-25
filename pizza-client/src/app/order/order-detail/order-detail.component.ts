import {Component, OnInit} from '@angular/core';
import {ActivatedRoute, Router} from "@angular/router";
import {PizzaService} from "../../_services/pizza.service";
import {TopingService} from "../../_services/toping.service";
import {AccountService} from "../../_services/account.service";
import {ToastrService} from "ngx-toastr";
import {Order, OrderState} from 'src/app/_models/order';
import {OrderService} from "../../_services/order.service";
import {User} from 'src/app/_models/user';
import {Pizza, State} from "../../_models/pizza";
import {BsModalRef, BsModalService} from "ngx-bootstrap/modal";
import {StateModalComponent} from "../../modals/state-modal/state-modal.component";
import {FormBuilder, FormGroup} from "@angular/forms";

@Component({
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.css']
})
export class OrderDetailComponent implements OnInit {
  order: Order | undefined;
  id: string | null;
  user: User;
  bsModalRef: BsModalRef;

  states = [
    {id: 0, name: State[0]},
    {id: 1, name: State[1]},
    {id: 2, name: State[2]},
    {id: 3, name: State[3]},
  ]
  selectState: FormGroup;

  constructor(private router: Router,
              private route: ActivatedRoute,
              private pizzasService: PizzaService,
              private topingService: TopingService,
              public accountService: AccountService,
              private toastr: ToastrService,
              private orderService: OrderService,
              private modalService: BsModalService,
              private fb: FormBuilder) {
    this.route.paramMap.subscribe(response => {
      this.id = response.get('id');
    })
    this.orderService.getOrderById(this.id).subscribe(response => {
      this.order = response;
      this.order?.pizzas.forEach(el => {
        el.stateAsString = State[el.state];
      })
      this.order.orderStateAsString = OrderState[this.order.orderState];
      this.order.pizzas.forEach(el => {
        let a = 0;
        el.topings.forEach(el2 => {
          a = a + el2.counter * 20
        })
        el.maxCost = a + el.cost;
      })
    })

  }

  ngOnInit(): void {
    this.selectState = this.fb.group({
      state: [] = []
    });
  }

  openStateModel(pizza: Pizza) {
    const config = {
      class: 'modal-dialog-centered',
      initialState: {
        pizza
      }
    }
    // @ts-ignore
    this.bsModalRef = this.modalService.show(StateModalComponent, config);
  }

  saveStates() {
    console.log("Form Submitted")
    console.log(this.selectState.value)
  }

  changeState(pizza: Pizza, item: any) {
    pizza.state = item.id;
    pizza.stateAsString = item.name;
  }

  mySelectHandler($event: any, pizza: Pizza) {
    pizza.state = $event.id;
    pizza.stateAsString = $event.name;
  }

  updateState(pizzas: Pizza[]) {
    this.updatePizzasState(pizzas);
  }

  updatePizzasState(pizzas: Pizza[]) {
    pizzas.forEach(el => {
      this.pizzasService.updatePizzaState(el).subscribe(response => {
          console.log(response);
          this.updateOrderState();
        }
      )
    })
  }

  updateOrderState() {
    console.log(this.order);
    // @ts-ignore
    this.orderService.updateOrderState(this.order?.orderId).subscribe(r => {
      console.log(r);
      if(r === true) { // @ts-ignore
        this.order.orderStateAsString = OrderState[this.order.orderState];
      }
    });
  }
}
