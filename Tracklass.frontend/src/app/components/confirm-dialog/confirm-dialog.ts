import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';

@Component({
  selector: 'app-confirm-dialog',
  standalone: true,
  imports: [],
  templateUrl: './confirm-dialog.html',
  styleUrl: './confirm-dialog.css'
})
export class ConfirmDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<ConfirmDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: boolean
  ) {}

  //*OnClick emitira un boolean, al ser verdadero (true) se ira a eliminar
  onNoClick(){
    this.dialogRef.close(false);
  }
  onConfirm(){
    this.dialogRef.close(true);
  }
}
