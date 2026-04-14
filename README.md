# 🎓 Dyadic - Project Approval System (PAS)

**PUSL2020 - Software Development Tools and Practices**

## 🗒️ Overview 

> A system that matches student research projects with academic supervisors using a **blind-match** workflow 🔍 - supervisors browse proposals without seeing student identities, preserving technical merit over unconscious bias ⚖️

![ASP.NET Core 8](https://img.shields.io/badge/ASP.NET%20Core-v8+-5C2D91?style=for-the-badge&labelColor=512BD4&logo=dotnet&logoColor=white)
![Entity Framework Core 8](https://img.shields.io/badge/Entity%20Framework%20Core-v8+-512BD4?style=for-the-badge&labelColor=6B46C1&logo=dotnet&logoColor=white)
![SQL Server 2022](https://img.shields.io/badge/MS%20SQL%20Server-v2022+-990000?style=for-the-badge&labelColor=CC2927&logo=microsoft-sql-server&logoColor=white)
![ASP.NET Identity](https://img.shields.io/badge/ASP.NET%20Identity-Auth-5C2D91?style=for-the-badge&labelColor=512BD4&logo=dotnet&logoColor=white)
![xUnit](https://img.shields.io/badge/xUnit-Testing-5C2D91?style=for-the-badge&labelColor=6B46C1&logo=dotnet&logoColor=white)
![Moq](https://img.shields.io/badge/Moq-Mocking-8A2BE2?style=for-the-badge&labelColor=9370DB)
![Docker](https://img.shields.io/badge/Docker-Containerized-0db7ed?style=for-the-badge&labelColor=2496ED&logo=docker&logoColor=white)
![Docker Compose](https://img.shields.io/badge/Docker%20Compose-Orchestration-0db7ed?style=for-the-badge&labelColor=2496ED&logo=docker&logoColor=white)

## 🚀 What It Does?

Students submit research proposals categorized by area (🤖 AI, 🌐 Web Dev, 🔐 Cybersecurity, etc.). Supervisors browse these proposals **anonymously** 👀, filtered by their areas of expertise. 

Once a supervisor expresses interest 🤝 and confirms a match, both parties' identities are revealed 🔓, unlocking direct collaboration.

The system is named after the **dyad** 👥 — a two-person unit — referencing the supervisor-student pair it produces.

## ✨ Key Features

- 🔍 **Blind Review Workflow** — supervisors see project content (title, abstract, tech stack, research area) without
  student names or IDs
- 🔓 **The Reveal** — identity exchange triggered only on mutual interest
- 👤 **Role-based Access** — Student / Supervisor / Module Leader / System Admin, strictly scoped
- 🎯 **Expertise-filtered Browsing** — supervisors see only projects in their declared areas
- 🛠️ **Admin Oversight** — Module Leaders can reassign, intervene, or audit matches

## 🏗️ Architecture

Clean-architecture layering, dependency direction: 
`Web → Infrastructure → Application → Domain`

### 📁 Structure

src/
├── Dyadic.Domain/          🧩 Entities, enums, domain interfaces
├── Dyadic.Application/     🧠 Services, business rules (blind-match logic lives here)
├── Dyadic.Infrastructure/  🏗️ DbContext, EF migrations, repositories
└── Dyadic.Web/             🌐 ASP.NET Core Razor Pages — UI + controllers

tests/
├── Dyadic.UnitTests/        🧪 Unit tests — mocked dependencies
└── Dyadic.IntegrationTests/ 🔗Integration tests — real DB + HTTP pipeline

## ⚡ Getting Started

```bash
# 📥 Git setup
git clone https://github.com/NSBM-SE-Projects/dyadic.git
cd dyadic

# 🛠️ Dev setup
make setup

# ▶️ Run the application
make run
```

📘 Full setup instructions, troubleshooting, and contribution workflow -> see CONTRIBUTING.md.

## 🧪 Testing

```bash
# ▶️ Run all tests
make test      

# 📊 Tests with coverage                                 
dotnet test --collect:"XPlat Code Coverage"
```

## 👥 Team

┌──────────┬────────────────────────────┬───────────────────────────────────────┐
│ Member   │            Role            │           GitHub                      │
├──────────┼────────────────────────────┼───────────────────────────────────────┤
│ Dwain    │ Team Lead                  │ https://github.com/dwainXDL           │
├──────────┼────────────────────────────┼───────────────────────────────────────┤
│ Thamindu │ TBA                        │ https://github.com/PWTMihisara        │
├──────────┼────────────────────────────┼───────────────────────────────────────┤
│ Ashen    │ TBA                        │https://github.com/drnykteresteinwayne │
├──────────┼────────────────────────────┼───────────────────────────────────────┤
│ Yameesha │ TBA                        │https://github.com/Yameeshaa           │
├──────────┼────────────────────────────┼───────────────────────────────────────┤
│ Thiranya │ TBA                        │https://github.com/thiranya123         │
├──────────┼────────────────────────────┼───────────────────────────────────────┤
│ Nisith   │ TBA                        │                                       │
├──────────┼────────────────────────────┼───────────────────────────────────────┤
│ Sewwandi │ TBA                        │https://github.com/kmss-sew            │
├──────────┼────────────────────────────┼───────────────────────────────────────┤
│ Isira    │ TBA                        │https://github.com/imanthaisira-beep   │
└──────────┴────────────────────────────┴───────────────────────────────────────┘

## 🎓 Academic context

⚠️ Use of this repository outside academic evaluation of this coursework requires permission from the team.

## 📫 Any Questions?

Feel free to contact [@dwainXDL](https://github.com/dwainXDL)