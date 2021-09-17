import { Component, Inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
// import { Console } from 'console';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.css']
})
export class HomeComponent {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private BaseUrl: string) {
    }
  private simulationInitialized = false;
  simulationRunning = false;
  private debugWrite(str) // debug
  {
    document.getElementsByClassName("simulation-view")[0].innerHTML += str + "<br />";
  }
  startSimulation()
  {
    this.debugWrite("start simulation"); // debug
    if (!this.simulationInitialized)
    {
      this.http.post(this.BaseUrl + "api/posts", "InitializeSimulation", { responseType: "text" }).subscribe(resp => {
        this.debugWrite("POST response: " + resp);
        if (resp == "Simulation initialized")
        {
          this.simulationInitialized = true;
          this.simulationRunning = true;
        }
      });
    }
    else
    {
      this.http.post(this.BaseUrl + "api/posts", "ResumeSimulation", { responseType: "text" }).subscribe(resp => {
        this.debugWrite("POST response: " + resp);
        if (resp == "Simulation resumed")
        {
          this.simulationRunning = true;
        }
      });
    }
  }
  stopSimulation()
  {
    this.debugWrite("stop simulation"); // debug
    this.http.post(this.BaseUrl + "api/posts", "StopSimulation", { responseType: "text" }).subscribe(resp => {
      this.debugWrite("POST response: " + resp);
      if (resp == "Simulation stopped")
      {
        this.simulationRunning = false;
      }
    });
  }
  loadMap()
  {
    this.debugWrite("load map"); // debug
  }
  saveMap()
  {
    this.debugWrite("save map"); // debug
    this.http.post(this.BaseUrl + "api/posts", "SaveMap", { responseType: "text" }).subscribe(resp => {
      this.debugWrite("POST response: " + resp);
    });
  }
  changeInitialParameters()
  {
    this.debugWrite("change initial parameters"); // debug
  }
}
