# ⏱️ Bullhorn Timesheet Automator

A C# console application that automates logging daily work hours into Bullhorn Staffing using [Microsoft Playwright](https://playwright.dev/dotnet/). Enter your time in, time out, and optional lunch break — the tool handles the rest.

---

## ✨ Features

- 🔐 Secure credential loading via `.env` file
- 📅 Select your work week and starting day
- 🔁 Loop through multiple days in a single session
- 🍽️ Optional lunch break entry
- 💾 Auto-saves each day's timesheet entry
- ✅ Prompts to submit the full timesheet when done
- 🛡️ Input validation for all time and date fields

---

## 📋 Prerequisites

- [.NET 6+](https://dotnet.microsoft.com/en-us/download)
- [Microsoft Playwright](https://playwright.dev/dotnet/) (`Microsoft.Playwright`)
- [DotNetEnv](https://github.com/tonerdo/dotnet-env) (`DotNetEnv`)
- A Bullhorn Staffing account

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/your-username/bullhorn-timesheet-automator.git
cd bullhorn-timesheet-automator
```

### 2. Install dependencies

```bash
dotnet add package Microsoft.Playwright
dotnet add package DotNetEnv
```

### 3. Install Playwright browsers

```bash
dotnet build
pwsh bin/Debug/net6.0/playwright.ps1 install
```

### 4. Configure your credentials

Create a `.env` file in the project root:

```env
BH_PASSWORD=your_password_here
```

> ⚠️ **Never commit your `.env` file.** Add it to `.gitignore` immediately (see below).

### 5. Add `.env` to `.gitignore`

```bash
echo ".env" >> .gitignore
```

### 6. Run the program

```bash
dotnet run
```

---

## 🖥️ Usage

When you run the program, you'll be guided through the following prompts:

| Prompt | Example Input | Notes |
|---|---|---|
| Current week | `3/10/2025` | Format: `M/D/YYYY` |
| Starting day | `monday` | Lowercase only |
| Time in | `08:30` | 24-hour or 12-hour format |
| Time out | `17:00` | 24-hour or 12-hour format |
| Took a lunch? | `yes` or `no` | |
| Lunch out | `12:00` | Only shown if lunch = yes |
| Lunch in | `13:00` | Only shown if lunch = yes |

After each day is saved, you'll be asked if you want to continue to the next day. Once all days are entered, you'll be prompted to submit the timesheet for approval.

---

## 📁 Project Structure

```
bullhorn-timesheet-automator/
├── Program.cs        # Main application logic
├── .env              # Your credentials (never commit this!)
├── .gitignore        # Should include .env
└── README.md
```

---

## ⚙️ Configuration

To run the browser in the background (headless mode), update this line in `Program.cs`:

```csharp
// Change this:
Headless = false

// To this:
Headless = true
```

---

## 🛡️ Security Notes

- Your password is stored only in the local `.env` file and is never hardcoded in source
- Always ensure `.env` is listed in your `.gitignore` before making any commits
- Do not share your `.env` file or commit it to version control

---

## 📝 License

This project is for personal use. Use responsibly and in accordance with your organization's policies.
