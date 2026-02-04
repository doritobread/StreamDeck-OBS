# StreamDeck Web App for OBS

A web-based stream controller that works like an Elgato Stream Deck. Control OBS from any device with a browser - phone, tablet, or computer.

## Features

- Dark theme optimized for streaming setups
- Responsive button grid (works on phones, tablets, desktops)
- Dynamic scene switching
- Start/stop streaming and recording
- Audio source mute toggles
- Real-time status updates

## Requirements

- Windows 10/11
- OBS Studio 28+ (has WebSocket built-in)
- Both devices on the same network (for phone/tablet access)

## OBS Setup

1. Open **OBS Studio**
2. Go to **Tools** > **WebSocket Server Settings**
3. Check **Enable WebSocket server**
4. Note the **Server Port** (default: 4455)
5. Optional: Enable authentication and set a password
6. Click **Apply**

## App Configuration

Edit `appsettings.json` before running:

```json
{
  "ObsSettings": {
    "Host": "localhost",
    "Port": 4455,
    "Password": ""
  }
}
```

- **Host**: Use `localhost` if OBS is on the same machine, or the IP address of the OBS machine
- **Port**: Match the port in OBS WebSocket settings (default 4455)
- **Password**: Leave empty if no authentication, otherwise enter your OBS WebSocket password

## Running the App

### From Published Executable
1. Run `StreamAp.exe`
2. Open browser to `http://localhost:5111/Home/Dashboard`
3. Click **Connect** to connect to OBS

### From Source Code
```bash
cd StreamAp
dotnet run
```

## Accessing from Phone/Tablet

1. Find your computer's IP address:
   - Open Command Prompt
   - Run `ipconfig`
   - Look for "IPv4 Address" (e.g., 192.168.1.86)

2. On your phone/tablet, open browser and go to:
   ```
   http://YOUR_COMPUTER_IP:5111/Home/Dashboard
   ```

3. If it doesn't connect, allow the app through Windows Firewall:
   - Open PowerShell as Administrator
   - Run: `New-NetFirewallRule -DisplayName "StreamDeck Web App" -Direction Inbound -Protocol TCP -LocalPort 5111 -Action Allow`

## Firewall Rules

You may need to allow these ports through Windows Firewall:

| Port | Purpose |
|------|---------|
| 5111 | Web app (for accessing from other devices) |
| 4455 | OBS WebSocket (if OBS is on a different machine) |

## Troubleshooting

**Can't connect to OBS:**
- Verify OBS is running and WebSocket is enabled
- Check the Host/Port/Password in `appsettings.json`
- Make sure OBS WebSocket port (4455) isn't blocked by firewall

**Can't access from phone:**
- Ensure phone is on the same WiFi network
- Check that port 5111 is allowed through firewall
- Verify you're using the correct IP address

**Scenes/Audio not showing:**
- Click the Connect button first
- Check browser console (F12) for errors

## License

MIT - Feel free to modify and share.
