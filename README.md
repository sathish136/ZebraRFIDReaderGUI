# Zebra RFID Reader GUI (Taskbar Version)

A Windows Forms application for interfacing with Zebra RFID readers, featuring taskbar integration and API endpoints.

## Features

- Real-time RFID tag reading and tracking
- Taskbar integration for minimal interface
- Live counter for total tags and read rates
- Easy configuration of reader IP address
- Connection status monitoring
- REST API endpoints for external integration
- System tray notifications

## Requirements

- Windows OS
- .NET Framework 4.7.2
- Zebra RFID FXSeries Host .NET SDK
- 64-bit system

## Dependencies

- Symbol.RFID3.Host (Zebra RFID SDK)
- Microsoft.AspNetCore 2.2.0
- Microsoft.AspNetCore.Cors 2.2.0
- Microsoft.AspNetCore.Mvc 2.2.0
- Swashbuckle.AspNetCore 6.5.0

## Setup

1. Install the Zebra RFID FXSeries Host .NET SDK
2. Ensure the RFIDAPI32PC.dll is available in the system
3. Build and run the application
4. Configure the RFID reader IP address in the settings

## Usage

1. Launch the application (minimizes to system tray)
2. The app will automatically attempt to connect to the configured RFID reader
3. Once connected, it will start reading tags automatically
4. Right-click the tray icon to access settings and controls
5. Use the API endpoints for programmatic access

## API Endpoints

- GET /api/tags - Retrieve all currently detected tags
- GET /api/status - Get reader connection status
- Additional endpoints for configuration and control

## Configuration

Default RFID Reader IP: 192.168.2.5
Port: 5084

## Version History

- v2.0.0 - Added taskbar integration and REST API
- v1.0.0 - Initial release with basic GUI

## License

[Your chosen license]
