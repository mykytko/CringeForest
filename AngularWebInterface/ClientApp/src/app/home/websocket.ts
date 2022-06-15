export class Map {

  private ws: WebSocket;
  private height: number;
  private width: number;
  private matrix: number[];

  initialize(): any {
    this.ws = new WebSocket("127.0.0.1:5002");
    var data: any;
    this.ws.onmessage = function (event) {
      data = event.data;
      alert(event.data);
    }
    return data;
  }
}
