package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net"
	"net/http"
	"sync"
	"time"
)

type Device struct {
	IP      string `json:"ip"`
	MAC     string `json:"mac"`
	Version string `json:"version"`
	Name    string `json:"name"`
}

type Event struct {
	IP        string `json:"ip"`
	Timestamp string `json:"timestamp"`
	Data      string `json:"data"`
	Name      string `json:"name"`
}

var (
	devices   = make(map[string]Device)
	events    = make([]Event, 0)
	mutex     sync.Mutex
	isListening bool
	tcpListener net.Listener
)

func main() {
	http.HandleFunc("/api/devices", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Content-Type", "application/json")
		mutex.Lock()
		defer mutex.Unlock()
		deviceList := []Device{}
		for _, v := range devices {
			deviceList = append(deviceList, v)
		}
		json.NewEncoder(w).Encode(deviceList)
	})

	http.HandleFunc("/api/events", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		w.Header().Set("Content-Type", "application/json")
		mutex.Lock()
		defer mutex.Unlock()
		json.NewEncoder(w).Encode(events)
	})

	http.HandleFunc("/api/search", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		if r.Method == "POST" {
			go searchDevices()
			w.WriteHeader(http.StatusOK)
		}
	})

	http.HandleFunc("/api/listen/start", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		if r.Method == "POST" {
			go startListening()
			w.WriteHeader(http.StatusOK)
		}
	})

	http.HandleFunc("/api/listen/stop", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		if r.Method == "POST" {
			stopListening()
			w.WriteHeader(http.StatusOK)
		}
	})

	http.HandleFunc("/api/opendoor", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		if r.Method == "POST" {
			ip := r.URL.Query().Get("ip")
			go openDoor(ip)
			w.WriteHeader(http.StatusOK)
		}
	})

	http.HandleFunc("/api/ledbuzzer", func(w http.ResponseWriter, r *http.Request) {
		w.Header().Set("Access-Control-Allow-Origin", "*")
		if r.Method == "POST" {
			ip := r.URL.Query().Get("ip")
			action := r.URL.Query().Get("action")
			actionByte := byte(0)
			if action != "" {
				fmt.Sscanf(action, "%d", &actionByte)
			}
			go setLedBuzzer(ip, actionByte)
			w.WriteHeader(http.StatusOK)
		}
	})

	fmt.Println("Backend server running on :8080")
	log.Fatal(http.ListenAndServe(":8080", nil))
}

func searchDevices() {
	// Send UDP broadcast to port 2168
	addr, err := net.ResolveUDPAddr("udp", "255.255.255.255:2168")
	if err != nil {
		fmt.Println("ResolveUDPAddr err:", err)
		return
	}
	conn, err := net.ListenUDP("udp", nil)
	if err != nil {
		fmt.Println("ListenUDP err:", err)
		return
	}
	defer conn.Close()

	// GNET Command to get device status
	cmd := []byte{0x01, 0x02, 0x01, 0x01, 0x00, 0x00, 0x0D}
	_, err = conn.WriteToUDP(cmd, addr)
	if err != nil {
		fmt.Println("WriteToUDP err:", err)
		return
	}

	conn.SetReadDeadline(time.Now().Add(2 * time.Second))
	buffer := make([]byte, 1024)
	for {
		n, remoteAddr, err := conn.ReadFromUDP(buffer)
		if err != nil {
			break
		}

		if n > 0 {
			mutex.Lock()
			devices[remoteAddr.IP.String()] = Device{
				IP:      remoteAddr.IP.String(),
				MAC:     "Mock-MAC",
				Version: "v1.0",
				Name:    "RER75x Mock Name",
			}
			mutex.Unlock()
		}
	}
}

func startListening() {
	mutex.Lock()
	if isListening {
		mutex.Unlock()
		return
	}
	isListening = true
	mutex.Unlock()

	var err error
	tcpListener, err = net.Listen("tcp", ":2168")
	if err != nil {
		fmt.Println("TCP Listen err:", err)
		return
	}

	for {
		conn, err := tcpListener.Accept()
		if err != nil {
			break
		}
		go handleConnection(conn)
	}
}

func stopListening() {
	mutex.Lock()
	isListening = false
	mutex.Unlock()
	if tcpListener != nil {
		tcpListener.Close()
	}
}

func handleConnection(conn net.Conn) {
	defer conn.Close()
	buffer := make([]byte, 1024)
	for {
		n, err := conn.Read(buffer)
		if err != nil {
			break
		}
		if n > 0 {
			mutex.Lock()
			events = append(events, Event{
				IP:        conn.RemoteAddr().String(),
				Timestamp: time.Now().Format(time.RFC3339),
				Data:      fmt.Sprintf("%x", buffer[:n]),
				Name:      "RER75x Device",
			})
			mutex.Unlock()
		}
	}
}

func openDoor(ip string) {
	conn, err := net.DialTimeout("tcp", ip+":2168", 2*time.Second)
	if err != nil {
		fmt.Println("Dial err:", err)
		return
	}
	defer conn.Close()

	cmd := []byte{0x01, 0x02, 0x14, 0x01, 0x00, 0x05, 0x0D}
	conn.Write(cmd)
}

func setLedBuzzer(ip string, action byte) {
	conn, err := net.DialTimeout("tcp", ip+":2168", 2*time.Second)
	if err != nil {
		fmt.Println("Dial err:", err)
		return
	}
	defer conn.Close()

	cmd := []byte{0x01, 0x02, 0x15, 0x01, 0x00, action, 0x0D}
	conn.Write(cmd)
}
