# .NET 10 Blazor Web App (Interactive Server) Boilerplate
### MudBlazor 9.11.0 + Tailwind CSS 4 Generic Lab Exam Template

A clean, domain-agnostic, and production-ready boilerplate template specifically designed for timed lab exams and technical assessments. Built with **.NET 10 (C# 13)**, **MudBlazor 9.11.0**, and **Tailwind CSS 4**.

---

## 🚀 Quick Start Instructions

All dependencies (`MudBlazor 9.11.0`, `@tailwindcss/cli`) have been pre-installed and restored.

### 1. Run the Application
In your terminal, run:
```bash
dotnet run --launch-profile http
```
The application will start at: `http://localhost:5046` (or `https://localhost:7103` on https profile).

### 2. Build the Project
```bash
dotnet build
```
*(The Tailwind CSS 4 compiler automatically runs during `dotnet build` to compile any updated utility classes into `wwwroot/css/tailwind.css`).*

### 3. (Optional) Watch Tailwind CSS Changes in Real Time
```bash
npm run watch:css
```

---

## 📁 Project Architecture & Layout

```text
midterm/
├── Components/
│   ├── App.razor                      # Root HTML shell, fonts, MudBlazor CSS/JS, and Tailwind CSS
│   ├── Routes.razor                   # Blazor routing configuration
│   ├── _Imports.razor                 # Global namespace imports (MudBlazor, Models, Services)
│   ├── ItemDialog.razor               # Reusable MudDialog form (TextField, NumericField, Select, DatePicker)
│   ├── ItemTable.razor                # MudTable with full-text search, filters, sorting & action buttons
│   ├── MetricCard.razor               # Summary metric cards with MudBlazor icons & Tailwind styling
│   ├── Layout/
│   │   ├── MainLayout.razor           # MudThemeProvider (dark/light mode), MudProviders, AppBar, Drawer
│   │   └── NavMenu.razor              # Responsive MudNavMenu navigation links
│   └── Pages/
│       ├── Dashboard.razor            # Landing page with metric cards, category & status breakdowns
│       ├── Management.razor           # Dedicated CRUD management page embedding ItemTable & dialogs
│       ├── Error.razor                # Unhandled error display component
│       └── NotFound.razor             # 404 page handler
├── Models/
│   └── GenericItem.cs                 # Domain-agnostic entity (Id, Title, Description, Category, Value, Status, Date)
├── Services/
│   └── ItemStateService.cs            # In-memory CRUD store with reactive event notifications & seed data
├── Styles/
│   └── tailwind.css                   # Tailwind CSS 4 source entrypoint with @import "tailwindcss"
├── wwwroot/
│   └── css/
│       └── tailwind.css               # Compiled Tailwind CSS 4 bundle
├── package.json                       # Tailwind CLI script configuration
├── Program.cs                         # MudBlazor services & Singleton ItemStateService DI registration
└── midterm.csproj                     # .NET 10 project definition with MudBlazor 9.11.0 & Tailwind build target
```

---

## 🛠️ Key Architectural Highlights for Lab Exams

1. **Reactive In-Memory State (`ItemStateService`)**:
   - Subscribed components automatically re-render when items are added, modified, or removed via the `OnChange` event.
   - Thread-safe in-memory operations with deep copy cloning (`item.Clone()`) to prevent dirty reads.
   - Built-in `ResetToDefaults()` method to quickly restore sample dummy data at any time during an exam.

2. **Domain-Agnostic Entity (`GenericItem`)**:
   - Easily mapped to any scenario prompt (e.g., tasks, assets, inventory, tickets, projects, orders) by simply renaming the labels or extending properties.

3. **MudBlazor + Tailwind CSS Synergy**:
   - MudBlazor provides robust data table controls, dialogs, snackbar alerts, date pickers, and popovers.
   - Tailwind CSS 4 provides modern glassmorphism, responsive cards, micro-interactions, and dark mode classes.
