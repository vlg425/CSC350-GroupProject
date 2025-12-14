# Subduing the Seas ⚓
> **A 2D Maritime Exploration & Spatial Inventory Simulator**

**Subduing the Seas** is a Unity-based simulation game developed as a final group project. It combines open-world physics-based sailing with a complex "spatial inventory" puzzle system. Unlike traditional RPGs with static lists, this project challenges players to physically manage their cargo hold, rotating and fitting fish of various geometric shapes (Tetris-style) to maximize profits.

---

## 🎮 Key Features
* **🚢 Physics-Based Navigation:** Omni-directional ship movement using force vectors (`Vector2`) and torque for realistic handling.
* **🎣 Skill-Based Acquisition:** A reflex-based Quick Time Event (QTE) minigame requiring precise timing to catch resources.
* **🧩 Spatial Inventory System:** A grid management system supporting non-uniform item shapes (L-shapes, T-shapes) and 90-degree rotation.
* **💰 Dynamic Economy:** A persistent shop interface where players liquidate assets to accumulate gold.

---

## 🕹️ Controls

| Context | Action | Input |
| :--- | :--- | :--- |
| **Sailing** | Accelerate / Reverse | **W / S** |
| | Rotate Hull | **A / D** |
| | Interact (Dock/Fish) | **Space** |
| **Inventory** | Drag Item | **Left Mouse Click** |
| | Rotate Item | **Right Mouse Click** |
| | Sell Item | **F** (While hovering at Dock) |
| **General** | Pause / Quit | **Esc** |

---

## 🛠️ Technical Implementation
This project demonstrates the integration of several software design patterns and data structures:

### Architecture
* **State Machine Pattern:** We utilized a Singleton `GameManager` to strictly manage game states (`Sailing`, `Fishing`, `Docked`). This decoupling prevents "Input Bleed" (e.g., moving the ship while inside a menu).
* **Flyweight Pattern:** Used via **ScriptableObjects** to store shared data (Item Name, Icon, Price, Shape Matrix) separately from the logic, optimizing memory usage.

### Data Structures
* **2D Arrays (`InventorySlot[,]`):** Used for the backing grid logic to allow **O(1)** constant-time access for slot validation.
* **Vectors (`Vector2`):** Essential for calculating physics forces and mapping screen-space mouse coordinates to grid-space indices.
* **RaycastHit2D:** Utilized for precise detection of UI elements and world interactions.
* **Lists:** Used for managing dynamic collections of active items in the scene.

---

## 👥 Team Members (MRCane272)
* **Victor ([VLG425](https://github.com/VLG425))**: UI Development, Inventory Algorithms, Shop Logic.
* **Jake Torres ([Jaket406](https://github.com/Jaket406))**: Minigame Logic, Win/Loss States.
* **Mehrab Arosh ([MehrabArosh](https://github.com/MehrabArosh))**: Ship Physics, Interaction Triggers.

---

## 📥 Installation & Setup
1.  **Clone the repository:**
    ```bash
    git clone [https://github.com/vlg425/CSC350-GroupProject.git](https://github.com/vlg425/CSC350-GroupProject.git)
    ```
2.  **Open in Unity:**
    * Launch **Unity Hub**.
    * Click **Add** and select the cloned project folder.
    * *Recommended Version:* Unity 2022.3 LTS (or your specific version).
3.  **Run the Game:**
    * Navigate to `Assets/Scenes`.
    * Open `TitleScreen.unity`.
    * Press **Play**.

---

## 🔮 Future Improvements
* **Serialization:** Implement JSON saving to persist the 2D array state between sessions.
* **Expanded Map:** Additional biomes and Points of Interest.
* **Visual Polish:** Particle effects for water wake and fishing success states.
**************************************************************************************************************************************************************



Weekly progress: 20 points = report (15 points) + Presentation/QA (5 points)
Team Name: [Enter your team name] 
________________________________________
1. Team Members & Task Assignment (1pts) 
Name	Assigned Tasks (Initial)	Changes/Updates to Task (if any)
Jake	Fishing mini game	[Any changes during the week]
Victor	Inventory system	[Any changes during the week]
Mehrab	Ship Movement and interations	[Any changes during the week]
Note: Please update if tasks shift during development.
________________________________________
2. GitHub Activity Summary (6pts)
Activity Type	Description and Link	Who Contributed	Date
Commit / Push 1	Init repo:https://github.com/vlg425/CSC350-GroupProject.git	Jake	Nov13
Commit / Push …	Created basic inventory system	victor	nov22
Commit / Push N	Added basic movement to ship	mehrab	nov26
Merge / Pull Request 1..	Updated snapping mechanics ans scripts	victor	[nov27]
Merge / Pull Request N	Made movement function and edited scripts	mehrab	nov28
Code Review / Comments 1	[Optional: any peer reviews]	[Student name]	[Date]
Note: Please include links if possible to commits or pull requests.
________________________________________
3. Progress Summary (3pts)
Present the weekly deliverables according to the following milestones
Course Project: Ship Game System Expansion
●	What did you accomplish this week?

 Developed Grid inventory system. Allows for unique shapes to fit into grid without going out of bounds or overlapping with other shapes.


●	What challenges did you face?

 Finding a method to keep track off all shapes as well as where they are allowed to be placed


●	How did you resolve them (or what is your plan to resolve them)?

A 2d array of bools to know what slots in inventory are available. Created Scriptable Object to help with creation of new shapes and sizes.
________________________________________
3. Team Collaboration (3pts)
●	How did your team collaborate this week? # of meetings and meeting agenda? 
 (e.g., meetings, pair programming, online discussions, etc.)
We met a few times over the week to discuss our progress. As well as check in to make sure that all is going well. Want to have our separate pieces working so that we can integrate them next time in class
________________________________________
5. Plan for Next Week (2pts)
●	Tasks to complete next week:

Integrate separate task to work together and have a semi functioning game loop.

●	Any anticipated challenges or support needed:

Hopefully our scripts do not conflict with each other and mesh without too much trouble.


________________________________________
6. Additional Notes (Optional)
Any other updates, requests
________________________________________
Submission Instructions:
●	Please complete and submit this report before each class presentation 
○	Nov 20 (both weekly report and presentation)
○	Nov 29 (weekly report only on Github)
○	Dec 4 (both weekly report and presentation)
○	Dec 13th (both weekly report and final presentation)

●	Ensure that your GitHub repository stays up to date and links to relevant commits.

●	Keep task assignments updated if roles or tasks shift over time.


