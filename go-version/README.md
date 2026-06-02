# RER750 Web Manager (Go Version)

This is a lightweight Go implementation of the RER750 Manager. It provides a simple backend HTTP server that handles device discovery, event listening, and control, paired with a clean HTML/JS frontend.

## How to run

1. Start the backend:
```bash
cd backend
go run main.go
```

2. Open the frontend:
Open `frontend/index.html` in your web browser.

## Features
- **Search Reader**: Broadcasts UDP packets to discover RER75x readers on the local network.
- **Tag Stream**: Listens on TCP port 2168 for incoming tag reads and events.
- **Control**: Allows opening the door relay and setting the LED/Buzzer state on the target reader.
