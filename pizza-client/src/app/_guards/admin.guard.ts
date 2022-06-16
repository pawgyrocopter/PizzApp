import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, UrlTree } from '@angular/router';
import {map, Observable} from 'rxjs';
import {AccountService} from "../_services/account.service";
import {User} from "../_models/user";

@Injectable({
  providedIn: 'root'
})
export class AdminGuard implements CanActivate {
  constructor(private accountService: AccountService) {
  }

  canActivate(): Observable<boolean> {
    // @ts-ignore
    return this.accountService.currentUser$.pipe(
      map((user: User) => {
          return user.roles.includes('Admin');
        }
      ));
  }

}
