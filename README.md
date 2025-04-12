# Zebra RFID Reader GUI

A Windows Forms application for interfacing with Zebra RFID readers, providing real-time tag reading and monitoring capabilities.

## Features

- Real-time RFID tag reading and tracking
- Live counter for total tags and read rates
- Easy configuration of reader IP address
- Connection status monitoring
- API endpoints for external integration

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

1. Launch the application
2. The app will automatically attempt to connect to the configured RFID reader
3. Once connected, it will start reading tags automatically
4. Use the Settings button to configure the reader IP address
5. Monitor tag counts and read rates in real-time

## Configuration

Default RFID Reader IP: 192.168.2.5
Port: 5084

[Your chosen license]
