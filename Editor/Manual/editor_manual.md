_Boo! Greedy Kid_ editor manual (Windows only)
==========================
> **Note**: the _Boo! Greedy Kid_ editor is only available on _Windows_ and there is no plan to port the editor to _Mac OS_ or _Linux_.

**Manual version**: 1.0
**Last modified**: 4th december 2018

<i class="icon-globe"></i> **Online version**: http://greedy-kid.com/editor_manual/

<i class="icon-mail"></i> **Contact and support**: _Boo! Greedy Kid_ editor _Steam_ forums

<p align="center"><img src="http://greedy-kid.com/editor_manual/images/editor.png" width="600"></p>

Table of contents
-----------------------
[TOC]

Introduction to the editor
------------------------------------
Welcome to the _Boo! Greedy Kid_ editor!

This tool allows you to create your own buildings (with up to 99 levels per building) and to upload them on the _Steam Workshop_ of the game.

Through this document, you will learn how to do all of this.
Let's get started!

#### Requirements
The editor requires a _Direct3D 10_ compliant graphics card and drivers, as well as the _.Net Framework 4.5_ (or newer).

If you're running the editor from _Steam_, you shouldn't need to install anything else.

If the editor fails to launch, it most probably means that your graphics card or drivers are not compatible.

> **Note**: the editor and the game don't use the same technology, hence they both have different requirements. Please refer to the game's _Steam_ page for its own requirements.

#### Launching the editor

##### <i class="icon-cloud"></i> From _Steam_
When starting _Boo! Greedy Kid_ from your _Steam_ library, a pop-up window will appear and will propose you to either start the game, or the editor.

IMG STEAM POP-UP

Select the editor (you will be able to start the game directly from the editor if need be).

##### <i class="icon-hdd"></i> From disk
The editor is located in the game installation folder, in the **Editor** <i class="icon-folder"></i> sub-folder. Double-click on "GreedyKidEditor.exe" to launch it.

If you are using the _Steam_ version of the game, the editor may require that _Steam_ is launched before it can run.

Creating a building and managing its levels
----------------------------------------------
#### Understanding the game structure
In _Boo! Greedy Kid_, the action takes place in a single building (also nammed "campaign" in-game).

Each building can have up to 99 levels. These levels are what you need to design individually and what makes gameplay space.

The editor save your building in a special file format with the **.gdk** <i class="icon-file"></i> extension. These files are only readable by the editor.

#### Setting up a new building
You may create a new building by clicking on the **File** menu and select **New**.

Then, rename your building by clicking on the top left **Rename** button. This is the name that will appear on the _Steam Workshop_ as well as in-game.

#### Managing levels (add / remove / re-order)
On the left of the editor, you can see your building configuration <i class="icon-list"></i>. By default, it only has one level, but you can **Add** more, **Remove** them, or adjust their order with the up <i class="icon-up"></i> and down <i class="icon-down"></i> buttons.

<p align="center"><img src="http://greedy-kid.com/editor_manual/images/level_manager.png" width="250"></p>

You can also **Rename** levels, but their name will not appear in-game or on the _Steam Workshop_. Their name is only for your personal organization and comments.

We are now ready to start creating a level. Click on the first level to make it the active level.

Editing a level and understanding the creation modes
-------------------------------------

#### The level editing space
All the editing is done within the **live preview zone**.

<p align="center"><img src="http://greedy-kid.com/editor_manual/images/live_preview.png" width="600"></p>

On the right, you can access the **creation toolbox** <i class="icon-wrench"></i>. Each icons represent a mode to progressively build your level.

<p align="center"><img src="http://greedy-kid.com/editor_manual/images/toolbox.png" width="200"></p>

#### <img src="http://greedy-kid.com/editor_manual/images/mode1.png" height="20"> Room mode
The first mode is the room mode. Click on it, and you may now create empty room by left clicking somewhere on the live preview zone.

You can adjust a room's size by hovering one of its walls, and press + maintain the space bar. You can now adjust its size as long as you maintain the space bar.

You can customize the appearance of a room by hovering its outer or inner walls and using the mouse wheel.

If need be, you can totally remove a room and its content by hovering it and right click on it. A confirmation message will appear before fully removing a room to prevent any unwanted clicks.

#### <img src="http://greedy-kid.com/editor_manual/images/mode2.png" height="20"> Decoration mode
The second creation mode is the decoration mode.

Once selected, you can add decorations by left clicking on a room. To change between the different decoration available, you can use the mouse wheel while hovering a decoration.

If you'd like to move a decoration, hover it and maintain the space bar just like room size adjustment.

You can also remove a decoration by right clicking on it.

If you have two decorations overlapping and wish to adjust which one should be over another one, you can use the C and V keys while hovering a decoration to change its layer.

#### <img src="http://greedy-kid.com/editor_manual/images/mode3.png" height="20"> Separation (white) door mode
This mode allows you to add white doors to divide a room into multiple spaces. NPC can't go through those doors when they are closed.

Adding, removing or moving a door works just like the other modes by using the mouse buttons and the space bar while hovering a door.

#### <img src="http://greedy-kid.com/editor_manual/images/mode4.png" height="20"> (Colored) Floor door mode

This mode allows you to add doors that will connect together to navigate to other rooms. NPC can go through these doors.

To select which room leads to another one, both doors must have the same color. To change a room's color, use the mouse wheel while hovering it. For instance, if you change two doors to orange, they will connect.

