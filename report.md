# Task Manager Clone - Project Report

This report provides a detailed technical breakdown of the Task Manager Clone application, explaining its architecture, logic flow, and core components. Use this as a guide to understand how the application was built from scratch.

## 1. Technology Stack

The application is built using modern Microsoft technologies and specialized libraries for hardware access:

-   **Framework:** .NET 8.0 (Windows)
-   **UI Framework:** WPF (Windows Presentation Foundation)
-   **Language:** C#
-   **Hardware Monitoring:** `LibreHardwareMonitorLib` (for temperatures and detailed GPU/CPU metrics)
-   **System Metrics:** `System.Diagnostics.PerformanceCounter` (for fallback CPU/RAM usage)
-   **Charting:** `LiveChartsCore.SkiaSharpView.WPF` (for real-time performance graphs)
-   **Dependency Injection:** `Microsoft.Extensions.DependencyInjection`

## 2. Architecture: MVVM Pattern

The project follows the **Model-View-ViewModel (MVVM)** design pattern to ensure a clean separation between UI and logic:

-   **Models:** Data structures like `ProcessInfo` representing a single system process.
-   **Views:** XAML files (`MainWindow.xaml`, `Views/*.xaml`) defining the visual layout and styles.
-   **ViewModels:** Classes (`MainViewModel`, `ProcessListViewModel`) that handle the logic, process data from services, and provide properties for the View to bind to.
-   **Services:** The `HardwareMonitorService` handles the "heavy lifting" of talking to the OS and hardware.

## 3. Application Logic Flow

### Step 1: Entry Point (Default WPF Startup)
WPF applications typically hide their `Main` method. By default, the build process generates a startup entry point that initializes the `App` class defined in `App.xaml`.

- **`App.xaml` (Line 1-6):** The `x:Class="TaskManagerClone.App"` attribute links the XAML definition to the C# partial class. The `Startup="Application_Startup"` attribute tells the OS to execute our specific logic as soon as the app engine is ready.

### Step 2: App Startup & Dependency Injection (`App.xaml.cs`)
The `Application_Startup` method is where the "brains" of the application are wired together.

```csharp
private void Application_Startup(object sender, StartupEventArgs e)
{
    // LINE 15: Create a ServiceCollection (a list of all "parts" the app needs).
    var serviceCollection = new ServiceCollection(); 

    // LINE 16: Call ConfigureServices to fill that list (see below).
    ConfigureServices(serviceCollection); 

    // LINE 18: "Build" the provider, creating a container that can give us these parts on demand.
    _serviceProvider = serviceCollection.BuildServiceProvider(); 

    // LINE 20: Get the MainWindow from the container. 
    // This automatically injects the MainViewModel and HardwareMonitorService into it.
    var mainWindow = _serviceProvider.GetRequiredService<MainWindow>(); 

    // LINE 21: Finally, display the window to the user.
    mainWindow.Show(); 
}
```

**`ConfigureServices` (Lines 24-34):**
- **Line 27:** `services.AddSingleton<IHardwareMonitorService, HardwareMonitorService>();` registers the hardware monitor. `AddSingleton` ensures that every part of the app uses the *same* sensor instance.
- **Line 30:** Registers the `MainViewModel`, which manages the UI data.
- **Line 33:** Registers the `MainWindow` itself so it can be managed by the DI container.

### Step 3: The UI Shell & Navigation (`MainWindow.xaml`)
The `MainWindow` acts as a host for different pages (Performance, Processes).

1.  **Sidebar (Lines 1-50):** Contains buttons hooked to `Commands` in the `MainViewModel`.
2.  **Breadcrumbs (Lines 52-60):** A `TextBlock` bound to `{Binding CurrentViewName}` so the user knows where they are.
3.  **Content Display (Line 108):** A `ContentControl` bound to `{Binding CurrentViewModel}`. This is a "window" that switches its contents based on which ViewModel is active.
4.  **View Mapping (`App.xaml` / `MainWindow.xaml` DataTemplates):** When `CurrentViewModel` is a `ProcessListViewModel`, WPF automatically searches for the `ProcessListView` and Renders it.

### Step 4: Real-time Monitoring Loop (`HardwareMonitorService.cs`)
This background service runs independently of the UI to ensure metrics are updated even if the user isn't looking at them.

**The Constructor (Lines 31-53):**
- **Lines 33-40:** Configures the `Computer` object from `LibreHardwareMonitor`. It enables sensors for CPU, GPU, Memory, and Network.
- **Line 42:** `_computer.Open();` creates the system handles and starts reading from hardware drivers.
- **Line 45-46:** Initializes `PerformanceCounter` objects. These are standard Windows tools used here as a reliable fallback for CPU and RAM usage.
- **Line 52:** `_updateTimer = new ...` creates a background thread that calls `UpdateMetrics` on a loop without freezing the UI.

**The Refresh Loop (Lines 65-117):**
- **Line 69-90:** Iterates through every hardware component (CPU, GPU, etc.) and calls `.Update()`.
- **Line 101:** `MetricsUpdated?.Invoke(...)` broadcasts the new data to the entire application. Any ViewModel listening to this event will immediately update its charts or text labels.

## 4. Core Component Breakdown

### HardwareMonitorService (`Services/`)
-   **Privilege Requirement:** To read hardware sensors directly, the app requires **Administrator Privileges**. This is configured in `app.manifest`.
-   **LibreHardwareMonitor:** Used for GPU temperature and load, as these are not available via standard Windows Performance Counters.
-   **PerformanceCounters:** Used as a fast, low-overhead way to get global CPU usage and available RAM.

### ProcessListViewModel (`ViewModels/`)
-   **Retrieval:** Uses `System.Diagnostics.Process.GetProcesses()` to snapshot the running system.
-   **Optimization:** Instead of recreating the list every 3 seconds, it "syncs" the list—adding new processes and removing ones that died—to keep the UI smooth.
-   **System Calls:** Uses `p.Kill()` to terminate processes and `Process.Start` to open file locations in Explorer.

### MainViewModel (`ViewModels/`)
-   **Graph Integration:** Manages the `ISeries` and `Axis` properties for the LiveCharts performance graphs.
-   **Data Buffering:** Maintains a small history (buffer) of metrics so the lines on the charts move continuously as new data arrives.

## 5. Security & Manifest

The `app.manifest` is critical. It contains:
```xml
<requestedExecutionLevel level="requireAdministrator" uiAccess="false" />
```
Without this, the hardware monitoring service would fail to access the low-level drivers needed for temperature monitoring.

## 6. How to Build/Run

1.  **Admin Mode:** Always run your IDE (Visual Studio) or Terminal as Administrator.
2.  **Dependencies:** Ensure `LibreHardwareMonitorLib` and `LiveChartsCore` are restored via NuGet.
3.  **Target:** Build for `win-x64` to ensure compatibility with the specialized hardware libraries.
