import { Component, input } from '@angular/core';
import { MatProgressSpinner} from '@angular/material/progress-spinner';

@Component({
  selector: 'app-spinner',
  imports: [MatProgressSpinner],
  templateUrl: './spinner.html',
  styleUrl: './spinner.css'
})
export class Spinner {
//*Nueva forma de trabajar con inputs
//*Inputs as signals
mensaje = input.required<string>();
}