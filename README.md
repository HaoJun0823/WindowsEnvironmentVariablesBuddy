Windows Environment Variables Buddy

Windows Environment Variables Buddy is a specialized utility designed to simplify the management of "Green Software" and custom CLI tool paths.

Instead of cluttering your system %PATH% with dozens of individual folder entries, this tool creates and maintains a single "container variable" called %BuddyVariables%.
🚀 Key Features

    Dynamic Management: Manage all your custom paths in one simple .txt file.

    Path Protection: Unlike standard .NET methods, this tool does not "flatten" or hard-code existing system variables (like %SystemRoot%). It preserves the integrity of your registry.

    Instant Refresh: Uses Win32 API broadcasting to notify the system of changes—no reboot required for new terminals.

    Registry-Safe: Uses REG_EXPAND_SZ to ensure nested variables are correctly resolved by Windows.

    Automatic Backup: Backs up your previous variable values before making changes.

🛠 How it Works

    Add your tool directories to WindowsEnvironmentVariablesBuddy.txt (one per line).

    Run the program.

    The tool updates %BuddyVariables% and ensures %PATH% contains a reference to it.

    Your CLI tools (like cmder, ffmpeg, etc.) are now globally accessible!
