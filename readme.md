# BattleShip WPF Project

BattleShip WPF is a two-player game implementation of the classic Battleship game. It was created to explore TCP networking concepts in a practical context and to take on the challenge of building an interactive WPF user interface that integrates seamlessly with the networked gameplay mechanics. This app is able to host a game or connect to a host.

![Intro](path/to/intro-placeholder.png)

## Project Structure
- **BattleShipServer**: This project contains the server-side logic for the Battleship game.
- **BattleShip**: This project contains the client-side logic and the user interface for the Battleship game.

## Getting Started

### Prerequisites
- .NET Framework 4.7.2
- Visual Studio 2019 or later

### Building the Project
1. Open the solution file `BattleShip.sln` in Visual Studio.
2. Restore the NuGet packages.
3. Build the solution.
4. Set `BattleShip` as the startup project.
5. Run the project.

## Features

### TCP Networking
- A server manages connections and game logic.
- Clients communicate with the server to send game actions (like placing ships and firing at opponents).
- Reliable message delivery and error handling.

### WPF Interface
- Interactive game grids for both your field and the opponent’s field.
- Custom controls to handle events like firing at enemy ships.
- Visual feedback for hit/miss results and ship placements.
- Responsive design to accommodate different screen sizes.

### Core Game Logic
- Players place ships according to the rules of Battleship.
- Turns are taken to fire at enemy coordinates until one player wins.
- Real-time updates and notifications for game events.

### Chat Functionality
- Players can send chat messages to each other during the game.
- Server can broadcast messages to all connected clients.

## Challenges Addressed

### Networking
- Establishing and maintaining reliable TCP connections between the server and multiple clients.
- Implementing message exchange protocols for game actions and chat messages.

### Concurrency
- Managing multiple client connections simultaneously on the server side.
- Ensuring thread safety and efficient resource utilization.

### UI Development
- Building a responsive and intuitive WPF interface that provides real-time feedback.
- Handling user interactions and updating the UI based on game events.

## How It Works

### Server
- **Initialization**: The server listens for incoming TCP connections on a specified port. It waits until two clients are connected before starting the game.
- **Game Management**: Once two clients are connected, the server initializes the game state, including setting up the game board and notifying clients that the game is starting.
- **Processing Actions**: The server receives game actions from clients (e.g., placing ships, firing shots) and processes these actions according to the game rules. It updates the game state and sends the results back to the clients.
- **State Updates**: The server continuously manages and updates the game state, ensuring that both clients are kept in sync with the latest game information.
- **Communication Protocol**: The server uses a custom message protocol to communicate with clients. Messages include game actions, results, and chat messages.
- **Error Handling**: The server includes mechanisms for handling errors and ensuring reliable communication with clients.
- **Chat Management**: The server handles chat messages sent by clients and broadcasts them to all connected clients.

### Client Interaction
- **Connection**: The app starts with the local IP displayed. The user can click "Créer" to host the game or "Rejoindre" with an IP to connect to a host. Then the app waits for the server to inform that two players are ready.
- **Ship Placement**: Players place their ships on their grid according to the rules of Battleship. Ships cannot overlap or extend beyond the grid.
- **Taking Turns**: Players take turns to fire at opponent positions by clicking on the opponent’s grid. Each action is sent to the server for processing and validation.
- **Game Feedback**: The server processes the actions and sends back the results (hit/miss/sunk). The client updates the UI to reflect these results.
- **Chat**: Players can send chat messages to each other during the game. The server can also broadcast messages to all connected clients.

### Networking
- **Message Exchange**: The server and clients exchange messages using TCP sockets. Messages include game actions, results, and chat messages.
- **Error Handling**: The system includes mechanisms for reliable message delivery and error handling to ensure smooth gameplay.

### WPF
- **User Interface**: The UI is built using WPF for an interactive and visually appealing experience. It includes interactive game grids, custom controls, and visual feedback for game events.
- **Responsive Design**: The design accommodates different screen sizes to ensure a consistent user experience.

## Screenshots

### Connection
![Connection](path/to/connection-placeholder.png)

### Ship Placement
![Ship Placement](path/to/ship-placement-placeholder.png)

### Ship Overlapping and Overriding Game Area During Placement
![Ship Overlapping](path/to/ship-overlapping-placeholder.png)

### Main Game
![Main Game](path/to/main-game-placeholder.png)

### Miss and Lose Feedback
![Miss and Lose](path/to/miss-lose-placeholder.png)

### Chats Between Players
![Player Chat](path/to/player-chat-placeholder.png)

### Chat Sent by the Server
![Server Chat](path/to/server-chat-placeholder.png)

## Future Enhancements

### Multiplayer Support
- Add support for more players or spectators to join the game.
- Implement a matchmaking system for online play.

### Visual Improvements
- Add animations or visual effects for hits, misses, and other game events.
- Enhance the overall visual appeal of the game interface.

### AI Opponent
- Create an AI opponent for single-player mode to allow users to play against the computer.

### Persistence
- Implement features to save game states or player statistics.
- Allow players to resume games from saved states.

### Additional Features
- Introduce new game modes or variations of the classic Battleship game.
- Add more customization options for players, such as different ship types or grid sizes.

## Contributing
Contributions are welcome! Please fork the repository and submit a pull request.