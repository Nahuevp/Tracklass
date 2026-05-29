import { Component, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected title = 'estudiantes-frontend';

  ngOnInit() {
    // Despierta el backend de la app de Microservicios para reducir el cold start
    // Fire and forget, no nos importa si falla o el resultado, solo queremos que levante.
    fetch('https://ecommerce-microservices-ow4d.onrender.com/')
      .catch(err => console.log('Ping a microservicios enviado (despertando backend)'));
  }
}
