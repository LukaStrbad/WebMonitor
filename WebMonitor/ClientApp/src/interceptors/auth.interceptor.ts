import { Injectable } from '@angular/core';
import { HttpRequest, HttpHandler, HttpEvent, HttpInterceptor, HttpErrorResponse } from '@angular/common/http';
import { catchError, Observable, throwError } from 'rxjs';
import { UserService } from "../services/user.service";

@Injectable()
export class AuthInterceptor implements HttpInterceptor {

  constructor(private userService: UserService) {
  }

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const token = this.userService.token;

    if (token === null)
      return next.handle(request);

    const cloned = request.clone({
      headers: request.headers.set("Authorization", `Bearer ${token}`)
    })

    return next
      .handle(cloned)
      .pipe(catchError(err => {
          if (err instanceof HttpErrorResponse) {
            if (err.status === 401) {
              this.userService.logout();
            }
          }

          return throwError(() => err);
        })
      );
  }
}
