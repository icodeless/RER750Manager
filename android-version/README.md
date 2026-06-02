# RER750 Manager (Android Kotlin / Compose Version)

This is a native Android application built with Kotlin and Jetpack Compose to manage and interact with RER75x readers.

## Features

- **Jetpack Compose UI**: Modern, declarative UI components.
- **Search Readers**: Broadcasts UDP packets to discover RER75x readers on the local network using Kotlin coroutines.
- **Data Stream**: Runs a local TCP ServerSocket in the background to listen for tag streams and reader events.
- **Control**: Connects to a specific reader IP via TCP to send Open Door or LED/Buzzer commands.

## Running the project

You can open the `/android-version` directory in Android Studio. Wait for Gradle to sync, and then build/run the project on an emulator or a physical device.

_Note: For the UDP broadcast and TCP listening features to work correctly, ensure your Android device is on the same local network as the RFID readers._
