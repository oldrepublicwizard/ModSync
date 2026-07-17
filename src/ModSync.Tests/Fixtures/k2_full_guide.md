KOTOR 2 Full - KOTOR Community Portal

# KOTOR 2 Full Build

Last Modified 07 July 2026

## Quick Info

Why use our mod builds?

See the mod build FAQ for answers to other common questions.

Need help? Ask in the#tech_support channel of our Discord server.

## Installation Notes

Important

If you were linked to this list directly without reading the install instructions, please go back and read them here! There are several critical setup and general best-practice steps you need to be aware of before beginning the install process. The instructions for how this list is installed are located on that page, not this build!

### Zeroing Step

Unless you have freshly installed the game or have never installed KOTOR 2 previously, please blank-slate your setup: uninstall the game, delete the full contents of your game's directory (the entire folder where the executable file previously was located; this does NOT always happen automatically when the game is uninstalled), and reinstall the game. A significant amount of troubleshooting support we offer is the result of leftover game files or file corruption from a game which was installed many years ago. Performing this zeroing step helps to ensure your local copy is fresh, uncorrupted, and free of any old mod data.

Once you have your freshly-installed copy of the game, please attempt to set your main game directory and all subfolders as not read-only. This is because executable mods will need to modify the files therein directly to install properly. Some operating systems (particularly Windows OS 8+) will lock the settings on these folders depending on their locations, so for many systems it will not be possible to actually pull them out of their read-only state, but it is important to at at least attempt to change the folders, as on some operating systems it is necessary for a functional install.

### KOTOR 2 on Steam

If you are playing KOTOR 2 on Steam, please actively check that you're not subscribed to any content on the Workshop, even if you're super positive you're not. Multiple times a month we have people come in asking for tech support who swear they're not subbed to anything on the Workshop, then they go check and it turns out that they subbed to TSLRCM a decade ago and completely forgot about it. It's not fun to find that out after you start your game.

Checking is very simple: just go to your Steam library, right-click KOTOR 2 and select properties, then go to the Workshop tab and confirm nothing appears in the list. Doing this not only helps you avoid major errors during the install process, it also helps us to help you in the event that something goes wrong with your install, because we'll know immediately that the Workshop isn't the problem.

### TSLPatcher Installation Requirements

Do not, under ANY CIRCUMSTANCES, run a TSLPatcher or HoloPatcher executable from within a file archive (IE a compressed file like a .rar, .zip, .7z, etc). You must extract all archives before installing mod content, or running an executable. Failure to extract before installing will break your game. Also, ensure you don't extract installer-based mods to the same folder. If, for example, you extract every single mod to a folder called "KOTOR Mods", each time you extract you'll be overwriting critical data from the previous mods. Make sure each installer-based mod is extracted to its own folder before running the installer.

Please also bear in mind: if you are asked to direct a TSLPatcher or Holopatcher executable to a folder to begin install, that folder should ALWAYS be the main game folder, where your game's .exe file is located (or the symlink to that folder created in the optional Quicker TSLPatching step, discussed further below). For mods that are NOT installed with the TSLPatcher or Holopatcher, unless otherwise noted their loose-file contents are placed directly in the Override directory. Unless you are following specific instructions (the mod author's or my own) that state otherwise, you should not be placing files in the game's main directory or Modules folder. You should never have any subfolders inside the Modules or Override folders.

As a final note, be aware that the TSLPatcher executable utilized in the installation process of many of these mods (frustratingly) does not always function on multi-monitor setups. If you have more than one monitor, you may need to disable your second while installing the mods, but you can re-enable it once the installation process is complete.

### Single Install

It is critical to ensure that you only have ONE installation of KOTOR on your device before beginning the installation process. If you have multiple installations, or even just two game executables in different locations, mod installers which are set up to auto-detect your game's installation directory will misidentify which one is correct and install the mods wherever they please, probably spiking your whole install in the process. If you want to make a backup of your vanilla gamestate, make sure to compress the entire game into an archive (.rar, .zip, .7z, etc.) before starting the install process.

### Updating from a Prior Build

If you currently have a prior mod build version installed and would like to update to the current Revision, due to the importance of install order to the mod builds' function, there is unfortunately no way to do so without resetting to a fresh install and reinstalling the new build iteration on top. To do this, just perform the zeroing step listed above: uninstall your game, delete all remaining contents in the game directory (remember, frequently, especially on the Steam release of the game, mod content will not be uninstalled as part of the application's uninstallation), and then reinstall the game before applying the new build version.

### Native Windows Archiver & Extracting Mods

Modern Windows OS versions come with their own archive extractors which can open files like .rar, .zip, etc which many of these mods are packaged as. The Windows extractor is quite bad at its job, however, and will fail to extract many of the mods included in this list because it doesn't understand their file structure and/or compression. In order to extract all the mods included in this list, please make sure to have a copy of a program like WinRAR or 7zip and use them to unpack the archives.

### Runtime Errors

