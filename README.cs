OutLock
=======

OutLock is an attempt to display the number of unread mails in an Outlook (tm) inbox running on the desktop on the Windows 8 lockscreen. The communication methods between desktop and WinRT applications are limited (on purpose) so this project contains a WinRT app which is responsible for the WinRT background 
sdf
as
fd
s
dftasks and the lock screen badge and an Outlook (tm) addin which provides the number of unread mails.

*Approach*

There are two methods available, file-based where the two parts communicate via a local file in the WinRT app's LocalState folder and a pu
sdf
as
df
as
df
asd

asd

asd
f
sd
sh-based one where the Azure Notification Hub is utilized to provide up to date information (the file-based approach is only updated every ~15 min).

*Usage* 

1. Clone
2. If you want to use push-notifications via the ServiceBus  you have to provide your Notification Hub name as well as listen/send secrets.
3. Build (use the appropriate certificates, associate with the store etc.)
4. Install the application, run, and configure
5. Install Outlook addin
