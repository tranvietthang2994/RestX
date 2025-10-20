"use strict";

var connection = new signalR.HubConnectionBuilder()
    .withUrl("/signalrServer")
  .build();

connection.on("ReceiveMessage", function () {
  loadData();
});

connection
  .start()
  .then(function () {
      console.log("Signalr is ready!");
  })
  .catch(function (err) {
    return console.error(err.toString());
  });

function loadData() {
  // Check if the product table body exists on the page
  console.log("Successfully receive signal from signalr!")
}
