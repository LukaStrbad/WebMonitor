import { AfterViewInit, Component, ViewChild } from '@angular/core';
import { UserService } from "../../services/user.service";
import { User } from "../../model/user";
import { showErrorSnackbar, showOkSnackbar } from "../../helpers/snackbar-helpers";
import { MatSnackBar } from "@angular/material/snack-bar";
import { MatTableDataSource } from "@angular/material/table";
import { MatCheckbox } from "@angular/material/checkbox";
import { MatDialog } from "@angular/material/dialog";
import { ActionsDialogComponent, ActionsDialogData } from "../components/actions-dialog/actions-dialog.component";
import { MatPaginator } from "@angular/material/paginator";
import { MatSort } from "@angular/material/sort";
import { animate, state, style, transition, trigger } from "@angular/animations";
import { SysInfoService } from "../../services/sys-info.service";
import { SupportedFeatures } from "../../model/supported-features";

@Component({
  selector: 'app-admin',
  templateUrl: './users.component.html',
  styleUrls: ['./users.component.css'],
  animations: [
    trigger('detailExpand', [
      state('collapsed', style({
        height: '0px',
        minHeight: '0'
      })),
      state('expanded', style({
        height: '*'
      })),
      transition('expanded <=> collapsed', animate('225ms cubic-bezier(0.4, 0.0, 0.2, 1)'))
    ])
  ]
})
export class UsersComponent implements AfterViewInit {
  me?: User;
  dataSource = new MatTableDataSource(new Array<User>());
  displayedColumns: string[] = ['displayName', 'username', 'isAdmin', 'delete', 'expand'];
  expandedUser: User | null = null;
  supportedFeatures?: SupportedFeatures;

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  constructor(
    public userService: UserService,
    private snackBar: MatSnackBar,
    private dialog: MatDialog,
    private sysInfo: SysInfoService
  ) {
    sysInfo.getSupportedFeatures().then(f => this.supportedFeatures = f);
  }

  ngAfterViewInit(): void {
    this.userService.me().then(me => {
      this.me = me;
    });

    // If the user is not an admin, return
    if (!this.userService.user?.isAdmin) {
      return;
    }

    this.refreshUsers();
  }

  refreshUsers() {
    this.userService.listUsers().then(
      users => {
        this.dataSource.data = users;
        // Set the paginator/sort after the data has been set, otherwise it will
        // be undefined because the table hasn't been rendered yet
        this.dataSource.paginator = this.paginator;
        this.dataSource.sort = this.sort;
      },
      (err) => showErrorSnackbar(this.snackBar, `Failed to load users: ${err}`)
    );
  }

  deleteAccount(user: User) {
    let title = `Delete ${user.displayName}'s account?`;
    if (user.username === this.me?.username) {
      title = "Delete your account?";
    }

    this.dialog.open(ActionsDialogComponent, {
      data: <ActionsDialogData>{
        title,
        content: "Account deletion is permanent and cannot be undone.",
        negativeButton: ["Cancel", () => {
        }],
        positiveButton: ["Delete", () => {
          this.userService.deleteUser(user.username)
            .then(() => {
              showOkSnackbar(this.snackBar, `Successfully deleted ${user.displayName}'s account`);
              // Only refresh the users if the current user is an admin
              // This will also stop the refresh if the current user was deleted
              if (this.userService.user?.isAdmin) {
                this.refreshUsers();
              }
            },
              err => showErrorSnackbar(this.snackBar, `Failed to delete ${user.displayName}'s account: ${err}`)
            );
        }]
      }
    })
  }

  /**
   * Promotes a user to admin
   * @param e Event
   * @param user User to promote
   */
  promoteToAdmin(e: Event, user: User) {
    const target = e.target as MatCheckbox | null;

    // Prevents the checkbox from being checked on click
    e.preventDefault();
    // If the user is already an admin or the target is null, return
    if (user.isAdmin || !target) {
      return;
    }

    // Ask the user if they are sure they want to promote the user to admin
    this.dialog.open(ActionsDialogComponent, {
      data: <ActionsDialogData>{
        title: "Promote to admin?",
        content: `Are you sure you want to promote ${user.displayName} to admin?`,
        positiveButton: ["Promote", () => {
          this.userService.promoteToAdmin(user.username)
            .then(async () => {
              user.isAdmin = true;
              // Check the checkbox after the user has been promoted
              target.checked = true;
              // Update the allowed features to reflect the new admin status
              // user.allowedFeatures = await this.sysInfo.getSupportedFeatures();
              showOkSnackbar(this.snackBar, `Successfully promoted ${user.displayName} to admin`);
            },
              err => showErrorSnackbar(this.snackBar, `Failed to promote ${user.displayName} to admin: ${err}`)
            );
        }]
      }
    });
  }

  leaveAdminRole() {
    this.dialog.open(ActionsDialogComponent, {
      data: <ActionsDialogData>{
        title: "Leave admin role?",
        content: "Are you sure you want to leave the admin role?",
        positiveButton: ["Leave", () => {
          this.userService.leaveAdminRole()
            .then(() => {
              showOkSnackbar(this.snackBar, "Successfully left admin role");
              this.me!.isAdmin = false;
            }, err => showErrorSnackbar(this.snackBar, `Failed to leave admin role: ${err}`));
        }],
        negativeButton: ["Cancel", () => {
        }]
      }
    });
  }

  /**
   * Allowed features can only be changed if the user is not an admin
   * @param user User to change allowed features for
   */
  canChangeAllowedFeatures(user: User) {
    return !user.isAdmin;
  }
}
