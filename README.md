# Dreams_AI_task
interview_task

Created a simple interactive webGL prototype of the task with modifiable grid dimensions and hit pattern.
Can be tried on: http://magicmotiongames.com/peter/

Goal: Make all the points blue!
Red means up, Blue means down.

Controls:
    Click where you want to hit
    Press Esc to open main menu and start a new game or reset the current board

I've setup the project so that it could collect play data if the website host access is setup.

![image](https://user-images.githubusercontent.com/61064454/112385163-30dc2580-8ce7-11eb-84ca-8b24f52b2e66.png)

![image](https://user-images.githubusercontent.com/61064454/112385205-3df91480-8ce7-11eb-8040-c8fd02a1f1d3.png)

InputManager handles the creation of the game instance and creating the visualisation of the circles and the main menu

Game handles the game logic, meaning current board state, apply hit on board.

DbManager handles the data packaging into a form that is sent to a php for sqlite data collection

The system could be extended to not just send data to the database, but be able to read it and play the moves at at a certain frequency to visualise the moves.
Using the current visualisation, a custom entered initial state could be added and other ways of generating the randomised state.

With some extra time the probabilistic hitting could be added.


For leading a project with the goal of making an AI system that can optimise the moves for this game. I would do the following plan:

	1. Formalise assumptions and goals of the AI and game environment
	2. Create the game environment suitable for both machine learning and human player usage (used for later in data collection and testing)
	3. Create variations of AI systems for testing
	4. Train AI, to see what architectures work and if any assumptions made previously are violated or are wrong
	5. Collect data with the environment created in (1.) to see human player behaviour.
	6. Compare the performance of human players and AI with available pool of players.
	7. Formalise assumptions and goal for the new problem of making AI better than human players
    	8. Review the systems
