# GitHub + Unity workflow guide for the Art Team

This document explains how our Art Team should work with **GitHub** and **Unity** for the project.

It is written for teammates who have **little or no GitHub experience**. We will still do a live overview call together, but this file should be your step-by-step reference afterward.

---

## Repository information

**Project repository:**  
https://github.com/DLuckYD/dragon-soup-game

**Main working branch for the team:**  
`development-branch`

---

## 1. Install GitHub Desktop

For our team, the easiest and safest option is **GitHub Desktop**.

Download it here:
https://desktop.github.com/download/

After installing it:
1. Open **GitHub Desktop**.
2. Sign in with your GitHub account.
3. If you do not have a GitHub account yet, create one first.

---

## 2. Send me your GitHub account details

Before I can give you access to the repository, please send me **one** of the following:
- your **GitHub username**, or
- the **email address connected to your GitHub account**.

I will use that to send you an invitation to the repository.

**Important:** you must **accept the invitation** before you can push changes.

---

## 3. Clone the repository to your computer

After you get access, you need to **clone** the repository.  
This means downloading the project from GitHub to your computer.

### Option A — from the GitHub website
1. Open the repository page in your browser.
2. Click **Code**.
3. Click **Open with GitHub Desktop**.
4. Choose where to save the project on your computer.
5. Click **Clone**.

### Option B — directly in GitHub Desktop
1. Open **GitHub Desktop**.
2. Click **File → Clone repository**.
3. Open the **URL** tab.
4. Paste this repository link:

```text
https://github.com/DLuckYD/dragon-soup-game
```

5. Choose a local folder on your computer.
6. Click **Clone**.

---

## 4. Open the project in Unity

After cloning the repository:
1. Open **Unity Hub**.
2. Click **Add project from disk** or **Open**.
3. Select the cloned project folder.
4. Open it with the correct Unity version used by the team.

The first import may take some time. That is normal.

---

## 5. Important rule: never work directly in `main` or `development-branch`

Our workflow is:
- `main` = stable branch
- `development-branch` = shared development branch
- **your own branch** = your personal working branch

### Rule
You should do your work only in **your own branch**, created from `development-branch`.

Do **not** work directly in:
- `main`
- `development-branch`

This protects the project and makes it easier to review changes.

---

## 6. Create your own branch from `development-branch`

### Step-by-step in GitHub Desktop
1. Open the repository in **GitHub Desktop**.
2. At the top, click **Current Branch**.
3. Switch to **`development-branch`** first.
4. Click **New Branch**.
5. Create your personal branch.
6. Click **Create Branch**.
7. Click **Publish branch** so it is uploaded to GitHub.

### Recommended branch name format
Use a clear name with your name in it, for example:

```text
art/anna
art/anna-ui-icons
art/kate
art/kate-main-menu
art/max-textures
```

Rules:
- use English letters
- no spaces
- keep it short and clear

**Important:** create your branch from **`development-branch`**, not from `main`.

---

## 7. Recommended daily workflow

Every time you work on the project, follow this order.

### Step 1 — open GitHub Desktop
Open the cloned repository.

### Step 2 — check your branch
At the top, in **Current Branch**, make sure you are on **your own branch**, not on `main` and not on `development-branch`.

### Step 3 — sync before starting work
Before starting a new task, it is good practice to sync your branch with the newest project state.

Basic safe version:
1. Make sure you have no unfinished uncommitted changes.
2. Switch to `development-branch`.
3. Pull the latest changes.
4. Switch back to your own branch.
5. Merge `development-branch` into your branch if needed.

If you are not sure how to do the merge step, ask first and do not guess.

### Step 4 — do your work in Unity
Examples:
- add art assets
- replace textures
- add icons
- update UI images
- add backgrounds
- update materials or sprites

### Step 5 — return to GitHub Desktop and review changed files
GitHub Desktop will show all changed files.

Before committing, check that:
- only the correct files changed
- you did not accidentally change unrelated files
- there are no unnecessary temporary files

### Step 6 — commit your changes
A **commit** is a saved checkpoint of your work.

In GitHub Desktop:
1. Write a short commit title.
2. Optionally add a longer description.
3. Click **Commit to _your-branch-name_**.

Good examples:

```text
Added new main menu background
Updated inventory icons
Replaced old forest textures
Added loading screen illustration
```

Bad examples:

```text
update
stuff
final
123
```

### Step 7 — push your changes
After the commit, click **Push origin**.

This uploads your changes from your computer to GitHub.

---

## 8. When your task is finished

When your task is ready:
1. Make sure everything is committed.
2. Click **Push origin**.
3. Tell me your branch is updated and ready.
4. Then we create or review a **Pull Request**.

The correct direction is:

```text
your-branch -> development-branch
```

Not:

```text
your-branch -> main
```

---

## 9. Unity-specific rules for the Art Team

