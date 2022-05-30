# ACT Notes Plugin
This Advanced Combat Tracker (ACT) plugin provides a means of organizing notes about zones and mobs.

The left hand panel provides a list of zones with subheadings for mobs within that zone.

The right hand panel provides a basic editor for any notes about the selected zone or mob.

All entries are user-generated, but can be shared using ACT's XML sharing mechanism.

An example is shown below.

![Overview](images/Overview.png)

## Upadate Notes
_Version 1.1_
* Added the ability to set the background color in the editor.
* Reworked the editor color pickers.
* To reduce the number of sections while sharing, notes with images can now compressed.
  * __If the receiver of the note is not running version 1.1 of the plugin, this will cause an error on their end. Be sure everyone has updated to version 1.1 before using this feature.__
  * Compressed notes will be incomprehensible when seen in the chat window.
* Updated matching mob notes with mobs killed. Should match more reliably.

## Adding Zones and Mobs
The **[Add Zone]** button creates a new entry in the zone list. 
The **[Add Mob]** button creates a new mob in the currently selected zone.

Zone and mob names can be arbitrary text, but if they match in-game zone and mob names, the plugin will track when you enter the zone or kill the mob.

The easiest way to add a zone or mob to the list is to use the encounter list on ACT's main tab.
The default text when adding a new zone or mob is the selected zone name or mob name in the main encounter list.

In the example below, the desired zone is selected, then the **[Add Zone]** button is pressed, then the `Enter` key
is pressed to accept the entry. Mobs are similarly added by selecting the mob, pressing the **[Add Mob]** button, 
then the `Enter` key. Mob names can be picked up from either the zone encounter list or a particular mob encounter list. Both methods are shown in the example below.

When a fight involves more than one name, if all the names are entered for the mob name (separated by commas), the plugin will try to match each name when monitoring kills. In the example below, two-mob fights are set up with minimal typing by using the following steps:
  1. On the ACT Main tab, select the first mob
   2. On the plugin tab, press **[Add Mob]** and `<Enter>`
   3. On the Main tab, select the second mob in the first mob's encounter list
   4. On the plugin tab, press **[Add Mob]**, then answer "Yes" to the append question

![New Items](images/add-mobs.gif)

## Adding Notes
Simply type in the editor panel after a zone or mob is selected to create a note for that zone or mob.

The editor allows the choice of font, colors, bullets, URLs, images, etc.

The editor can save many image types using either `Ctrl-V` to insert from the clipboard, or the `Insert image from file` toolbar button (which also uses the clipboard).
**Note:** The plugin saves the images as text bitmaps, which is not very efficient for loading, storing, or sharing.

## Monitoring
Whenever a player enters a zone, the plugin searches its zone list for a matching zone name.
If a match is found, that zone is selected. 

If the zone itself has a note, that note is displayed.

If there is no note for the zone, but there are mobs, the first mob in the zone's list is selected, whether it has a note or not.

When an encounter ends, the plugin looks for the killed enemy's name in its mob list for the zone. 
If the name is found, the next mob in the zone list is automatically selected. 
The intent is to display the note for the mob you are about to kill. 
But this process is not 100% reliable. 
It's just an aid to save you maybe one click to select the notes for your next mob.

If the mob order in the zone list is not in the order you usually kill them, 
the list can be re-ordered by draggging and dropping the mobs into the desired order.

## Sharing Notes
Right-click the zone name in the tree to share a zone note. Right-click a mob name in the tree to share a mob note.

The notes are stored as Rich Text Format (RTF). This format provides the capability to change the font, color, bullets, etc, but does add overhead.

The `Copy to XML` menu allows sharing notes in ACT with other users of the plugin. The `Export to RTF` menu allows exporting the notes out of ACT to a WordPad file.

### Share Channel
For the first XML share in a zone, the plugin guesses whether you are likely in a group or raid from the selected zone name
and chooses the supposed prefix, `/g` or `/r`. This can be changed by selecting the appropriate button.
To paste in a different channel, use the `custom` choice. For example, if you wanted to paste to the guild, you would select the `custom` button and enter `/gu` in the textbox. The selected prefix is saved for the zone.

### Note Segmenting
EQII can only paste around 250 characters at a time in a chat window, and allows about 1000 characters per line and 16 lines in a macro. Even a small shared note requires multiple sections.

