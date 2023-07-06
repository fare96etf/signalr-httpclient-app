import { HttpClient } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  private hubConnectionBuilder!: HubConnection;
  banksMessage: string = "";
  creditCardsMessage: string = "";
  banksSize: number = 0;
  creditCardsSize: number = 0;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.hubConnectionBuilder = new HubConnectionBuilder()
      .withUrl('http://localhost:5145/hub')
      .build();
    
    this.hubConnectionBuilder
      .start()
      .then(() => console.log('Connection started.......!'))
      .catch(err => console.log('Error while connect with server'));

    this.hubConnectionBuilder.on('InformAboutUpdatedBanks', (message: string) => {
      this.banksMessage = message;
    });

    this.hubConnectionBuilder.on('InformAboutUpdatedCreditCards', (message: string) => {
      this.creditCardsMessage = message;
    });
  }

  updateBanks() {
    this.http.get<any>(`http://localhost:5145/banks?size=${this.banksSize}`).subscribe(
      data => console.log(data)
    );
  }

  updateCreditCards() {
    this.http.get<any>(`http://localhost:5145/credit-cards?size=${this.creditCardsSize}`).subscribe(
      data => console.log(data)
    );
  }
}
