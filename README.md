# Telnet File Manager
<br>
I'm afraid this app fall into the category of 'Look on my works, ye Mighty, and despair'!<br>
This is a sort of UI built primarily for use with my PiPDP11 running BSD 2.11.
As I will no doubt be reminded, BSD 2.x never had a UI!<br>
This is a Visual Studio app written in C#. You will need this devenv to run the app<br>
as Windows binaries are a no no these days.....<br>
<br>
Usage:<br>
1. Click 'Connect' and fill in the boxes. The most critical is the 'Prompt' entry.<br>
   This is used to determine if the remote system is ready for more input.<br>
   It is a good idea to telnet into your system to see what this is.<br>
   At least 2 characters are required to minimise any ambiguity.<br>
   I use '$ ' (note space) for this. This set in the .login script in the user's directory.<br>
   Also, the exact spelling and case for 'login:' and 'Password:' may need to be changed in Client.cs/login(...).<br>
   The current text is set as above and matches to BSD 2.11 distro I have from: https://github.com/chasecovello/211bsd-pidp11<br>
   Click 'Connect'.<br>
   If it is your lucky day, the list box should fill with the contents of the user's root directory.<br>
   NB the Connect process will save your settings in the config file for the next run.<br>
   NNB the first run of this app will show a message that there is no config file. Just click 'OK' and continue.
<br>
<br>
2. The rest of the menu items should be fairly obvious. Needless to say, you can (if you must) open a console window<br>
   so you can do more complicated stuff ... eg compile a programme etc. (and see below)<br>
   A right click in the list allows for viewing/creating/deleting/copying a file.<br><br>
   a. Copy is not the same as you are used to in X11. If you click this over a file, the app will ask for a destination filename.<br>
      Create/delete do as expected.<br>
      View is an interesting option. Click this to open a file in Notepad. However, if you change the file, and close<br>
      Notepad and save the file, it will be written back into the file system. This option is primarily used to edit<br>
      script files which may be executable. As we all know, distinguishing between binaries and executable scripts is a bit of an art.<br>
      Use view to see what is in the file first.<br><br>
   b. A double click on a file will either open it in Notepad for text files or, open the console and run the file as a binary.<br>
      Then you can use the console to interact with running programme. Similarly, executable scripts will be run in the console as well.<br>
  <br><br>
  This app can be made to work with most of the old Unices, I have tried with BSD 2.9 and Ultrix 3.1 (they both need a Telnet accessible DL interface).
  Finally, although I am a vi fan, I'm afraid that even Notepad has its advantages!!!<br><br>
  This app works via the good old expect/send protocol. This is not entirely reliable ... patience is a virtue!!!
  <br><br>
  Ian Schofield<br>
  November 2026<br>
  