The plugin automatically breaks a note into appropriate sharable sizes.

There are two methods for sharing a note. The first method is similar to how a trigger is shared in ACT. The second method uses EQII macros.

 Notes with even a small image in them will result in a lot of sections, making the macro the only reasonable option. Version 1.1 of the plugin adds an option to compress notes with images. This typically reduces the number of sections. But the recipient of the note must be running version 1.1 of the plugin to decode the compression. **Version 1.0 of the plugin will generate an error every time it tries to view the note if it receives a compressed note.**

Once all recipients have updated to plugin version 1.1, the **Compress Images** checkbox can remain checked.

Below is an example that results in 200 copy sections or three macros. This particular note is 61 sections or one macro when compressed.

![icon](images/big-image-sections.png)

#### Method 1: Paste Share
To paste into a chat window, the sender of the note needs to copy each section from the plugin using the plugin's **[Copy]** button and then use `Ctrl-V` to paste it in the chat window, as shown below. In this example, three sections are required. The `/g` selection prefixes the section with the groupsay command. The user follows the procedure below:
1. verify or select the appropriate prefix and image compression
2. press the **[Copy]** button to put the first section in the clipboard
2. click the game chat window and press `Ctrl-V`
3. the plugin automatically moved to the second section, so the user can just click the plugin **[Copy]** again
4. repeat until all sections are copied to game chat
5. press **[Done]** to dismiss the dialog

This process is illustrated below.

![copy-paste](images/paste.gif)

#### Method 2: Macro Share
The plugin's **[Macro]** button generally takes fewer steps to share a note. Pressing the **[Macro]** button generates enough text files to share the note. The text file names follow the format `note-macroX.txt` where `X` is a number starting with 1 and incrementing until enough files are created to share the entire note, e.g. the first 16,000 or so characters would be shared using the macro file `note-macro1.txt`. To share using macros, the user follows the procedure below:
1. Verify or select the appropriate prefix and image compression
2. press the **[Macro]** button
3. the plugin creates the macro file(s)
4. the note is shared in EQII using the slash command `/do_file_commands note-macro1.txt` in an EQII chat window
5. continue with `/do_file_commands note-macro2.txt`, etc. as required
6. press **[Done]** to dismiss the dialog

This process is illustrated below.

![macro](images/macro.gif)

#### Section Reception
The receiver of the note needs to accept each section. A single macro can contain up to 16 sections.

If lots of sections arrive faster than the receiver can click the ACT `Add Now` button for XML shares,
they can still accept them on ACT's `Options` tab, `Configuration Import/Export` heading,
`XML Share Snippets` section. Select each one listed in the yellow box and press the **[Import Above Data]** under the white box. **Note:** the `Add Now` button can be bypassed for trusted players by adding them to the `Automatically accept data from` in the green list.

When using macros, all sections from each macro should be accepted before another macro is shared to avoid mixing sections between macros. For example, make sure all sections from `note-macro1.txt` have been accepted before sharing `note-macro2.txt`

### Previous Note Replacement
If the receiving player does not have an existing note for the incoming zone or mob, 
the plugin creates the appropriate entities.

The plugin provides several options for receiving shared notes when the receiving player already has a note for that zone or mob.

![append](images/incoming.png)

* **Append**: The incoming note will be appended to the existing note.
* **Replace**: The incoming note will replace the existing note.
* **Ask**: The plugin will prompt for whether to append, replace, or ignore the incoming note.
* **Accept**: The incoming note will replace the existing note if the sender is on the receiver's whitelist.
  * The note is appended if the sender is not whitelisted.
  * This option is not present if the user's whitelist is empty.

This setting is saved for each zone.

### Compare Preexisting and Received Notes
When an incoming note is appended to an existing note, a delimiter line is added between the old note and the new one and the **[Compare]** button becomes active. Pressing the **[Compare]** button opens a new window showing the difference between the text above the first delimiter and the text below the first delimiter. If there are multiple append delimiters, the comparison is only between the first two parts.

The comparison shows the ***text*** differences at the line level. Formatting is ignored. Multiple spaces are ignored. Character case is ignored. 

Lines that are different are shown in yellow. Grey lines are used to keep the unchanged lines aligned - this works best if the difference window is wide enough to hold an entire note line on one screen line.

An example is shown below:

![compare](images/compare.png)