This part is very important.

### 9.1 Keep Unity `.meta` files
Unity uses `.meta` files for assets and folders.

**Do not delete them.**  
If you add, move, rename, or remove assets, the related `.meta` files matter too.

If GitHub Desktop shows both:
- your asset file, and
- a `.meta` file

that is usually normal.

### 9.2 Do not commit generated Unity folders
Normally, our `.gitignore` should already exclude generated folders.  
You should generally **not** commit folders like these:

```text
Library/
Temp/
Obj/
Logs/
UserSettings/
```

If GitHub Desktop suddenly shows a lot of files from folders like these, stop and ask before committing.

### 9.3 Close Unity before pulling or switching branches when possible
Best practice:
- save your work
- close Unity
- pull / switch branch / merge
- reopen Unity

This reduces the chance of broken imports, locked files, or weird scene changes.

### 9.4 Avoid editing the same scene or prefab at the same time as someone else
Unity projects can create conflicts when multiple people change the same file.

Please coordinate first if you want to edit:
- the same scene
- the same prefab
- the same UI layout
- the same shared material or asset

### 9.5 Wait for Unity to finish importing before committing
If you add new assets:
- let Unity fully import them
- check that they appear correctly
- only then commit the changes

### 9.6 If Unity changed something by accident, do not panic
Sometimes Unity touches files you did not intend to modify.

Before committing, check the file list in GitHub Desktop.  
If a file changed by accident, it is often better to **discard that change** than to commit random edits.

### 9.7 Use consistent file names and folder structure
Please avoid names like:

```text
image_final_final2.png
newtexture_real_final.psd
background_test_last_v7.png
```

Better examples:

```text
mainmenu_background
inventory_icon_sword
forest_loading_screen
ui_button_play
```

---

## 10. Unity project settings to keep for version control

For Unity projects under version control, the important settings are usually:
- **Version Control Mode:** `Visible Meta Files`
- **Asset Serialization Mode:** `Force Text`

In Unity, these are found in:

```text
Edit → Project Settings → Editor
```

If the project is already configured correctly, do not randomly change these settings.

---

## 11. What to do if something goes wrong

### If you see merge conflicts
Stop and ask for help.

Do not randomly click through conflict windows if you do not understand what changed.

### If GitHub Desktop shows too many changed files
Possible reasons:
- Unity reimported assets
- you opened the wrong project folder
- generated files are being tracked by mistake
- someone changed shared files

Again: stop, screenshot it, and ask.

### If push fails because a file is too large
Tell me first.  
Large art files sometimes need a different solution.

### If you accidentally changed the wrong file
In GitHub Desktop, you can often discard the unwanted change before committing.

---

## 12. Very short version

If you only remember one thing, remember this:

```text
Install GitHub Desktop -> send me your GitHub username/email -> accept invite -> clone repository -> open the Unity project -> switch to development-branch -> create your own branch -> publish branch -> do your work -> commit -> push -> let me know when it is ready
```

---

## 13. Quick checklist

### One-time setup
- [ ] Install GitHub Desktop
- [ ] Create or sign in to a GitHub account
- [ ] Send me your GitHub username or GitHub email
- [ ] Accept the repository invitation
- [ ] Clone the repository
- [ ] Open the project in Unity Hub
- [ ] Switch to `development-branch`
- [ ] Create your own branch
- [ ] Publish your branch

### Every time you work
- [ ] Open the repository in GitHub Desktop
- [ ] Make sure you are on your own branch
- [ ] Sync if needed
- [ ] Do the task in Unity
- [ ] Review changed files
- [ ] Commit changes
- [ ] Push origin
- [ ] Tell me when the task is ready

---

## 14. Useful links

### GitHub Desktop
Download GitHub Desktop:  
https://desktop.github.com/download/

GitHub Desktop documentation:  
https://docs.github.com/en/desktop

How to clone a repository in GitHub Desktop:  
https://docs.github.com/en/desktop/adding-and-cloning-repositories/cloning-and-forking-repositories-from-github-desktop

How to manage branches in GitHub Desktop:  
https://docs.github.com/en/desktop/making-changes-in-a-branch/managing-branches-in-github-desktop

How to push changes from GitHub Desktop:  
https://docs.github.com/en/desktop/making-changes-in-a-branch/pushing-changes-to-github-from-github-desktop

How pull requests work:  
https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests

### Unity
Unity version control integration / editor settings:  
https://docs.unity3d.com/Manual/Versioncontrolintegration.html

Unity asset metadata (`.meta` files):  
https://docs.unity3d.com/2023.2/Documentation/Manual/AssetMetadata.html

---

## 15. Optional note for later

We can later improve this file by adding:
- screenshots from GitHub Desktop
- screenshots from Unity Hub
- a small visual workflow diagram
- branch naming examples for each team member
- a short conflict-handling section with pictures

