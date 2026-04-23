# E-Service - Multi-Platform Windows Application

## 📌 Overview
E-Service is a multi-platform Windows application developed as part of a diploma thesis project.  
It provides a unified login system that grants access to three different roles:

- Student
- Professor
- Administrator

The system is designed with a focus on security, automation, and stability in an academic environment.

---

## ⚙️ Features

### 🔐 Security & Access Control
- Optimized IP-based access control with improved handling of edge cases in public/free networks (e.g., Vala-free, public WiFi)
- Device Fingerprinting based on hardware identity for improved user/device recognition
- Enhanced OS-level security hardening mechanisms
- Protection against unauthorized access and bypass attempts
- - Protection against brute-force attacks (IP blocking for 24 hours after 3 failed attempts)
- SQL Injection protection mechanisms
- Password and personal data encryption using SHA-256 + SALT
- VPN / Proxy / Tor detection (to prevent bypass attempts of IP blocking mechanisms)
- Log auditing system including:
  - LoginLogs
  - FailedLogins
  - PasswordChangeLogs
  - BlockedIPs
- Role-based access control:
  - Administrator
  - Professor
  - Student
- Google Authentication (restricted to AAB university domain only)
- Passwords and personal identification data are securely stored using SHA-256 + SALT, preventing rainbow table attacks, one of the most common techniques used to crack compromised databases

### 🤖 Automation System
- Academic workflow automation scripts
  - Automatic student progression (semester & academic year advancement based on completion status)
  - Automatic grade processing:
    - Temporary exam grades are transferred to final grades after a 1-week review period
    - Students have the option to reject grades before finalization

### 🖥️ Application Features
- Multi-role system (Student / Professor / Administrator)
- Unified login interface for all roles
- Notify Icon + Background Mode support
  - Minimize to system tray
  - Continue running in background
  - Restore with a single click


---

## 🧰 Tech Stack

- .NET Framework (Windows Application) 4.7.2 version framework compatibile with visual studio 19,22 and so on.
- SQL Server (SSMS) ssms 19 version
- Go (supporting modules / utilities)
- Newtonsoft.Json (API handling)
- Guna UI Framework (modern UI design)

---

## 📁 
```
/E-Service
 ├── illy -- Application
 ├── Gjenerimi/gjenerimi.go 
 ├── Scripts/Automation Scripts // scripts folder should be to navigated to C:\
 ├── Security Layer
 └── Database
```

---

## 🚀 Getting Started

### Prerequisites
- Windows OS
- .NET Framework installed
- SQL Server (SSMS)

### Run the project
```bash
git clone https://github.com/Yllix5/E-Service.git
```

Open the solution in Visual Studio and run the application.

---

## 📸 Screenshots


---

## ⚠️ Disclaimer
This project is developed for **educational purposes only** as part of a diploma thesis.  
It is published as open-source for learning and academic use.

---

## 📬 Feedback
Any feedback, suggestions, or contributions are welcome.

---

## 🙏 Thank You