For some reason which I can't even begin to fathom, on certain hardware (as far as we can identify, Nvidia cards primarily in the RTX series and mostly 20xx and up, as well as on certain AMD cards presently of unknown version), runtime errors are beginning to pop up when cutscenes are queued to play after a loadscreen. We don't know why this is happening, but the problem only exists on a small percentage of the affected cards, so it isn't an issue with the builds. As far as we've been able to diagnose to this point, there are four ways to attempt fix this. The first involves disabling all software overlays interacting with the game: Discord overlay, Steam overlay, etc. Sometimes this can fix the problem outright. If that doesn't resolve it, if your card is Nvidia, you can go into Nvidia control panel, open program control, and set low latency mode to on/ultra. This reduces the render queue and seems to bypass the issue on some setups. If neither of those options work, you will need to resort to workarounds each time you experience runtime error crashing (don't worry--these issues seem to be rare even for those users who experience them). The first of these workarounds is to put the game into windowed mode during the problem load screen—you can do this by going into your game folder, opening the .ini file with a text editor of your choice, and editing the line "Fullscreen=1" to =0, and adding the line "AllowWindowedMode=1" somewhere under the Graphics Options heading. You can then reverse the process (Fullscreen=1, AllowWindowedMode=0) whenever you've gotten past the load screen to put the game back to fullscreen. The second workaround resolves the issue, but unfortunately stops you from being able to see the cutscene, so it should be treated as a last resort only if the above fixes don't work for you. Rename the movies folder (don't delete it, just rename it to something like "movies1"), relaunching the game, and attempt the load again. If successful, simply rename the movies folder back when you've finished the load.

### Linux Players

Please note, case-sensitivity is an issue with many Linux systems (as you may expect). For those not running a case-insensitive distro, you will need to batch-lowercase any mod files--any mod files, not just the files that end up in your override directory. It's likely you will need to both lowercase the files before installing any installer-based mod and then again at the end of the process as a batch action in your game directory and all subfolders, as the TSLPatcher/HoloPatcher will generate several files as part of their install processes which will have capitals that are misread.

Most Linux players appear to be using Proton these days, and for those that do there are some known issues with it, both related to crashes with certain older Proton versions and with rendering issues on newer Proton releases. I don't use Linux and don't feel qualified to try to summarize the issues and fixes here, but just be aware if you run into trouble there are several pins in the Discord's #tech_support channel dealing with Proton and how to resolve both issues (the crashing & rendering problems).

### Known Bugs

- Although all instances of this error should now be fixed, some textures may still have a partial transparency effect applied to them (you will be able to faintly see through them, or part of them, as if they were a slightly more opaque hologram). If you encounter any instances of such textures please report them so I can fix any leftovers.
- When sparring with Visas on the Ebon Hawk, her lightsaber may be invisible.
- There are significant camera angle errors in the scene where Carth is on the Sojourn's bridge approaching Telos to engage the Ravager. We currently don't know why this is and are looking into it actively. The scene still plays properly, including audible dialogue, the camera simply doesn't face the proper direction when doing so.
- For some users, the game may very rarely crash on load when trying to enter a new area due to a data overflow. This is extremely uncommon and most users should never encounter it, but it is impossible to fully prevent. Fortunately, it is easy to fix if it does happen: simply temporarily disable the 'Frame Buffer Effects' and "Soft Shadows" settings in your advanced graphics options, proceed through the load screen, then re-enable the settings once on the other side. Again, most players should never encounter this.

### Total Filesize on Disk

For your reference, the total filesize of all mods before extraction, excluding movies, is approximately 6.5GB. The total size of the install when extracted, including basegame files but excluding upscaled movies, is approximately 15.5GB. The total size of the install when extracted (plus HD movies (Pops Maellard's version) in 1920x1080 resolution) is approximately 35GB. Please bear this in mind if you intend to keep backups and/or are operating on a system with limited available memory.

## Optional Install Assistant - Quicker TSLPatching

Because of the way KOTOR mods install, it's virtually impossible to have a true mod manager/installer hybrid like those you may be familiar with for other games, like MO2 or FOMM. The best we can really get are tools to accelerate installations, of which one is supported for the mod builds.

### Quicker TSLPatching

Quicker TSLPatching is a system workaround (specifically a registry edit) which makes it easier to install mods. This script will not work on Mac operating systems (neither Linux, I believe) not running Windows on a partition, but there are some comments in the Posts section of the download page that might offer alternatives that allow it to function properly.

This mod features no level of automation, and only serves to speed up the install process of mods installed via the TSLPatcher executable. Mods will frequently use the TSLPatcher to install, and the TSLPatcher often requires you to manually point the installer at your game's base installation directory. Sometimes the patchers do this automatically, but sometimes they fail, which is where this script comes in; it'll create a symlink to your KOTOR and KOTOR 2 install directories direct to your desktop, where the TSLPatcher defaults to pointing, so you only need to scroll down and click the folder to install without navigating through your system's file structure to find the game folder. The script is confirmed safe and once your install is complete you can delete the desktop folders with no problems. To install, you only need to run the script and follow the instructions it presents, choosing your KOTOR directory in the first popup window and your KOTOR 2 directory in the second. If the script does not function for you, check the Posts section of the mod download page to see some alternate suggestions for manually forcing the functionality of the script.

## CRITICAL: PC Version Differences & Fixes

Warning

Your entire install could be unstable if the information in this step is not taken into account. I hope that's gotten everyone's attention, because it really is that important. This section will be a bit long by necessity, but it is critical.

KOTOR 2 has multiple different versions. The easiest way to refer to these are the Aspyr patch and legacy version. The legacy version is the original release, while the Aspyr patch is a newer update and the default version of the Steam, GoG, and Mac Appstore game copies. It may also be the default on some more obscure storefronts but we haven't identified any others--if you're ever unsure about which copy you're using, you can boot a freshly-installed copy of the game up with a controller plugged in and see if it responds to controller commands: if it does, you have the patch. If you don't have a controller handy, you can check the resolution options available ingame; if you see 1920x1080 or ultrawide options, you have the patch.

As implied by the above, the Aspyr patch adds some useful functionality like native controller and widescreen support. The patch unfortunately also adds several bugs. Until recently most of these problems were not fixable. Now, however, all of the serious problems caused by the Aspyr patch can be fixed. The only significant issues of note that remain are:

1. The possibility of losing savegames (this can be entirely avoided just by turning off cloud saves)
2. What can best be described as "dialogue floatiness". Sometimes the dialogue will just... skip, for no good reason. This is different than memory leak-related dialogue skipping, if you're familiar with that in the context of KOTOR. It's more like delayed input reading, where one mouse press to bypass a dialogue line will register as two and instantly skip the next dialogue as well. This is a really annoying bug, but thankfully is fairly rare.
3. When playing with a controller plugged in, even if not actually using the controller, it can cause an issue where the camera becomes vertically offset after upgrading an item in the workbench. Unplugging any controllers while playing resolves the issue entirely, but if you intend to play with a controller the issue must be resolved by saving and loading after it occurs.

All of the above is being brought up now for two important reasons: first, if you're using the Aspyr patch, you must apply the fix packages linked further down in this section for this build to function properly. And second, if your game version isn't using the Aspyr version, you need to decide whether or not you want it to be.

Unlike the original KOTOR, where the widescreen application is so good that running it actually makes the game more stable, the same is very much not true of KOTOR 2. Without getting into the weeds on why, here's the takeaway: if you don't use the Aspyr patch, you can't apply widescreen to your game safely. So even though it has some issues, if you either want widescreen support or to have controller support for your game and are not on GoG/Steam/Mac Appstore, you NEED to apply the Aspyr patch to your game copy. My personal recommendation, having played KOTOR 2 an embarrassing 35+ times on the legacy version and a single time on the Aspyr version with the fixes, is to use the Aspyr version if you can. No, it isn't perfect. But the better stability it affords is well worth the few annoying bugs it creates. If you want to use it, you can install it from the link provided by Tarhiel in this thread on the GoG forums. We've checked the files and confirmed that they are just a copy of the Aspyr patch: just extract the files and move them directly into your KOTOR 2 game folder, overwriting when prompted, and you will update your install to the Aspyr version.

For those who have the Aspyr patch installed, the below are the fixes which must be applied in order for this build to function normally:

### 3C-FD Patcher

Author: J

Description: Actual unadulterated black magic fuckery that fixes a huge number of the persistent and glaring problems of the Aspyr patch.

Category & Tier: Bugfix, Graphics Improvement / 1 - Essential

Installation Method: Executable

Compatibility Warning

If you didn't read the wall of text that preceded this mod, you can brick your entire install here. You'd seriously better not have just skipped straight here.

Ensure to only use this if on the most recent Steam/GoG patch, or if having manually applied the Aspyr patch to your game! Also ensure your executable is a fresh copy with no edits: it CANNOT have a 4GB/Large Address Aware patch already applied to it (this mod adds that patch). Finally, please note that this mod cannot be utilized on the Mac Appstore version of the game, since it doesn't launch via an executable; for Mac Appstore players, you will unfortunately need to play without these fixes. Since your games will not have the capacity to use 4GB of RAM, we recommend against utilizing the "Ultimate" series of upscales included later in this install, as a stability measure. Without the upscales, the Mac Appstore version should still be able to run the rest of the build without memory issues.

When attempting to run this executable, Windows/your antivirus might attempt to block it, because it's an unsigned executable. The patch is made by a trusted member of the community, I have used it myself and can vouch for it, and the source code is publicly available HERE--please feel assured that this program is safe, and you can override Windows/your AV and run it. When running 3C-FD, select your game's executable and choose to apply all patches except the borderless windowed patch (this patch is not inherently incompatible with the builds, but because it is currently experimental I recommend not using it).

### Water Restoration

Author: J

Description: Restores many of the water effects which were removed with the Aspyr patch.

Category & Tier: Bugfix, Graphics Improvement / 1 - Essential

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Stutter Fix

Name: Stutter Fix and Force Cage Update

Author: J

Description: Inexplicably, Force cages were given certain water texture properties which caused them to generate horrid visual lag whenever the player's camera faced their direction. This simple mod removes this property to resolve the issue, and also optionally enhances the force cage's model and texture

Category & Tier: Bugfix, Graphics Improvement / 1 - Essential

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

If you want both the fix and the new force cage model, download just forcecage_updated_1.1.0.zip

## Mod List

### 4GB Patcher

Author: Daniel Pistelli

Description: This is a program which modifies your game's executable file to be able to utilize up to 4 gigabytes of RAM, since KOTOR is a 32-bit application and by default can only utilize 2 gigabytes. This will help the game run smoothly even with the larger texture throughput this build creates, and also reduces issues caused by a memory leak which is native to the game, preventing crashes and savegame corruption.

Category & Tier: Patch / 1 - Essential

Installation Method: Executable

Installation Instructions

This step should only be performed if you are NOT on the Aspyr patch and have not applied J's 3C-FD patcher! 3C-FD applies this as part of its fixes to the Aspyr version. Just like 3C-FD, since the Mac Appstore version is not an executable, this program cannot be utilized on the Mac Appstore edition of the game.

### TSLRCM

Name: The Sith Lords Restored Content Mod

Author: zbyl2, DarthStoney, Hassat Hunter & VarsityPuppet

Description: The whole reason why you would mod KOTOR 2, the legendary mod that includes bugfixes, restored content, and altered scenes to return the game to what it was meant to be. This mod isn't just essential—it's mandatory.

Category & Tier: Bugfix, Immersion, Mechanics Change & Restored Content / 1 - Essential

Non-English Functionality: YES, but you must install the version of TSLRCM which corresponds to your language of choice.

Installation Method: Executable

Compatibility Warning

Recall that no version of M4-78 is compatible with this build; do not use it alongside TSLRCM! Remember also that the Workshop version of this or any other mod is NOT to be used!

### TSLRCM Tweak Pack

Name: TSLRCM Tweak Pack

Author: Pavijan (update by Fair Strides)

Description: Reverts and otherwise modifies certain TSLRCM settings to make the game more sensible and internally consistent (mostly unrestoring content which was likely intentionally cut by Obsidian). This includes removing Jedi Padawan Kaevee.

Masters: TSLRCM

Category & Tier: Patch / 1 - Essential

Non-English Functionality: PARTIAL - All components can be installed except for Trayus Mandalore Conversation.

Installation Method: Multi-Run TSLPatcher

Installation Instructions

Don't use the complete installer, instead selecting the individual component installer—this is critical for compatibility, not just to choose specific options. The installer for this mod will need to be run 6 times, once to install each of the options we'll be using: Kaevee Removal Parts 1 & 2, Saedhe's Head, Kreia-Atris Dialogue Tweak, Trayus Mandalore Conversation, and Trayus Sith Lord Masks. Most of the other options for this mod are untested with the mod builds, but will likely function--I still recommend against using them, since compatibility isn't 100% guaranteed, but you can try. The one exception is Atton at the End, which is completely incompatible with one of the most important mods in this build, which is a much more comprehensive implementation of Atton's dialogue at endgame.

### Silent Sion

Name: Silent Sion Restoration

Author: Emperor Devon

Description: Another TSLRCM change I am not personally thrilled with—when Sion encounters the Exile for the first time in TSLRCM on the Harbinger, he speaks to them a line of warning. That engagement takes so much tension out of the scene, and this mod reverts it to the vanilla standard: when Sion appears at the end of the hall he only slowly walks towards the party, threatening and menacing.

Masters: TSLRCM

Category & Tier: Immersion / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File

Installation Instructions

Move just 153sion.dlg to the override.

### Thematic KOTOR 2 Companions

Name: Thematic KOTOR 2 Companions

Author: Sniggles & JCarter426

Description: In the original KOTOR, BioWare did a few wacky things with character progression. Obsidian, not wanting to be outdone, decided to be even worse by just straight-up copying the statlines of several KOTOR companions for KOTOR 2 companions!

As part of the Thematic series of mods by JC and myself, Thematic KOTOR 2 Companions is a rework of companion starting attributes, skills, feats and powers to lock all companions into having the exact amount of bonuses they should for their level, as well as re-tuning several of their setups to more closely match the characters' behaviors and backstories. The goal is to ensure that companions' statlines are not only thematically consistent, but also broadly mechanically balanced, to better-incentivize the use of under-utilized party members.

Masters: TSLRCM

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher

Installation Instructions

If you would like to have Visas's class as Sith Assassin, install the "Standard + Sith Assassin Visas" option. Otherwise, simply install "Standard."

### Saving Throw Fixes

Name: Prestige Class Saving Throw Fixes

Author: Rovan

Description: For some reason, some prestige classes in KOTOR 2 have worse saving throws than their predecessor class. This mod fixes this, ensuring that the more advanced classes always have slightly better throws.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

I advise users to only install the Jedi Master/Sith Lord fixes.

### Classic Class Attack

Name: Classic Class Attack Bonus

Author: Crimson Knight

Description: In the original KOTOR, Soldier and Guardian classes had a 1:1 attack bonus; they gained +1 attack for every 1 level. For every other class, this ratio was 0.75:1, which made classes feel different ingame and contributed to a difficulty scale. For unknown reasons, KOTOR 2 did away with this differentiation and gave every class a 1:1 ratio. This mod reverts this, giving classes which aren't explicitly melee-focused the 0.75:1 ratio back, restoring a bit of difficulty to what is otherwise a very easy game.

Category & Tier: Mechanics Change / 1 - Essential

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

Strongly recommend downloading the standard option. Weaker Consulars can be extremely punishing, so if you choose this option be aware that you're in for a rough time at some places!

### Robes with Shadows

Name: Robes with Shadows for TSL

Author: PapaZinos

Description: What use, I ask you, is the Soft Shadows setting ingame if your shadows are still 10 polygons? This mod gives all robes proper shadows that match their models more directly, for that extra touch of realism.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Extract the mod. Ignore the included subfolders, and move just the files in the base folder to your override. Overwrite when prompted.

### K2CP

Name: K2 Community Patch

Author: Multiple; A Future Pilot, JCarter426 Curates

Description: A community bugfix and general improvement patch for KOTOR 2 meant to be a supplement to TSLRCM, fixing bugs that TSLRCM didn't catch or viewed too minor to address; highly recommended.

Masters: TSLRCM

Category & Tier: Graphics Improvement & Bugfix / 1 - Essential

Non-English Functionality: NO

Installation Method: TSLPatcher Mod

Installation Instructions

Remember, warnings during an installation are normal—errors are not.

### Remote Tells Influence

Name: Remote Tells Influence and PATCH

Author: tk102, patch by Leilukin

Description: This is going to require a bit of an explanation. I personally believe this mod breaks immersion—so why add it? The bottom-line is that KOTOR 2's influence system is asinine, and major plot points are locked behind very high—or very low—influence with several companions. Your influence with companions is also extremely obfuscated, only really discernible by their LS/DS alignment relative to your alignment, but this is a very buggy system which breaks with high Charisma values on the player, or with certain feats. There is no other way ingame to tell what your influence is without this mod. I mark it as optional because it does break immersion, but for many players I think having a system to determine influence will be extremely important for enjoyment of the story.

Category & Tier: Mechanics Change / 4 - Optional

Non-English Functionality: NO

Installation Method: Loose-File Mod & Patch

### Better Czerka Salvager

Name: Fixed / Better Czerka Salvager

Author: OzilsEyes

Description: I'm betting that anyone who has played KOTOR 2 before will remember that fucking son of a bitch Czerka salvager in the Telos military base. His pathfinding would make the most awful escort quest target in history blush in embarassment. This mod by Ozils is an outright lifesaver, applying much-enhanced follower logic to the salvager and automatically teleporting him to the party if you get too far away. No more 20 minutes of painstakingly dragging his ass through the base!

Category & Tier: Bugfix / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher

### Ultimate HR Textures

Name: Ultimate High Resolution Texture Pack

Author: ShiningRedHD

Description: In addition to fully upscaling all of the NPCs and equipment in the game, ShiningRed has also produced a series of mods using the same techniques that update all the planet textures. This mod upscales the two prologue worlds, as well as some interior modules.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Download the .tpc variant of the mod. Delete PER_Gr01.tpc, TEL_rock.tpc through TEL_rock07.tpc, and TEL_wl05.tpc before moving the files to your override folder.

### Ultimate Character Overhaul

Name: Ultimate Character Overhaul

Author: ShiningRedHD

Description: Ultimate Character Overhaul is a comprehensive AI-upscale of every character and piece of equipment in the game. Unlike previous AI upscales, the Ultimate series has no transparency problems while still retaining reflections on character textures, all without any additional steps required. This is an incredibly high-quality mod, and ShiningRed has even gone through the trouble to make compatibility patches for the mod builds, upscaling some of the later textures we use!

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

For now, ignore all patches (those will be installed later). Just download the main package, either Full or Lite based on your quality preferences (I recommend the 2x version, as the graphical differences are minimal). Just MAKE SURE they are in .tpc file format! Delete N_OndSoldMH1.tpc, PMBJ02.tpc, PMHC03.tpc, PMHC03D1.tpc, and PMHC03D2.tpc before moving to override.

### Ultimate Nar Shaddaa

Name: Ultimate Nar Shaddaa High Resolution

Author: ShiningRedHD

Description: This mod upscales the steel jungle of Nar Shaddaa.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Download the .tpc variant of the mod. Delete NAR_fl01.tpc, NAR_Met4.tpc, and NAR_wl07.tpc before moving to your Override.

### Ultimate Dantooine

Name: Ultimate Dantooine High Resolution

Author: ShiningRedHD

Description: This mod upscales the ravaged agrarian world of Dantooine.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Download the .tpc variant of the mod. Delete DAN_birds.tpc, DAN_MWFl.tpc, DAN_NEW1.tpc and DAN_wall03.tpc before moving to your Override.

### Ultimate Dxun

Name: Ultimate Dxun High Resolution and Patch

Author: ShiningRedHD

Description: This mod upscales the mysterious jungles of Dxun, the heart of the Mandalorian crusade.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Download the .tpc variant of the mod. Delete DXN_BWa1 through DXN_BWa8.tpc before moving the files to your override folder, then apply the patch.

### Ultimate Onderon

Name: Ultimate Onderon High Resolution

Author: ShiningRedHD

Description: This mod upscales the great city of Iziz, sole city of Onderon.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Download the .tpc variant of the mod. Delete OND_dor1.tpc & OND_dor3.tpc before moving to your Override.

### Ultimate Korriban

Name: Ultimate Korriban High Resolution

Author: ShiningRedHD

Description: This mod upscales the Sith world of Korriban.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Download the .tpc variant of the mod. Delete KOR_water01.tpc before moving the files to your override folder.

### Ultimate Malachor

Name: Ultimate Malachor High Resolution

Author: ShiningRedHD

Description: This mod upscales Malachor, where it all begins and ends.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Download the .tpc variant of the mod. For this mod only, do not overwrite if prompted!

### Korriban Sith Art

Name: Korriban Sith Art HD

Author: MessesWithWolves

Description: Though this mod is for K1, I've tested it with K2 and it not only works, it looks fine even with K2's drastically different atmosphere on Korriban. It dramatically improves the aesthetics of the artistic panels on the floor of the Korriban academy and the inset murals on the doors.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Download and install both files.

### Ritual and Water Textures

Name: Ritual and Water Textures

Author: Nehua

Description: A very nice retexture of the ritual "dome" during the party assault on Freedon Nadd's tomb. Note that the water texture from this mod is not (and cannot be) used for game stability, due to the way the mod is packaged; we will only use the ritual texture.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Move only the files from the ritual folder, not the water.

### PFHC06 Fix

Author: Ferc Kast

Description: Fixes a small grey area on the head of the sixth caucasian female head.

Category & Tier: Bugfix / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Droid Special Weapons Fix

Name: Droid Special Weapons Fix

Author: LoneWanderer

Description: Several droid special weapons have inaccurate descriptions or plain wrong damage and effect values listed. This mod fixes all that up to make the descriptions of the items and their effects match what they actually do ingame.

Category & Tier: Bugfix / 2 - Recommended

Non-English Functionality: NO

Installation Method: TSLPatcher Mod

Download Instructions

Use the V2 version of the mod.

### Honest Merchant

Author: TK-664

Description: For those gluttons for punishment among us, this mod reverts the inventory of Dendis Dobo, the honest but struggling merchant on Citadel Station, to its pre-TSLRCM state. In TSLRCM all merchants have improved inventories, but with this mod Dendis has only a pittance of poor goods, to represent the cost of his honesty and exactly how much his business is struggling compared to his brother.

Masters: TSLRCM

Category & Tier: Immersion / 4 - Optional

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

### JC's Minor Fixes

Name: JC's Minor Fixes

Author: JCarter426

Description: This collection from JCarter includes many small fixes, upscaled textures, and other helpful improvements too small to warrant their own mod, but well worth it when compiled as it is here.

Category & Tier: Bugfix & Graphics Improvement & Immersion / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

If you are using the K2 Community Patch, install the contents of every folder but Straight Fixes (that was already in the K2CP). Delete DXn_grassfx.tga and DXN_jungfx.tga in Aesthetic Improvements before moving files to override.

### Maintenance Reskin

Name: Maintenance Officer Realistic Reskin

Author: Malkior

Description: The poor maintenance officer on Peragus makes it very clear that he was shot several times before he died. This reskin gives him the burn wounds from the mining droid lasers.

Category & Tier: Immersion / 4 - Optional

Non-English Functionality: YES

Installation Method: Loose-File Mod

### HD Sign Placeable

Author: Dark Hope

Description: If you used the KOTOR build, you may remember some of the following mods. They're all made for the first game, but work just fine when ported into the second; there'll be many mods like this throughout the build. This specific mod dramatically improves the sign placeables found throughout the game. There's no side-by-side but trust me, this looks worlds better.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### HD Kiosk

Name: Kiosk HD and Patch

Author: Dark Hope, patch by Marcus Ramikin

Description: Although again there's no side-by-side, this mod is a huge improvement over the default kiosk texture, which just looks vaguely like there's junk strewn over it.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Download the "Kiosk HD 15.03.2024" version.

### HD Desk

Author: Dark Hope

Description: Still no side-by-side, but I promise, it looks much better than the default.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Hires Beam Effects

Name: Hi-Res Beam Effects

Author: InSidious

Description: Improves the in-game beam and lightning effect quality. The still screenshots don't do this mod justice; the live effects are great.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Fire and Ice HD

Name: Fire And Ice HD

Author: Cinder Skye

Description: Improves the resolution and textures of fire and ice effects in the game.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Animated Energy Shields

Name: Animated Energy Shields

Author: Dark Hope

Description: Improves and adds more nuanced animations to all the energy shields in the game. Yes, despite this mod being listed for KOTOR, it works for KOTOR 2 also!

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Blaster Visual Effects

Name: Blaster Visual Effects

Author: JCarter426

Description: Improves the quality of the various blaster bolt types seen in the game (except sonic).

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Move everything from the override folder to your game's override directory, unless you would like yellow/green disruptors, in which case those files should be moved from the optional folder after moving all loose files to the override first.

### Head Model Fixes

Name: TSL Head Model Fixes

Author: Redrob41

Description: Fixes some glitched head models present in the game.

Category & Tier: Bugfix / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### New Holocron Textures

Name: New Holocron Textures

Author: Sith Holocron

Description: Retextures the Sith Holocrons present in Atris's meditation chamber on Telos and gives them a pulsing animation.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Thigh-High Boots for Twi'lek

Name: Thigh-High Boots for Twi'lek

Author: DarthParametric

Description: In vanilla, Twi'lek thigh-high boots are painted on the character's texture rather than added to their equipped clothing as an item, which would give them realistic three-dimensional depth and higher resolution. This mod adds the boots as an equipped object, increasing the quality and appearance of the boot textures.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Ignore the "Player Bodies" folder. Go into the NPC Replacement folder and move all the loose files to the override directory. Ignore the optional folder.

### Better Twi'lek Heads

Name: Better Twi'lek Heads

Author: Ashton Scorpius

Description: Unlike female Twi'lek, male Twi'lek are supposed to have ears. Operative word: supposed to. Their ears are barely visible at all in KOTOR, which this mod fixes using TOR ear geometry.

Category & Tier: Appearance Change / 3 - Suggested

Non-English Functionality: YES

Installation Method: HoloPatcher Mod

Installation Instructions

Choose whether to use the slim or original necks; your preference.

### Twi'lek Male Diversity

Name: TSL Twi'lek Male NPC Diversity

Author: Leilukin

Description: Most of the Twi'lek textures used in the game are very samey, ultimately boiling down to just a few alternate textures. This mod makes use of alternate textures created as a modder's resource to significantly differentiate the appearances of many unique Twi'lek NPCs.

Category & Tier: Appearance Change / 3 - Suggested

Non-English Functionality: YES

Installation Method: HoloPatcher Mod

Installation Instructions

Run the installer first. Once completed, move the upscaled textures to override (if desired). If using the original necks option of Ashton Scorpius's Better Twi'lek Males mod, also ensure to move the files in the "Optional - Original Necks" folder.

### Dahnis Unique Look

Name: Dahnis Unique Look

Author: Leilukin

Description: Female Twi'lek have the same appearance differentiation problem the males do, but unlike the males I find most of the alternate textures out there for females a bit too flashy for my taste. This is a somewhat subdued change more in line with vanilla, but still giving a unique look which makes the NPC Dahnis stand out.

Category & Tier: Appearance Change / 3 - Suggested

Non-English Functionality: YES

Installation Method: HoloPatcher Mod

Installation Instructions

The use of the upscaled textures or not is your preference, though I recommend them.

### Darth Malak's Armor

Name: Darth Malak's Armor

Author: DarthParametric

Description: In KOTOR 2, Darth Malak's armor is an incredibly rare late-game drop not available through normal means without TSLRCM. Unfortunately, in-game the armor doesn't look like Malak's at all. This mod fixes that.

Category & Tier: Graphics Improvement & Immersion / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Supermodel Fix

Author: JCarter426

Description: Fixes several model errors present due to misnamed animations and misplaced weapon positions.

Category & Tier: Bugfix / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Known Bugs

This mod will cause female models wielding blaster rifles or blaster repeaters to run... well, strangely, with the rifle merged into their torso, if using the "K1 running" option. This is a known bug which JC is looking to fix, but for now as long as you aren't playing a woman wielding a rifle or repeater, you should never notice it. If you do intend on doing a female rifle-wielder run, make sure not to choose the K1-style running.

Installation Instructions

Move the files from the override folder to your override directory. Optionally, if you prefer the aesthetic of the K1 running animation, you can also move the files from the 'K1 Style Running' folder to your override and overwrite.

### Worn-Out Mando Armor

Name: Mandalorian Worn-Out Armor

Author: LordRevan999

Description: Makes in-game Mandalorian armor appear realistically worn out. It's been 10 years since the Mandalorian Wars and the last time the Mandalorians had a state, and their suits should reflect that.

Category & Tier: Appearance Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### HD NPC Portraits

Name: HD NPC Portraits

Author: ndix UR

Description: An extremely high-quality series of NPC portraits done in vanilla style, perfect for those intending to play the game at higher resolutions. Cleans up a lot of problems with the low-quality vanilla versions.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### HD PC Portraits

Name: HD PC Portraits

Author: ndix UR

Description: Ditto the above, except this time it's ndix giving us a series of upscaled portraits for the player character.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Stylized Portraits TSL

Name: Stylized Portraits TSL

Author: Tinman888 & Sdub

Description: If you'd prefer stylized art of the KOTOR 2 party instead of the above party portraits, this mod is an alternate option to the above.

Category & Tier: Appearance Change / 4 - Option

Non-English Functionality: YES

Installation Method: Loose-File Mod

### PFHC03 Restoration

Author: Effix

Description: For some reason, this head is included in vanilla but not enabled due to missing Dark Side transitions. This mod adds the transitions and re-enables the head as an option.

Category & Tier: Restored Content / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher

Installation Instructions

Recommend the 'zombie-like' appearance, as it matches vanilla DS transitions.

### 4K Atton

Author: Constar

Description: An extremely high-resolution Atton texture, including his default clothing.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Download the .tpc version of the mod.

### Atton with Scruff

Name: Atton Rand with Scruff

Author: felixfelicitas

Description: Reskins Atton's head model to have a bit of scruff. Fits his character very nicely.

Category & Tier: Appearance Change / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Ignore the MacOS folder, only move the .TGA files.

### Rounder G0-T0

Author: dirigibalistic

Description: The definition of "round" was apparently different in 2003. Instead of an awful hexagonal abomination, this mod properly makes the G0-T0 droid spherical, significantly improving its appearance ingame.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Dark Harbinger

Author: NiuHaka

Description: Reskins a player head to look better, as well as having a unique Dark Side transformation.

Category & Tier: Appearance Change, Bugfix & Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Choose which version of the head model you would like to use from the screenshots provided. Place the files from the corresponding folder into your override.

### HD Vrook

Author: Dark Hope

Description: Though this is a KOTOR mod, it also works with KOTOR 2. This mod reskins Vrook to look older, more worn-down and more pale—a fitting vision of the Jedi Master after years of hiding on Dantooine, and witnessing the destruction of almost the entire Jedi Order.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### HD Darth Nihilus

Name: HD Darth Nihilus

Author: Darth_Sapiens

Description: This mod improves the resolution of Darth Nihilus and adds some extra color to his mask, like it appeared in the concept art.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Download and install the .tpc filetype version: it is nearly lossless, much more performant due to the smaller size, and critically fixes several issues caused by the very large texture size of the original upload.

### Darth Sion Remake

Name: Darth Sion Remake

Author: FF97

Description: This mod improves the resolution of Sion's base textures, as well as adding detail to his wounds and atrophied eye. The screenshots don't do it proper justice; this is a well-executed improvement.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Install the files within the Override folder.

### Detran's Revan

Author: Detran

Description: This mod by Detran (the same one used in the KOTOR build) improves Revan's texture for the Tomb of Ludo Kressh on Korriban, standardizing the appearance across the two games.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Lore-Friendly Mandalore

Name: Lore-Friendly Mandalore Mask

Author: constantinople33

Description: In old Legends canon, Mandalore's mask looks nothing at all like what it looks like ingame. This mod is an attempt to rectify this while still keeping the majority of Mandalore's aesthetic intact. It's a well-executed attempt in my opinion, though I recommend using the all-silver version, I don't really think it works otherwise.

Category & Tier: Immersion & Appearance Change / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Visas Reduced Model Clipping

Name: Visas Reduced Model Clipping

Author: Feeling

Description: Visas's chin REALLY likes to atomically fuse with her weird Sith turtleneck. This mod does the best it can to separate the two.

Category & Tier: Bugfix / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Either option is fine, choose which you prefer based on the screenshots.

### HD Visas Marr

Name: HD Visas Marr

Author: Darth_Sapiens

Description: This mod improves the resolution of Visas Marr and adds some additional detail to both her LS and DS transformations.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Only move the four .dds filetype files to your override, ignore the remainder. If installing this mod alongside either the K2CP or Ultimate Character Overhaul, P_VisasBB.tpc, P_VisasH01.tpc/.tga, P_VisasHD01.tpc/.tga, and P_VisasHD02.tpc/.tga must be deleted from the override so this mod's .dds-based files will load.

### Mira Unpoofed

Author: Ashton Scorpius

Description: Let's be honest, Mira has a Karen cut. This mod resolves her deeply internalized desire to ask for the manager.

Category & Tier: Appearance Change / 4 - Optional

Non-English Functionality: YES

Installation Method: Loose-File Mod

### T3M4 HD 2K

Name: T3M4 HD 2K

Author: Darth_Sapiens

Description: A high-quality up-res of T3.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

If present in your Override, P_t3m4_01.tpc/.tga must be deleted so this mod's .dds-based files will load.

### HD Astromechs

Name: Refurbished Astromech Droids

Author: Darth_Sapiens and Sith Holocron

Description: Utilizes the above reskin of T3 as a base for the appearance of all other astromech droids in the game.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### War Droid HD

Name: War Droid Mk.1

Author: Dark Hope

Description: Reskins the war droids to HD editions. Although this mod is listed for KOTOR, it also works for KOTOR 2.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### HD Duros

Name: Duros HD and Patch

Author: Dark Hope

Description: This mod in particular is super high-quality and one of my favorites.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Before moving the files to override, make a copy of N_Duros02.tga and paste it into the same location you extracted the mod to. On a Windows OS, this should create a file called "N_Duros02 - Copy.tga". Rename this file to "N_Duros04.tga". Repeat this process with the file N_Duros03.tga, creating two copies and naming them N_Duros05.tga and N_Duros06.tga. When you move all the files to the override, you should be moving eight total. Following this, download and install the patch.

### HD Ithorians

Author: Dark Hope

Description: This another one of my Dark Hope favorites, the Ithorians look absolutely excellent ingame.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Similar to the above, make a copy of C_Ithorian03.tga and rename it to N_Chodo.tga. You should move a total of 5 files to the override.

### N_oldAMH01 HD

Author: Dark Hope

Description: Dark Hope once again striking with the worst-named mods in history.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Canonical Jedi Exile

Name: Canonical Jedi Exile

Author: michaelfung2000

Description: Reskins one of the player heads to match the canon appearance of the main character.

Category & Tier: Appearance Change / 4 - Optional

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Lightsaber Visual Effects

Name: JC's Lightsaber Visual Effects

Author: JCarter426

Description: This mod by JC significantly upscales the texture of lightsaber blades, in the process making them much sharper and brighter, while also recoloring the various color crystals to make them appear more naturally like the color they're meant to represent

Category & Tier: Appearance Change & Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Recommend using the default without optional textures, because J's mod below uses the defaults as its base for its reflections.

### Lightsaber Hilt Variety

Name: Enhanced Lightsaber Hilt Variety

Author: J

Description: J's mod here is some more black magic fuckery, finding out a way to make lightsaber "textures" (of a sort) apply both on the legacy patch and Aspyr patch versions of the game, when they were previously broken on the latter. What's more, he's made this mod compatible with JC's lightsaber visual effects above, and also added Crazy34's lightsaber reflections to his sabers, so your (and enemy!) lightsabers will dynamically reflect off of nearby surfaces. The model differentiations here aren't groundbreaking, but they're better than vanilla where all the sabers are entirely the same, especially when you see just how great the reflections look ingame.

Masters: 3C-FD or a non-Aspyr game version

Category & Tier: Appearance Change & Graphics Improvement / 1 - Essential

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

If you don't like the little semi-random bronze pieces on each lightsaber hilt, download w_lghtsbr_001.tga and move that file to your override after installing the base version of the mod

### Random Panels

Name: KOTOR 2 Random Panels

Author: Sith Holocron

Description: Reskins and sharpens several ingame computer panels without losing sight of the vanilla aesthetic.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Neglected Computer Panel

Name: Neglected Computer Panel

Author: ScrierOne

Description: There's a computer panel which, for years, hasn't been involved in any various texture improvement mods. This mod closes that gap.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Download only the Damaged version, it contains files for both.

### Improved Peragus Asteroids

Name: Improved Peragus Asteroids

Author: VasiliiZaytsev

Description: Reskins some criminally low-res asteroids on Peragus to an acceptable resolution.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Swoop Monitor Replacement

Name: Swoop Monitor Replacement Pack

Author: Sith Holocron

Description: For being a fast-paced sport, the swoop monitors sure were lovely still-frames. This mod fixes that by animating them into a slideshow.

Category & Tier: Immersion / 4 - Optional

Non-English Functionality: YES

Installation Method: Loose-File Mod

### HD Terminal Texture

Author: Dark Hope

Description: This mod significantly improves and animates the computer panel texture. Although it's listed for KOTOR, it also works in KOTOR 2.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Which version to use is up to you; the first image on the mod page is the version of the texture from the "PLC_CompPnl(Vurt's KotOR Visual Resurgence) 2026.rar" download, the second image is the original download, and the third image is the vanilla terminal texture.

Installation Instructions

The files in this mod need to be duplicated and those duplicates renamed. On a Windows OS you can do this by copying the two files within and pasting them into the same folder which you copied them from. This should create duplicates of the original files with a slightly different filename (such as PLC_CompPnl-COPY). Take the two copied files and rename them to PLC_CompPnl_b, retaining their original file extensions (that means the .tga filetype file should be the larger one!). When the files are moved to the override, you should be moving four files: PLC_CompPnl.tga, PLC_CompPnl.txi, PLC_CompPnl_b.tga, and PLC_CompPnl_b.txi

### Animated Computer Panel

Name: Animated Computer Panel

Author: ebmar

Description: Switches the computer panels on Citadel Station to be animated, as well as (optionally) having a transparent hologram display. This gives them a unique appearance compared to the textures impacted by the above mod.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

Recommend solid options. Do NOT use experimental install options!

### HD Workbench

Author: Dark Hope

Description: Upgrades the visuals of the game's crafting benches and lab stations, and also animates the computer interface terminal they have.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Backdrop Improvements

Name: TSL Backdrop Improvements

Author: Kexikus

Description: The goal of this mod is to renovate the backdrops visible from the bridges of various ships as seen in the game, which it accomplishes with stellar results (no pun intended).

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Install the files from the included Override directory only. Delete all the files beginning with "leh_". There should be four.

### Citadel Station Backdrop

Name: JC's Citadel Station Backdrop

Author: JCarter426

Description: With all of the incredible modding capabilities arrayed before us, none of the other mods actually fix the horrid Citadel Station backdrop textures. Blessedly, JC's got our backs.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### HQ Skyboxes II

Name: HQ Skyboxes II

Author: Kexikus

Description: Dramatically improves the quality of every single skybox in the game—one of the more impressive graphics mods ever released.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Download Instructions

Download just the main file (HQSkyboxesII_TSL.7z or HQSkyboxesII_TSL_1k.7z), not any of the compatches. Whether you use the 1k version is your choice or not; on older systems, the 1k version will give better performance.

Installation Instructions

After this mod has finished installing, if you are using the K2CP, extract the K2CP once again—do not re-run its TSLPatcher! Open the TSLPatchdata folder and move 231teld.mdl and 231teld.mdx to your override, and overwrite when prompted.

### Ebon Hawk Cockpit Upgrade I

Name: Ebon Hawk Cockpit Upgrade (LEH_Scre01)

Author: Sith Holocron

Description: Cleans up and enhances the cockpit terminal texture without making it garish or too busy.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Ebon Hawk Cockpit Upgrade II

Name: Ebon Hawk Cockpit Upgrade (LEH_Scre02)

Author: Sith Holocron

Description: Similar treatment to the above for the various other computer panels on the ship.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Recommend the version without overlays, but it's personal preference.

### Spark Effect

Name: Spark Effect - Ebon Hawk

Author: PapaZinos

Description: You won't notice it until you see the side-by-side, but the vanilla spark effect is AWFUL. This mod is an incredible improvement on the base effect.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Transparent Cockpit Windows

Name: Transparent Cockpit Windows TSL

Author: WildKarrde

Description: By default the Ebon Hawk's cockpit is opaque when seen from outside the ship. This mod fixes that, letting you see into the cockpit from the outside.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Download Instructions

I recommend the reskin-friendly version; the reflectivity version is untested with the builds and may look odd.

Installation Instructions

Apply the main installation, then go into the Compatibility Patches folder and apply any of the following you use in this order: K2CP ("K2CP and_or Nar Shaddaa Landing Pad Repair by PapaZinos"), "High Quality Skyboxes II by Kexikus" (but NOT the 'M4-78 with HQ Skyboxes II' folder!), and "Spark Effect - Ebon Hawk by PapaZinos". DO NOT USE the content of the "Korriban Distorted Model Fix" folder, even if you are on the Aspyr patch! With the 3C-FD patcher, this is no longer a concern.

### HD Cockpit Skyboxes

Name: HD Cockpit Skyboxes

Author: tjsase

Description: Vastly improves the quality of exterior areas as viewed from the cockpit of the Ebon Hawk.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Download the version of the mod you would like based upon how detailed you want the cockpit skyboxes to appear. Keep in mind that load time into the Ebon Hawk could be significantly increased if using very large cockpit textures, and there's also a risk of save corruption when using very large texture sizes—I recommend the Medium texture option for the best balance of quality and size/performance. Whatever size you decide to use, make sure to use the .tpc format files!

Installation Instructions

Move the loose files to the override, followed by the file in the "With Nar Realistic Skybox" folder if you use HQ Skyboxes II.

### FTL Loop

Name: FTL Hyperspace Loop

Author: Nym182

Description: A beautiful animated texture of traveling in hyperspace. Check out the video on the mod's download page to see it in motion.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Aleema Keto's Robe Fix

Name: Aleema Keto's Robe Description Correction

Author: milestails

Description: Aleema Keto's Robe, as described in KOTOR 2, doesn't fit with the old EU. This mod fixes the robe description to match Keto's actual backstory.

Category & Tier: Immersion / 4 - Optional

Non-English Functionality: NO

Installation Method: Loose-File Mod

Download Instructions

Download the larger of the two files.

### Short-Term Memory Fix

Name: Vaklu Short-Term Memory Fix

Author: SuperChameau

Description: In your dialogue with Vaklu when supporting him as your candidate for the Onderon throne, he virtually repeats himself verbatim in two sentences back-to-back. I see some grey on those temples man, but it's a bit too early for dementia. This mod tweaks his dialogue so he says something meaningfully different. This change is fully voiced using original voice work thanks to dialogue splicing.

Category & Tier: Immersion / 3 - Suggested

Non-English Functionality: NO

Installation Method: HoloPatcher Mod

Installation Instructions

Recommend Drew's fix, as it preserves more of the original dialogue.

### Onderon News Makes Sense

Name: Onderon News Make Sense

Author: SuperChameau

Description: In vanilla, there's either a misrecorded or miswritten line which refers to Telos Station as "Peragus Station." This mod fixes the issue by splicing dialogue from the same news holo so that the anchor says "Telos Station" both times.

Category & Tier: Bugfix / 3 - Suggested

Non-English Functionality: NO

Installation Method: HoloPatcher Mod

### Hawk Downloadable Map

Name: Ebon Hawk Downloadable Map

Author: Ashton Scorpius

Description: If you fall into that group of people that gets really annoyed with all the fog on the edges of the Ebon Hawk minimap, boy howdy do I have a mod for you.

Category & Tier: Immersion / 4 - Optional

Non-English Functionality: NO

Installation Method: HoloPatcher Mod

### Kinrath Egg Crystal Drop

Name: Kinrath Egg Bash Crystal Drop Fix

Author: Hassat Hunter

Description: In KOTOR, bashing Kinrath eggs had a chance to result in red lightsaber crystals. This mod adds the same chance to KOTOR 2's Kinrath eggs.

Category & Tier: Immersion / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Thorium Charge Mod

Name: Thorium Charge Mod

Author: darth_gil

Description: Thorium charges are used to blow open heavily-fortified rooms, of which there are two in the game. There are also two thorium charges in the vanilla game—however. One of them is hidden behind completing a quest and then re-checking the merchant Akkere's inventory (which you are never informed has updated), and Akkere can die during the fighting on Dantooine. I recommend using this mod, which adds a thorium charge on Dxun (where it most logically would be), just to save a little bit of hassle.

Category & Tier: Added Content / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Kill the Ithorian

Name: Kill the Ithorian

Author: Markus Ramikin

Description: This mod allows you to kill the Ithorian on Citadel Staton you were previously only able to save.

Category & Tier: Immersion / 3 - Suggested

Non-English Functionality: NO

Installation Method: Loose-File Mod

### Red Floating Lightsabers

Name: Red Floating Lightsabers

Author: Crimson Knight

Description: At the end of the game, the lightsabers Kreia attacks you with are purple, and also treated ingame as organic—that is, not immune to fear, paralysis or stun. This mod makes them immune to those statuses and also makes them red—logically, red is what Kreia would've had on hand.

Category & Tier: Appearance Change & Bugfix / 2 - Recommended

Non-English Functionality: NO

Installation Method: Loose-File Mod

### Mira or Hanharr

Name: Choose Mira or Hanharr

Author: Hassat Hunter

Description: This mod lets you choose whether you want to have Mira or Hanharr as your champion against Visquis in the Jekk'Jekk Tar, regardless of Dark or Light alignment. This means you can have Hanharr while LS, or vice-versa with Mira.

Category & Tier: Immersion & Mechanics Change / 1 - Essential

Non-English Functionality: NO

Installation Method: Loose-File Mod

### Kreia's Fall Cutscene

Name: Kreia's Fall Ingame Cutscene

Author: danil-ch

Description: Improves the quality of the cutscene which reveals Kreia's casting out of the Triumvirate by rendering it in-game rather than as a low-quality movie. Even if you intend to use the High-Resolution Cutscenes mod included under the extended widescreen support section at the bottom of this list, I still recommend this mod; they function basically the same, but by using this mod the characters in the cutscene will use their new textures if any are applied, thereby making it more immersive and further increasing its quality above that of the pre-recorded cutscene, which relies on vanilla textures.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: PARTIAL - Compatibility with English, French and German

Installation Method: TSLPatcher Mod

### PartySwap

Author: DarthTyren & Leilukin

Description: PartySwap is a legendary mod, and one of the best ever made for KOTOR 2. Through some unobtrusive scripting magic, it allows you to bring both the Disciple and Handmaiden with you as companions simultaneously, without removing another companion from the party wheel. You can experience all their content and use them both throughout the entire campaign, as was originally intended by Obsidian.

The mod gives you a communicator which you use to contact the Ebon Hawk to manage the Handmaiden and Disciple, which you can use while in the overworld. This can be used to switch which of the two companions is currently in the party, or even have both Disciple and the Handmaiden at once if you so choose. This communicator appears in the shields quickbar at the bottom-left of the screen as soon as the player has both the Handmaiden and Disciple as companions.

Masters: TSLRCM

Category & Tier: Immersion & Mechanics Change / 1 - Essential

Non-English Functionality: NO

Installation Method: HoloPatcher Mod

Installation Instructions

Run the HoloPatcher executable. Select the default install, not M4-78. When the install is completed, go into the "Compatibility Patches" folder, enter the one for Kreia's Fall Ingame Cutscene, and install that patch as well if you've chosen to use the above mod.

### Extended Enclave

Author: danil-ch & Darth Hayze

Description: Restores some additional content to the Dantooine Enclave, including more variance depending upon your influence with Kreia.

Masters: TSLRCM

Category & Tier: Restored Content / 1 - Essential

Non-English Functionality: English and Russian ONLY

Installation Method: TSLPatcher Mod

Steam Notes

This mod disables the ability to unlock certain achievements! Other mods might do the same, but this one has been confirmed incompatible with at least one achievement. I recommend it regardless, but if you care about achievements over story this mod won't be for you.

### PartySwap-EE Compatch

Name: PartySwap/Extended Enclave Compatch

Author: Leilukin

Description: PartySwap and Extended Enclave are both large mods that modify many of the same files, so they don't play well together natively. Thankfully, this compatch fixes all those issues and makes them completely compatible.

Masters: PartySwap + Extended Enclave

Category & Tier: Patch / 1 - Essential

Non-English Functionality: NO

Installation Method: TSLPatcher Mod

Installation Instructions

Use the 'No M4-78EP Installed' option. The 'Extended Enclave Tweaks' is a separate mod we don't use in this build.

### Extended Korriban Arrival

Name: Extended Korriban Arrival

Author: danil-ch

Description: Slightly extends the scene which plays out immediately before the Ebon Hawk touches down on Korriban.

Masters: TSLRCM

Category & Tier: Restored Content / 3 - Suggested

Non-English Functionality: NO

Installation Method: TSLPatcher Mod

### Prologue Item Recovery

Name: Prologue Item Recovery

Author: Leilukin

Description: A mod made at my request by Leilukin, for which I'm very grateful! Originally, even if you play the short prologue on the Ebon Hawk, you're not able to keep your items from that sequence, even though none of them are overpowered or unreasonable for that stage of the game. This mod gives them back to you, so long as T3 is able to breach the door to the security room in the hanger.

Category & Tier: Immersion / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

Use the TSLPatcher installer method (mandatory for compatibility), not the manual install.

### Balance Tweak Pack

Name: Balance Tweak Pack

Author: Pavijan

Description: Modifies an imbalanced power and removes an unused feat to prevent feat waste.

Masters: TSLRCM

Category & Tier: Mechanics Change & Patch / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

Run the installers for Force Crush Balance and Disable Droid Interface Feat. The other mod options are untested with the builds and I don't personally recommend them.

### Mines Overhaul

Author: offthegridmorty

Description: This is a really, really cool mod that does a lot of things: fixes basegame bugs with mines, restores several mine types to the game, rebalances the mines, and even lets enemy troops plant mines in combat whenever it's logical for them to do so!

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: NO

Installation Method: HoloPatcher Mod

Installation Instructions

Recommend running all three options, the main install first and then the two optionals.

### No Health Regeneration

Name: No Health Regeneration

Author: offthegridmorty

Description: Having health regen by default in KOTOR 2 is bad for many reasons: it makes an already easy game easier; it negates the benefit of the health regeneration feat they added; and it makes the use & crafting of medkits and other health-restoring tools much less useful. I played with this mod and the game was still pretty easy by the end, but it was WAY more fun early on. It made me have to worry about health and especially healing items for quite a bit more time, which preserved the challenge beyond Telos.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: HoloPatcher Mod

Installation Instructions

The option to gradually restore health while on the Ebon Hawk is compatible with the builds--if you'd like to use it, re-run the HoloPatcher and install that option after installing the base mod first.

### Thematic Sith Lords

Name: Thematic Sith Lords

Author: Sniggles & JCarter426

Description: The results of my first foray into KOTOR modding. Everybody knows that the Sith Lords in KOTOR 2 are disappointing as bosses—the goal of this mod is to make them thematically unique, so that fights against them are fun and that certain character builds excel at fighting some Lords and struggle against others.

Masters: TSLRCM

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

First choose your base install option, either Standard or No Force Rating (the latter removes the inherent DS alignment bonus to saves in some modules, which also gives an equivalent malus to LS characters in those modules). After this is installed, if you would like for Visas to ambush you as a Sith Assassin (which is a very difficult fight—remember, she will have Sneak Attack, and that fight forcibly stuns you), re-run the patcher and also install that option.

### Thematic Jedi Masters

Name: Thematic Jedi Masters

Author: Sniggles & JCarter426

Description: The much younger brother of Thematic Sith Lords, Thematic Jedi Masters was born of me discovering that all of the Masters except Atris have the same class (Guardian), and one doesn't even have any Force Points for your encounter! These fights were extremely half-baked, and that's where this mod comes in. Unlike Thematic Sith Lords the design philosophy was not of a rock-paper-scissors style (so you aren't going to find a tremendous glaring weakness with a Master as you might on a Lord) but rather to accentuate a given master's strengths in line with their respective natures.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

### Better Disciple Meditation

Name: Better Disciple Meditation

Author: offthegridmorty

Description: Let's be honest, Disciple kinda sucks. This mod makes him suck marginally less by making his meditation ability actually useful, so you might have some incentive to take him around with you at least some of the time, or bring him out to re-apply his meditation bonus.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: NO

Installation Method: TSLPatcher Mod

### Bao-Dur & Heavy Armor

Name: Bao-Dur Can Wear Heavy Armor

Author: Effix

Description: Bao-Dur gets more than a little screwed. He can't wear robes because they never made a model for them with his mechanical arm cutout, and he can't wear heavy armor because they didn't make a model for that, either. Especially with his stat distribution in mind, that makes him kind of garbage. This mod restores the ability to give him heavy armor, at least, which gives him at least a bit better utility.

Category & Tier: Bugfix & Immersion / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

### JC's Crystal Attunement

Name: JC's Crystal Attunement

Author: JCarter426

Description: For being a crystal which is specifically attuned to the player, your little pet rock wasn't very malleable. This mod adds a ton of different alignment levels for your player crystal based upon your alignment and class, fixes several bugs with the crystal, and also allows the player to attune the crystal at workbenches when Kreia isn't available.

Category & Tier: Mechanics Change, Bugfix & Immersion / 2 - Recommended

Non-English Functionality: NO

Installation Method: TSLPatcher

Installation Instructions

Practically speaking there are only two relevant install options: "Class-Based Properties + Workbench Attunement", which is the full package, or "Bug Fixes Only". If you don't like the mod's changes but would like to have its bugfixes, install the latter; otherwise, install the former.

### Jekk'Jekk'Tarr Thugs

Name: Better Jekk'Jekk'Tarr Thugs

Author: SuperChameau

Description: The thugs in the Jekk'Jekk'Tarr, despite many of them being bounty hunters in a seedy bar, for the most part didn't carry any weapons at all. This mod fixes that, giving the patrons weapons with which to defend themselves.

Category & Tier: Immersion / 2 - Recommended

Non-English Functionality: YES

Installation Method: HoloPatcher

Masters: TSLRCM

### Logical Jekk'Jekk Tarr

Author: N-DReW25

Description: The Jekk'Jekk'Tarr sequence in the vanilla game, and even in TSLRCM, has a lot of problems. The most glaring was the player's ability to enter the bar without suffering severe ill-effects, even though dialogue elsewhere suggested that even skin contact would be nearly instantly fatal. This mod resolves almost all of the idiosyncracies of the sequence in a very lore-friendly and logical way.

Category & Tier: Bugfix & Immersion / 2 - Recommended

Non-English Functionality: NO

Installation Method: HoloPatcher

Masters: TSLRCM

Installation Instructions

Install part 1 and then part 2, then, if using Better Jekk'Jekk Tarr Thugs, also install the compatch.

### Mira's Vanilla Escape

Name: Mira's Vanilla Escape for TSLRCM

Author: WildKarrde

Description: TSLRCM edits the scene with Mira escaping the tunnels beneath the Jekk'Jekk'Tarr, restoring one scene but, in the process, removing another. This mod restores the vanilla sequence while also keeping the dialogue restored by TSLRCM, and even adding in a few lines unrestored by TSLRCM in the process.

Category & Tier: Restored Content / 2 - Recommended

Non-English Functionality: NO

Installation Method: TSLPatcher

Masters: TSLRCM

Installation Instructions

I recommend the "With Additional Scene" option for maximum restored content and internal consistency for the sequence.

### Kreia's Longsword

Author: bead-v

Description: This mod fixes the sequence where Kreia is confronted with Sion on the Harbinger so that Kreia doesn't just pull a vibroblade out of nowhere. Instead, she reaches down and picks up a longsword from the nearby corpse of a Republic soldier, more sensibly.

Category & Tier: Immersion / 3 - Suggested

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

The lightsaber version of this mod is broken when used alongside the mod builds, so please ensure to use specifically the longsword version.

### Harbinger Performance Fix

Name: Harbinger Arrival Performance Enhancement

Author: PapaZinos

Description: The Harbinger cutscene sequence is absolutely the most ambitious cutscene the game ever attempts. With multiple module loads, split .bik movie cutscenes, panning and circling cameras, the arrival of the Harbinger is one of the most impressive visual sequences in the entire game, at least from a technical perspective. But my god, the engine does not like it. This mod tightens everything to do with the sequence up and makes the entire thing much more stable, less prone to crashes or forced minimizations on modern hardware.

Category & Tier: Bugfix / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

Use the installer version.

### Visas VO Tweak

Name: Visas Voiceover Tweak

Author: danil-ch

Description: Replaces lower-quality in-game dialogue in the initial Ravager scene between Nihilus and Visas with versions from one of the game's pre-rendered cutscenes.

Masters: TSLRCM

Category & Tier: Immersion / 3 - Suggested

Non-English Functionality: NO

Installation Method: Loose-File Mod

### Sion and a Male Exile

Name: Darth Sion and the Male Exile

Author: Leilukin

Description: I find that Sion's character comes off completely one-dimensional, ruining both his own interesting character arc and his relationship with Kreia, unless he can express a feeling of sentimentality for the Exile. Unfortunately this is conveyed via a romantic obsession between Sion and the player, and by default this is locked to a female Exile. In the interests of presenting Sion at his best as a character, this mod causes Sion to become obsessed with the player regardless of their gender.

Masters: TSLRCM

Category & Tier: Immersion / 2 - Recommended

Non-English Functionality: NO

Installation Method: HoloPatcher Mod

### Assassins with Lightsabers

Name: Sith Assassins with Lightsabers

Author: Lewok2007

Description: Replaces the Sith Assassins' default weaponry with lightsabers. It makes more sense that they would hunt the last of the Jedi with weapons meant to face them. Be ready for a hard slog through the Harbinger if you choose to use this mod.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: HoloPatcher

### True Sith Assassins

Name: True Sith Assassins

Author: offthegridmorty

Description: By default, Sith Assassins aren't actually using stealth, and you don't actually roll Awareness to detect them. This mod not only gives the Sith Assassins some levels of Sneak Attack (they are literally stealthed assassins, after all) it also makes them actually use stealth just like the player would, and the party either needs to damage them or roll a successful Awareness check to detect them. As you may expect, this mod can be hard depending upon your character build, and also whether you're using Sith Assassins with Lightsabers. It does have an option to reduce the difficulty, though, which makes it more immersive and less about you getting your shit kicked in, if that's appealing to you.

Category & Tier: Mechanics Change & Immersion / 2 - Recommended

Non-English Functionality: YES

Installation Method: Multi-Run HoloPatcher

Installation Instructions

Unless you intend on playing a character with very high awareness, I recommend the reduced difficulty option if using Lewok's Sith Assassins with Lightsabers, because the damage will be ridiculous. Remember to also apply the compatch for Lewok's mod after running the main install option of your choice, if using it.

### For Mandalore!

Author: offthegridmorty

Description: Sensed a pattern with Morty and continually putting out banger mods yet? This is a neat one which gives the player the ability to summon Mandalorian reinforcements to support them. Normally this would be too overpowered and kind of out-of-place, but at my request Morty has graciously made an install option specifically for use in the mod builds which only adds the Mandalorian support squads to the Ravager, where it makes sense for them to be present and available to come assist the player.

Category & Tier: Mechanics Change & Immersion / 3 - Suggested

Non-English Functionality: NO

Installation Method: HoloPatcher

Installation Instructions

Strongly recommend using the Snigaroo Cut for balance and immersion's sake.

### Peragus Medical Bay Enhancement

Name: Peragus Medical Bay Enhancement

Author: WildKarrde

Description: A really neat mod which makes the Peragus medical bay a little bit more coherent—you and the miners are no longer expected to breathe bacta gel, for instance. A small mod on balance, but one of my favorites.

Category & Tier: Immersion / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher

Installation Instructions

When installing, for best internal consistency choose install options 1 or 2 (not underwear). Option 2 is technically the most consistent but the visual differentiation is very slight and likely would not be noticeable. When the install is completed, if you would like the miners to have burned skin (as they realistically would have had) move the files from the "OPTION - Burned Skin Textures" folder (or the upscaled textures therein) to your override.

### HQ Blasters

Name: High Quality Blasters

Author: Sithspecter

Description: Massively improves the appearance of almost all blaster weapons in-game without grossly altering their original design.

Category & Tier: Graphics Improvement / 1 - Essential

Non-English Functionality: PARTIAL - Some text will be blank or in English

Installation Method: TSLPatcher Mod

### Droid Model Fix

Name: Droid Models Animation Fix

Author: CaptainSpoque

Description: By default there are several droid NPCs in both KOTOR games which have no animations set for them to dodge, which causes them to just sort of freeze whenever the player attacks them but doesn't land a hit. I've noticed this for years and never realized what it was, but Spoque has put together a package here that adds the dodges to the droid models and fixes them freezing mid-fight.

Category & Tier: Bugfix / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Known Bugs

This mod breaks some death animations (droids stand straight up after death, for example). I only found this happening to a single droid, though, so I believe the fixes this mod provides outweighs the bugs, which have already been reported and hopefully will soon be fixed.

Installation Instructions

Move all the various files inside the 7 folders within the "TSL Version" folder, EXCEPT for "War Droid K1," if using HQ Blasters above.

### Quarterstaff Replacements

Name: Quarterstaff Replacement Pack

Author: DeadMan

Description: Replaces or retextures every quarterstaff in KOTOR 2, including the Guidon Beacon, restored in TSLRCM.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

### Upscaled Icon Overhaul

Name: AI Upscaled Icon Overhaul

Author: Pramod Marlon

Description: This mod uses AI upscaling to increase the resolution and quality of all of the over one thousand icons in the game, from Force powers to Feats to gear and lightsaber crystals.

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Delete if_finesse01.tga, if_finesse02.tga, iw_dblsbr_010.tga, iw_dsrptpstl_001.tga, iw_dsrptpstl_002.tga, iw_dsrptpstl_003.tga, iw_dsrptpstl_004.tga,iw_Lghtsbr_011.tga, iw_qtrstaff_001.tga, iw_qtrstaff_002.tga, and iw_qtrstaff_003.tga before moving the files to the override and overwriting any conflicts.

### Rescaled Trandoshans

Author: Schizo

Description: Canonically Trandoshans are huge, almost as tall as a Wookiee. This mod rescales them to the proper height.

Category & Tier: Immersion / 4 - Optional

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

### Visually Repair HK

Name: Visually Repair HK-47

Author: Kexikus

Description: This mod brings to KOTOR 2 the same concept that the original game had with repairing HK. In addition to moderately increasing HK's stats over the course of the repairs (a much-welcome improvement, as droids in KOTOR 2 tend to underperform anyway), this mod also allows you to visually see HK become his old self again as the HK-50 components are gradually integrated, fixed and painted-over. The mod even includes some excellent reskins of the HK-50s and -51s! AND it's fully-voiced in a seamless way using original game dialogue! You'll have to forgive me, this is like a wet dream for me, seriously.

Category & Tier: Added Content, Appearance Change & Immersion / 2 - Recommended

Non-English Functionality: NO

Installation Method: TSLPatcher Mod

Installation Instructions

Select between one of the two TSLRCM-compatible install options.

### Expanded Ending

Name: TSL Expanded Ending

Author: WildKarrde

Description: WildKarrde has quietly but consistently put out extremely high-quality content, but they really outdid themselves with this one. This is a comprehensive and incredibly professional rework of the ending sequence of KOTOR 2. TSLRCM is famous for tightening the ending up, but it still leaves several plot holes flapping in the wind--how does Malachor get destroyed in an LS ending if HK-47 is never rebuilt and G0-T0 isn't stopped? Why does the Exile have to preserve Malachor if they're DS? How is the party saved? What next? None of these questions are answered, but WildKarrde's mod not only answers them all, it does it with an incredible level of professionalism which makes it feel like it was content straight-up made by Obsidian, on a level more seamless than even restored content from TSLRCM often achieves.

Category & Tier: Restored Content & Immersion / 1 - Essential

Non-English Functionality: NO

Installation Method: TSLPatcher Mod

Masters: TSLRCM

Installation Instructions

If you are NOT playing the game on a 4:3 aspect ratio monitor, regardless of what your aspect ratio is, install the 16:9 main install option. Then re-run the patcher and apply the Transparent Cockpit Windows - Retexture Friendly option, if using the Transparent Cockpit Windows mod.

### Consistent Bastila Recognition

Name: Consistent Bastila Recognition

Author: Leilukin

Description: The player sees Bastila twice in KOTOR 2: once in a holo-recording left in T3's memory banks, and once in a vision from the Tomb of Ludo Kressh. The problem is that the player recognizes Bastila in Kressh's tomb, but not in the holo-recording. There's no reason why this should be, and this mod fixes the discrepancy: the player now recognizes Bastila in T3's recording also.

Category & Tier: Immersion / 2 - Recommended

Non-English Functionality: NO

Installation Method: HoloPatcher Mod

Masters: TSLRCM

Installation Instructions

Run the patcher using the Default install option, then re-run and apply the Expanded Ending compatch, if using Expanded Ending.

### Handmaiden Fit Model

Name: Handmaiden - Fit and Athletic and Patch

Author: Fair Strides, patch by JCarter426

Description: The Handmaiden has an inhumanly thin waist and scrawny arms by default. This mod fixes both as best it can, leaving her appearing more realistic and giving her at least some indication of upper arm strength.

Category & Tier: Appearance Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod & Loose-File Patch

Installation Instructions

Ignore all the loose files in the folder; use the TSLPatcher to install the mod. Then copy the two files from the patch into override.

### JC's Handmaiden Sisters

Name: JC's Handmaiden Sisters

Author: JCarter426

Description: By default, KOTOR 2 has all the handmaidens look exactly the same, except for Brianna herself. This mod restores the original intended appearance of the other sisters, so it actually makes sense when they say that Brianna honors the face of her mother.

Category & Tier: Restored Content & Immersion / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

Install the TSLRCM option.

### Replacement Loadscreens

Name: Replacement Loading Screens and Part 2 and Part 3

Author: Sith Holocron

Description: This mod replaces all the rendered loading screens with in-game shots at higher resolution and graphical quality, improving all the transitions without altering their angles or design.

Category & Tier: Graphical Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Move all the files but load_301NARa (in Part 2) to the override.

### Vibrocutter Changes

Author: Lewok2007

Description: This is just a thing what bothers me, from a game design perspective. Why would you get a weapon and then get another weapon immediately after, before facing any enemies with the first weapon? This mod moves the vibrocutter outside the Peragus medical wing to the body of one of the active droids on the route to the security room, forcing the player to either use the plasma torch or hand-to-hand combat to fight the droids for at least one room.

Category & Tier: Immersion / 4 - Optional

Non-English Functionality: YES, however some text will be in English

Installation Method: HoloPatcher Mod

Installation Instructions

Recommend the "Location and Medpac" install option, it gives the player an extra medpac as compensation for having to fight the first droids with a plasma torch.

### Peragus Monitor Adjust

Name: Peragus Large Monitor Adjustment

Author: Sith Holocron

Description: A hi-res reskin of Peragus's main monitor.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Use the files in the "Alternate Textures" folder.

### Replacement Peragus II

Name: Replacement Peragus II Artwork

Author: Sith Holocron

Description: Replaces the image of Peragus on the Administration Level with a piece of high-quality artwork for the planet.

Category & Tier: Graphics Improvement / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Move the files from the Original folder to the override, as well as the file from the "One Replacement Screen" folder if using Peragus Large Monitor Adjustment.

### TSL Galaxymap Fixpack

Name: TSL Galaxy Map Fix Pack

Author: bead-v, Kexikus, and Sith Holocron

Description: Moves the planets of KOTOR 2 to their old canon galactic positions and animates the Ebon Hawk's galaxy map.

Category & Tier: Graphics Improvement & Immersion / 3 - Suggested

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Installation Instructions

Install the Vanilla/TSLRCM option.

### Fixed Hologram Models

Name: Fixed Hologram Models & Admiralty Redux

Author: DarthParametric

Description: By default the hologram models in TSL are essentially just character models with a transparency effect, which means you can see their models' eye sockets and other internal features through the hologram filter. This mod fixes all these model errors that it can, while also bringing back the Admiralty mod of yore, which reskins Carth and Cede to properly wear Republic officer uniforms in their holograms and in-game appearances.

Masters: TSLRCM

Category & Tier: Bugfix & Graphics Improvement & Appearance Change / 1 - Essential

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

Download Instructions

Download just the main file, not the robes patch.

### Relighting TSL

Author: J

Description: KOTOR 2 has some absolutely shittily-lit sections. This mod by J is an attempt to fix that awful lighting--although it's currently in beta, the sections which J has already finished are fully-functional ingame.

Category & Tier: Bugfix & Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Download and apply all files, unless NOT using TSLRCM (in which case, skip relightingtsl_298TELk_1.0.zip).

### Companion Clothes

Name: Companion Starting Armors

Author: Ol' Cappy

Description: Giving companions a unique version of their default clothes so they can keep their 'standard' look throughout the entire game while still having mechanically powerful equipment is a very common mod type. I typically don't like these mods, not because I disagree with the premise but because I tend to balk at +10 WIS modifiers on starting gear. Ol' Cappy, in my opinion, does it right here—these are unique clothes for most of the human companions which don't give insane bonuses, but instead small, rational, and most importantly balanced, lore-friendly and internally-consistent bonuses. These items are never going to be the best your team can get, but they are flavorful additions which will let you keep them in their default get a bit longer, if you so choose.

Category & Tier: Immersion / 3 - Suggested

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

### Improved Enlightenment/Corruption

Name: Improved LS Enlightenment & DS Corruption Feats

Author: offthegridmorty

Description: Light Side Enlightenment (not to be confused with the Force power Enlightenment) and Dark Side Corruption, the bonus feats given to Jedi Master and Sith Lord prestige classes, are really bad. Not only do they only impact a companion's alignment (which is rarely of use to the player), they are also buggy and can overflow, flinging the companion to the complete opposite alignment than they should be! They basically only exist to make things worse. This mod by Morty gives those feats a clear niche: now, instead of impacting companions' alignment directly, it impacts it indirectly by tripling the effects of any influence gain or loss.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Repair Affects Stun Droid

Name: Repair Affects Stun Droid

Author: offthegridmorty

Description: Because logically it should, right? Force abilities aren't magical... well, okay, they sort of are, but you know what I mean; their strength and finesse are dependent upon the skill and knowledge of the person using them. It makes sense that a Jedi who is very knowledgable about droids would know exactly how to disable them.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

### Improved Force Sight

Name: Improved Force Sight

Author: offthegridmorty

Description: Another by Morty, and a great companion to his True Sith Assassins mod, this mod improves Force Sight by giving it some more immediate combat utility, allowing it to significantly boost the player's Awareness stat, while also tweaking its behavior for Kreia and Visas.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: HoloPatcher Mod

Installation Instructions

Recommend installing all three modules.

### Alignment Affects Force Powers

Name: Alignment Affects Force Powers

Author: offthegridmorty

Description: Why is it that, if you've fallen to the absolute depth of the Dark Side, you can throw lightning no more powerful than when you were first beginning to fall? Why is it that, as a paragon of the Light, the strength of your connections changes nothing about the power of the Force you can use to defend yourself? Well, now it does. Be an enlightened Jedi with mighty buffs that last an extended duration, or a fierce Sith Lord whose lightnings are easy to cast and mercilessly damaging. But better still, this mod isn't about a power fantasy—there are consequences that come from aligning strongly with one side, leading to atrophied abilities from the other extreme. It's a well-thought-out design that is well-balanced and feels great ingame while also being logically consistent. In my view, that makes this mod a must-have.

Category & Tier: Mechanics Change & Immersion / 1 - Essential

Non-English Functionality: YES, though some text may be blank or in English

Installation Method: TSLPatcher

Installation Instructions

I strongly recommend the combined install of "Alignment Affects Force Powers + Treat Injury Affects Force Healing."

### Ultimate Character Overhaul Patches

Name: Ultimate Character Overhaul Patches

Author: ShiningRedHD

Description: This is the point at which we're going to install all the various patches produced for the Ultimate Character Overhaul for any of the other texture mods which you've installed.

Masters: Ultimate Character Overhaul & Various Texture Mods

Category & Tier: Patch & Graphics Improvement / 1 - Essential

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Scroll down to the "optional files" section of the mod page. You will install the compatches for all the content you chose to install, in the following order:

- TSLRCM
- The K2CP, ensuring to delete C_DrdAstro01.tga, C_DrdAstro02.tga, C_DrdAstro03.tga, C_Hutt03.tga, C_Kinrath01.tga, C_Kinrath02.tga, C_Kinrath03.tga, C_Kinrath04.tga, C_SenBall01D.tga, p_hk47_01.tga, C_hk50.tga, and C_hk51.tga before moving to your override. If utilizing Darth Sapiens's HD Visas mod, also delete P_VisasH01.tga, P_VisasHD01.tga and P_VisasHD02.tga.
- JC's Minor Fixes
- All remaining content that matches content you chose to use from the builds (skipping Better Twi'lek Male Heads, if using Leilukin's Male Twi'lek Diversity mod)

### Bao-Dur/Darth Maul

Author: Nimduril

Description: Reskins Bao-Dur to have a different default appearance, but also to have a complete Dark Side transformation.

Category & Tier: Appearance Change / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Realistic Visual Effects

Name: Realistic Visual Effects

Author: Shem

Description: Removes glowing and other similar unrealistic visual effects when using power strikes, flurries, etc.

Category & Tier: Appearance Change / 2 - Recommended

Non-English Functionality: YES

Installation Method: TSLPatcher Mod

### Remove Duplicate TGA/TPC

Name: Remove Duplicate TGA/TPC, OR an .sh script for Linux users

Author: Flachzangen, Malacand for the .sh script

Description: This is a script written by Flachzangen for Windows and Malacand for Linux which allows you to remove duplicate .TGA or .TPC files in your game directory. Files with the .TPC filetype take priority, so for several of the patches we've downloaded which are of the .tga filetype, we need to delete any .TPC duplicates in order to get them to read properly. That's what this script will do.

Note

It is CRITICAL to either use this script or manually delete these duplicates, as, if .tpc files take priority in some instances, the game will crash.

Category & Tier: Patch / 1 - Essential

Installation Method: .bat Patcher / .sh script

Installation Instructions

For Windows: Place DelDuplicateTGA-TPC in your main game folder (NOT override!), run it, say that TPC should be deleted and do not manually confirm. If you are on a Mac or other device that cannot run .bat files, you can also try downloading the 3C-FD tool to identify the duplicates. If you aren't comfortable with downloading C3-FD, a list of all the files you will need to manually delete can be found here.

MAKE SURE THIS SCRIPT RUNS PROPERLY! On some operating systems, overzealous antivirus setups, or filepaths with particular characters in them it may fail. If the file outputs "Finished, Press any key to continue" but does not list any files it deleted, it is not working, and if it does not work it will crash your game. You can try to resolve this by moving your Override folder to a different location (I recommend a subfolder in your Documents or Downloads directories), putting the .bat file there and running it through that directory, but if that doesn't work you will need to manually go through and delete the duplicates from the link above.

For Linux: Download the script from the linked GitHub page. The script does assume you're using the default installation path for the Steam version; if you aren't, you just need to change the GAMEDIR variable at the top of the script to point to your game folder (NOT your override folder) before you run it. You may have to run the script twice, if a given TPC file has BOTH a matching TGA file and a matching DDS file.

### TOMB

Name: Thematic Obscured Mausoleum Burial

Author: Sniggles & JCarter426

Description: In brief, this mod serves to fix many idiosyncrasies with the visions encountered in the Tomb of Ludo Kressh, as well as adding more player agency and dynamism into the final encounter with your doppelganger and the vision-Revan. For a full breakdown of all changes, please see the attached document on the mod page.

Do note—the fights here can be quite challenging depending upon your choices. Winning them is not mandatory, however, and you will still be able to continue even if you lose at some point.

Category & Tier: Mechanics Change / 2 - Recommended

Non-English Functionality: NO

Installation Method: TSLPatcher

### Character Textures & Model Fixes

Name: Character Textures & Model Fixes

Author: Redrob41

Description: This mod is an upscale of virtually all NPC textures in the game, including aliens and some creatures. But that's not all—Rob is skilled with texture work, and although this package uses AI as a base, he's gone over it by hand and corrected, adjusted, and in some cases entirely retextured parts of the textures to eliminate hallucinations, burring or fuzziness, and preserve the original aesthetic of the texture. What's more, Rob's forte is modeling, and he's included a huge series of model adjustments and fixes along with this package which fixes various rendering issues. I can't recommend this package highly enough.

Category & Tier: Graphics Improvement / 1 - Essential

Non-English Functionality: YES

Installation Method: Loose-File (with optional automation)

Download Instructions

Strongly recommend the 2x .tpc version; the fidelity loss for 2x is minimal, .tpc is a lossless conversion and is much slimmer in filesize, and the automated compatibility program for this mod relies on the filetype being .tpc to function.

Installation Instructions

If you are on a system which can run .bat files, I strongly recommend using the automated system to deal with compatibility for this mod. To do so, first extract redrob's mod from its archive, then enter the "TSL Optional Kreia Model" folder, copy the four model files within, paste them into the "Copy contents to TSL's Override folder" and overwrite. After this, navigate to this page and click on the button with the arrow pointing down at the top of the code list (the one which says "download raw file"). Place cleaner.bat in the folder from redrob's mod called "Copy contents to TSL's Override folder", then go to here and repeat the same step, placing cleanlist_k2.txt in the same location. You can now run cleanlist.bat: follow the instructions onscreen, selecting whether you did or didn't use any of the given mods (note that the first thing it will ask you is whether you've installed "Mandatory Deletions"; rather than a mod, this is a required step to delete files that need to be removed regardless of what mods you're using, so just approve it). The batch file will automatically delete the incompatible files based on which mods you select that you've used in your install. Once you reach the end of the list, you can exit the .bat window, delete cleaner.bat and cleanlist_k1_sf.txt, and then move the remaining files in the folder to your override.

If you cannot run .bat files or feel uncomfortable running this file, you will need to manually delete the files yourself, following the instructions here.

## Enhanced Widescreen Support Mods

Assuming that you are on the Aspyr patch of the game, the following mods can enhance your widescreen gameplay. These mods should only be installed if you are on the Aspyr patch; while as previously mentioned widescreen can technically be applied to a legacy KOTOR 2 installation independently of the Aspyr patch it is destabilizing to do so, and strongly recommended against when utilizing this guide.

### Main Menu Fix

Name: Main Menu Fix for Widescreen

Author: DarthParametric

Description: A fix for TSL's stretched menu when playing the game widescreen.

Category & Tier: Graphics Improvement & Bugfix / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

Installation Instructions

Apply the contents of the "for override folder". If you would like to have the TSLRCM version of the KOTOR 2 logo on your main menu, you will need to BOTH move the contents of the "FOR TSLRCM" folder to your override, as well as the contents of the separate download "Updated_TSLRCM_Logo_v1.8.6".

### K2 Cutscenes Rescaled

Name: K2 Cutscenes Rescaled

Author: Pops Maellard

Description: This mod is a killer cutscene improvement mod, using a very clean, very sharp AI pass to upscale and clarify the cutscenes.

Unfortunately, there's a price for the quality of Pops's work: the files are extremely hefty, coming in at a whopping 15 gigs. This mod is by far the better of the two cutscene mods available, but if you're on limited data, a poor connection or have limited drive space, it's a much better idea to use naelavok's remastered cutscenes below rather than Pops's version here. If you've got the space and the speed, though, this is where it's at.

Category & Tier: Graphics Improvement / 1 - Essential

Non-English Functionality: YES

Installation Method: Loose-File Mod

Download Instructions

Choose the resolution option which most closely fits your monitor's screen size during the download process. If using TSLRCM or Extended Enclave, download both the main file most closely corresponding to your monitor resolution as well as the file of the same resolution with "_mods" appended to the filename (for instance, k2rs_30fps_1920x1080.7z and k2rs_30fps_mods_1920x1080.7z).

Installation Instructions

When installing, there will be several optional files based on how dialogue in cutscenes are displayed (floating text or not, etc). Which of these you choose is your preference; use the mod screenshots to determine. Install the main module first, then open the "mods" version of the file and apply the upscaled cutscenes for any mods which you use from the main build list. Bear in mind that the files from this mod go in your movies folder, not override.

If you experience issues with the game minimizing during cutscenes, it might be necessary to go to this video and follow the instructions to use RAD tools to "upscale" the cutscenes to match your exact resolution, though I have not personally attempted this so I don't know how much time it would take to complete. I would estimate around 8 hours, which is the typical time it takes for the native movies to be scaled to 1920x1080 using the same system, but remember that this would be a fully-automated process.

### K2 Remastered Cutscenes

Name: KOTOR 2 Remastered Cutscenes

Author: naelavok

Description: Using predictive AI, /u/naelavok has upscaled the cutscenes for both games out of their horrendous default resolution, to be much more crisp and viewable. A movie replacer like this is necessary to fix the fullscreen bug (plus they're just really good), so it's hugely recommended to use it whether you experience issues with fullscreen play or not.

Category & Tier: Graphics Improvement / 1 - Essential

Non-English Functionality: YES

Installation Method: Loose-File Components, possible .bat patcher

Installation Instructions

Use only Pops Maellard's cutscenes or this version by naelavok. To install, choose the resolution option which most closely fits your monitor's screen size during the download process. Bear in mind that the files from this mod go in your movies folder, not override.

If you experience issues with the game minimizing during cutscenes, it might be necessary to go to this video and follow the instructions to use RAD tools to "upscale" the cutscenes to match your exact resolution, though I have not personally attempted this so I don't know how much time it would take to complete. I would estimate around 8 hours, which is the typical time it takes for the native movies to be scaled to 1920x1080 using the same system, but remember that this would be a fully-automated process.

### Loading Screen Rescaled

Name: K2 Loading Screen Rescaled

Author: Pops Maellard

Description: If you decide not to use Pops Mallard's cutscene mod above, this mod is a standalone version of their enhanced legal screen (which I personally enjoy) that you can download.

Category & Tier: Graphics Improvement / 4 - Optional

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Workbench Camera Tweak

Name: Workbench Upgrade Screen Camera Tweak

Author: DarthParametric

Description: This mod pulls the camera on the Workbench in closer to show off the textures of what you're editing better. Works best at high resolution and with texture mods, which is why it's listed here in widescreen improvements.

Category & Tier: Appearance Change / 3 - Suggested

Non-English Functionality: YES

Installation Method: Loose-File Mod

### Upscaled Maps

Author: Tunalex

Description: This mod upscales every map, including the minimap, to be much more visually clear. Much prettier and also less blurry!

Category & Tier: Graphics Improvement / 2 - Recommended

Non-English Functionality: YES

Installation Method: Loose-File Mod

## Misc. Basegame Issues & Fixes

### Crash After Character Creation

This is a common issue which users tend to think is caused by the mod builds, but 99.9% of the time you don't have a mod error, and this is a basegame issue. Sometimes the game refuses to load when you have the Frame Buffer Effects setting enabled, so just disabling it in your Advanced Graphics Options ingame will allow you to load the game. This can also get you past loadscreen crashes that may rarely occur later in the game.

### Character Stuck After Combat

This isn't a bug that modding can fix (or cause), it's an engine bug. It's related to the game's framerate being unsynced with your monitor's refresh rate. If you're on a 60hz monitor, you can just toggle v-sync. If you're 144hz or similar, you can either set your monitor to 60hz manually for the duration of play and also toggle v-sync, OR edit the game's .ini file to set the refresh rate setting to your monitor's refresh rate. Note that if you opt for the former option, some software can interfere with the monitor's refresh rate and game's framerate actually syncing, including but not limited to AI frame prediction; graphics software-based v-sync; and some capture software. If you set your refresh rate to 60hz and enable v-sync ingame and the issue still happens, start hunting through your system software. Something is interfering with the values syncing properly.

### Swoop Racing

Although I don't recommend staying in compatibility mode for Windows XP SP3 long-term, as it can exacerbate loadscreen crashes, sometimes swoop racing can mess up in a way that makes it necessary. If you find your swoop bike floating several meters off the track, exit the game and launch it in compatibility mode for XP SP3. You should be able to race normally, and you can turn off the compatibility mode after the racing segment is over.

### Dialogue Skipping

Dialogue skipping is when an NPC's dialogue line is registered by the game, but jumped past immediately as it's switched to—you briefly see the subtitles, then they're gone with no other indication that character was speaking. I often get asked if this is a build problem, but it's not. The builds exacerbate it in some ways by adding more data to the game (it seems to be related to a memory leak in some way), but the biggest culprit by far is the Aspyr patch, though the issue can happen on any game version. Thankfully, with the 3C-FD patcher, this issue is basically no longer a problem--though bear in mind that it is different from the "dialogue floatiness" I describe in the section on the Aspyr section, and the input delay dialogue skipping with the Aspyr patch still exists. That issue is not related to a memory leak and is not consistent, however, as traditional dialogue skipping is.