There can only be two doors with the same color in a level. But this rule doesn't apply to grey doors. Grey doors don't work with colors, but instead with vertical alignment.

When you move a grey door, you will see vertical guidelines to help you align them. If two grey doors are perfectly aligned (to the pixel), they will connect.

Sounds complicated? Don't worry, the editor will tell you if there's anything wrong with your doors.

#### <img src="http://greedy-kid.com/editor_manual/images/mode5.png" height="20"> Furniture mode
Furniture are objects behind which the kid can hide.

You can add, change, or move furniture like any other modes, by using your mouse buttons / wheel and the space bar.

Like the decorations, you can adjust their layer with the C and V keys if you'd like to fine tune overlapping furnitures.

#### <img src="http://greedy-kid.com/editor_manual/images/mode6.png" height="20"> Elevator mode
Each level must have an entrance elevator, and an exit elevator.

To place an entrance, left click where you'd like to add it. To place an exit, use the right mouse button.

#### <img src="http://greedy-kid.com/editor_manual/images/mode7.png" height="20"> Retiree mode
In this mode you can place retirees in the level.

You can customize their appearance with the mouse wheel.

You can also adjust their life with the C and V keys while hovering them, or adjust their money with the B and N keys.

Life points represent the number of time it is required to scare them.

#### <img src="http://greedy-kid.com/editor_manual/images/mode8.png" height="20"> Nurse mode
This mode is for placing nurses, and works just like the retiree mode (to the exception that nurses don't drop money).

Life points represent the number of time it is required to scare them.

#### <img src="http://greedy-kid.com/editor_manual/images/mode9.png" height="20"> Cop mode
The cop mode allows you to add cops who will be present in the level right from the start of the level.

You can change the cop type by using the mouse wheel while hovering a cop.

If you prefer to add a timer before cops will enter the level, have a look at the lower left of the editor.

Here you can adjust how many seconds from the start of the level are required for cops to spawn, and which kind of cops should arrive.

Note that you can have cops already in the level and cops arriving later at the same time (e.g. you can have two cops wandering from the start, and another one arriving after 10 seconds).

#### <i class="icon-award"></i> Setting the target time for scoring
When players complete a level, they are awarded up to 3 stars <i class="icon-star"></i><i class="icon-star"></i><i class="icon-star"></i>depending on the following:

- 1 star <i class="icon-star"></i> for completing the level;
- 1 star <i class="icon-star"></i> for collecting all the money without loosing any;
- 1 star <i class="icon-star"></i> for completing the level under the target time. 

The target time is a value that you must set yourself in the editor. It is represented by the timer at the top of the live preview zone.

You can change the target time by hovering it and use the mouse wheel (to adjust it by 1 second at a time) or the left / right mouse buttons (to adjust it by 5 seconds at a time).

Be sure to thoroughly test your level target time to make sure that the score is beatable.

<i class="icon-gamepad"></i> Testing your buildings in-game
--------------------------------------------

#### <i class="icon-ok-circled"></i> Level design rules
For a level to work in-game, it must follow these design rules:

- Your building must have a name;
- All levels must have an entrance elevator and an exit elevator;
- The exit elevator must be reachable by the player;
- All retirees must be reachable by the player;
- All doors must be connected (no single door are allowed);
- Doors can't be connected to more than one door;
- Spawn timers can not be set if there is no cop count assigned to them.

You don't have to remember these rules as the editor will warn you of any errors and will prevent any export or upload of broken levels.

#### <i class="icon-hdd"></i> Exporting a test building
If all the design rules are met, you are now all set to test your building in-game!

First save your building through the "File" menu.

To test your level in game, use the "File" menu and click on "Export to test in-game".

This will add your building only to your game and you can now start the game and test it. You can start the game directly from the editor.

#### <i class="icon-gamepad"></i> Live testing without restarting the game
To test your freshly exported level, start the game and go to the _Steam Workshop_ menu. Your building should appear here and be labeled as "test" after its name.

Note that you can keep the game running and go back to the _Steam Workshop_ menu while you edit your levels in the editor. If you export new buildings they will automatically appear without the need to restart the game.

Don't forget to export your building each time you are making changes to try them.

<i class="icon-upload-cloud"></i> Uploading your buildings to the _Steam Workshop_
-------------------------------------------------------
When you are satisfied with your building, you can share it with the world!

#### Initial upload
Click on the "File" menu and select "Upload to Steam Workshop".
Complete your upload by adding a description, selecting a language and adding a 16:9 preview image (e.g. a 1280x720 PNG file), and hit the upload button.

<p align="center"><img src="http://greedy-kid.com/editor_manual/images/workshop_config.png" width="400"></p>

If everything went well, your level is now available on the Steam Workshop!

<p align="center"><img src="http://greedy-kid.com/editor_manual/images/progress.gif" width="400"></p>

#### <i class="icon-refresh"></i> Updating a building already uploaded
When you first upload a building to the _Steam Workshop_, your building is attributed a unique identifier that is stored in your building's .gdk file.

This identifier is then used to update an existing _Steam Workshop_ item.

If you try to re-upload a building using the same .gdk file, the editor will automatically proceed with an update of the existing _Steam Workshop_ item based on the .gdk internal identifier.

This identifier can not be edited or viewed. This means that if you want to be able to update an existing _Steam Workshop_ item, **you must not loose the original .gdk file used for the first upload**. <i class="icon-attention"></i>