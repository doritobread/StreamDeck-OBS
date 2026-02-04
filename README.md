# StreamDeck for OBS

Control your OBS stream from your phone, tablet, or another screen. Works like an Elgato Stream Deck but free.

---

## Download

1. Go to [Releases](https://github.com/doritobread/StreamDeck-OBS/releases)
2. Download the latest `.zip` file
3. Extract it to a folder (right-click > Extract All)

---

## Setup (One Time)

### Step 1: Set up OBS

1. Open **OBS Studio**
2. Click **Tools** in the menu bar
3. Click **WebSocket Server Settings**
4. Check the box **Enable WebSocket server**
5. Set a password if you want (optional but recommended)
6. Click **OK**

### Step 2: Configure the app

1. Open the folder where you extracted the app
2. Open **appsettings.json** with Notepad
3. Change these settings:

```
"Host": "localhost"        <-- Leave this if OBS is on the same computer
"Port": 4455               <-- Leave this unless you changed it in OBS
"Password": ""             <-- Put your OBS password here (between the quotes)
"TwitchChannel": ""        <-- Put your Twitch username here for chat
```

4. Save the file

---

## Running the App

1. Double-click **StreamAp.exe**
2. A window will pop up - this is normal, keep it open
3. Open your web browser and go to: **http://localhost:5111**
4. Click the **Connect** button

---

## Using from Your Phone

1. Make sure your phone is on the same WiFi as your computer
2. Find your computer's IP address:
   - Press **Windows key + R**
   - Type **cmd** and press Enter
   - Type **ipconfig** and press Enter
   - Look for **IPv4 Address** (looks like `192.168.1.XXX`)
3. On your phone's browser, go to: **http://YOUR_IP:5111**
   - Example: `http://192.168.1.86:5111`

### If it doesn't work from your phone:

Windows might be blocking it. Do this once:

1. Press **Windows key**, type **firewall**, click **Windows Defender Firewall**
2. Click **Allow an app or feature through Windows Defender Firewall**
3. Click **Change settings** (need admin)
4. Click **Allow another app**
5. Click **Browse** and find **StreamAp.exe** in your folder
6. Click **Add**, then check both **Private** and **Public** boxes
7. Click **OK**

---

## What the Buttons Do

| Button | What it does |
|--------|--------------|
| **Connect** | Connects to OBS (click this first) |
| **Stream** | Starts/stops your stream (turns red when live) |
| **Record** | Starts/stops recording (turns orange when recording) |
| **Scene buttons** | Switches between your OBS scenes |
| **Audio buttons** | Mutes/unmutes audio sources |

---

## Troubleshooting

**"Can't connect to OBS"**
- Make sure OBS is running
- Make sure WebSocket is enabled in OBS (Tools > WebSocket Server Settings)
- Check your password matches

**"Page won't load on phone"**
- Make sure the app is running on your computer
- Make sure your phone is on the same WiFi
- Check the IP address is correct
- Try the firewall fix above

**"No scenes showing up"**
- Click the Connect button first
- Make sure you're connected (green dot in top right)

---

## Questions?

Because I probably have the same question as well. Idk wtf I'm doing. But feel free to open an issue or msg me.
