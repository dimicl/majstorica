import { Coordinates } from '../types/coordinates.type';

export interface MapMarkerData {
  position: Coordinates;
  title?: string;
  color?: string;
}
