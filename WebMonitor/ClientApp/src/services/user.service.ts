import { Inject, Injectable, signal } from '@angular/core';
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { catchError, firstValueFrom, Subject, throwError } from "rxjs";
import { User } from "../model/user";
import { LoginResponse } from "../model/responses/login-response";
import { AllowedFeatures } from "../model/allowed-features";

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl: string;
  errorEmitter = new Subject<string>();
  authorized = signal(false);
  onLogout = new Subject<void>();
  allowedFeaturesChanged = new Subject<AllowedFeatures>();

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

  private set user(user: User | null) {
    if (user === null) {
      localStorage.removeItem("user");
    } else {
      localStorage.setItem("user", JSON.stringify(user));
    }
  }

  async requireUser(): Promise<User> {
    if (this.user) {
      return this.user;
    }
    if (this.token) {
      this.user = await this.me();
      return this.user;
    }

    throw new Error("User is not logged in");
  }

  get token(): string | null {
    return localStorage.getItem("token");
  }

  private set token(token: string | null) {
    if (token === null) {
      localStorage.removeItem("token");
    } else {
      localStorage.setItem("token", token);
    }
  }

  private onSuccess(loginResponse: LoginResponse) {
    this.user = loginResponse.user;
    this.token = loginResponse.token;
    this.authorized.set(true);
  }

  async refreshUser() {
    const user = await this.me();

    let allEqual = true;
    for (const k in user.allowedFeatures) {
      if (!(k in user.allowedFeatures)) {
        continue;
      }
      const key = k as keyof AllowedFeatures;
      if (user.allowedFeatures[key] !== this.user?.allowedFeatures[key]) {
        allEqual = false;
        break;
      }
    }

    if (allEqual) {
      return;
    }

    this.user = user;
    this.allowedFeaturesChanged.next(user.allowedFeatures);
  }

  logout() {
    localStorage.removeItem("user");
    localStorage.removeItem("token");
    this.onLogout.next();
    this.authorized.set(false);
  }

  handleError(err: HttpErrorResponse) {
    if (err.error) {
      this.errorEmitter.next(err.error);
      return throwError(() => err.error);
    } else if (err.message) {
      this.errorEmitter.next(err.message);
    }

    return throwError(() => err.message);
  }

  /**
   * Checks if a user exists
   */
  async someUserExists() {
    return await firstValueFrom(this.http.get<boolean>(this.apiUrl + "someUserExists")
      .pipe(catchError(err => this.handleError(err)))
    );
  }

  /**
   * Register a new user
   * @param user Data of the new user
   */
  async register(user: { username: string, displayName: string, password: string }) {
    const response = await firstValueFrom(this.http.post<LoginResponse>(this.apiUrl + "register", user)
      .pipe(catchError(err => this.handleError(err)))
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
      .pipe(catchError(err => this.handleError(err)))
    );

    this.onSuccess(response);

    return response;
  }

  /**
   * Get information about the current user
   */
  async me() {
    return await firstValueFrom(this.http.get<User>(this.apiUrl + "me")
      .pipe(catchError(err => this.handleError(err)))
    );
  }

  /**
   * Promote a user to admin (admin only)
   * @param username Username of the user to promote
   */
  async promoteToAdmin(username: string) {
    return await firstValueFrom(this.http.post<string>(this.apiUrl + "promoteToAdmin", { username })
      .pipe(catchError(err => this.handleError(err)))
    );
  }

  /**
   * Demotes the current user from admin (admin only)
   */
  async leaveAdminRole() {
    return await firstValueFrom(this.http.post<string>(this.apiUrl + "leaveAdminRole", {})
      .pipe(catchError(err => this.handleError(err)))
    );
  }

  /**
   * Lists all users (admin only)
   */
  async listUsers() {
    return await firstValueFrom(this.http.get<User[]>(this.apiUrl + "listUsers")
      .pipe(catchError(err => this.handleError(err)))
    );
  }

  /**
   * Delete a user (admin only)
   * @param username Username of the user to delete
   */
  async deleteUser(username: string) {
    return await firstValueFrom(this.http.delete<string>(`${this.apiUrl}deleteUser?username=${username}`)
      .pipe(catchError(err => this.handleError(err)))
    );
  }

  async deleteSelf() {
    const response = await firstValueFrom(this.http.delete<string>(`${this.apiUrl}deleteSelf`)
      .pipe(catchError(err => this.handleError(err)))
    );

    this.logout();

    return response;
  }

  /**
   * Changes the allowed features of a user (admin only)
   * @param request Username and allowed features
   */
  async changeAllowedFeatures(request: { username: string, allowedFeatures: AllowedFeatures }) {
    return await firstValueFrom(this.http.post(this.apiUrl + "changeAllowedFeatures", request)
      .pipe(catchError(err => this.handleError(err)))
    );
  }
}
