import { AfterViewInit, Component, Input } from '@angular/core';
import { User } from "../../../model/user";
import { MatCheckbox } from "@angular/material/checkbox";
import { UserService } from "../../../services/user.service";
import { showErrorSnackbar, showOkSnackbar } from "../../../helpers/snackbar-helpers";
import { MatSnackBar } from "@angular/material/snack-bar";
import { AllowedFeatureData, getAllowedFeaturesList } from '../allowed-features-card/allowed-features-card.component';

@Component({
  selector: 'app-features-changer',
  templateUrl: './features-changer.component.html',
  styleUrls: ['./features-changer.component.css']
})
export class FeaturesChangerComponent implements AfterViewInit {
  @Input({ required: true }) user!: User;

  featuresList: AllowedFeatureData[] = [];

  constructor(
    private userService: UserService,
    private snackBar: MatSnackBar
  ) {
  }

  ngAfterViewInit(): void {
    this.refreshFeaturesList();
  }

  refreshFeaturesList() {
    const featuresList = getAllowedFeaturesList(this.user.allowedFeatures);

    if (this.featuresList.length === 0) {
      this.featuresList = featuresList;
      return;
    }

    for (let i = 0; i < this.featuresList.length; i++) {
      this.featuresList[i].allowed = featuresList[i].allowed;
    }
  }

  async changeFeatures(e: Event, feature: AllowedFeatureData) {
    const target = e.target as MatCheckbox | null;

    e.preventDefault();
    if (!target) {
      return;
    }

    const features = this.user.allowedFeatures;
    const featureName = feature.featureName;
    // If the feature name is wrong, return
    if (features[featureName] === undefined) {
      return;
    }

    try {
      // Change the feature
      features[featureName] = !features[featureName];
      await this.userService.changeAllowedFeatures({ username: this.user.username, allowedFeatures: features });
      this.refreshFeaturesList();
      target.checked = features[featureName];
      showOkSnackbar(this.snackBar, `Successfully ${features[featureName] ? 'enabled' : 'disabled'} the '${feature.name}' feature`);
    } catch (e) {
      // Revert the change in case of an error
      features[featureName] = !features[featureName];
      showErrorSnackbar(this.snackBar, `Failed to change the features: ${e}`);
    }
  }
}
