--The values are read in as follows:

Run in FreeMode or NeuralMode. [0 == FreeMode, 1 == NeuralMode]
Read in previously saved pool. [0 == no, 1 == yes]
	//These two numbers will be altered as the User selects which mode they use to run via the Main Menu within the game.
	
Run in normal Mode or Debug Mode. [0 == Normal, 1 == Debug]
    //Debug mode will not perform as well and you will need to restart the program and change this value to return to normal mode after.
    //At this point it renders the empty padding tiles to show the user that they do in fact exist and it will also disable the spriteRenderer of any Tile that the Player or an Enemy currently occupies it.
	
Int value for the levels Y dimension.
Int value for the levels X dimension.
The sequence of values as they would be inputed into said 2D array to make up the level itself.

	//The values for the tiles and objects are as follows:

	* 0 = Background Tile.
	* 1 = Platform (Generic)
	* 2 = Player Spawn + Empty Tile
	* 3 = Enemy Spawn + Empty Tile.
	* 4 = Edge of level tile, like platform but primarially used for padding.
	* 5 = EmptyTile, used for padding.
	* 6 = Death Tile, kills on contact.
	 
	Only Tiles/Objects [0,1,2,3,6] should be used.
	4 and 5 are used to pad out the level to ensure everything works correctly and they are generated automatically.
	Do not worry about using those.

--The Input this far might look like this:
// 1
// 1
// 0
// 5
// 4
// 1 1 1 1
// 0 0 0 0
// 0 0 0 0
// 0 2 0 0
// 1 1 1 1
--

The program will also check to see if the config file exists when the user attempts to start Free or Neural Mode.
If the file does not exist the program will inform the user and create a dummy file that the user can then use to run the program.

Should the file exist it will check if it is correctly formatted. Should the file fail any of the following criteria the user will be informed as such and will not be able to continue until thre formatting is correct.
	
	* Exaclty 1 player object exists.
	* 50 or less enemies exist. No enemies existing is valid.
	* The first three values are either 0 or 1.
	* The defined level dimensions match up with the sequence of values that make up the levels layout.
	* No invalid characters are used.
	
	** While not a requirement to pass the format check, the end of the level should be possible to reach (An empty tile as indiciated in the above example) by the player to make use of the bonus to fitness granted to a run as well as the speed of completion.

	