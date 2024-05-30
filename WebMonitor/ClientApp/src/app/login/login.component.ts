import { Component, OnDestroy } from '@angular/core';
import { UserService } from "../../services/user.service";
import { ActivatedRoute, Router } from "@angular/router";
import { Subscription } from "rxjs";
import { MatSnackBar, MatSnackBarModule } from "@angular/material/snack-bar";
import { showOkSnackbar } from "../../helpers/snackbar-helpers";
import { LoginResponse } from 'src/model/responses/login-response';
import { SysInfoService } from 'src/services/sys-info.service';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatInputModule,
    MatIconModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule
  ]
})
export class LoginComponent implements OnDestroy {
  isRegister: boolean | null = null;
  showFirstUserMessage = false;
  submitted = false;
  error?: string;
  hidePassword = true;
  hideConfirmPassword = true;
  private subscription: Subscription;

  username = "";
  loginPassword = "";
  password = "";
  confirmPassword = "";
  displayName = "";

  constructor(
    public userService: UserService,
    private router: Router,
    private snackBar: MatSnackBar,
    private sysInfo: SysInfoService
  ) {
    userService.someUserExists().then(async exists => {
      // If there are no users, redirect to register
      if (!exists && !this.router.url.includes("register")) {
        await this.router.navigate(["/register"]);
      }
      this.showFirstUserMessage = !exists;
    }).finally(() => {
      // Set isRegister after the user check to avoid briefly showing the wrong form
      this.isRegister = this.router.url.includes("register");
    })

    this.subscription = userService.errorEmitter.subscribe(err => {
      this.error = err;
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  /**
   * Switches between login and register
   */
  async switchLoginRegister() {
    if (this.isRegister) {
      await this.router.navigate(["/login"]);
    } else {
      await this.router.navigate(["/register"]);
    }
  }

  async login() {
    const response = await this.userService.login({
      username: this.username,
      password: this.loginPassword
    });

    await this.onLoginSuccess(response);
  }

  async register() {
    const response = await this.userService.register({
      username: this.username,
      password: this.password,
      displayName: this.displayName
    });

    this.onLoginSuccess(response);
  }

  async onLoginSuccess(response: LoginResponse) {
    this.sysInfo.startService();
    showOkSnackbar(this.snackBar, `Successfully logged in as ${response.user.displayName}`);
    await this.router.navigate(["/"]);
  }

  onSubmit() {
    this.submitted = true;
  }

  /**
   * Called when the user clicks the login/register button
   */
  async onLogin() {
    if (this.isRegister) {
      await this.register();
    } else {
      await this.login();
    }
  }

  /**
   * Called when there was an error and the user clicks the try again button
   */
  tryAgain() {
    this.username = "";
    this.loginPassword = "";
    this.password = "";
    this.confirmPassword = "";
    this.displayName = "";
    this.hidePassword = true;
    this.hideConfirmPassword = true;
    this.submitted = false;
    this.error = undefined;
  }
}
