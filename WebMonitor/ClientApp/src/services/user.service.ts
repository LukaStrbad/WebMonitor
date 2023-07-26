import { Inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { catchError, firstValueFrom, Subject, throwError } from "rxjs";
import { User } from "../model/user";
import { LoginResponse } from "../model/responses/login-response";
import { MatSnackBar } from "@angular/material/snack-bar";

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl: string;
  errorEmitter = new Subject<string>();
  authorized = signal(false);

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.apiUrl = baseUrl + "User/";
    this.authorized.set(this.user !== null);
  }

  get user(): User | null {
    const user = localStorage.getItem("user");
    if (user === null) {
      return null;
    }
    return JSON.parse(user);
  }

  get token(): string | null {
    return localStorage.getItem("token");
  }

  private onSuccess(loginResponse: LoginResponse) {
    localStorage.setItem("user", JSON.stringify(loginResponse.user));
    localStorage.setItem("token", loginResponse.token);
    this.authorized.set(true);
  }

  logout() {
    localStorage.removeItem("user");
    localStorage.removeItem("token");
    this.authorized.set(false);
  }

  /**
   * Register a new user
   * @param user Data of the new user
   */
  async register(user: { username: string, displayName: string, password: string }) {
    const response = await firstValueFrom(this.http.post<LoginResponse>(this.apiUrl + "register", user)
      .pipe(catchError((err: HttpErrorResponse) => {
        if (err.error) {
          this.errorEmitter.next(err.error);
        }

        return throwError(() => err.error);
      }))
    );

    this.onSuccess(response);

    return response;
  }

  /**
   * Login a user
   * @param user Data of the user
   */
  async login(user: { username: string, password: string }) {
    const response = await firstValueFrom(this.http.post<LoginResponse>(this.apiUrl + "login", user)
      .pipe(catchError((err: HttpErrorResponse) => {
        if (err.error) {
          this.errorEmitter.next(err.error);
        }

        return throwError(() => err.error);
      }))
    );

    this.onSuccess(response);

    return response;
  }

  /**
   * Get information about the current user
   */
  async me() {
    return await firstValueFrom(this.http.get<User>(this.apiUrl + "me")
      .pipe(catchError((err: HttpErrorResponse) => {
        if (err.error) {
          this.errorEmitter.next(err.error);
        }

        return throwError(() => err.error);
      }))
    );
  }

  /**
   * Promote a user to admin (admin only)
   * @param username Username of the user to promote
   */
  async promoteToAdmin(username: string) {
    return await firstValueFrom(this.http.post<string>(this.apiUrl + "promoteToAdmin", { username })
      .pipe(catchError((err: HttpErrorResponse) => {
        if (err.error) {
          this.errorEmitter.next(err.error);
        }

        return throwError(() => err.error);
      }))
    );
  }

  /**
   * Lists all users (admin only)
   */
  async listUsers() {
    return await firstValueFrom(this.http.get<User[]>(this.apiUrl + "listUsers")
      .pipe(catchError((err: HttpErrorResponse) => {
        if (err.error) {
          this.errorEmitter.next(err.error);
        }

        return throwError(() => err.error);
      }))
    );
  }

  /**
   * Delete current user
   */
  async deleteSelf() {
    const response = await firstValueFrom(this.http.delete<string>(this.apiUrl + "deleteSelf")
      .pipe(catchError((err: HttpErrorResponse) => {
        if (err.error) {
          this.errorEmitter.next(err.error);
        }

        return throwError(() => err.error);
      }))
    );

    this.logout();
    return response;
  }

  /**
   * Delete a user (admin only)
   * @param username Username of the user to delete
   */
  async deleteUser(username: string) {
    return await firstValueFrom(this.http.delete<string>(this.apiUrl + "deleteUser/" + username)
      .pipe(catchError((err: HttpErrorResponse) => {
        if (err.error) {
          this.errorEmitter.next(err.error);
        }

        return throwError(() => err.error);
      }))
    );
  }
}
