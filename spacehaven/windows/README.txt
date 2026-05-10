Space Haven Save Editor - Windows
=================================

Requirements
------------

Install Python 3.10 or newer from:

https://www.python.org/downloads/windows/

During install, enable "Add Python to PATH".

Usage
-----

1. Exit Space Haven. The game may lock the save file while running.
2. Double-click run.bat in this folder.
3. Select a save from the top drop-down and click Load.
4. Edit Bank, Crew, or Resources values.
5. Click Save (with backup).

The original file is backed up as:

    game.bak-YYYYMMDD-HHMMSS

Save Locations
--------------

Default Windows location:

    %APPDATA%\Spacehaven\savegames\

Discovery order:

1. Path passed on the command line.
2. SPACEHAVEN_SAVES environment variable.
3. Common default save locations.

You can also use Browse... and choose the save\game file manually.

Restore
-------

To restore a backup, rename the chosen backup file back to:

    game

Resource Names
--------------

If a Space Haven update changes resource IDs, regenerate resource_names.json:

    python extract_names.py "<SPACEHAVEN_GAME_ROOT>\spacehaven.jar"
