##Sera Inject

Old Lua Injector from 2010. Mainly used for League of Legends.

===========

Info:
  * Recompiled 2014 - C# 2010. 
  * Made in Framework 2.0 < Works on nearly every Windows OS. 
  * References > LuaInterface.dll 

===========


Contains current functions and their parameters 
 

beep(frequency, duration) - Creates a tone at the given frequency for the given amount of time
	frequency		Frequency in hertz
	duration		Duration in milliseconds

cleanPatternFile(filename) - Removes duplicate entries from a pattern file
	filename		File Location

clr() - Aliases cls() - Clears the screen.

cls() - Clears the screen.

CreatePatternMaskFromFile(fileLocation) - Takes a file and creates a pattern mask according to each line in the file.
	fileLocation		Location to file that contains at least two lines of patterns

FindPattern(dwStart, dwEnd, szPattern) - Finds a pattern or signature inside of the given Process and memory range
	dwStart		Address on which the search will start.
	dwEnd		Address on which the search will end.
	szPattern		A hexadecimal string representing the pattern to be found. Ex: "4C 61 F3"

FindPatternMask(dwStart, dwEnd, szPattern, szMask) - Finds a pattern / signature inside of the given Process and memory range
	dwStart		Address on which the search will start.
	dwEnd		Address on which the search will end.
	szPattern		A hexadecimal string representing the pattern to be found. Ex: "4C 61 F3"
	szMask		A string of 'x' (match), '!' (not-match), or '?' (wildcard).

FindPatternMaskRetry(dwStart, dwEnd, szPattern, szMask) - Tries and continues to try to find a pattern / signature inside of the given Process and memory range
	dwStart		Address on which the search will start.
	dwEnd		Address on which the search will end.
	szPattern		A hexadecimal string representing the pattern to be found. Ex: "4C 61 F3"
	szMask		A string of 'x' (match), '!' (not-match), or '?' (wildcard).

GetProcessID(processName) - Returns the ID to the first process found by specified process name.
	processName		Process' name without .exe

help() - List available commands.

helpcmd(strCmd) - Show help for a given command or package
	strCmd		Command / Package to get help of.

IntToHex(integer) - Converts an integer into it's equivalent hex
	integer		Memory Location To Read

newVM() - Restarts the LuaVM instance

playSound(location) - Plays the .wav sound file from the given location
	location		File Location

ProcessVersionByID(processID) - Returns the FileVersion to the first process found by specified process ID.
	processID		Process' ID

ProcessVersionByName(processName) - Returns the FileVersion to the first process found by specified process name.
	processName		Process' name without .exe

quit() - Exit the program.

readline() - Waits for the user to input text and hit return and then returns their entered text.

ReadMemoryAsHex(location, length) - Reads the given location in the memory as hex of the given length
	location		Memory Location To Read
	length		Length of bytes to read

ReadMemoryAsInt(location) - Read's the given location in memory as a 4 byte integer
	location		Memory Location To Read

ReadMemoryAsIntRetry(location) - Tries and continues to try reading the given location in memory as a 4 byte integer
	location		Memory Location To Read

ReadMemoryAsString(location, length) - Reads the given location in the memory as a string of the given length
	location		Memory Location To Read
	length		Length of bytes to read

runfile(s) - Runs the specified Lua file.
	s		File Name

runFile(s) - Runs the specified Lua file.
	s		File Name

SetMemoryReader(ID) - Set the what process ID to read from
	ID		Process ID

SettingsClose() - Creates or opens the given ini file.

SettingsOpen(fileName) - Creates or opens the given ini file.
	fileName		Path to ini file

SettingsRead(key) - Reads back the given key's value as a string
	key		Key Name

SettingsSection(section) - Changes the section of where the keys and values are saved to.
	section		Section Name

SettingsWrite(key, value) - Writes the given value to the given key.
	key		Key Name
	value		Value to be written to the key

sortPatternFile(filename) - Sorts all the entries entries from a pattern file
	filename		File Location

wait(time) - Waits the specified amount of time in milliseconds
	time		Time to wait in milliseconds

waitkey() - Waits until a key is pressed

windowname(windowName) - Sets the window's name
	windowName		New name for the console window

WriteMemoryAsInt(location, integer) - Writes the given integer to the specified memory location
	location		Memory Location To Read
	integer		Integer to write to the memory

