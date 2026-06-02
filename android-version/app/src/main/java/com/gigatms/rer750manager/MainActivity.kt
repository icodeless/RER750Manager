package com.gigatms.rer750manager

import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.compose.foundation.border
import androidx.compose.foundation.clickable
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.lazy.LazyColumn
import androidx.compose.foundation.lazy.items
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.unit.dp
import kotlinx.coroutines.Dispatchers
import kotlinx.coroutines.launch
import kotlinx.coroutines.withContext
import java.net.DatagramPacket
import java.net.DatagramSocket
import java.net.InetAddress
import java.net.ServerSocket
import java.net.Socket
import java.text.SimpleDateFormat
import java.util.*

class MainActivity : ComponentActivity() {
    private var tcpServer: ServerSocket? = null
    private var isListening = false

    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        setContent {
            MaterialTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    RER750ManagerApp()
                }
            }
        }
    }

    @Composable
    fun RER750ManagerApp() {
        var devices by remember { mutableStateOf(listOf<Device>()) }
        var events by remember { mutableStateOf(listOf<Event>()) }
        var selectedIp by remember { mutableStateOf("") }
        var selectedLedAction by remember { mutableStateOf<Byte>(1) }
        val coroutineScope = rememberCoroutineScope()

        Column(modifier = Modifier.padding(16.dp)) {
            Text("RER750 Manager", style = MaterialTheme.typography.headlineMedium, fontWeight = FontWeight.Bold)
            Spacer(modifier = Modifier.height(16.dp))

            // Device Search Section
            Text("Device Search", style = MaterialTheme.typography.titleMedium)
            Button(onClick = {
                coroutineScope.launch {
                    val foundDevices = searchDevices()
                    devices = foundDevices
                }
            }) {
                Text("\uD83D\uDD0D Search Readers")
            }
            LazyColumn(modifier = Modifier.height(150.dp).fillMaxWidth().border(1.dp, Color.Gray).padding(8.dp)) {
                items(devices) { device ->
                    Text(
                        text = "${device.ip} - ${device.name}",
                        modifier = Modifier.fillMaxWidth().clickable { selectedIp = device.ip }.padding(4.dp),
                        color = if (selectedIp == device.ip) MaterialTheme.colorScheme.primary else Color.Black
                    )
                }
            }

            Spacer(modifier = Modifier.height(16.dp))

            // Data Stream Section
            Text("Data Stream", style = MaterialTheme.typography.titleMedium)
            Row {
                Button(onClick = {
                    coroutineScope.launch {
                        startListening { event ->
                            events = events + event
                        }
                    }
                }) { Text("▶ Start") }
                Spacer(modifier = Modifier.width(8.dp))
                Button(onClick = { stopListening() }) { Text("⏹ Stop") }
                Spacer(modifier = Modifier.width(8.dp))
                Button(onClick = { events = emptyList() }) { Text("\uD83D\uDDD1 Clear") }
            }
            LazyColumn(modifier = Modifier.height(150.dp).fillMaxWidth().border(1.dp, Color.Gray).padding(8.dp)) {
                items(events) { event ->
                    Text(text = "${event.timestamp}: ${event.data} from ${event.ip}", style = MaterialTheme.typography.bodySmall)
                }
            }

            Spacer(modifier = Modifier.height(16.dp))

            // Control Section
            Text("Control", style = MaterialTheme.typography.titleMedium)
            OutlinedTextField(
                value = selectedIp,
                onValueChange = { selectedIp = it },
                label = { Text("Selected IP") },
                modifier = Modifier.fillMaxWidth()
            )
            Spacer(modifier = Modifier.height(8.dp))
            Row {
                RadioButton(selected = selectedLedAction == 0.toByte(), onClick = { selectedLedAction = 0 })
                Text("Off")
                Spacer(Modifier.width(8.dp))
                RadioButton(selected = selectedLedAction == 1.toByte(), onClick = { selectedLedAction = 1 })
                Text("Green")
                Spacer(Modifier.width(8.dp))
                RadioButton(selected = selectedLedAction == 3.toByte(), onClick = { selectedLedAction = 3 })
                Text("Red")
            }
            Row {
                Button(onClick = {
                    if (selectedIp.isNotEmpty()) {
                        coroutineScope.launch { openDoor(selectedIp) }
                    }
                }) { Text("\uD83D\uDEAA Open Door") }
                Spacer(modifier = Modifier.width(8.dp))
                Button(onClick = {
                    if (selectedIp.isNotEmpty()) {
                        coroutineScope.launch { setLedBuzzer(selectedIp, selectedLedAction) }
                    }
                }) { Text("\uD83D\uDCA1 Set LED/Buzzer") }
            }
        }
    }

    private suspend fun searchDevices(): List<Device> = withContext(Dispatchers.IO) {
        val foundDevices = mutableListOf<Device>()
        try {
            val socket = DatagramSocket()
            socket.broadcast = true
            val cmd = byteArrayOf(0x01, 0x02, 0x01, 0x01, 0x00, 0x00, 0x0D)
            val addr = InetAddress.getByName("255.255.255.255")
            val packet = DatagramPacket(cmd, cmd.size, addr, 2168)
            socket.send(packet)

            socket.soTimeout = 2000
            val buffer = ByteArray(1024)
            while (true) {
                try {
                    val response = DatagramPacket(buffer, buffer.size)
                    socket.receive(response)
                    foundDevices.add(Device(response.address.hostAddress ?: "", "Mock-MAC", "v1.0", "RER75x Device"))
                } catch (e: Exception) {
                    break // Timeout reached
                }
            }
            socket.close()
        } catch (e: Exception) {
            e.printStackTrace()
        }
        foundDevices.distinctBy { it.ip }
    }

    private suspend fun startListening(onEvent: (Event) -> Unit) = withContext(Dispatchers.IO) {
        if (isListening) return@withContext
        isListening = true
        try {
            tcpServer = ServerSocket(2168)
            while (isListening) {
                val client = tcpServer?.accept()
                client?.let {
                    launch { handleClient(it, onEvent) }
                }
            }
        } catch (e: Exception) {
            e.printStackTrace()
        } finally {
            isListening = false
        }
    }

    private fun stopListening() {
        isListening = false
        try {
            tcpServer?.close()
            tcpServer = null
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    private suspend fun handleClient(client: Socket, onEvent: (Event) -> Unit) = withContext(Dispatchers.IO) {
        try {
            val input = client.getInputStream()
            val buffer = ByteArray(1024)
            while (isListening) {
                val bytesRead = input.read(buffer)
                if (bytesRead == -1) break

                val hexData = buffer.copyOfRange(0, bytesRead).joinToString("") { "%02x".format(it) }
                val time = SimpleDateFormat("HH:mm:ss", Locale.getDefault()).format(Date())

                withContext(Dispatchers.Main) {
                    onEvent(Event(client.inetAddress.hostAddress ?: "", time, hexData, "RER75x Device"))
                }
            }
        } catch (e: Exception) {
            e.printStackTrace()
        } finally {
            client.close()
        }
    }

    private suspend fun openDoor(ip: String) = withContext(Dispatchers.IO) {
        try {
            val socket = Socket(ip, 2168)
            socket.soTimeout = 2000
            val cmd = byteArrayOf(0x01, 0x02, 0x14, 0x01, 0x00, 0x05, 0x0D)
            socket.getOutputStream().write(cmd)
            socket.close()
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }

    private suspend fun setLedBuzzer(ip: String, action: Byte) = withContext(Dispatchers.IO) {
        try {
            val socket = Socket(ip, 2168)
            socket.soTimeout = 2000
            val cmd = byteArrayOf(0x01, 0x02, 0x15, 0x01, 0x00, action, 0x0D)
            socket.getOutputStream().write(cmd)
            socket.close()
        } catch (e: Exception) {
            e.printStackTrace()
        }
    }
}

data class Device(val ip: String, val mac: String, val version: String, val name: String)
data class Event(val ip: String, val timestamp: String, val data: String, val name: String)
