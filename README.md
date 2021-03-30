# Valkyrie-Crusade-HD-Card-Farming
This is a program that looks like "miner" which will "mine" Valkyrie Crusade HD card from nubee's(or mynet?) server. 

## How to use：
* Copy the "thumb" folder from your phone, which will be inside "/sdcard/Android/data/com.nubee.valkyriecrusade/card/thumb" into the program's folder.
* Start the program and it will ask you for how many threads will be used to farm the cards. Warning: DON'T USE MORE THAN 10 THREADS ELSE YOU ARE DOING AN ATTACK TO THE SERVER!
* Start mining with getting the Valkyrie Crusade previous event lists and get the cards from 2 hours before event start at that day.
* The process is almost like "Bitcoin Mining" and will extreme slow as the cards need to guess it's upload timestamp. (So the example calculation will be at 11/10/2015 01:00:00 UTC-0, check all 8000 cards; at 11/10/2015 01:00:01 UTC-0, check all 8000 cards again. However the card numbers will reduced if you already gained the url of the card. )
* The url format will be https://d2n1d3zrlbtx8o.cloudfront.net/download/CardHD.zip/(cardid).(upload time). Our program will based on this to spam the server. It will return 200 if found and 403 if not found.
* You can close the program anytime, it will continue farm cards based on last found card. All the card url list will be stored in url.txt.
* Use this as your own risk, as this might becomes cyber attack if too many users using it. Valkyrie Crusade might charge you as DDOS the server. I'm not responsible for that!
* It will takes about 2 years of no shutting down PC to gain all urls. (Including game updated cards in this 2 years) About 17 trillion of guess urls will be spammed to the server. (So if about 20 users are using then Valkyrie Crusade server might not hold it too long!)

> Why I share this? 
> > I'm just learning how to multi threaded to request webpages and using HEAD to get headers only for fast url checking. 
> > I'm just learning how to decrypt htmls to gain previous events.
> > I'm just boring and no programs to write.

***

Again, I'm creating this program for learning purpose. User can limit the request speed so if they use this as an attack I'm no responsible for that! 

# Valkyrie-Crusade-HD-Card-Farming v2
So now as the game is now closing at May 30th, so I decided to make things fast, crawl the fandom wiki for images!
## How to use:
* Download the exe
* Open and wait
* All files will be downloaded in `Download` folder in same directory