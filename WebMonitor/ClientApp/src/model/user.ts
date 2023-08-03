import { AllowedFeatures } from "./allowed-features";

export interface User {
  username: string;
  displayName: string;
  isAdmin: boolean;
  allowedFeatures: AllowedFeatures;
}
