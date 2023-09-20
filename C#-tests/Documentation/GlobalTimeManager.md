# GlobalTimeManager Documentation

**Namespace**: `GlobalTimeManagment`

## Overview

`GlobalTimeManager` manages the global time for an application, ensuring high accuracy and precision. It uses internal ticks to track time and offers mechanisms to schedule and manage tasks.

---

## Properties

There are no publicly accessible properties for this class.

---

## Methods

### Public Methods

1. **GlobalTimeManager** : Constructor
   - Initializes the global time manager.
   - Automatically starts the global ticker.

2. **StartGlobalTicker** : `void`
   - Initializes and starts the global ticker thread.

3. **StopGlobalTicker** : `void`
   - Stops the global ticker thread.

4. **AddToCycleExecutionList** : `void`
   - Parameters: `Action action, int intervalMs`
   - Adds a function to the execution list, which is called every `intervalMs` milliseconds.

5. **AddToExecutionQueue** (Sync version) : `void`
   - Parameters: `List<Action> actionsList`
   - Adds a list of actions to the execution queue for immediate execution.

6. **AddToExecutionQueue** (Async version) : `Task`
   - Parameters: `List<Action> actionsList, int delayMs`
   - Asynchronously adds a list of actions to the execution queue after waiting for `delayMs` milliseconds.

7. **GetTimeLine** : `(long tickOrdinalNumber, string invokedMethodName)[]`
   - Returns a sorted array of tuples containing all time stamps.

8. **Debug** : `void`
   - Parameters: `bool doWriteToConsole = false`
   - Outputs diagnostic information, either to console or a file.

### Private Methods

- **RunTicker**: Controls the main ticker loop.
- **StopTicker**: Stops the ticker loop.
- **ExecuteEveryTick**: Invokes the tasks scheduled for each tick.
- **BusyWaitUntilNextTick**: Waits for the next tick, ensuring precision.
- **ExecuteCycleFunctions**: Executes tasks that are scheduled to run in a cycle.
- **ExecuteNextFunctionInQueue**: Executes the next function in the queue.
- **RecordTimeStamp** (Overloaded): Records the time stamp of function invocations or custom text.
- **Debug**: Outputs diagnostic information to a file or console.

---

## Fields

### Private Fields

- `_gtmTickStepMs`: Main ticker step in milliseconds.
- `_gtmMinimumTickStepMs`: Minimum permissible step of the ticker in milliseconds.
- `_gtmTicksPassed`: Counter for custom ticks passed since the start of the global ticker.
- `_doRunTicker`: Flag to start/stop the Global Ticker.
- `_stopWatchFrequencyPerMs`: Stopwatch frequency per millisecond.
- `_spinWait`: Number of spin wait iterations for precision control.
- `_stopWatch`: Inbuilt C# stopwatch for accurate time measurements.
- `_tickPermissibleErrorPercent`: Permissible error percentage for tick intervals.
- `_executionQueue`: Queue for event-dependent functions.
- `_executeInCycleList`: List of functions that are called at regular intervals.
- `_globalTicker`: Thread that manages the entire Global Ticker.
- `_timeStamps`: Thread-safe structure for tracking significant time stamps.
- `_gtmKeyWords`: Dictionary of keywords used within the Global Time Manager.

---

## Constructors and Destructors

1. **Constructor**: Sets the windows time resolution to its minimum value and starts the global ticker.
2. **Destructor**: Resets the windows time resolution to its original value.

---

## Remarks

- The class manages tasks and their execution timing with high precision, making it useful in scenarios demanding accurate time management.
- The "windows time resolution" is modified upon the instantiation and termination of this class, potentially influencing other time-related operations on the system.
- The global ticker thread runs with a priority set to `AboveNormal` to ensure timely execution.
- The class provides a mechanism to track time stamps of significant events, useful for debugging and performance analysis.

- The main components of the class are two queues and a "ticker."
  The "ticker" checks every millisecond to see if there are any tasks to execute.
  If there are, it invokes these functions. It checks both queues:

- The first queue is for tasks that are known from the start of the program
  and execute at a constant frequency.
  Examples include port scanning to check for disconnected cables,
  rendering the GUI at 60fps, and similar tasks.

- The second queue is for dynamically generated commands,
  such as sending packets to the robot, rendering scenes in virtual reality
  glasses at a specific time, logging keypresses, or playing sound at a designated moment.

---